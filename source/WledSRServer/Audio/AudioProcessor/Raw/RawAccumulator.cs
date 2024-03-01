namespace WledSRServer.Audio.AudioProcessor.Raw
{
    internal class RawDataAccumulator : RawData
    {
    }

    internal class RawAccumulator : Processor
    {
        private RawDataAccumulator _accumulator;
        private RawData _raw;
        private readonly int _bytesToAccumulate;

        public RawAccumulator(int bytesToAccumulate)
        {
            _bytesToAccumulate = bytesToAccumulate;
        }

        public override void Init(AudioProcessChain chain)
        {
            _accumulator = chain.DefineContext(new RawDataAccumulator());
            _raw = chain.GetContext<RawData>();
        }

        public override bool Process()
        {
            if (_raw.Length > _accumulator.Values.Length) // Chop, nothing to accumulate
            {
                _raw.Length = _bytesToAccumulate;
                return true;
            }

            _accumulator.EnsureSize(_raw.Length + _accumulator.Length);

            Array.Copy(_raw.Values, 0, _accumulator.Values, _accumulator.Length, _raw.Length);
            _accumulator.Length += _raw.Length;

            if (_accumulator.Length < _bytesToAccumulate)
            {
                _raw.Length = 0;
                return false;
            }

            _raw.EnsureSize(_bytesToAccumulate);

            Array.Copy(_accumulator.Values, 0, _raw.Values, 0, _bytesToAccumulate);
            _raw.Length = _bytesToAccumulate;

            _accumulator.Length -= _bytesToAccumulate;
            Array.Copy(_accumulator.Values, _bytesToAccumulate, _accumulator.Values, 0, _accumulator.Length);

            return true;
        }
    }
}
