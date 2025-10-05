using System.Diagnostics;
using WledSRServer.Audio.AudioProcessor.Raw;

namespace WledSRServer.Audio.AudioProcessor
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
        private List<Processor> _processors = new List<Processor>();
        private Dictionary<Processor, double?> _processorRuntimes = new();
        private Stopwatch _processorRuntimeSW = new Stopwatch();

        private Dictionary<Type, Context> _contextStore = new Dictionary<Type, Context>();
        private RawData _raw;

        #region Builder

        internal class AudioProcessChainBuilder
        {
            private AudioProcessChain _chain = new AudioProcessChain();

            public AudioProcessChainBuilder AddProcessor(Processor processor)
            {
                _chain._processors.Add(processor);
                processor.Init(_chain);
                return this;
            }

            public AudioProcessChain Build()
            {
                return _chain;
            }
        }

        public static AudioProcessChain Build(Action<AudioProcessChainBuilder> builder)
        {
            var b = new AudioProcessChainBuilder();
            builder.Invoke(b);
            return b.Build();
        }

        #endregion

        public AudioProcessChain()
        {
            _raw = DefineContext(new RawData());
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

        public TProcessor? GetProcessor<TProcessor>() where TProcessor : Processor
        {
            return _processors.Where(p => p.GetType() == typeof(TProcessor)).FirstOrDefault() as TProcessor;
        }

        public void Process(byte[] rawBytes, int length)
        {
            _processors.ForEach(p => _processorRuntimes[p] = null);

            // Debug.WriteLine($"Capture length : {length}");
            _raw.EnsureSize(length);
            Array.Copy(rawBytes, _raw.Values, length);
            _raw.Length = length;
            foreach (var p in _processors)
            {
                _processorRuntimeSW.Restart();
                var cont = p.Process();
                _processorRuntimes[p] = _processorRuntimeSW.Elapsed.TotalMicroseconds;
                if (!cont) break;
            }

            var totalRuntime = _processorRuntimes.Sum(kvp => kvp.Value) ?? 0;
            var processSpeed = totalRuntime > 0 ? (1000000 / totalRuntime) : 0;
            Debug.WriteLine($"AudioProcessChain: Processed {length} bytes in {totalRuntime:0.00} µs ({processSpeed:0.00} Hz)");
        }
    }
}
