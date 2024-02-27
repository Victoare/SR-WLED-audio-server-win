using WledSRServer.Audio.AudioProcessor;

namespace WledSRServer.Audio.AudioProcessor.Sample
{
    internal class CheckSampleSilence : Processor
    {
        private SampleData _sample;
        private double _squelch;
        private Func<bool> _onSilence;


        public CheckSampleSilence(double squelch, Func<bool> onSilence)
        {
            _squelch = squelch;
            _onSilence = onSilence;
        }

        public CheckSampleSilence(double squelch, Action onSilence, bool stopOnSilence = true)
        {
            _squelch = squelch;
            _onSilence = () => { onSilence(); return !stopOnSilence; };
        }

        public override void Init(AudioProcessChain chain)
        {
            _sample = chain.GetContext<SampleData>();
        }

        public override bool Process()
        {
            if (_sample.MaxSampleAbsValue >= _squelch) return true;
            return _onSilence.Invoke();
        }
    }

}
