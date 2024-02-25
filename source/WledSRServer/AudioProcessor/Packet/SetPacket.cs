using System.Diagnostics;
using WledSRServer.AudioProcessor.FFT;
using WledSRServer.AudioProcessor.FFTBuckets;

namespace WledSRServer.AudioProcessor.Packet
{
    internal class SetPacket : Processor
    {
        private readonly AudioSyncPacket_v2 _packet;
        private FFTData _fft;
        private BeatData _beat;
        private FFTBucketData _buckets;
        private double _agcMaxValue;

        public SetPacket(AudioSyncPacket_v2 packet)
        {
            _packet = packet;
        }

        public override void Init(AudioProcessChain chain)
        {
            _fft = chain.GetContext<FFTData>();
            _beat = chain.GetContext<BeatData>();
            _buckets = chain.GetContext<FFTBucketData>();
        }

        public override bool Process()
        {
            var bucketMinValue = _buckets.Values.Min(b => b.Value);
            var bucketMaxValue = _buckets.Values.Max(b => b.Value);
            var bucketAvgValue = _buckets.Values.Average(b => b.Value);

            if (_agcMaxValue < bucketMaxValue)
                _agcMaxValue = bucketMaxValue;
            else
                _agcMaxValue = (_agcMaxValue * 0.8 + bucketMaxValue * 0.2);

            //var bucketSpan = bucketMaxValue - bucketMinValue;
            //var bucketSpan = peakPower - bucketMin;
            var bucketSpan = _agcMaxValue - bucketMinValue;

            for (var bucket = 0; bucket < _buckets.Values.Length; bucket++)
                _packet.FFT_Bins[bucket] = (byte)((_buckets.Values[bucket].Value - bucketMinValue) * 255 / bucketSpan);

            //var raw = (float)(_fft.PeakValue / _agcMaxValue * 255);
            //var raw = (float)(bucketMaxValue / _agcMaxValue * 255);
            //var raw = (float)(_fft.PeakValue / bucketMaxValue * 255);
            var raw = (float)(bucketAvgValue / bucketMaxValue * 2048);
            //var raw = (float)((bucketAvgValue-bucketMinValue) / bucketSpan * 1024);

            Debug.WriteLine($"RAW: {raw}");

            _packet.SampleRaw = raw; // what is the range? 0...1023 ?
            _packet.SampleSmth = raw;
            _packet.SamplePeak = (byte)(_beat.Detected ? 1 : 0);

            _packet.FFT_Magnitude = (float)_fft.PeakValue;
            _packet.FFT_MajorPeak = (float)_fft.PeakFrequency;

            return true;
        }
    }
}
