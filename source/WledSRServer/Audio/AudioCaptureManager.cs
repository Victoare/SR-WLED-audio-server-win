using NAudio.CoreAudioApi;
using System.Data;
using System.Diagnostics;
using WledSRServer.Audio.AudioProcessor;
using WledSRServer.Audio.AudioProcessor.FFT;
using WledSRServer.Audio.AudioProcessor.FFTBuckets;
using WledSRServer.Audio.AudioProcessor.Packet;
using WledSRServer.Audio.AudioProcessor.Raw;
using WledSRServer.Audio.AudioProcessor.Sample;

namespace WledSRServer.Audio
{
    internal static class AudioCaptureManager
    {
        #region GetDevices

        public record SimpleDeviceDescriptor(string ID, string Name);

        public static SimpleDeviceDescriptor[] GetDevices()
        {
            var mmde = new MMDeviceEnumerator();
            var endpoints = mmde.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            return endpoints.Select(d => new SimpleDeviceDescriptor(d.ID, d.FriendlyName))
                            .Prepend(new SimpleDeviceDescriptor("", "Loopback (system output)"))
                            .ToArray();
        }

        #endregion

        public static string[]? FFTfreqBands;
        public static AudioProcessChain? ActiveChain;

        public delegate void PacketUpdatedHandler();
        public static event PacketUpdatedHandler? PacketUpdated;

        private static Thread? _managerThread;
        private static WasapiCapture? _capture;
        private static volatile bool _autoRestartCapture = false;
        private static ManualResetEventSlim _captureStopped = new(false);

        #region public methods

        public static void Run()
        {
            _autoRestartCapture = true;
            _managerThread = new Thread(new ThreadStart(RunThread)) { Name = "Audio input" };
            _managerThread.Start();
        }

        public static void Stop()
        {
            _autoRestartCapture = false;
            _capture?.StopRecording();

            if (_managerThread == null)
                return;

            _managerThread.Join();
            _managerThread = null;
        }

        public static void RestartCapture()
        {
            _capture?.StopRecording();
        }

        #endregion

        private static void RunThread()
        {
            var audioDeviceEventWatcher = new AudioDeviceEventWatcher();
            audioDeviceEventWatcher.DefaultDeviceChanged += (flow, role, defaultDeviceId) =>
            {
                Debug.WriteLine("ADEW: DefaultDeviceChanged");
                if (string.IsNullOrEmpty(Properties.Settings.Default.AudioCaptureDeviceId))
                {
                    RestartCapture();
                }
            };

            while (_autoRestartCapture)
            {
                if (!StartCapture())
                {
                    Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Error;
                    Thread.Sleep(1000); // wait a bit to restart
                    continue;
                }
                _captureStopped.Wait(); // wait capturing to stop
            }

            audioDeviceEventWatcher.Dispose();
        }

        private static WasapiCapture? SetupCaptureDevice()
        {
            try
            {
                var deviceId = Properties.Settings.Default.AudioCaptureDeviceId;
                var audioBufferMs = 50; // min. seems to be 50
                if (string.IsNullOrEmpty(deviceId))
                    return new WasapiLoopbackCaptureEx(audioBufferMillisecondsLength: audioBufferMs);
                else
                    return new WasapiCapture(new MMDeviceEnumerator().GetDevice(deviceId), false, audioBufferMs);
            }
            catch (Exception ex)
            {
                Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Error;
                Program.ServerContext.AudioCaptureErrorMessage = ex.Message;
            }
            return null;
        }

