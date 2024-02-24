namespace WledSRServer.AudioProcessor.Raw
{
    internal class CheckRawSilence : Processor
    {
        private RawData _raw;
        private Func<bool> _onSilence;

        public CheckRawSilence(Func<bool> onSilence)
        {
            _onSilence = onSilence;
        }

        public CheckRawSilence(Action onSilence, bool stopOnSilence = true)
        {
            _onSilence = () => { onSilence(); return !stopOnSilence; };
        }

        public override void Init(AudioProcessChain chain)
        {
            _raw = chain.GetContext<RawData>();
        }

        public override bool Process()
        {
            if (_raw.Length > 0) return true; // maybe check for all zero byte in _raw.Bytes?
            return _onSilence.Invoke();
        }
    }

}
