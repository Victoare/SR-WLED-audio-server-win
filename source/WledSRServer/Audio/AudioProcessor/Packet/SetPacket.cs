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

            _packet.FFT_Magnitude = (float)_buckets.PeakValue;
            _packet.FFT_MajorPeak = (float)_buckets.PeakFrequency;

            _packet.ZeroCrossingCount = (ushort)(_sample.ZeroCrossingCount / _sample.Length * 255);

            _packet.Pressure = (float)Math.Pow(_sample.MaxSampleAbsValue * 16, 2);

            return true;
        }
    }
}
