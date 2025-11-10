using WledSRServer.Audio.AudioProcessor.FFT;
using WledSRServer.Audio.AudioProcessor.FFTBuckets;

namespace WledSRServer.Audio.AudioProcessor.Packet
{
    internal class SetPacket : Processor
    {
        private readonly AudioSyncPacket_v2 _packet;
        private Sample.SampleData _sample;
        private FFTData _fft;
        private BeatData _beat;
        private FFTBucketData _buckets;
        private BucketGainControlData _agc;

        // theoretical maximum in wled for PeakValueMax
        private static float wledPeakValueMax = 255 * 16; // WLED divides this value by 16 (or 8 / 4 / 2) to make it fit an uint8_t (casting causing modulo)
                                                          // https://github.com/wled/WLED/blob/main/wled00/FX.cpp
                                                          //   DJLight    -> PeakValue/2  -> 0..255
                                                          //   Freqmap    -> PeakValue/4  -> 0..255
                                                          //   Waterfall  -> PeakValue/8  -> 0..255
                                                          //   Freqpixels -> PeakValue/16 -> 0..255
                                                          //   Rocktaves  -> PeakValue/16 ~> 0..255

        public SetPacket(AudioSyncPacket_v2 packet)
        {
            _packet = packet;
        }

        public override void Init(AudioProcessChain chain)
        {
            _sample = chain.GetContext<Sample.SampleData>();
            _fft = chain.GetContext<FFTData>();
            _beat = chain.GetContext<BeatData>();
            _buckets = chain.GetContext<FFTBucketData>();
            _agc = chain.GetContext<BucketGainControlData>();
        }

        public override bool Process()
        {
            // var bucketMinValue = _buckets.Values.Min(b => b.Value);
            var bucketMaxValue = _buckets.Values.Max(b => Math.Abs(b.Value));
            var bucketAvgValue = _buckets.Values.Average(b => Math.Abs(b.Value));

            for (var bucket = 0; bucket < _buckets.Values.Length; bucket++)
            {
                _packet.FFT_Bins[bucket] = (byte)Math.Clamp((_buckets.Values[bucket].Value + _agc.Offset) / _agc.Span * 255, 0, 255);
            }

            var raw = (float)(bucketAvgValue / bucketMaxValue * 255);

            _packet.SampleRaw = raw;
            _packet.SampleSmth = raw;
            _packet.SamplePeak = (byte)(_beat.Detected ? 1 : 0);

            _packet.FFT_Magnitude = (float)((_buckets.PeakValue + _agc.Offset) / _agc.Span * wledPeakValueMax);
            _packet.FFT_MajorPeak = (float)_buckets.PeakFrequency;

            _packet.ZeroCrossingCount = (ushort)(_sample.ZeroCrossingCount / _sample.Length * 255);

            _packet.Pressure = (float)Math.Pow(_sample.MaxSampleAbsValue * 16, 2);

            return true;
        }
    }
}
