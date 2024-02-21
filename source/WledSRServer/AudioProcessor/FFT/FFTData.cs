namespace WledSRServer.AudioProcessor.FFT
{
    internal class FFTData : Context
    {
        public double[] Values { get; set; } = Array.Empty<double>();
        public double[] Frequencies { get; set; } = Array.Empty<double>();

        public double PeakValue { get; set; }
        public double PeakFrequency { get; set; }
    }
}
