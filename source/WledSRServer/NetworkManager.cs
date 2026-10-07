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
        private static readonly object _sendLock = new();
        private volatile static bool _muted = false;
        private volatile static Action? _sendSilencePacket;

        private const int SilencePacketCount = 3;
        private const int SilencePacketIntervalMs = 20;

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

        /// <summary>
        /// Sends a few silent packets, then stops sending until <see cref="Unmute"/>.
        /// Without this, WLED keeps showing the last sound when the sending stops (shutdown, sleep, exit).
        /// </summary>
        public static void SendSilenceAndMute()
        {
            _muted = true;
            // A few times, as a single UDP packet can get lost
            for (var i = 0; i < SilencePacketCount; i++)
            {
                if (i > 0)
                    Thread.Sleep(SilencePacketIntervalMs);
                _sendSilencePacket?.Invoke();
            }
        }

        public static void Unmute()
        {
            _muted = false;
        }

        public static void Stop()
        {
            SendSilenceAndMute();

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

        /// <summary>
        /// Picks the local IPv4 address of the LAN interface (used when no local IP is set).
        /// VPNs (Tailscale, ...), VirtualBox / Hyper-V / WSL adapters are often listed first, so the first address is not good enough:
        /// prefer interfaces that have a default gateway, and among them the one Windows routes through.
        /// </summary>
        public static IPAddress? GetAutoLocalIPAddress()
        {
            var candidates = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.Supports(NetworkInterfaceComponent.IPv4)
                          && ni.OperationalStatus == OperationalStatus.Up
                          && !ni.IsReceiveOnly
                          && ni.NetworkInterfaceType is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel or NetworkInterfaceType.Ppp))
                .Select(ni => (Properties: ni.GetIPProperties(), Interface: ni))
                .SelectMany(ni => ni.Properties.UnicastAddresses
                    .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork && ua.PrefixLength < 32) // /32: point-to-point (VPN), no broadcast
                    .Select(ua => new
                    {
                        ua.Address,
                        HasGateway = ni.Properties.GatewayAddresses.Any(g => g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any)),
                    }))
                .ToList();

            var withGateway = candidates.Where(c => c.HasGateway).Select(c => c.Address).ToList();
            if (withGateway.Count > 1)
            {
                // More LAN interfaces (e.g. Ethernet + Wi-Fi): use the one Windows would route through
                var routed = GetRoutedLocalIPAddress();
                if (routed != null && withGateway.Contains(routed))
                    return routed;
            }

            return withGateway.FirstOrDefault() ?? candidates.Select(c => c.Address).FirstOrDefault();
        }

        // Source address Windows picks for an outside destination. Connecting a UDP socket sends no packet.
        private static IPAddress? GetRoutedLocalIPAddress()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53));
                return (socket.LocalEndPoint as IPEndPoint)?.Address;
            }
            catch
            {
                return null;
            }
        }

        #region Local network selection

        // Settings.Default.LocalIPToBind values:
        //   ""              -> automatic (GetAutoLocalIPAddress)
        //   "if:{id}"       -> a network interface (NetworkInterface.Id), its current IPv4 address is used
        //   anything else   -> a manually entered IP address
        public const string InterfacePrefix = "if:";

        public record LocalInterface(string Id, string Name, IPAddress? Address);

        /// <summary>
        /// IPv4 capable network interfaces (Address is null if the interface is not connected)
        /// </summary>
        public static List<LocalInterface> GetLocalInterfaces()
            => NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.Supports(NetworkInterfaceComponent.IPv4)
                              && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                              && !ni.IsReceiveOnly)
                    .Select(ni => new LocalInterface(
                        ni.Id,
                        ni.Name,
                        ni.OperationalStatus != OperationalStatus.Up ? null
                            : ni.GetIPProperties().UnicastAddresses
                                .Select(ua => ua.Address)
                                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)))
                    .ToList();

        public static string? GetInterfaceIdFromSetting(string setting)
            => setting.StartsWith(InterfacePrefix) ? setting.Substring(InterfacePrefix.Length) : null;

        /// <summary>
        /// Old versions stored the local IP address only. If it belongs to a current interface, store the interface instead,
        /// so a changing (DHCP) address does not break the setting.
        /// </summary>
        public static void MigrateLocalIPSetting()
        {
            if (!IPAddress.TryParse(Settings.Default.LocalIPToBind, out var address))
                return;
            var match = GetLocalInterfaces().FirstOrDefault(i => address.Equals(i.Address));
            if (match == null)
                return;
            Settings.Default.LocalIPToBind = InterfacePrefix + match.Id;
            Settings.Default.Save();
        }

        /// <summary>
        /// The local IP address the packets are sent from with the current setting (null if none found)
        /// </summary>
        public static IPAddress? GetLocalIPToBind()
            => localIPToBind.Equals(IPAddress.Any) ? null : localIPToBind;

        private static IPAddress localIPToBind
        {
            get
            {
                var setting = Settings.Default.LocalIPToBind;
                var interfaceId = GetInterfaceIdFromSetting(setting);
                if (interfaceId != null)
                {
                    // Selected interface not connected: fall back to the automatic selection
                    var address = GetLocalInterfaces().FirstOrDefault(i => i.Id == interfaceId)?.Address;
                    if (address != null)
                        return address;
                }
                else if (IPAddress.TryParse(setting, out var manualAddress))
                {
                    return manualAddress;
                }
                return GetAutoLocalIPAddress() ?? IPAddress.Any;
            }
        }

        #endregion

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

                        // Automatic or interface selection: the address can change (DHCP, cable plugged back, ...)
                        if (!IPAddress.TryParse(Settings.Default.LocalIPToBind, out _))
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
                        var send = new Action<bool>((silence) =>
                        {
                            // Called from the audio thread, the auto packet timer and SendSilenceAndMute
                            lock (_sendLock)
                            {
                                if (_muted && !silence)
                                    return;

                                try
                                {
                                    if (silence)
                                        Program.ServerContext.Packet.SetToZero(); // FrameCounter = 0 too, like the silence detection does

                                    Program.ServerContext.Packet.FrameCounter++;

                                    var packetBytes = Program.ServerContext.Packet.AsByteArray();
                                    foreach (var ep in endpoints)
                                        client.Send(packetBytes, ep);

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
                            }
                        });
                        var sendPacket = new Action(() => send(false));
                        _sendSilencePacket = () => send(true);

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

                        _sendSilencePacket = null;
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
