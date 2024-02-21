using System.Diagnostics;

namespace WledSRServer.AudioProcessor.Raw
{
    internal class RawLogger : Processor
    {
        private RawData _rawData;
        private string _pre;

        public RawLogger(string pre)
        {

        }

        public override void Init(AudioProcessChain chain)
        {
            _rawData = chain.GetContext<RawData>();
        }

        public override bool Process()
        {
            Debug.WriteLine($"RAW({_pre}): Len:{_rawData.Length}");
            return true;
        }
    }
}
