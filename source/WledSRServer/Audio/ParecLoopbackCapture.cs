using System.Diagnostics;
using NAudio.Wave;

namespace WledSRServer.Audio
{
    /// <summary>
    /// Linux loopback capture: shells out to `parec` (PulseAudio client tools, also served
    /// by PipeWire's pipewire-pulse compatibility layer) to read the default sink's monitor
    /// source, or a specific source when one is given.
    /// </summary>
    internal sealed class ParecLoopbackCapture : IAudioCapture
    {
        public WaveFormat WaveFormat { get; }
        public CaptureState CaptureState { get; private set; } = CaptureState.Stopped;

        public event EventHandler<WaveInEventArgs>? DataAvailable;
        public event EventHandler<StoppedEventArgs>? RecordingStopped;

        private readonly string _device;
        private Process? _process;
        private Thread? _readThread;
        private volatile bool _stopRequested;

        /// <param name="device">A pactl source name, or the "@DEFAULT_MONITOR@" alias for
        /// whatever the default sink's monitor currently is.</param>
        public ParecLoopbackCapture(string device = "@DEFAULT_MONITOR@", int sampleRate = 48000, int channels = 2)
        {
            _device = device;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        public void StartRecording()
        {
            if (CaptureState != CaptureState.Stopped)
                throw new InvalidOperationException("Capture already started.");

            CaptureState = CaptureState.Starting;
            _stopRequested = false;

            var psi = new ProcessStartInfo("parec")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("--raw");
            psi.ArgumentList.Add("--format=float32le");
            psi.ArgumentList.Add($"--rate={WaveFormat.SampleRate}");
            psi.ArgumentList.Add($"--channels={WaveFormat.Channels}");
            psi.ArgumentList.Add("--latency-msec=10");
            psi.ArgumentList.Add($"--device={_device}");

            _process = new Process { StartInfo = psi };

            try
            {
                _process.Start();
            }
            catch (Exception ex)
            {
                CaptureState = CaptureState.Stopped;
                throw new InvalidOperationException(
                    "Failed to start 'parec'. Is pulseaudio-utils (or pipewire-pulse) installed?", ex);
            }

            CaptureState = CaptureState.Capturing;
            _readThread = new Thread(ReadLoop) { Name = "parec capture", IsBackground = true };
            _readThread.Start();
        }

        public void StopRecording()
        {
            if (CaptureState == CaptureState.Stopped)
                return;

            _stopRequested = true;
            CaptureState = CaptureState.Stopping;

            try
            {
                if (_process is { HasExited: false })
                    _process.Kill();
            }
            catch
            {
                // best effort; ReadLoop's finally block still runs and settles state
            }

            _readThread?.Join(TimeSpan.FromSeconds(2));
        }

        private void ReadLoop()
        {
            Exception? error = null;
            // 10ms worth of audio, matching --latency-msec above.
            var buffer = new byte[WaveFormat.AverageBytesPerSecond / 100];

            try
            {
                var stdout = _process!.StandardOutput.BaseStream;
                int bytesRead;
                while (!_stopRequested && (bytesRead = stdout.Read(buffer, 0, buffer.Length)) > 0)
                    DataAvailable?.Invoke(this, new WaveInEventArgs(buffer, bytesRead));
            }
            catch (Exception ex) when (!_stopRequested)
            {
                error = ex;
            }
            catch
            {
                // reading from a pipe we just killed via StopRecording() throws; expected
            }
            finally
            {
                try { _process?.WaitForExit(2000); } catch { /* ignore */ }
                CaptureState = CaptureState.Stopped;
                RecordingStopped?.Invoke(this, new StoppedEventArgs(error));
            }
        }

        public void Dispose()
        {
            StopRecording();
            _process?.Dispose();
        }
    }
}
