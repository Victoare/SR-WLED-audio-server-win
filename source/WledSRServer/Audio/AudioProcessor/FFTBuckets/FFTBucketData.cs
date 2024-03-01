namespace WledSRServer.Audio.AudioProcessor.FFTBuckets
{
    internal class FFTBucketData : Context
    {
        public struct Bucket
        {
            public double FreqLow { get; set; }
            public double FreqHigh { get; set; }
            public int DataCount { get; set; }
            public bool Interpolated { get; set; }
            public double Value { get; set; }
        }

        public Bucket[] Values { get; set; } = Array.Empty<Bucket>();
    }
}
