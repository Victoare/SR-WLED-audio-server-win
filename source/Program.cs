using FftSharp;
using NAudio.Wave;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WledSRServer;

internal class Program
{
    volatile static bool keepRunning = true;
    private static ManualResetEventSlim showDisplay = new();
    volatile static int audioProcessMs = 0;
    volatile static int packetSendMs = 0;
    volatile static int packetTimingMs = 0;

    volatile static AudioSyncPacket_v2 packet = new();

    private static void Main(string[] args)
    {
        Console.WriteLine("===========================================================================");
        Console.WriteLine("                      WLED SoundReactive audio server                      ");
        Console.WriteLine("===========================================================================");

        if (!StartCapture())
            return;

        var noDisplay = args.Any(a => a.Equals("-noinfo", StringComparison.InvariantCultureIgnoreCase));

        var sender = new Thread(new ThreadStart(SenderThread));
        sender.Start();
        if (!noDisplay)
        {
            var display = new Thread(new ThreadStart(DisplayThread));
            display.Start();
        }

        Console.WriteLine("Press a key to exit");
        Console.ReadKey();
        keepRunning = false;

        sender.Join();
        // if (!noDisplay)
        //     display.Join();

        Console.WriteLine("End.");
    }

    private static bool StartCapture()
    {
        // var mmde = new MMDeviceEnumerator(); - endpoints
        var capture = new WasapiLoopbackCapture();

        var channelToCapture = 0;

        Console.WriteLine($"Capture WaveFormat: {capture.WaveFormat}");
        if (capture.WaveFormat.Channels < 1)
        {
            Console.Write($"Zero channel detected. We need at least one.");
            return false;
        }

        // NOTE: https://github.com/naudio/NAudio/issues/900 (WasapiLoopbackCapture WaveFormat conversion)

        Func<byte[], int, double> converter;
        switch (capture.WaveFormat.BitsPerSample)
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
                if (capture.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                    converter = (buffer, position) => BitConverter.ToSingle(buffer, position);
                else
                    converter = (buffer, position) => BitConverter.ToInt32(buffer, position); // needs test case
                break;
            default:
                Console.Write("Unsupported format");
                return false;
        }

        var fftWindow = new FftSharp.Windows.Hanning();

        var outputBands = packet.fftResult.Length;

        // logarithmic freq scale
        var minFreq = 20;
        var maxFreq = capture.WaveFormat.SampleRate / 2; // fftFreq[fftFreq.Length - 1];
        var freqDiv = maxFreq / minFreq;
        var freqBands = Enumerable.Range(0, outputBands + 1).Select(i => minFreq * Math.Pow(freqDiv, (double)i / outputBands)).ToArray();

        var sw = new Stopwatch();

        double agcMaxValue = 0;
        var buckets = new double[outputBands];
        var bucketFreq = new string[outputBands];

        capture.DataAvailable += (s, e) =>
        {
            sw.Restart();
            if (e.BytesRecorded == 0)
            {
                SetPackToZero();
                return;
            }

            // ===[ Collect samples ]================================================================================================

            int sampleCount = e.BytesRecorded / capture.WaveFormat.BlockAlign;  // All available Sample
            //sampleCount = (int)Math.Pow(2, Math.Floor(Math.Log2(sampleCount))); // Samples to FFT (must be pow of 2) - or use FftSharp.Pad.ZeroPad(windowed_samples);
            //freq count = (2^(exponent-1))+1 - 6=33, 7=65, 8=129, 9=257, 10=513, 11=1025 ...

            var values = new double[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                int position = (i + channelToCapture) * capture.WaveFormat.BlockAlign;
                values[i] = converter(e.Buffer, position);
            }

            var valMax = values.Max();
            if (valMax < 0.00001)
            {
                SetPackToZero();
                return;
            }

            // ===[ FFT ]================================================================================================

            fftWindow.ApplyInPlace(values, true);
            values = Pad.ZeroPad(values);
            double[] fftPower = FFT.Magnitude(FFT.Forward(values));
            double[] fftFreq = FFT.FrequencyScale(fftPower.Length, capture.WaveFormat.SampleRate);

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
                if (freqRange.Any())
                {
                    var min = freqRange.First();
                    var max = freqRange.Last();
                    var bucketItems = fftPower.Skip(min.idx).Take(max.idx - min.idx + 1).ToArray();
                    buckets[bucket] = bucketItems.Max();
                    bucketFreq[bucket] = $"{min.freq:f2}hz - {max.freq:f2}hz - {bucketItems.Length} count";
                }
                else
                {
                    buckets[bucket] = 0;
                    bucketFreq[bucket] = $"{freqBands[bucket]:f2}hz - {freqBands[bucket + 1]:f2}hz - {0} count";
                }
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

            audioProcessMs = (int)sw.ElapsedMilliseconds;

            if (!keepRunning)
                capture.StopRecording();
        };

        capture.RecordingStopped += (s, e) =>
        {
            keepRunning = false;
            capture.Dispose();
        };

        capture.StartRecording();
        return true;
    }

