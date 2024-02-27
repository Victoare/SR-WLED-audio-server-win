using WledSRServer.Audio.AudioProcessor;

namespace WledSRServer.Audio.AudioProcessor.Raw
{

    internal class RawData : Context
    {
        public byte[] Values { get; private set; } = Array.Empty<byte>();
        public int Length { get; set; }

        public void EnsureSize(int size)
        {
            if (size <= Values.Length) return;

            var tempArray = new byte[size];
            Array.Copy(Values, tempArray, Values.Length);
            Values = tempArray;
        }
    }
}