        private static bool StartCapture()
        {
            Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.unknown;

            StopCapture();

            _capture = SetupCaptureDevice();
            if (_capture == null)
                return false;

            _captureStopped.Reset();

            Debug.WriteLine($"AUDIO: Capture WaveFormat: {_capture.WaveFormat}");
            if (_capture.WaveFormat.Channels < 1)
            {
                Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Error;
                Program.ServerContext.AudioCaptureErrorMessage = "Zero channel detected. We need at least one.";
                return false;
            }

            // NOTE: https://github.com/naudio/NAudio/issues/900 (WasapiLoopbackCapture WaveFormat conversion)

            try
            {
                var chain = SetupChain();
                ActiveChain = chain;
                _capture.DataAvailable += (s, e) => chain.Process(e.Buffer, e.BytesRecorded);
            }
            catch (Exception ex)
            {
                Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Error;
                Program.ServerContext.AudioCaptureErrorMessage = ex.Message;
                return false;
            }


            _capture.RecordingStopped += (s, e) =>
            {
                if (e.Exception != null)
                {
                    Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Error;
                    Program.ServerContext.AudioCaptureErrorMessage = e.Exception.Message;
                }
                _captureStopped.Set();
            };

            try
            {
                _capture.StartRecording();
            }
            catch (Exception ex)
            {
                Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Error;
                Program.ServerContext.AudioCaptureErrorMessage = ex.Message;
                _capture?.Dispose();
                _capture = null;
                return false;
            }

            return true;
        }

        private static void StopCapture()
        {
            if (_capture == null)
                return;

            if (_capture.CaptureState != CaptureState.Stopped)
            {
                _capture.StopRecording();
                while (_capture.CaptureState != CaptureState.Stopped)
                    Thread.Sleep(100);
            }

            _capture?.Dispose();
            _capture = null;
        }

        private static AudioProcessChain SetupChain()
        {
            var settings = Properties.Settings.Default;

            var onSilence = () =>
            {
                // Program.ServerContext.Packet.SetToZero();
                Program.ServerContext.Packet.DecayValues(0.85f);
                Program.ServerContext.Packet.FFT_MajorPeak = 0;
                Program.ServerContext.Packet.SamplePeak = 0;
                Program.ServerContext.Packet.FrameCounter = 0; // without this, leds remains lit with like the last valid packet value
                Program.ServerContext.Packet.ZeroCrossingCount = 0;

                Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Capturing_Silence;
                Program.ServerContext.AudioCaptureErrorMessage = string.Empty;
                PacketUpdated?.Invoke();
            };

            var chain = new AudioProcessChain();
            //chain.AddProcessor(new RawLogger("Begin"));
            chain.AddProcessor(new CheckRawSilence(onSilence));
            chain.AddProcessor(new RawAccumulator((int)Math.Pow(2, 13))); // 13=8192 14=16384 (data length Wasapi 50ms~=11k, 100ms~=23K)
            //chain.AddProcessor(new RawLogger("After acc"));
            chain.AddProcessor(new SampleConverter(_capture.WaveFormat));
            chain.AddProcessor(new CheckSampleSilence(0.0001, onSilence));
            chain.AddProcessor(new External(() =>
            {
                Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Capturing_Sound;
                Program.ServerContext.AudioCaptureErrorMessage = string.Empty;
            }));
            chain.AddProcessor(new FFTransform(
                                        new FftSharp.Windows.FlatTop(),
                                        _capture.WaveFormat.SampleRate
                                   ));
            chain.AddProcessor(new BeatDetector(100, 500));
            chain.AddProcessor(new Bucketizer(
                                        16,
                                        settings.FFTLow,
                                        settings.FFTHigh,
                                        settings.FFTFreqLogScale,
                                        Bucketizer.ScaleFromString(settings.FFTValueScale, Bucketizer.Scale.SquareRoot)
                                    ));
            chain.AddProcessor(new External<FFTBucketData>((bucketData) =>
            {
                FFTfreqBands = bucketData.Values.Select(b => $"{b.FreqLow:F0}Hz - {b.FreqHigh:F0}Hz{(b.DataCount == 0 ? b.Interpolated ? " [<-/->]" : " [NO DATA]" : $" [{b.DataCount}]")}").ToArray();
            }));
            //chain.AddProcessor(new BucketAverager(10));
            chain.AddProcessor(new BucketGainControl(
                manual: settings.ManualGain,
                manualSpanReference: settings.ManualGainReference
            ));
            chain.AddProcessor(new SetPacket(Program.ServerContext.Packet));
            chain.AddProcessor(new External(() =>
            {
                Program.ServerContext.Packet.FrameCounter++;
                // Debug.WriteLine($"UpdateWatchers : {PacketUpdated?.GetInvocationList().Length}"); // check for proper unregistration
                PacketUpdated?.Invoke();
            }));

            return chain;
        }
    }
}
