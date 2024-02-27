using WledSRServer.Audio.AudioProcessor;

namespace WledSRServer.Audio.AudioProcessor.Sample
{
    internal class SampleData : Context
    {
        public double[] Values { get; private set; } = Array.Empty<double>();
        public int Length { get; set; }
        public double MaxSampleAbsValue { get; set; }

        public void EnsureSize(int size)
        {
            if (size <= Values.Length) return;

            var tempArray = new double[size];
            Array.Copy(Values, tempArray, Values.Length);
            Values = tempArray;
        }
    }
}
