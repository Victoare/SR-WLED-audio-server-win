namespace WledSRServer
{
    internal class ServerContext
    {
        public volatile AudioCaptureStatus AudioCaptureStatus = AudioCaptureStatus.unknown;
        public volatile string AudioCaptureErrorMessage = string.Empty;

        public volatile PacketSendingStatus PacketSendingStatus = PacketSendingStatus.unknown;
        public volatile string PacketSendErrorMessage = string.Empty;

        public volatile int PacketCounter = 0;

        public volatile AudioSyncPacket_v2 Packet = new();
    }

    internal enum AudioCaptureStatus
    {
        unknown,
        Capturing_Sound,
        Capturing_Silence,
        Error
    }

    internal enum PacketSendingStatus
    {
        unknown,
        Sending,
        Error
    }

}
