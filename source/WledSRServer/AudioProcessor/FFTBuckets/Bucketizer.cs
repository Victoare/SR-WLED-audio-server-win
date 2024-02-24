using WledSRServer.AudioProcessor.FFT;
using static WledSRServer.AudioProcessor.FFTBuckets.FFTBucketData;

namespace WledSRServer.AudioProcessor.FFTBuckets
{
    internal class Bucketizer : Processor
    {
        private readonly int _bucketCount;
        private readonly Func<double, double> _valueScaler;
        private readonly double[] _freqPoints;
        private FFTData _fft;
        private FFTBucketData _buckets;

        public enum Scale
        {
            Linear,
            Logarithmic,
            SquareRoot
        }

        public static Scale ScaleFromString(string setting, Scale def)
            => Enum.TryParse<Scale>(setting, true, out var scale) ? scale : def;

        public Bucketizer(int bucketCount, int freqMin, int freqMax, bool logFreqScale, Scale valueScale)
        {
            _bucketCount = bucketCount;
            _valueScaler = GetValueScaler(valueScale);
            _freqPoints = GetFreqBands(freqMin, freqMax, logFreqScale, bucketCount);
        }

        private static double[] GetFreqBands(int freqMin, int freqMax, bool logFreqScale, int count)
        {
            if (logFreqScale)
                return Enumerable.Range(0, count + 1).Select(i => freqMin * Math.Pow(freqMax / freqMin, (double)i / count)).ToArray();
            else
                return Enumerable.Range(0, count + 1).Select(i => freqMin + (freqMax - freqMin) / (double)count * i).ToArray();
        }

        private static Func<double, double> GetValueScaler(Scale scale)
        {
            switch (scale)
            {
                case Scale.Linear:
                    return (val) => val;
                case Scale.Logarithmic:
                    return (val) => val == 0 ? 0 : Math.Log(Math.Abs(val));
                case Scale.SquareRoot:
                    return (val) => Math.Sqrt(Math.Abs(val));
            }
            throw new Exception("Invalid scaling");
        }

        public override void Init(AudioProcessChain chain)
        {
            _fft = chain.GetContext<FFTData>();
            _buckets = chain.DefineContext(new FFTBucketData());

            _buckets.Values = new Bucket[_bucketCount];
            for (var bucket = 0; bucket < _bucketCount; bucket++)
            {
                _buckets.Values[bucket].FreqLow = _freqPoints[bucket];
                _buckets.Values[bucket].FreqHigh = _freqPoints[bucket + 1];
            }
        }

        public override bool Process()
        {
            for (var bucket = 0; bucket < _bucketCount; bucket++)
            {
                var freqIndexes = _fft.Frequencies.Select((freq, idx) => new { freq, idx })
                                                  .Where(itm => itm.freq >= _freqPoints[bucket] && itm.freq <= _freqPoints[bucket + 1])
                                                  .Select(itm => itm.idx)
                                                  .ToArray();
                var bucketCount = 0;
                if (freqIndexes.Any())
                {
                    var minIndex = freqIndexes.First();
                    var maxIndex = freqIndexes.Last();
                    var bucketItems = _fft.Values.Skip(minIndex).Take(maxIndex - minIndex + 1).Select(_valueScaler).ToArray();
                    _buckets.Values[bucket].Value = bucketItems.Max();
                    //buckets[bucket] = bucketItems.Average();
                    bucketCount = bucketItems.Length;
                }
                else
                {
                    _buckets.Values[bucket].Value = 0;
                }

                _buckets.Values[bucket].DataCount = bucketCount;
            }

            return true;
        }
    }
}
