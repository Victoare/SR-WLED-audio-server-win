using System.Runtime.InteropServices;

namespace WledSRServer
{
    // private struct audioSyncPacket_v2 {
    //     char[6] header;        // 06 bytes, last byte is '\0' as string terminator.
    //     float sampleRaw;       // 04 Bytes  - either "sampleRaw" or "rawSampleAgc" depending on soundAgc setting
    //     float sampleSmth;      // 04 Bytes  - either "sampleAvg" or "sampleAgc" depending on soundAgc setting
    //     byte samplePeak;       // 01 Bytes  - 0 no peak; >=1 peak detected. In future, this will also provide peak Magnitude
    //     byte reserved1;        // 01 Bytes  - reserved for future extensions like loudness
    //     byte[16] fftResult;    // 16 Bytes  - FFT results, one byte per GEQ channel
    //     float FFT_Magnitude;   // 04 Bytes  - magnitude of strongest peak in FFT
    //     float FFT_MajorPeak;   // 04 Bytes  - frequency (in hz) of strongest peak in FFT    
    // }

    [StructLayout(LayoutKind.Sequential, Pack = 1)] // CharSet = CharSet.Ansi
    internal class AudioSyncPacket_v2
    {
        /// <summary>Version header - last byte is '\0' as string terminator</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string header = "00002";

        /// <summary>Padding?</summary>
        [MarshalAs(UnmanagedType.U1)]
        private byte fill0 = 0;
        /// <summary>Padding?</summary>
        [MarshalAs(UnmanagedType.U1)]
        private byte fill1 = 0;

        /// <summary>Either "sampleRaw" or "rawSampleAgc" depending on soundAgc setting</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float sampleRaw;

        /// <summary>Either "sampleAvg" or "sampleAgc" depending on soundAgc setting</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float sampleSmth;

        /// <summary>0 no peak; >=1 peak detected. In future, this will also provide peak Magnitude</summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte samplePeak;

        /// <summary>reserved for future extensions like loudness</summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte reserved1;

        /// <summary>FFT results, one byte per GEQ channel</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] fftResult = new byte[16];

        /// <summary>Padding?</summary>
        [MarshalAs(UnmanagedType.U1)]
        private byte fill2 = 0;
        /// <summary>Padding?</summary>
        [MarshalAs(UnmanagedType.U1)]
        private byte fill3 = 0;

        /// <summary>Magnitude of strongest peak in FFT</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float FFT_Magnitude;

        /// <summary>Frequency (in hz) of strongest peak in FFT</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float FFT_MajorPeak;
    }

    internal static class AudioSyncPacketExtensions
    {

        public static byte[] AsByteArray(this AudioSyncPacket_v2 data)
        {
            var size = Marshal.SizeOf<AudioSyncPacket_v2>();
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }

        public static AudioSyncPacket_v2 ToAudioSyncPacket_v2(this byte[] data)
        {
            var size = data.Length;
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);
            var asp = Marshal.PtrToStructure<AudioSyncPacket_v2>(ptr);
            Marshal.FreeHGlobal(ptr);
            return asp;
        }

    }
}
