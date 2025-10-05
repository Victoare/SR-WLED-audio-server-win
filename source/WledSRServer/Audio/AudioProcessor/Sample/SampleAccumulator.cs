using WledSRServer.Audio.AudioProcessor.Sample;

namespace WledSRServer.Audio.AudioProcessor.Raw
{
    internal class SampleDataAccumulator : SampleData
    {
    }

    internal class SampleAccumulator : Processor
    {
        private SampleDataAccumulator _accumulator;
        private SampleData _samples;
        private readonly int _samplesToAccumulate;
        private readonly int? _samplesToSlide;

        /// <summary>
        /// Accumulate sample data for further processing
        /// </summary>
        /// <param name="samplesToAccumulate">Amount of sample to the further processors (overflow will kept for next round)</param>
        /// <param name="samplesToSlide">Amount of slide 0...samplesToAccumulate or null for no sliding</param>
        public SampleAccumulator(int samplesToAccumulate, int? samplesToSlide = null)
        {
            _samplesToAccumulate = samplesToAccumulate;
            _samplesToSlide = samplesToSlide;
        }

        public override void Init(AudioProcessChain chain)
        {
            _accumulator = chain.DefineContext(new SampleDataAccumulator());
            _samples = chain.GetContext<SampleData>();
        }

        public override bool Process()
        {
            //System.Diagnostics.Debug.WriteLine($"SampleAccumulator: Incoming samples: {_samples.Length}, Accumulated samples: {_accumulator.Length} of {_samplesToAccumulate} with slide of {_samplesToSlide}");

            if (_samples.Length > _samplesToAccumulate) // Chop - too much data would lead to overaccumulation
            {
                _samples.Length = _samplesToAccumulate;
                return true;
            }

            // add samples to accumulator
            _accumulator.EnsureSize(_accumulator.Length + _samples.Length);

            Array.Copy(_samples.Values, 0, _accumulator.Values, _accumulator.Length, _samples.Length);
            _accumulator.Length += _samples.Length;

            // If not accumulated enough, stop processing
            if (_accumulator.Length < _samplesToAccumulate)
            {
                _samples.Length = 0;
                return false;
            }

            // We have enough accumulated data, provide it to the next processor
            _samples.EnsureSize(_samplesToAccumulate);

            Array.Copy(_accumulator.Values, 0, _samples.Values, 0, _samplesToAccumulate);
            _samples.Length = _samplesToAccumulate;

            // Remove used samples (or the sliding part) from accumulator
            var samplesToRemove = _samplesToSlide ?? _samplesToAccumulate;
            _accumulator.Length -= samplesToRemove;

            // Trim accumulator if it grew too much (should not happen with correct parameters)
            var maxSamplesToKeep = _samplesToAccumulate + (_samplesToSlide ?? 0);
            if (_accumulator.Length > maxSamplesToKeep)
            {
                System.Diagnostics.Debug.WriteLine($"SampleAccumulator overaccumulation: Trimming accumulator from {_accumulator.Length} to {maxSamplesToKeep}. Increase sliding size!");
                _accumulator.Length = maxSamplesToKeep;
            }
            Array.Copy(_accumulator.Values, samplesToRemove, _accumulator.Values, 0, _accumulator.Length); // keep the rest of accumulated data

            return true;
        }
    }
}
