namespace WledSRServer.AudioProcessor
{
    internal class External : Processor
    {
        private Func<bool> _onProcess;

        public External(Func<bool> onProcess)
        {
            _onProcess = onProcess;
        }
        public External(Action onProcess)
        {
            _onProcess = () => { onProcess(); return true; };
        }

        public override void Init(AudioProcessChain chain)
        {
        }

        public override bool Process()
        {
            return _onProcess.Invoke();
        }
    }

    internal class External<TContext> : Processor where TContext : Context
    {
        private Func<TContext, bool> _onProcess;
        private TContext _context1;

        public External(Func<TContext, bool> onProcess)
        {
            _onProcess = onProcess;
        }

        public External(Action<TContext> onProcess)
        {
            _onProcess = (context) => { onProcess(context); return true; };
        }

        public override void Init(AudioProcessChain chain)
        {
            _context1 = chain.GetContext<TContext>();
        }

        public override bool Process()
        {
            return _onProcess.Invoke(_context1);
        }
    }
}

