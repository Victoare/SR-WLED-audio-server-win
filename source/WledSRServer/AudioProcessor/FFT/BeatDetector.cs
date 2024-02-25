namespace WledSRServer.AudioProcessor.FFT
{
    internal class BeatData : Context
    {
        public bool Detected { get; set; } = false;
    }

    // https://en.wikipedia.org/wiki/Beat_detection

    internal class BeatDetector : Processor
    {
        private FFTData _fft;
        private BeatData _beat;
        public double _freqLow;
        public double _freqHigh;
        public List<double> _history = new List<double>();
        //public const int _maxHistory = 25; // ~1.5sec (~16 process / sec)
        public const int _maxHistory = 25;
        public const double _tresholdMultiplier = 1.20;

        public BeatDetector(double freqLow, double freqHigh)
        {
            _freqLow = freqLow;
            _freqHigh = freqHigh;
        }

        public override void Init(AudioProcessChain chain)
        {
            _fft = chain.GetContext<FFTData>();
            _beat = chain.DefineContext(new BeatData());
        }

        public override bool Process()
        {
            var currVal = _fft.GetValuesByFreq(_freqLow, _freqHigh).Max();

            if (_history.Count == _maxHistory)
                _beat.Detected = currVal > _history.Average() * _tresholdMultiplier;

            _history.Add(currVal);
            if (_history.Count > _maxHistory) _history.RemoveAt(0);

            return true;
        }
    }
}
