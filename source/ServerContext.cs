namespace WledSRServer
{
    internal class ServerContext
    {
        public volatile int AudioProcessMs = 0;
        public volatile int PacketCounter = 0;
        public volatile bool PacketSendError = false;
        public volatile AudioSyncPacket_v2 Packet = new();
    }
}
