using System.Data;
using WledSRServer.Audio;
using WledSRServer.Audio.AudioProcessor.FFT;

namespace WledSRServer.UserControls
{
    public partial class BeatDetectorGraph : UserControl
    {
        public BeatDetectorGraph()
        {
            InitializeComponent();
            SetupRedrawOnNewPacket();
        }

        private void SetupRedrawOnNewPacket()
        {
            var cancelUpdate = new CancellationTokenSource();

            var packetUpdated = new AudioCaptureManager.PacketUpdatedHandler(Invalidate);
            AudioCaptureManager.PacketUpdated += packetUpdated;

            Disposed += (s, e) =>
            {
                AudioCaptureManager.PacketUpdated -= packetUpdated;
                cancelUpdate.Cancel();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
                return;

            var fft = AudioCaptureManager.ActiveChain?.GetContext<FFTData>();
            var beat = AudioCaptureManager.ActiveChain?.GetContext<BeatData>();
            var bd = AudioCaptureManager.ActiveChain?.GetProcessor<BeatDetector>();

            if (fft == null || beat == null || bd == null) return;

            var dispFreqMin = 10;
            var dispFreqMax = 1000;

            var indexes = fft.GetIndexesByFreq(dispFreqMin, dispFreqMax);

            var fftMaxValue = fft.Values.Max();
            var scaleFFTValue = new Func<double, float>(v => this.Height - (float)(v / fftMaxValue * this.Height));

            //e.Graphics.Clear(Color.FromKnownColor(KnownColor.Control));
            if (beat.Detected)
                e.Graphics.Clear(Color.Silver);
            else
                e.Graphics.Clear(Color.DarkGray);

            float bdX1 = -1;
            float bdX2 = -1;
            for (var i = 0; i < indexes.Length - 1; i++)
            {
                float x1 = this.Width * i / indexes.Length;
                float x2 = this.Width * (i + 1) / indexes.Length;
                float y1 = scaleFFTValue(fft.Values[indexes[i]]);
                float y2 = scaleFFTValue(fft.Values[indexes[i + 1]]);

                if (fft.Frequencies[i] >= bd._freqLow && fft.Frequencies[i] <= bd._freqHigh)
                {
                    if (bdX1 == -1) bdX1 = x1;
                    e.Graphics.FillRectangle(Brushes.Green, x1, y1, x2 - x1, Height - y1);
                }
                else
                {
                    if ((bdX1 != -1) && (bdX2 == -1)) bdX2 = x1;
                    e.Graphics.DrawLine(Pens.Gray, x1, y1, x2, y2);
                }
            }

            if (bd._history.Count > 1)
            {
                var avg = bd._history.Where((h, i) => i < bd._history.Count - 1).Average();
                var yAvg = scaleFFTValue(avg);
                e.Graphics.DrawLine(Pens.Blue, bdX1, yAvg, bdX2, yAvg);

                var yDet = scaleFFTValue(avg * BeatDetector._tresholdMultiplier);
                e.Graphics.DrawLine(Pens.LightGreen, bdX1, yDet, bdX2, yDet);
            }

            var yCurr = scaleFFTValue(bd._history.Last());
            e.Graphics.DrawLine(Pens.Yellow, bdX1, yCurr, bdX2, yCurr);

        }

    }
}