    private static void SetPackToZero()
    {
        packet.sampleRaw = 0;
        packet.sampleSmth = 0;
        packet.samplePeak = 0;
        packet.fftResult = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        packet.FFT_Magnitude = 0;
        packet.FFT_MajorPeak = 0;
    }

    private static async void SenderThread()
    {
        var endpoint = new IPEndPoint(IPAddress.Parse("239.0.0.1"), AppConfig.WLedMulticastGroupPort);
        Console.WriteLine($"UDP endpoint: {endpoint}");
        Console.WriteLine($"Binding to address: {AppConfig.LocalIPToBind}");

        var retryCount = 0;
        var targetPPS = 40; // Pack("frame") per second (max 50!)

        while (keepRunning)
        {
            try
            {
                using (var client = new UdpClient(AddressFamily.InterNetwork))
                {
                    client.Client.Bind(new IPEndPoint(AppConfig.LocalIPToBind, 0));

                    Console.WriteLine("UDP connected, sending data");
                    Console.WriteLine();
                    showDisplay.Set();

                    var sw = new Stopwatch();

                    /*
                    var tmr = new Timer(new TimerCallback(_ =>
                    {
                        packetTimingMs = (int)sw.ElapsedMilliseconds;
                        sw.Restart();
                        client.Send(packet.AsByteArray(), endpoint);
                        packetSendMs = (int)sw.ElapsedMilliseconds;
                    }), null, 0, 1000 / targetPPS);

                    while (keepRunning)
                        Thread.Sleep(100);

                    tmr.Dispose();

                    */

                    while (keepRunning)
                    {
                        sw.Restart();
                        client.Send(packet.AsByteArray(), endpoint);
                        packetSendMs = (int)sw.ElapsedMilliseconds;

                        // while (keepRunning && sw.ElapsedMilliseconds < 1000 / targetPPS) {  } // precise, high CPU
                        // while (keepRunning && sw.ElapsedMilliseconds < 1000 / targetPPS) { Thread.Sleep(1); } // semi precise, medium CPU
                        // if (packetSendMs < (1000 / targetPPS)) Thread.Sleep((1000 / targetPPS) - packetSendMs); // least precise, low CPU
                        Thread.Sleep(1000 / targetPPS);

                        packetTimingMs = (int)sw.ElapsedMilliseconds;
                    }

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                // resume after after hibernation causes exceptions => ex.SocketErrorCode==SocketError.NoBufferSpaceAvailable
                // TODO: Maybe differentiate between exceptions?
                // log, restart
                retryCount++;
                if (retryCount == 10)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    keepRunning = false;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("Sender thread stopped.");
    }

    private static void DisplayThread()
    {
        showDisplay.Wait();
        var curTop = Console.CursorTop;
        while (keepRunning)
        {
            Console.CursorVisible = false;
            Console.CursorTop = curTop;
            Console.CursorLeft = 0;
            Console.WriteLine("===[ packet preview ]======================================================");

            Console.WriteLine($"sampleRaw  : {packet.sampleRaw,-20:F45}");
            Console.WriteLine($"sampleSmth : {packet.sampleSmth,-20:F45}");
            Console.WriteLine($"samplePeak : {packet.samplePeak,-20}");
            for (var i = 0; i < packet.fftResult.Length; i++)
            {
                string bar = new('#', (int)(packet.fftResult[i] / 4));
                Console.WriteLine($"[{bar.PadRight(63, '-')}] {packet.fftResult[i],-3}  ");
            }
            Console.WriteLine($"FFT_Magnitude : {packet.FFT_Magnitude,-20:F32}");
            Console.WriteLine($"FFT_MajorPeak : {packet.FFT_MajorPeak,10:F4} (hz)                  ");
            Console.WriteLine();

            Console.WriteLine("===[ stats ]===============================================================");
            Console.WriteLine($"audioProcess: {audioProcessMs}ms    ");
            Console.WriteLine($"packetSend:   {packetSendMs}ms      ");
            Console.WriteLine($"packetTiming: {packetTimingMs}ms => {(packetTimingMs == 0 ? "" : 1000 / packetTimingMs):D} pps(fps)    ");

            Thread.Sleep(10);
        }

        Console.CursorVisible = true;
    }
}