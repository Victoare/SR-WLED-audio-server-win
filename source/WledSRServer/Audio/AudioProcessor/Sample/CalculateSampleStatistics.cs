using NAudio.Wave;
using WledSRServer.Audio.AudioProcessor.Raw;

namespace WledSRServer.Audio.AudioProcessor.Sample
{
    internal class CalculateSampleStatistics : Processor
    {
        private SampleData _sample;

        public CalculateSampleStatistics()
        {
        }

        public override void Init(AudioProcessChain chain)
        {
            _sample = chain.GetContext<SampleData>();
        }

        public override bool Process()
        {
            var zeroCrossings = 0;
            for (int i = 0; i < _sample.Length; i++)
            {
                if (i > 0)
                {
                    if (_sample.Values[i - 1] > 0 && _sample.Values[i] <= 0) zeroCrossings++;
                    if (_sample.Values[i - 1] < 0 && _sample.Values[i] >= 0) zeroCrossings++;
                }
            }
            _sample.ZeroCrossingCount = zeroCrossings;
            _sample.MaxSampleAbsValue = _sample.Values.Max(Math.Abs);

            return true;
        }
    }
}
