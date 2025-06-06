using System.Diagnostics.Eventing.Reader;

namespace WledSRServer.Audio.AudioProcessor.FFTBuckets
{
    internal class BucketGainControlData : Context
    {
        public bool First { get; set; } = true;
        public double Offset { get; set; }
        public double Span { get; set; }
    }

    internal class BucketGainControl : Processor
    {
        private BucketGainControlData _agc;
        private FFTBucketData _buckets;

        private bool manual = false;
        private float manualSpanReference;  // Reverence value to calculate manual span
        private double max = 0;

        public BucketGainControl(bool manual = false, float manualSpanReference = 50)
        {
            this.manual = manual;
            this.manualSpanReference = manualSpanReference;
        }

        public override void Init(AudioProcessChain chain)
        {
            _agc = chain.DefineContext(new BucketGainControlData());
            _buckets = chain.GetContext<FFTBucketData>();
        }

        public override bool Process()
        {
            //var bucketMin = _buckets.Values.Min(b => b.Value);
            var bucketMax = _buckets.Values.Max(b => b.Value);
            // var bucketAvg = _buckets.Values.Average(b => b.Value);

            // _agc.Offset = -bucketMin;
            _agc.Offset = 0;
            var span = bucketMax + _agc.Offset;

            if (_agc.First)
            {
                _agc.First = false;
                _agc.Span = span;
            }
            else
            {
                if (manual)
                {
                    //_agc.Span = (101 - manualSpanReference) / 101;
                    _agc.Span = 1 - Math.Log10(manualSpanReference + 1) / Math.Log10(101);
                }
                else
                {
                    if (_agc.Span < span)
                        _agc.Span = (_agc.Span * 25 + span * 75) / 100; // faster converge for louder sounds
                    else
                        _agc.Span = (_agc.Span * 90 + span * 10) / 100; // slower converge for quieter sounds
                }
            }

            // if (_agc.Span > max) // 0.042 seems to be the max in my system
            // {
            //     max = _agc.Span;
            //     System.Diagnostics.Debug.WriteLine($"MAX Span: {max}");
            // }
            // _agc.Span = max;

            return true;
        }
    }
}
