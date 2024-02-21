using WledSRServer.AudioProcessor.Raw;

namespace WledSRServer.AudioProcessor
{
    internal class Context
    {
    }

    internal abstract class Processor
    {
        public abstract void Init(AudioProcessChain chain);
        public abstract bool Process();
    }

    internal class AudioProcessChain
    {
        private Dictionary<Type, Context> _contextStore = new Dictionary<Type, Context>();
        private List<Processor> _processors = new List<Processor>();

        private RawData _raw;

        public AudioProcessChain()
        {
            _raw = DefineContext(new RawData());
        }

        public void AddProcessor(Processor processor)
        {
            _processors.Add(processor);
            processor.Init(this);
        }

        public TContext GetContext<TContext>() where TContext : Context
        {
            if (!_contextStore.ContainsKey(typeof(TContext)))
                throw new Exception($"Context {typeof(TContext).Name} not found");
            return (TContext)_contextStore[typeof(TContext)];
        }

        public TContext DefineContext<TContext>(TContext context) where TContext : Context
        {
            if (_contextStore.ContainsKey(typeof(TContext)))
                throw new InvalidOperationException("Context already exists!");
            _contextStore.Add(typeof(TContext), context);
            return context;
        }

        public void Process(byte[] rawBytes, int length)
        {
            _raw.EnsureSize(length);
            Array.Copy(rawBytes, _raw.Values, length);
            _raw.Length = length;
            foreach (var p in _processors)
                if (!p.Process()) break;
        }
    }
}
