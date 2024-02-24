using WledSRServer.AudioProcessor.FFT;
using WledSRServer.AudioProcessor.FFTBuckets;

namespace WledSRServer.AudioProcessor.Packet
{
    internal class SetPacket : Processor
    {
        private readonly AudioSyncPacket_v2 _packet;
        private FFTData _fft;
        private FFTBucketData _buckets;
        private double _agcMaxValue;

        public SetPacket(AudioSyncPacket_v2 packet)
        {
            _packet = packet;
        }

        public override void Init(AudioProcessChain chain)
        {
            _fft = chain.GetContext<FFTData>();
            _buckets = chain.GetContext<FFTBucketData>();
        }

        public override bool Process()
        {
            var bucketMinValue = _buckets.Values.Min(b => b.Value);
            var bucketMaxValue = _buckets.Values.Max(b => b.Value);

            if (_agcMaxValue < bucketMaxValue)
                _agcMaxValue = bucketMaxValue;
            else
                _agcMaxValue = (_agcMaxValue * 0.8 + bucketMaxValue * 0.2);

            //var bucketSpan = bucketMaxValue - bucketMinValue;
            //var bucketSpan = peakPower - bucketMin;
            var bucketSpan = _agcMaxValue - bucketMinValue;

            for (var bucket = 0; bucket < _buckets.Values.Length; bucket++)
                _packet.fftResult[bucket] = (byte)((_buckets.Values[bucket].Value - bucketMinValue) * 255 / bucketSpan);

            var raw = (float)(_fft.PeakValue / _agcMaxValue * 255);
            //var raw = (float)(_fft.PeakValue / bucketMaxValue * 255);

            _packet.sampleRaw = raw; // 0...1023 ?
            _packet.sampleSmth = raw;
            _packet.samplePeak = (byte)raw;

            _packet.FFT_Magnitude = raw;
            _packet.FFT_MajorPeak = (float)_fft.PeakFrequency;

            return true;
        }
    }
}
