using NAudio.CoreAudioApi;

namespace WledSRServer.Audio
{

    /// <summary>
    /// WASAPI Loopback Capture
    /// based on a contribution from "Pygmy" - http://naudio.codeplex.com/discussions/203605
    /// </summary>
    public class WasapiLoopbackCaptureEx : WasapiCapture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WasapiCapture" /> class.
        /// </summary>
        /// <param name="captureDevice">The capture device. Null to use default device.</param>
        /// <param name="useEventSync">true if sync is done with event. false use sleep.</param>
        /// <param name="audioBufferMillisecondsLength">Length of the audio buffer in milliseconds. A lower value means lower latency but increased CPU usage.</param>
        public WasapiLoopbackCaptureEx(MMDevice? captureDevice = null, bool useEventSync = false, int audioBufferMillisecondsLength = 100)
            : base(captureDevice ?? GetDefaultLoopbackCaptureDevice(), useEventSync, audioBufferMillisecondsLength)
        {
        }

        /// <summary>
        /// Gets the default audio loopback capture device
        /// </summary>
        /// <returns>The default audio loopback capture device</returns>
        public static MMDevice GetDefaultLoopbackCaptureDevice()
        {
            MMDeviceEnumerator devices = new MMDeviceEnumerator();
            return devices.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        /// <summary>
        /// Specify loopback
        /// </summary>
        protected override AudioClientStreamFlags GetAudioClientStreamFlags()
        {
            return AudioClientStreamFlags.Loopback | base.GetAudioClientStreamFlags();
        }
    }
}

