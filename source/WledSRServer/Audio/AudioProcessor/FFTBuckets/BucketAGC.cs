namespace WledSRServer.Audio.AudioProcessor.FFTBuckets
{
    internal class BucketAGCData : Context
    {
        public bool First { get; set; } = true;
        public double Offset { get; set; }
        public double Span { get; set; }
    }

    internal class BucketAGC : Processor
    {
        private BucketAGCData _agc;
        private FFTBucketData _buckets;

        public override void Init(AudioProcessChain chain)
        {
            _agc = chain.DefineContext(new BucketAGCData());
            _buckets = chain.GetContext<FFTBucketData>();
        }

        public override bool Process()
        {
            var bucketMin = _buckets.Values.Min(b => b.Value);
            var bucketMax = _buckets.Values.Max(b => b.Value);
            // var bucketAvg = _buckets.Values.Average(b => b.Value);

            // _agc.Offset = -bucketMinValue;
            _agc.Offset = 0;
            var span = bucketMax + _agc.Offset;

            if (_agc.First)
            {
                _agc.First = false;
                _agc.Span = span;
            }
            else
            {
                if (_agc.Span < span)
                    _agc.Span = (_agc.Span * 25 + span * 75) / 100; // faster converge
                else
                    _agc.Span = (_agc.Span * 90 + span * 10) / 100; // slower converge 
            }

            // Debug.WriteLine($"AGC {_agc.Offset} {_agc.Span}");

            return true;
        }
    }
}
