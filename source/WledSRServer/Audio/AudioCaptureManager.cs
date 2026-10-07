#if WINDOWS
using NAudio.CoreAudioApi;
#endif
using NAudio.Wave;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
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
#if WINDOWS
            if (OperatingSystem.IsWindows())
                return GetDevicesWindows();
#endif

            if (OperatingSystem.IsLinux())
                return GetDevicesLinux();

            throw new PlatformNotSupportedException($"Audio device enumeration is not supported on {Environment.OSVersion.Platform}.");
        }

#if WINDOWS
        private static SimpleDeviceDescriptor[] GetDevicesWindows()
        {
            var mmde = new MMDeviceEnumerator();
            var endpoints = mmde.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            return endpoints.Select(d => new SimpleDeviceDescriptor(d.ID, d.FriendlyName))
                            .Prepend(new SimpleDeviceDescriptor("", "Loopback (system output)"))
                            .ToArray();
        }
#endif

        private sealed class PactlSource
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";

            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }

        // This app only ever taps into an output (loopback of whatever this machine is
        // playing), never a real input like a mic - that's WLED's job, on its own line-in.
        // So only monitor sources (one per output/sink, capturable via `parec --device=`)
        // are listed here; real capture sources (mics, line-in ADCs, etc.) are excluded.
        private static SimpleDeviceDescriptor[] GetDevicesLinux()
        {
            const string notInstalled = "Failed to start 'pactl'. Is pulseaudio-utils (or pipewire-pulse) installed?";
            // JSON output (-f json) needs pactl from PulseAudio 16+ (or PipeWire's pipewire-pulse)
            const string tooOld = "'pactl' gave no usable device list. JSON output needs PulseAudio 16 or newer (or pipewire-pulse).";

            var psi = new ProcessStartInfo("pactl")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            psi.ArgumentList.Add("-f");
            psi.ArgumentList.Add("json");
            psi.ArgumentList.Add("list");
            psi.ArgumentList.Add("sources");

            Process? process;
            try
            {
                process = Process.Start(psi);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(notInstalled, ex);
            }

            using (process ?? throw new InvalidOperationException(notInstalled))
            {
                var stderrTask = process.StandardError.ReadToEndAsync();
                var json = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                var stderr = stderrTask.Result.Trim();

                if (process.ExitCode != 0)
                    throw new InvalidOperationException(
                        $"'pactl' failed (exit code {process.ExitCode}){(stderr.Length > 0 ? $": {stderr}" : "")}. "
                        + "Is the sound server running? JSON output also needs PulseAudio 16 or newer (or pipewire-pulse).");

                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidOperationException(tooOld);

                List<PactlSource>? sources;
                try
                {
                    sources = JsonSerializer.Deserialize<List<PactlSource>>(json);
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException(tooOld, ex);
                }

                return BuildLinuxDeviceList(sources ?? new List<PactlSource>());
            }
        }

        private static SimpleDeviceDescriptor[] BuildLinuxDeviceList(List<PactlSource> sources)
        {

            return sources
                .Where(s => s.Name.EndsWith(".monitor", StringComparison.Ordinal))
                .Select(s => new SimpleDeviceDescriptor(s.Name, string.IsNullOrEmpty(s.Description) ? s.Name : s.Description))
                .Prepend(new SimpleDeviceDescriptor("", "Loopback (system output, follows the default on capture restart)"))
                .ToArray();
        }

        #endregion

        public static string[]? FFTfreqBands;
        public static AudioProcessChain? ActiveChain;

        public delegate void PacketUpdatedHandler();
        public static event PacketUpdatedHandler? PacketUpdated;

        private static Thread? _managerThread;
        private static IAudioCapture? _capture;
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
            // Only WASAPI gives us default-device-changed notifications today; on Linux,
            // `parec --device=@DEFAULT_MONITOR@` is reconnected on each capture restart instead.
#if WINDOWS
            AudioDeviceEventWatcher? audioDeviceEventWatcher = null;
            if (OperatingSystem.IsWindows())
            {
                audioDeviceEventWatcher = new AudioDeviceEventWatcher();
                audioDeviceEventWatcher.DefaultDeviceChanged += (flow, role, defaultDeviceId) =>
                {
                    Debug.WriteLine($"ADEW: DefaultDeviceChanged ({flow}, {role})");
                    // Only the default render/multimedia device is captured (see WasapiLoopbackCaptureEx.GetDefaultLoopbackCaptureDevice)
                    if (flow != DataFlow.Render || role != Role.Multimedia)
                        return;
                    if (string.IsNullOrEmpty(Properties.Settings.Default.AudioCaptureDeviceId))
                    {
                        RestartCapture();
                    }
                };
            }
