using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace WledSRServer
{
    internal static class Network
    {
        private static Thread? _sender;
        private volatile static bool _keepThreadRunning = true;

        public static void Start()
        {
            Stop();

            _keepThreadRunning = true;
            _sender = new Thread(new ThreadStart(SenderThread));
            _sender.Start();
        }

        public static void Stop()
        {
            _keepThreadRunning = false;
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

        private static IPEndPoint endpoint => new IPEndPoint(IPAddress.Parse("239.0.0.1"), Properties.Settings.Default.WledUdpMulticastPort);
        private static IPAddress localIPToBind
        {
            get
            {
                if (!IPAddress.TryParse(Properties.Settings.Default.LocalIPToBind, out var address))
                    if (!IPAddress.TryParse(GetLocalIPAddresses().FirstOrDefault(), out address))
                        address = IPAddress.Any;
                return address;
            }
        }

        public static bool TestLocalIP(IPAddress localIp)
        {
            try
            {
                using (var client = new UdpClient(AddressFamily.InterNetwork))
                {
                    client.Client.Bind(new IPEndPoint(localIp, 0));
                    var testPacket = new byte[5]; // intentionally wrong packet!
                    client.Send(testPacket, endpoint);
                    client.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static void SenderThread()
        {
            var retryCount = 0;
            var targetPPS = 45; // Pack("frame") per second (max 50!)

            while (_keepThreadRunning)
            {
                Exception? exception = null;

                try
                {
                    using (var client = new UdpClient(AddressFamily.InterNetwork))
                    {
                        client.Client.Bind(new IPEndPoint(localIPToBind, 0));

                        var sw = new Stopwatch();
                        var stopSending = new ManualResetEventSlim();

                        var tmr = new System.Threading.Timer(new TimerCallback(_ =>
                        {
                            sw.Restart();

                            try
                            {
                                client.Send(Program.ServerContext.Packet.AsByteArray(), endpoint);
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                                stopSending.Set();
                            }

                            if (!_keepThreadRunning)
                                stopSending.Set();

                            Program.ServerContext.PacketSendError = false;
                            Program.ServerContext.PacketCounter++; // = (Program.ServerContext.PacketCounter++) % 1000;
                            if (Program.ServerContext.PacketCounter > 100) Program.ServerContext.PacketCounter = 0;
                        }), null, 0, 1000 / targetPPS);

                        stopSending.Wait();

                        tmr.Dispose();
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                if (exception != null)
                {
                    // resume after after hibernation causes exceptions => ex.SocketErrorCode==SocketError.NoBufferSpaceAvailable
                    // TODO: Maybe differentiate between exceptions?
                    // log, restart
                    Program.ServerContext.PacketSendError = true;
                    retryCount++;
                    if (retryCount == 30)
                    {
                        Program.LogException(exception);
                        _keepThreadRunning = false;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }

    }
}
