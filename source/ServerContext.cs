namespace WledSRServer
{
    internal class ServerContext
    {
        public volatile int AudioProcessMs = 0;
        public volatile int PacketSendMs = 0;
        public volatile int PacketTimingMs = 0;

        public volatile AudioSyncPacket_v2 Packet = new();
    }
}
