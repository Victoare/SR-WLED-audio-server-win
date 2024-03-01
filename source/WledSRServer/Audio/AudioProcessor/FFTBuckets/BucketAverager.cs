namespace WledSRServer.Audio.AudioProcessor.FFTBuckets
{
    internal class BucketAverager : Processor
    {
        private readonly int _historyCount;
        private FFTBucketData _buckets;
        private List<double[]> _history = new List<double[]>();

        public BucketAverager(int historyCount)
        {
            _historyCount = historyCount;
        }

        public override void Init(AudioProcessChain chain)
        {
            _buckets = chain.GetContext<FFTBucketData>();
        }

        public override bool Process()
        {
            const int expo = 10;

            _history.Add(_buckets.Values.Select(b => b.Value).ToArray());
            if (_history.Count > _historyCount) _history.RemoveAt(0);

            for (var b = 0; b < _buckets.Values.Length; b++)
            {
                double div = 0;
                double sum = 0;
                double mul = 0;
                for (var i = 0; i < _history.Count; i++)
                {
                    mul = Math.Pow(i + 1, expo);
                    sum += _history[i][b] * mul;
                    div += mul;
                }
                _buckets.Values[b].Value = sum / div;
            }

            return true;
        }
    }
}
