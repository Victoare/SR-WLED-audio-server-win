using FftSharp;

namespace WledSRServer.AudioProcessor.FFT
{
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

            _fft.Values = FftSharp.FFT.Magnitude(FftSharp.FFT.Forward(values), true);
            //_fft.Values = FftSharp.FFT.Power(FftSharp.FFT.Forward(Values), true);

            _fft.Frequencies = FftSharp.FFT.FrequencyScale(_fft.Values.Length, _sampleRate);

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
