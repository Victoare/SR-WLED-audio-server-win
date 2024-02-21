namespace WledSRServer.AudioProcessor.Raw
{
    internal class CheckRawSilence : Processor
    {
        private RawData _raw;
        private Action _onSilence;

        public CheckRawSilence(Action onSilence)
        {
            _onSilence = onSilence;
        }

        public override void Init(AudioProcessChain chain)
        {
            _raw = chain.GetContext<RawData>();
        }

        public override bool Process()
        {
            if (_raw.Length > 0) return true; // maybe check for all zero byte in _raw.Bytes?
            _onSilence.Invoke();
            return false;
        }
    }

}
