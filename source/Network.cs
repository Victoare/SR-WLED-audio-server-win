using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WledSRServer
{
    internal static class Network
    {
        private static Thread? _sender;
        private volatile static bool _keepRunning = true;

        public static void Start()
        {
            Stop();

            _keepRunning = true;
            _sender = new Thread(new ThreadStart(SenderThread));
            _sender.Start();
        }

        public static void Stop()
        {
            _keepRunning = false;
            if (_sender == null)
                return;
            _sender.Join();
            _sender = null;
        }

        public static string[] GetLocalIPAddresses()
            => NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.Supports(NetworkInterfaceComponent.IPv4)
                              && ni.OperationalStatus == OperationalStatus.Up
                              && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                              && !ni.IsReceiveOnly
                           )
                    .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                    .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ua => ua.Address.ToString())
                    .ToArray();

        private static async void SenderThread()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("239.0.0.1"), Properties.Settings.Default.WledUdpMulticastPort);
            // Console.WriteLine($"UDP endpoint: {endpoint}");
            // Console.WriteLine($"Binding to address: {AppConfig.LocalIPToBind}");

            var retryCount = 0;
            var targetPPS = 40; // Pack("frame") per second (max 50!)

            while (_keepRunning)
            {
                try
                {
                    if (!IPAddress.TryParse(Properties.Settings.Default.LocalIPToBind, out var localIpToBind))
                        if (!IPAddress.TryParse(GetLocalIPAddresses().FirstOrDefault(), out localIpToBind))
                            localIpToBind = IPAddress.Any;

                    using (var client = new UdpClient(AddressFamily.InterNetwork))
                    {
                        client.Client.Bind(new IPEndPoint(localIpToBind, 0));

                        Console.WriteLine("UDP connected, sending data");
                        Console.WriteLine();

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

                        while (_keepRunning)
                        {
                            sw.Restart();
                            client.Send(Program.ServerContext.Packet.AsByteArray(), endpoint);
                            Program.ServerContext.PacketSendMs = (int)sw.ElapsedMilliseconds;

                            // while (keepRunning && sw.ElapsedMilliseconds < 1000 / targetPPS) {  } // precise, high CPU
                            // while (keepRunning && sw.ElapsedMilliseconds < 1000 / targetPPS) { Thread.Sleep(1); } // semi precise, medium CPU
                            // if (packetSendMs < (1000 / targetPPS)) Thread.Sleep((1000 / targetPPS) - packetSendMs); // least precise, low CPU
                            Thread.Sleep(1000 / targetPPS);

                            Program.ServerContext.PacketTimingMs = (int)sw.ElapsedMilliseconds;
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
                        Program.LogException(ex);
                        _keepRunning = false;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            // Console.WriteLine();
            // Console.WriteLine("Sender thread stopped.");
        }

    }
}
