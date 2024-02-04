using FftSharp;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WledSRServer
{
    internal static class AudioCapture
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

        private static WasapiCapture? _capture;

        public static bool Capturing => _capture?.CaptureState == CaptureState.Capturing;
        public static string[]? FFTfreqBands;

        public static bool Start()
        {
            Stop();

            try
            {
                var deviceId = Properties.Settings.Default.AudioCaptureDeviceId;
                if (string.IsNullOrEmpty(deviceId))
                    _capture = new WasapiLoopbackCapture();
                else
                    _capture = new WasapiCapture(new MMDeviceEnumerator().GetDevice(deviceId));
            }
            catch (COMException)
            {
                _capture?.Dispose();
                _capture = null;
                return false;
            }

            var channelToCapture = 0;

            // Console.WriteLine($"Capture WaveFormat: {capture.WaveFormat}");
            if (_capture.WaveFormat.Channels < 1)
            {
                // Console.Write($"Zero channel detected. We need at least one.");
                return false;
            }

            // NOTE: https://github.com/naudio/NAudio/issues/900 (WasapiLoopbackCapture WaveFormat conversion)

            Func<byte[], int, double> converter;
            switch (_capture.WaveFormat.BitsPerSample)
            {
                case 8:
                    converter = (buffer, position) => (sbyte)buffer[position]; // - probably bad, need test case
                    break;
                case 16:
                    converter = (buffer, position) => BitConverter.ToInt16(buffer, position); // needs test case
                    break;
                // case 24:
                //     // 3 byte => int32
                //     converter = (buffer, position) => BitConverter.ToInt32(buffer, position);
                //     byteStep = 3;
                //     break;
                case 32:
                    if (_capture.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        converter = (buffer, position) => BitConverter.ToSingle(buffer, position);
                    else
                        converter = (buffer, position) => BitConverter.ToInt32(buffer, position); // needs test case
                    break;
                default:
                    Console.Write("Unsupported format");
                    return false;
            }

            var fftWindow = new FftSharp.Windows.Hanning();

            var packet = Program.ServerContext.Packet;
            var outputBands = packet.fftResult.Length;

            // logarithmic freq scale
            // var minFreq = 20;
            // var maxFreq = _capture.WaveFormat.SampleRate / 2; // fftFreq[fftFreq.Length - 1];
            var minFreq = Properties.Settings.Default.FFTLow;
            var maxFreq = Properties.Settings.Default.FFTHigh;
            var freqDiv = maxFreq / minFreq;
            var freqBands = Enumerable.Range(0, outputBands + 1).Select(i => minFreq * Math.Pow(freqDiv, (double)i / outputBands)).ToArray();

            var sw = new Stopwatch();

            double agcMaxValue = 0;
            var buckets = new double[outputBands];
            FFTfreqBands = new string[outputBands];

            _capture.DataAvailable += (s, e) =>
            {
                sw.Restart();
                if (e.BytesRecorded == 0)
                {
                    packet.SetToZero();
                    Program.ServerContext.PacketUpdated.Set();
                    return;
                }

                // ===[ Collect samples ]================================================================================================

                int sampleCount = e.BytesRecorded / _capture.WaveFormat.BlockAlign;  // All available Sample
                //sampleCount = (int)Math.Pow(2, Math.Floor(Math.Log2(sampleCount))); // Samples to FFT (must be pow of 2) - or use FftSharp.Pad.ZeroPad(windowed_samples);
                //freq count = (2^(exponent-1))+1 - 6=33, 7=65, 8=129, 9=257, 10=513, 11=1025 ...

                var values = new double[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    int position = (i + channelToCapture) * _capture.WaveFormat.BlockAlign;
                    values[i] = converter(e.Buffer, position);
                }

                // Debug.WriteLine($"AudioCapture: {values.Length}"); // 2880 per event

                var valMax = values.Max();
                if (valMax < 0.00001)
                {
                    packet.SetToZero();
                    Program.ServerContext.PacketUpdated.Set();
                    return;
                }

                // ===[ FFT ]================================================================================================

                fftWindow.ApplyInPlace(values, true);
                values = Pad.ZeroPad(values);
                double[] fftPower = FFT.Magnitude(FFT.Forward(values));
                double[] fftFreq = FFT.FrequencyScale(fftPower.Length, _capture.WaveFormat.SampleRate);

                // ===[ Peaks ]================================================================================================

                double peakFreq = 0;
                double peakPower = 0;
                for (int i = 0; i < fftPower.Length; i++)
                {
                    if (fftPower[i] > peakPower)
                    {
                        peakPower = fftPower[i];
                        peakFreq = fftFreq[i];
                    }
                }

                // ===[ AGC ]================================================================================================

                if (agcMaxValue < peakPower)
                    agcMaxValue = peakPower;
                else
                    agcMaxValue = (agcMaxValue * 0.9 + peakPower * 0.1);

                // ===[ Output FFT buckets ]================================================================================================

                for (var bucket = 0; bucket < buckets.Length; bucket++)
                {
                    var freqRange = fftFreq.Select((freq, idx) => new { freq, idx }).Where(itm => itm.freq >= freqBands[bucket] && itm.freq <= freqBands[bucket + 1]).ToArray();
                    var bucketCount = 0;
                    if (freqRange.Any())
                    {
                        var min = freqRange.First();
                        var max = freqRange.Last();
                        var bucketItems = fftPower.Skip(min.idx).Take(max.idx - min.idx + 1).ToArray();
                        buckets[bucket] = bucketItems.Max();
                        bucketCount = bucketItems.Length;
                        // FFTfreqBands[bucket] = $"{min.freq:f2}hz - {max.freq:f2}hz - {bucketItems.Length} count";
                    }
                    else
                    {
                        buckets[bucket] = 0;
                    }
                    // FFTfreqBands[bucket] = $"{freqBands[bucket]:F0}Hz - {freqBands[bucket + 1]:F0}Hz [{bucketCount} bin]";
                    FFTfreqBands[bucket] = $"{freqBands[bucket]:F0}Hz - {freqBands[bucket + 1]:F0}Hz";
                    if (bucketCount == 0) FFTfreqBands[bucket] += " [NO DATA]";
                }

                // ===[ Set packet properties ]================================================================================================

                var bucketMin = buckets.Min();
                //var bucketSpan = buckets.Max() - bucketMin;
                //var bucketSpan = peakPower - bucketMin;
                var bucketSpan = agcMaxValue - bucketMin;
                for (var bucket = 0; bucket < buckets.Length; bucket++)
                    packet.fftResult[bucket] = (byte)((buckets[bucket] - bucketMin) * 255 / bucketSpan);

                var raw = (float)(peakPower / agcMaxValue * 255);

                packet.sampleRaw = raw; // 0...1023 ?
                packet.sampleSmth = raw;
                packet.samplePeak = (byte)raw;
                packet.FFT_Magnitude = raw;
                packet.FFT_MajorPeak = (float)peakFreq;

                // ===[ Rinse and repeat ]================================================================================================

                Program.ServerContext.PacketUpdated.Set();
            };

            // _capture.RecordingStopped += (s, e) =>
            // {
            //     Program.ServerContext.KeepRunning = false;
            // };

            try
            {
                _capture.StartRecording();
            }
            catch (COMException)
            {
                _capture?.Dispose();
                _capture = null;
                return false;
            }

            return true;
        }

        public static void Stop()
        {
            if (_capture == null)
                return;

            _capture.StopRecording();
            while (_capture.CaptureState != CaptureState.Stopped)
                Thread.Sleep(100);

            _capture.Dispose();
            _capture = null;
        }


    }
}
