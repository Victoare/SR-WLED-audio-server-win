using WledSRServer;
using WledSRServer.Audio;
using WledSRServer.Properties;

internal class Program
{
    public static ServerContext ServerContext { get; } = new ServerContext();

    private static readonly System.Diagnostics.Stopwatch _statusThrottle = System.Diagnostics.Stopwatch.StartNew();

    private static void Main(string[] args)
    {
        if (args.Contains("--list-devices"))
        {
            ListDevices();
            return;
        }

        foreach (var arg in args)
        {
            var parts = arg.Split('=', 2);
            if (parts.Length != 2 || !parts[0].StartsWith("--"))
            {
                Console.Error.WriteLine($"Ignoring unrecognised argument: {arg} (expected --key=value)");
                continue;
            }

            var (key, value) = (parts[0][2..], parts[1]);
            switch (key)
            {
                case "device": Settings.Default.AudioCaptureDeviceId = value; break;
                case "target-ip":
                    Settings.Default.NetworkTargetIPList = value;
                    Settings.Default.NetworkSendMode = (int)NetworkManager.SendMode.TargetIPList;
                    break;
                case "broadcast-ip":
                    Settings.Default.NetworkBroadcastIPList = value;
                    Settings.Default.NetworkSendMode = (int)NetworkManager.SendMode.BroadcastSubNet;
                    break;
                case "udp-port": Settings.Default.WledUdpMulticastPort = int.Parse(value); break;
                case "fft-low": Settings.Default.FFTLow = int.Parse(value); break;
                case "fft-high": Settings.Default.FFTHigh = int.Parse(value); break;
                default: Console.Error.WriteLine($"Unknown option: --{key}"); break;
            }
        }

        Console.WriteLine("WLED SR Server (Linux CLI)");
        Console.WriteLine($"  Audio device : {(string.IsNullOrEmpty(Settings.Default.AudioCaptureDeviceId) ? "@DEFAULT_MONITOR@ (default output monitor)" : Settings.Default.AudioCaptureDeviceId)}");
        Console.WriteLine($"  Send mode    : {(NetworkManager.SendMode)Settings.Default.NetworkSendMode}");
        Console.WriteLine($"  UDP port     : {Settings.Default.WledUdpMulticastPort}");
        Console.WriteLine("Press Ctrl+C to stop.");
        Console.WriteLine();

        var stopSignal = new ManualResetEventSlim(false);
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stopSignal.Set(); };
        AppDomain.CurrentDomain.ProcessExit += (_, _) => stopSignal.Set();

        AudioCaptureManager.PacketUpdated += OnPacketUpdated;
        AudioCaptureManager.Run();
        NetworkManager.Run();

        stopSignal.Wait();

        Console.WriteLine();
        Console.WriteLine("Stopping...");
        AudioCaptureManager.PacketUpdated -= OnPacketUpdated;
        AudioCaptureManager.Stop();
        NetworkManager.Stop();
    }

    private static void ListDevices()
    {
        AudioCaptureManager.SimpleDeviceDescriptor[] devices;
        try
        {
            devices = AudioCaptureManager.GetDevices();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not list audio devices: {ex.Message}");
            Environment.ExitCode = 1;
            return;
        }

        Console.WriteLine("Available audio devices (use with --device=<id>):");
        foreach (var device in devices)
        {
            var id = string.IsNullOrEmpty(device.ID) ? "(default)" : device.ID;
            Console.WriteLine($"  {id,-60} {device.Name}");
        }
    }

    private static void OnPacketUpdated()
    {
        if (_statusThrottle.ElapsedMilliseconds < 200)
            return;
        _statusThrottle.Restart();

        var ctx = ServerContext;
        Console.Write(
            $"\r{DateTime.Now:HH:mm:ss}  capture={ctx.AudioCaptureStatus,-17} send={ctx.PacketSendingStatus,-8} " +
            $"peak={ctx.Packet.FFT_MajorPeak,7:F1}Hz  samplePeak={ctx.Packet.SamplePeak,3}  pkts={ctx.PacketCounter,6}   ");
    }
}
