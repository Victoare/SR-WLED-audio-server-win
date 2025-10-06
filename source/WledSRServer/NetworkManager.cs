using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using WledSRServer.Audio;
using WledSRServer.Properties;

namespace WledSRServer
{
    internal static class NetworkManager
    {
        private static Thread? _managerThread;
        private volatile static bool _keepThreadRunning = true;
        private volatile static AutoResetEvent _restartNetworkClient = new(false);
        private static List<IPEndPoint> endpoints = new();

        public static string NetworkError = "";

        public enum SendMode
        {
            [Display(Name = "Broadcast LAN (default)")]
            BroadcastLAN = 0,

            [Display(Name = "Broadcast SubNet")]
            BroadcastSubNet = 1,

            [Display(Name = "Multicast")]
            Multicast = 2,

            [Display(Name = "Target IP List")]
            TargetIPList = 3,
        }

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


        private static IPAddress localIPToBind
        {
            get
            {
                if (!IPAddress.TryParse(Settings.Default.LocalIPToBind, out var address))
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
                    foreach (var ep in endpoints)
                        client.Send(testPacket, ep);
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

        public static List<IPAddress> IPAddressList(string list)
        {
            return Regex.Split(list, "[^0-9.]").Where(s => !string.IsNullOrWhiteSpace(s)).Select(IPAddress.Parse).ToList();
        }

        private static void SenderThread()
        {
            while (_keepThreadRunning)
            {
                Exception? exception = null;

                try
                {
                    using (var client = new UdpClient(AddressFamily.InterNetwork))
                    {
                        Debug.WriteLine($"NETWORK: Bind");
                        client.Client.Bind(new IPEndPoint(localIPToBind, 0));

                        //var IpListSplitter = new Regex("^(?:(?:(?:0{0,2}\\d|0?[1-9]\\d|1\\d\\d|2[0-4]\\d|25[0-5])\\.){3}(?:0{0,2}\\d|0?[1-9]\\d|1\\d\\d|2[0-4]\\d|25[0-5]))$");
                        var IpListSplitter = new Regex("[^0-9.]");

                        switch (Settings.Default.NetworkSendMode)
                        {
                            case (int)SendMode.Multicast:
                                endpoints = new() { new IPEndPoint(IPAddress.Parse("239.0.0.1"), Settings.Default.WledUdpMulticastPort) };
                                client.JoinMulticastGroup(endpoints.First().Address, localIPToBind); // Needs IGMP Snooping on the router!
                                //client.MulticastLoopback = true;
                                break;
                            case (int)SendMode.BroadcastSubNet:
                                // subnet broadcast would be: 192.168.0.255 for /8 subnets
                                endpoints = IPAddressList(Settings.Default.NetworkBroadcastIPList)
                                                          .Select(ip => new IPEndPoint(ip, Settings.Default.WledUdpMulticastPort)).ToList();
                                client.EnableBroadcast = true;
                                break;
                            case (int)SendMode.TargetIPList:
                                endpoints = IPAddressList(Settings.Default.NetworkTargetIPList)
                                                          .Select(ip => new IPEndPoint(ip, Settings.Default.WledUdpMulticastPort)).ToList();
                                break;
                            default: // case (int)SendMode.BroadcastLAN:
                                endpoints = new() { new IPEndPoint(IPAddress.Parse("255.255.255.255"), Settings.Default.WledUdpMulticastPort) };
                                client.EnableBroadcast = true;
                                break;
                        }

                        #region Check if default local ip has changed (and reset client if needed)

                        System.Threading.Timer? ipCheckTimer = null;

                        if (string.IsNullOrEmpty(Settings.Default.LocalIPToBind))
                        {
                            // sometimes after Hibernation the automatic IP detection (when Settings.Default.LocalIPToBind is empty) detects the wrong address
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
                                Program.ServerContext.Packet.FrameCounter++;

                                foreach (var ep in endpoints)
                                    client.Send(Program.ServerContext.Packet.AsByteArray(), ep);

                                Program.ServerContext.PacketSendingStatus = PacketSendingStatus.Sending;
                                Program.ServerContext.PacketSendErrorMessage = string.Empty;

                                swPackageTiming.Restart();

                                Program.ServerContext.PacketCounter++; // = (Program.ServerContext.PacketCounter++) % 1000;
                                if (Program.ServerContext.PacketCounter > 10000) Program.ServerContext.PacketCounter = 0;
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

                        // sync to Audio events
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
