using WledSRServer.Audio.AudioProcessor.FFT;
using static WledSRServer.Audio.AudioProcessor.FFTBuckets.FFTBucketData;

namespace WledSRServer.Audio.AudioProcessor.FFTBuckets
{
    internal class Bucketizer : Processor
    {
        private readonly int _bucketCount;
        private readonly Func<double, double> _valueScaler;
        private readonly double[] _freqPoints;
        private FFTData _fft;
        private FFTBucketData _buckets;

        // loudest signals measured on my system, not too scientific, but good enough for now
        private static float normScaleLinearMax = 0.001769422435994416f;
        private static float normScaleLogarithmicMax = 7.4804198503512f;
        private static float normScaleSquareRootMax = 0.04193937709831769f;

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

            // Debug.WriteLine($"FreqBands: {string.Join(";", _freqPoints.Select(f => f.ToString()))}");
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
                    return (val) => Math.Abs(val) / normScaleLinearMax;
                case Scale.SquareRoot:
                    return (val) => Math.Sqrt(Math.Abs(val)) / normScaleSquareRootMax;
                case Scale.Logarithmic:
                    return (val) => val == 0 ? 0 : Math.Max(0, Math.Log(Math.Abs(val) * 1000000)) / normScaleLogarithmicMax;
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
            // Debug.WriteLine($"FreqBands: {string.Join(";", _fft.Frequencies.Select(f => f.ToString()))}");

            var freqIndexes = _fft.GetIndexesByFreq(_freqPoints[0], _freqPoints[_freqPoints.Length - 1]);
            var peakIndex = freqIndexes.MaxBy(idx => _fft.Values[idx]);
            _buckets.PeakValue = _fft.Values[peakIndex];
            _buckets.PeakFrequency = _fft.Frequencies[peakIndex];

            for (var bucket = 0; bucket < _bucketCount; bucket++)
            {
                var values = _fft.GetValuesByFreq(_freqPoints[bucket], _freqPoints[bucket + 1]);
                var bucketCount = 0;
                if (values.Any())
                {
                    _buckets.Values[bucket].Interpolated = false;
                    _buckets.Values[bucket].Value = values.Select(_valueScaler).Max();
                    // _buckets.Values[bucket].Value = values.Select(_valueScaler).Average();
                    bucketCount = values.Length;
                }
                else
                {
                    _buckets.Values[bucket].Interpolated = true;
                }

                _buckets.Values[bucket].DataCount = bucketCount;
            }

            for (var bucket = 1; bucket < _bucketCount - 1; bucket++)
                if (_buckets.Values[bucket].Interpolated)
                    _buckets.Values[bucket].Value = (_buckets.Values[bucket - 1].Value + _buckets.Values[bucket + 1].Value) / 2;
            // to improve: multi band interpolation

            return true;
        }
    }
}
