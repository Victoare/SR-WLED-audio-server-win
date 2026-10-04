using WledSRServer;
using WledSRServer.Audio;
using WledSRServer.Properties;

internal class Program
{
    public static ServerContext ServerContext { get; } = new ServerContext();

    private static readonly System.Diagnostics.Stopwatch _statusThrottle = System.Diagnostics.Stopwatch.StartNew();

    private static void Main(string[] args)
    {
        if (args.Contains("--help") || args.Contains("-h"))
        {
            PrintUsage();
            return;
        }

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
                case "udp-port": if (TryParseInt(key, value, out var udpPort)) Settings.Default.WledUdpMulticastPort = udpPort; break;
                case "fft-low": if (TryParseInt(key, value, out var fftLow)) Settings.Default.FFTLow = fftLow; break;
                case "fft-high": if (TryParseInt(key, value, out var fftHigh)) Settings.Default.FFTHigh = fftHigh; break;
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

    private static bool TryParseInt(string key, string value, out int result)
    {
        if (int.TryParse(value, out result))
            return true;
        Console.Error.WriteLine($"Ignoring --{key}={value}: expected a whole number");
        return false;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: wled-sr-server-cli [--key=value ...]");
        Console.WriteLine();
        Console.WriteLine("  --list-devices         List capturable audio devices and exit");
        Console.WriteLine("  --device=<id>          Audio device (default: the default output's monitor)");
        Console.WriteLine("  --target-ip=<list>     Send to these IPs (target IP list mode)");
        Console.WriteLine("  --broadcast-ip=<list>  Send to these subnet broadcast addresses");
        Console.WriteLine("  --udp-port=<n>         UDP port (default from settings, 11988)");
        Console.WriteLine("  --fft-low=<hz>         Lowest FFT frequency");
        Console.WriteLine("  --fft-high=<hz>        Highest FFT frequency");
        Console.WriteLine("  --help, -h             Show this help");
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
