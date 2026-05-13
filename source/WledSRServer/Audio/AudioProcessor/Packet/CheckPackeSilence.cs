using System.Diagnostics;

namespace WledSRServer.Audio.AudioProcessor.Packet
{
    internal class CheckPackeSilence : Processor
    {
        private AudioSyncPacket_v2 _packet;
        private double _msDelay;
        private Stopwatch _silenceTime;
        private bool _isSilent;
        private Func<bool> _onSilence;

        public CheckPackeSilence(AudioSyncPacket_v2 packet, double msDelay, Func<bool> onSilence)
        {
            _packet = packet;
            _msDelay = msDelay;
            _onSilence = onSilence;
            _silenceTime = new Stopwatch();
        }

        public CheckPackeSilence(AudioSyncPacket_v2 packet, double msDelay, Action onSilence, bool stopOnSilence = true)
            : this(packet, msDelay, () => { onSilence(); return !stopOnSilence; }) { }

        public override void Init(AudioProcessChain chain)
        {

        }

        public override bool Process()
        {
            if (_packet.FFT_Bins.Any(v => v > 0))
            {
                _isSilent = false;
                _silenceTime.Stop();
                return true;
            }
            
            if (!_isSilent)
            {
                if (!_silenceTime.IsRunning)
                {
                    Debug.WriteLine("Packet silence detected, starting timer...");
                    _silenceTime.Restart();
                    return true;
                }
                if (_silenceTime.ElapsedMilliseconds < _msDelay)
                {
                    return true;
                }

                Debug.WriteLine($"Packet silence detected after {_silenceTime.ElapsedMilliseconds}ms");
                _silenceTime.Stop();
                _isSilent = true;
            }

            return _onSilence.Invoke();
        }
    }
}
