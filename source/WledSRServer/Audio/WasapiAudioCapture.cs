using NAudio.Wave;
using NAudioCoreAudio = NAudio.CoreAudioApi;

namespace WledSRServer.Audio
{
    /// <summary>
    /// Adapts NAudio's WASAPI capture (<see cref="NAudioCoreAudio.WasapiCapture"/>, and by
    /// extension <see cref="WasapiLoopbackCaptureEx"/>) to <see cref="IAudioCapture"/>.
    /// </summary>
    internal sealed class WasapiAudioCapture : IAudioCapture
    {
        private readonly NAudioCoreAudio.WasapiCapture _capture;

        public WasapiAudioCapture(NAudioCoreAudio.WasapiCapture capture)
        {
            _capture = capture;
            _capture.DataAvailable += (s, e) => DataAvailable?.Invoke(this, e);
            _capture.RecordingStopped += (s, e) => RecordingStopped?.Invoke(this, e);
        }

        public WaveFormat WaveFormat => _capture.WaveFormat;

        public CaptureState CaptureState => _capture.CaptureState switch
        {
            NAudioCoreAudio.CaptureState.Starting => CaptureState.Starting,
            NAudioCoreAudio.CaptureState.Capturing => CaptureState.Capturing,
            NAudioCoreAudio.CaptureState.Stopping => CaptureState.Stopping,
            _ => CaptureState.Stopped,
        };

        public event EventHandler<WaveInEventArgs>? DataAvailable;
        public event EventHandler<StoppedEventArgs>? RecordingStopped;

        public void StartRecording() => _capture.StartRecording();
        public void StopRecording() => _capture.StopRecording();
        public void Dispose() => _capture.Dispose();
    }
}
