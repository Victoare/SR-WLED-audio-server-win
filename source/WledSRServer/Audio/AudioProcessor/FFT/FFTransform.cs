using FftSharp;
using WledSRServer.Audio.AudioProcessor;

namespace WledSRServer.Audio.AudioProcessor.FFT
{
    internal class FFTData : Context
    {
        public double[] Values { get; set; } = Array.Empty<double>();
        public double[] Frequencies { get; set; } = Array.Empty<double>();

        public double PeakValue { get; set; }
        public double PeakFrequency { get; set; }

        public int[] GetIndexesByFreq(double freqLow, double freqHigh)
            => Frequencies.Select((freq, idx) => new { freq, idx })
                          .Where(f => f.freq >= freqLow && f.freq <= freqHigh)
                          .Select(f => f.idx)
                          .ToArray();

        public double[] GetValuesByFreq(double freqLow, double freqHigh)
            => GetIndexesByFreq(freqLow, freqHigh).Select(idx => Values[idx]).ToArray();
    }

    internal class FFTransform : Processor
    {
        private Sample.SampleData _sample;
        private FFTData _fft;
        private readonly IWindow _fftWindow;
        private readonly int _sampleRate;

        public FFTransform(IWindow fftWindow, int SampleRate)
        {
            _fftWindow = fftWindow;
            _sampleRate = SampleRate;
        }

        public override void Init(AudioProcessChain chain)
        {
            _sample = chain.GetContext<Sample.SampleData>();
            _fft = chain.DefineContext(new FFTData());
        }

        public override bool Process()
        {
            var values = new double[_sample.Length];
            Array.Copy(_sample.Values, values, _sample.Length);

            values = Pad.ZeroPad(values);
            _fftWindow.ApplyInPlace(values, true);

            var complexData = FftSharp.FFT.Forward(values);
            var positiveOnly = true; // using only half of the spectrum

            _fft.Values = FftSharp.FFT.Magnitude(complexData, positiveOnly); // WLED based on Magnitude (Scaling appliend in bucketizer)
            // _fft.Values = FftSharp.FFT.Power(complexData, positiveOnly);     // value[i] = 20 * Math.Log10(value[i])

            _fft.Frequencies = FftSharp.FFT.FrequencyScale(_fft.Values.Length, _sampleRate, positiveOnly);

            for (int i = 0; i < _fft.Values.Length; i++)
            {
                if (_fft.Values[i] > _fft.PeakValue)
                {
                    _fft.PeakValue = _fft.Values[i];
                    _fft.PeakFrequency = _fft.Frequencies[i];
                }
            }

            return true;
        }
    }
}
