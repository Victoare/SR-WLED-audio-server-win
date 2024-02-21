using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using WledSRServer.Properties;

namespace WledSRServer
{
    internal static class NetworkManager
    {
        private static Thread? _managerThread;
        private volatile static bool _keepThreadRunning = true;
        private volatile static AutoResetEvent _restartNetworkClient = new(false);

        public static void Run()
        {
            _keepThreadRunning = true;
            _managerThread = new Thread(new ThreadStart(SenderThread)) { Name = "Network send" };
            _managerThread.Start();
        }

        public static void ReStart()
        {
            _restartNetworkClient.Set();
        }

        public static void Stop()
        {
            _keepThreadRunning = false;
            _restartNetworkClient.Set();

            if (_managerThread == null)
                return;

            _managerThread.Join();
            _managerThread = null;
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

        public static bool TestLocalIP(IPAddress localIp, out string? error)
        {
            try
            {
                using (var client = new UdpClient(AddressFamily.InterNetwork))
                {
                    client.Client.Bind(new IPEndPoint(localIp, 0));
                    client.MulticastLoopback = false;
                    var testPacket = new byte[5]; // intentionally wrong packet!
                    client.Send(testPacket, endpoint);
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            error = null;
            return true;
        }

        private static void SenderThread()
        {
            var maxPPS = 45; // Pack("frame") per second (max 50!)

            while (_keepThreadRunning)
            {
                Exception? exception = null;

                try
                {
                    using (var client = new UdpClient(AddressFamily.InterNetwork))
                    {
                        Debug.WriteLine($"NETWORK: Bind");
                        client.Client.Bind(new IPEndPoint(localIPToBind, 0));
                        client.MulticastLoopback = false;

                        #region Check if default local ip has changed (and reset client if needed)

                        System.Threading.Timer? ipCheckTimer = null;

                        if (string.IsNullOrEmpty(Settings.Default.LocalIPToBind))
                        {
                            // sometimes after Hibernation the automatic IP detection (when Properties.Settings.Default.LocalIPToBind is empty) detects the wrong address
                            ipCheckTimer = new System.Threading.Timer(new TimerCallback((_) =>
                            {
                                if ((client.Client.LocalEndPoint is not IPEndPoint clientEndpoint) ||
                                    clientEndpoint.Address.ToString() != localIPToBind.ToString())
                                {
                                    _restartNetworkClient.Set();
                                }
                            }), null, 0, 5000);

                        }

                        #endregion

                        var swPackageTiming = Stopwatch.StartNew();
                        var sendPacket = new Action(() =>
                        {
                            try
                            {
                                if (swPackageTiming.ElapsedMilliseconds > 1000 / maxPPS)
                                {
                                    client.Send(Program.ServerContext.Packet.AsByteArray(), endpoint);
                                    Program.ServerContext.PacketSendingStatus = PacketSendingStatus.Sending;
                                    Program.ServerContext.PacketSendErrorMessage = string.Empty;
                                    swPackageTiming.Restart();

                                    Program.ServerContext.PacketCounter++; // = (Program.ServerContext.PacketCounter++) % 1000;
                                    if (Program.ServerContext.PacketCounter > 100) Program.ServerContext.PacketCounter = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                                _restartNetworkClient.Set();
                            }
                        });

                        // Send packets even without audio
                        var autoPacketTimer = new System.Threading.Timer(new TimerCallback((_) =>
                        {
                            if (swPackageTiming.ElapsedMilliseconds < 500) return;
                            sendPacket();
                        }), null, 500, 500);

                        var sendPacketHandler = new AudioCaptureManager.PacketUpdatedHandler(sendPacket);
                        AudioCaptureManager.PacketUpdated += sendPacketHandler;

                        _restartNetworkClient.WaitOne();

                        AudioCaptureManager.PacketUpdated -= sendPacketHandler;
                        autoPacketTimer?.Dispose();
                        ipCheckTimer?.Dispose();

                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"NETWORK client error: {ex}");
                    exception = ex;
                }

                // ===[ Check for exception during send ]======================================================================

                if (exception != null)
                {
                    // resume after after hibernation causes exceptions => ex.SocketErrorCode==SocketError.NoBufferSpaceAvailable
                    // TODO: Maybe differentiate between exceptions?
                    // log, restart
                    Program.ServerContext.PacketSendingStatus = PacketSendingStatus.Error;
                    Program.ServerContext.PacketSendErrorMessage = exception.Message;
                    Debug.WriteLine($"NETWORK error - sleeping");
                    Thread.Sleep(1000);
                }
            }
        }

    }
}
