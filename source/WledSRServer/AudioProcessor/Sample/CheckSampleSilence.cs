namespace WledSRServer.AudioProcessor.Sample
{
    internal class CheckSampleSilence : Processor
    {
        private SampleData _sample;
        private double _squelch;
        private Action _onSilence;

        public CheckSampleSilence(double squelch, Action onSilence)
        {
            _squelch = squelch;
            _onSilence = onSilence;
        }

        public override void Init(AudioProcessChain chain)
        {
            _sample = chain.GetContext<SampleData>();
        }

        public override bool Process()
        {
            if (_sample.MaxSampleAbsValue >= _squelch) return true;
            _onSilence.Invoke();
            return false;
        }
    }

}
