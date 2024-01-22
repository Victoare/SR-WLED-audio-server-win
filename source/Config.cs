using System.Configuration;
using System.Net;

namespace WledSRServer
{
    internal static class AppConfig
    {
        public static int WLedMulticastGroupPort
        {
            get
            {
                if (!int.TryParse(ConfigurationManager.AppSettings["WledUdpMulticastPort"], out var value))
                    value = 11988;
                return value;
            }
        }

        public static IPAddress LocalIPToBind
        {
            get
            {
                if (!IPAddress.TryParse(ConfigurationManager.AppSettings["LocalIPToBind"], out var value))
                    value = IPAddress.Any;
                return value;
            }
        }

    }
}
