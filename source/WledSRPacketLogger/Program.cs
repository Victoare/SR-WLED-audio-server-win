using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace WledSRPacketLogger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var localIP = IPAddress.Parse("192.168.0.90");
            var localIP = IPAddress.Any;
            var multicastGroup = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 11988);

            using (var client = new UdpClient(new IPEndPoint(localIP, multicastGroup.Port)))
            {
                client.JoinMulticastGroup(multicastGroup.Address, localIP);

                var sw = Stopwatch.StartNew();
                var ipAdd = new IPEndPoint(IPAddress.Any, 0);
                while (sw.Elapsed.TotalSeconds < 10)
                {
                    var pack = client.Receive(ref ipAdd);
                    Console.WriteLine($"{sw.ElapsedMilliseconds,6} - {pack.Length} - {ipAdd}");
                }

                client.DropMulticastGroup(multicastGroup.Address);
            }
        }
    }
}
