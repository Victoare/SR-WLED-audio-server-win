using NAudio.Wave;

namespace WledSRServer.Audio
{
    // Mirrors NAudio.CoreAudioApi.CaptureState's states, but that type lives in NAudio's
    // Windows-only WASAPI assembly, so platform-agnostic code (AudioCaptureManager) needs
    // its own copy to depend on.
    internal enum CaptureState
    {
        Stopped,
        Starting,
        Capturing,
        Stopping,
    }

    /// <summary>
    /// Common surface for audio capture backends, so AudioCaptureManager can drive either
    /// the Windows WASAPI implementation (<see cref="WasapiAudioCapture"/>) or the Linux
    /// PulseAudio/PipeWire one (<see cref="ParecLoopbackCapture"/>) without knowing which.
    /// </summary>
    internal interface IAudioCapture : IDisposable
    {
        WaveFormat WaveFormat { get; }
        CaptureState CaptureState { get; }

        event EventHandler<WaveInEventArgs>? DataAvailable;
        event EventHandler<StoppedEventArgs>? RecordingStopped;

        void StartRecording();
        void StopRecording();
    }
}
