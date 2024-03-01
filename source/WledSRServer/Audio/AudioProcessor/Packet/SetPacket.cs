using WledSRServer.Audio.AudioProcessor.FFT;
using WledSRServer.Audio.AudioProcessor.FFTBuckets;

namespace WledSRServer.Audio.AudioProcessor.Packet
{
    internal class SetPacket : Processor
    {
        private readonly AudioSyncPacket_v2 _packet;
        private FFTData _fft;
        private BeatData _beat;
        private FFTBucketData _buckets;
        private BucketAGCData _agc;

        public SetPacket(AudioSyncPacket_v2 packet)
        {
            _packet = packet;
        }

        public override void Init(AudioProcessChain chain)
        {
            _fft = chain.GetContext<FFTData>();
            _beat = chain.GetContext<BeatData>();
            _buckets = chain.GetContext<FFTBucketData>();
            _agc = chain.GetContext<BucketAGCData>();
        }

        public override bool Process()
        {
            // var bucketMinValue = _buckets.Values.Min(b => b.Value);
            var bucketMaxValue = _buckets.Values.Max(b => b.Value);
            var bucketAvgValue = _buckets.Values.Average(b => b.Value);

            for (var bucket = 0; bucket < _buckets.Values.Length; bucket++)
            {
                _packet.FFT_Bins[bucket] = (byte)Math.Clamp((_buckets.Values[bucket].Value + _agc.Offset) / _agc.Span * 255, 0, 255);
            }

            var raw = (float)(bucketAvgValue / bucketMaxValue * 2048);

            //Debug.WriteLine($"RAW: {raw}");

            _packet.SampleRaw = raw; // what is the range? 0...1023 ?
            _packet.SampleSmth = raw;
            _packet.SamplePeak = (byte)(_beat.Detected ? 1 : 0);

            _packet.FFT_Magnitude = (float)_fft.PeakValue;
            _packet.FFT_MajorPeak = (float)_fft.PeakFrequency;

            return true;
        }
    }
}
