using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace WledSRServer.Audio
{
    /// <summary>
    /// Adapts NAudio's <see cref="WasapiRecorder"/> to <see cref="IAudioCapture"/>.
    /// </summary>
    internal sealed class WasapiAudioCapture : IAudioCapture
    {
        private readonly WasapiRecorder _recorder;
        private byte[] _buffer = Array.Empty<byte>();

        private WasapiAudioCapture(WasapiRecorder recorder)
        {
            _recorder = recorder;
            // WasapiRecorder reports the raw mix format (usually WAVE_FORMAT_EXTENSIBLE); the sample
            // converter expects the plain encoding (e.g. IeeeFloat), as the old WasapiCapture reported it.
            WaveFormat = recorder.WaveFormat.AsStandardWaveFormat();
            _recorder.DataAvailable += OnDataAvailable;
            _recorder.RecordingStopped += (s, e) => RecordingStopped?.Invoke(this, e);
        }

        /// <summary>
        /// Loopback capture of the default render device (multimedia role, the one <see cref="AudioDeviceEventWatcher"/> watches).
        /// </summary>
        public static WasapiAudioCapture Loopback(int bufferMilliseconds)
        {
            var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return new WasapiAudioCapture(CreateBuilder(device, bufferMilliseconds).WithLoopbackCapture().Build());
        }

        /// <summary>
        /// Capture from a specific capture (input) device.
        /// </summary>
        public static WasapiAudioCapture FromDevice(string deviceId, int bufferMilliseconds)
        {
            var device = new MMDeviceEnumerator().GetDevice(deviceId);
            return new WasapiAudioCapture(CreateBuilder(device, bufferMilliseconds).Build());
        }

        private static WasapiRecorderBuilder CreateBuilder(MMDevice device, int bufferMilliseconds)
            => new WasapiRecorderBuilder()
                .WithDevice(device)
                .WithPollingSync()
                .WithBufferLength(bufferMilliseconds);

        private void OnDataAvailable(ReadOnlySpan<byte> data, AudioClientBufferFlags flags, long devicePosition, long qpcPosition)
        {
            // Empty packets would trip the chain's raw silence check
            if (data.IsEmpty)
                return;

            // The span is only valid during the callback, and the chain takes a byte[]
            if (_buffer.Length < data.Length)
                _buffer = new byte[data.Length];
            data.CopyTo(_buffer);
            DataAvailable?.Invoke(this, new WaveInEventArgs(_buffer, data.Length));
        }

        public WaveFormat WaveFormat { get; }

        public CaptureState CaptureState => _recorder.CaptureState;

        public event EventHandler<WaveInEventArgs>? DataAvailable;
        public event EventHandler<StoppedEventArgs>? RecordingStopped;

        public void StartRecording() => _recorder.StartRecording();
        public void StopRecording() => _recorder.StopRecording();
        public void Dispose() => _recorder.Dispose();
    }
}
