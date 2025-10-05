using NAudio.Wave;
using WledSRServer.Audio.AudioProcessor.Raw;

namespace WledSRServer.Audio.AudioProcessor.Sample
{
    internal class SampleConverter : Processor
    {
        private RawData _raw;
        private SampleData _sample;
        private Func<byte[], int, double> _converter;
        private int _bytesPerSample;
        private int _channels;

        public SampleConverter(WaveFormat format)
        {
            _converter = GetConverter(format) ?? throw new Exception($"Unsupported wave format: {format}");
            _bytesPerSample = format.BitsPerSample / 8;
            _channels = format.Channels;
        }

        public override void Init(AudioProcessChain chain)
        {
            _raw = chain.GetContext<RawData>();
            _sample = chain.DefineContext(new SampleData());
        }

        private static Func<byte[], int, double>? GetConverter(WaveFormat waveFormat)
        {
            switch (waveFormat.BitsPerSample)
            {
                case 8:
                    return (buffer, position) => (sbyte)buffer[position]; // - probably bad, need test case
                case 16:
                    return (buffer, position) => BitConverter.ToInt16(buffer, position); // needs test case
                // case 24:
                //     // 3 byte => int32
                //     converter = (buffer, position) => BitConverter.ToInt32(buffer, position);
                //     byteStep = 3;
                //     break;
                case 32:
                    if (waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        return (buffer, position) => BitConverter.ToSingle(buffer, position);
                    else
                        return (buffer, position) => BitConverter.ToInt32(buffer, position); // needs test case
                    break;
            }

            return null;
        }

        public override bool Process()
        {
            _sample.Length = _raw.Length / (_bytesPerSample * _channels);
            _sample.EnsureSize(_sample.Length);

            var pos = 0;
            for (int i = 0; i < _sample.Length; i++)
            {
                var avg = 0.0;
                for (var c = 0; c < _channels; c++)
                {
                    avg += _converter(_raw.Values, pos);
                    pos += _bytesPerSample;
                }
                _sample.Values[i] = avg / _channels;

                // int position = (i + channelToCapture) * _capture.WaveFormat.BlockAlign;
                // values[i] = converter(e.Buffer, position);
            }

            return true;

        }
    }
}