#endif

            while (_autoRestartCapture)
            {
                if (!StartCapture())
                {
                    Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Error;
                    Thread.Sleep(1000); // wait a bit to restart
                    continue;
                }
                _captureStopped.Wait(); // wait capturing to stop
                if (_autoRestartCapture && Program.ServerContext.AudioCaptureStatus == AudioCaptureStatus.Error)
                    Thread.Sleep(1000); // capture died on its own: back off instead of respawning in a tight loop
            }

#if WINDOWS
            audioDeviceEventWatcher?.Dispose();
#endif
        }

        private static IAudioCapture? SetupCaptureDevice()
        {
            try
            {
                var deviceId = Properties.Settings.Default.AudioCaptureDeviceId;

#if WINDOWS
                if (OperatingSystem.IsWindows())
                {
                    var audioBufferMs = 10; // 25ms seems to be the minimum. Any lower will give the same timing of ~14ms -> (Default Windows timer resolution).
                    if (string.IsNullOrEmpty(deviceId))
                        return new WasapiAudioCapture(new WasapiLoopbackCaptureEx(audioBufferMillisecondsLength: audioBufferMs));
                    else
                        return new WasapiAudioCapture(new WasapiCapture(new MMDeviceEnumerator().GetDevice(deviceId), false, audioBufferMs));
                }
#endif

                if (OperatingSystem.IsLinux())
                {
                    var device = string.IsNullOrEmpty(deviceId) ? "@DEFAULT_MONITOR@" : deviceId;
                    return new ParecLoopbackCapture(device);
                }

                throw new PlatformNotSupportedException($"Audio capture is not supported on {Environment.OSVersion.Platform}.");
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
                // var dataAvailableSW = Stopwatch.StartNew();
                ActiveChain = chain;
                _capture.DataAvailable += (s, e) =>
                {
                    // Debug.WriteLine($"_capture.DataAvailable called after {dataAvailableSW.Elapsed.TotalMilliseconds:0.00} ms. BytesRecorded: {e.BytesRecorded}");
                    // dataAvailableSW.Restart();
                    chain.Process(e.Buffer, e.BytesRecorded);
                };
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

            return AudioProcessChain.Build(chainBuilder =>
            {
                //chainBuilder.AddProcessor(new RawLogger("Begin"));
                chainBuilder.AddProcessor(new CheckRawSilence(onSilence));
                //chainBuilder.AddProcessor(new RawAccumulator((int)Math.Pow(2, 13))); // 13=8192 14=16384 (data length Wasapi audioBufferMs:50ms~=11k, audioBufferMs:100ms~=23K)
                //chainBuilder.AddProcessor(new RawLogger("After acc"));
                chainBuilder.AddProcessor(new SampleConverter(_capture.WaveFormat));
                chainBuilder.AddProcessor(new SampleAccumulator((int)Math.Pow(2, 11), (int)Math.Pow(2, 10))); // FFT needs 2^n samples. FFT resoultion would be half of this
                chainBuilder.AddProcessor(new CalculateSampleStatistics());
                chainBuilder.AddProcessor(new CheckSampleSilence(0.00001, onSilence));
                chainBuilder.AddProcessor(new FFTransform(
                                            new FftSharp.Windows.FlatTop(),
                                            _capture.WaveFormat.SampleRate
                                       ));
                chainBuilder.AddProcessor(new BeatDetector(100, 500));
                chainBuilder.AddProcessor(new Bucketizer(
                                            16,
                                            settings.FFTLow,
                                            settings.FFTHigh,
                                            settings.FFTFreqLogScale,
                                            Bucketizer.ScaleFromString(settings.FFTValueScale, Bucketizer.Scale.SquareRoot)
                                        ));
                chainBuilder.AddProcessor(new External<FFTBucketData>((bucketData) =>
                {
                    FFTfreqBands = bucketData.Values.Select(b => $"{b.FreqLow:F0}Hz - {b.FreqHigh:F0}Hz{(b.DataCount == 0 ? b.Interpolated ? " [<-/->]" : " [NO DATA]" : $" [{b.DataCount}]")}").ToArray();
                }));
                //chainBuilder.AddProcessor(new BucketAverager(10));
                chainBuilder.AddProcessor(new BucketGainControl(
                    manual: settings.ManualGain,
                    manualSpanReference: settings.ManualGainReference
                ));
                chainBuilder.AddProcessor(new SetPacket(Program.ServerContext.Packet));
                chainBuilder.AddProcessor(new CheckPackeSilence(Program.ServerContext.Packet, 1000, onSilence));
                chainBuilder.AddProcessor(new External(() =>
                {
                    Program.ServerContext.AudioCaptureStatus = AudioCaptureStatus.Capturing_Sound;
                    Program.ServerContext.AudioCaptureErrorMessage = string.Empty;

                    // Debug.WriteLine($"UpdateWatchers : {PacketUpdated?.GetInvocationList().Length}"); // check for proper unregistration
                    PacketUpdated?.Invoke();
                }));
            });
        }
    }
}
