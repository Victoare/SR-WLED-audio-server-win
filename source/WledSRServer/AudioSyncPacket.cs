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

    // new "V2" audiosync struct - 44 Bytes
    // struct __attribute__ ((packed)) audioSyncPacket {  // WLEDMM "packed" ensures that there are no additional gaps
    //   char    header[6];          // 06 Bytes  offset 0 - "00002" for protocol version 2 ( includes \0 for c-style string termination) 
    //   uint8_t pressure[2];        // 02 Bytes, offset 6  - sound pressure as fixed point (8bit integer,  8bit fraction) 
    //   float   sampleRaw;          // 04 Bytes  offset 8  - either "sampleRaw" or "rawSampleAgc" depending on soundAgc setting
    //   float   sampleSmth;         // 04 Bytes  offset 12 - either "sampleAvg" or "sampleAgc" depending on soundAgc setting
    //   uint8_t samplePeak;         // 01 Bytes  offset 16 - 0 no peak; >=1 peak detected. In future, this will also provide peak Magnitude
    //   uint8_t frameCounter;       // 01 Bytes  offset 17 - rolling counter to track duplicate/out of order packets
    //   uint8_t fftResult[16];      // 16 Bytes  offset 18 - 16 GEQ channels, each channel has one byte (uint8_t)
    //   uint16_t zeroCrossingCount; // 02 Bytes, offset 34 - number of zero crossings seen in 23ms
    //   float  FFT_Magnitude;       // 04 Bytes  offset 36 - largest FFT result from a single run (raw value, can go up to 4096)
    //   float  FFT_MajorPeak;       // 04 Bytes  offset 40 - frequency (Hz) of largest FFT result
    // };

    [StructLayout(LayoutKind.Sequential, Pack = 1)] // CharSet = CharSet.Ansi
    internal class AudioSyncPacket_v2
    {
        /// <summary>Version header - last byte is '\0' as string terminator</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string Header = "00002";

        // https://github.com/MoonModules/WLED-MM/blob/7cb8eebba61e0e14f15cbf036f68f6030e9f5ca0/usermods/audioreactive/audio_reactive.h#L1715
        /// <summary>Sound pressure as two byte fixed point. Should be 0..255 as 5db..105db</summary> 
        public SoundPressure Pressure = 0;

        /// <summary>Either "sampleRaw" or "rawSampleAgc" depending on soundAgc setting</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float SampleRaw;

        /// <summary>Either "sampleAvg" or "sampleAgc" depending on soundAgc setting</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float SampleSmth;

        /// <summary>0 no peak; >=1 peak detected. In future, this will also provide peak Magnitude</summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte SamplePeak;

        /// <summary>reserved for future extensions like loudness</summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte FrameCounter;

        /// <summary>FFT results, one byte per GEQ channel</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FFT_Bins = new byte[16];

        /// <summary>number of zero crossings seen in 23ms</summary>
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 ZeroCrossingCount = 0;

        /// <summary>Magnitude of strongest peak in FFT</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float FFT_Magnitude;

        /// <summary>Frequency (in hz) of strongest peak in FFT</summary>
        [MarshalAs(UnmanagedType.R4)]
        public float FFT_MajorPeak;
    }

    /// <summary>Sound pressure as two byte fixed point. Should be 0..255 as 5db..105db</summary> 
    [StructLayout(LayoutKind.Sequential, Size = 2)]
    internal struct SoundPressure
    {
        [MarshalAs(UnmanagedType.U1)]
        byte Integer = 0;

        [MarshalAs(UnmanagedType.U1)]
        byte Fraction = 0;

        public SoundPressure() { }

        // https://github.com/MoonModules/WLED-MM/blob/7cb8eebba61e0Be14f15cbf036f68f6030e9f5ca0/usermods/audioreactive/audio_reactive.h#L1715
        public static implicit operator SoundPressure(float value)
        {
            var clampedValue = (int)(Math.Clamp(value, 0, 255) * 256.0f);
            var integerPart = (byte)Math.Floor(clampedValue / 256.0m);
            var fractionPart = (byte)(clampedValue % 256);
            return new SoundPressure { Integer = integerPart, Fraction = fractionPart };
        }

        public static implicit operator float(SoundPressure value)
            => value.Integer + value.Fraction / 255.0f;

        public static implicit operator SoundPressure(double value) => (float)value;
        public static implicit operator double(SoundPressure value) => (double)value;
    }

    internal static class AudioSyncPacketExtensions
    {
        public static void SetToZero(this AudioSyncPacket_v2 data)
        {
            data.SampleRaw = 0;
            data.SampleSmth = 0;
            data.SamplePeak = 0;
            data.FFT_Bins = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            data.FFT_Magnitude = 0;
            data.FFT_MajorPeak = 0;
            data.Pressure = 0;
            data.FrameCounter = 0;
            data.ZeroCrossingCount = 0;
        }

        public static void DecayValues(this AudioSyncPacket_v2 data, float decayRate)
        {
            data.SampleRaw = DecayToZero(data.SampleRaw, decayRate);
            data.SampleSmth = DecayToZero(data.SampleSmth, decayRate);
            for (var i = 0; i < data.FFT_Bins.Length; i++)
                data.FFT_Bins[i] = (byte)Math.Floor(data.FFT_Bins[i] * decayRate);
            data.FFT_Magnitude = DecayToZero(data.FFT_Magnitude, decayRate);
            data.Pressure = DecayToZero(data.Pressure, decayRate);
        }

        private static float DecayToZero(float value, float decayRate)
        {
            if (value < 0.01f) return 0;
            return value * decayRate;
        }

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
