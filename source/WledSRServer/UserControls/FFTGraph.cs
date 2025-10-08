using System.Data;
using WledSRServer.Audio;
using WledSRServer.Audio.AudioProcessor.FFT;
using WledSRServer.Audio.AudioProcessor.FFTBuckets;

namespace WledSRServer.UserControls
{
    public partial class FFTGraph : UserControl
    {
        public double MinFreq { get; set; } = 10;
        public double MaxFreq { get; set; } = 1000;
        public bool BeatFlash { get; set; } = true;

        public FFTGraph()
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
            var bucket = AudioCaptureManager.ActiveChain?.GetContext<FFTBucketData>();
            var beat = AudioCaptureManager.ActiveChain?.GetContext<BeatData>();
            var bd = AudioCaptureManager.ActiveChain?.GetProcessor<BeatDetector>();

            if (fft == null || beat == null || bd == null) return;
            if (fft.Values.Length == 0) return;

            var dispFreqMin = MinFreq;
            var dispFreqMax = MaxFreq;

            var displayedFFTindexes = fft.GetIndexesByFreq(dispFreqMin, dispFreqMax);
            var firstDisplayedFFTindex = displayedFFTindexes.First();
            var lastDisplayedFFTindex = displayedFFTindexes.Last();
            var oneIndexSize = (float)this.Width / (displayedFFTindexes.Length - 1);
            var halfIndexSize = oneIndexSize / 2;
            var xCoordsByFFTIndex = Enumerable.Range(0, fft.Values.Length).Select(i => oneIndexSize * (i - firstDisplayedFFTindex)).ToArray();

            var fftMaxValue = fft.GetValuesByFreq(dispFreqMin, dispFreqMax).Max();
            var scaleFFTValue = new Func<double, float>(v => this.Height - (fftMaxValue == 0 ? 0 : (float)(v / fftMaxValue * this.Height)));

            #region clear and beat flash

            if (beat.Detected && BeatFlash)
                e.Graphics.Clear(Color.Silver);
            else
                e.Graphics.Clear(Color.DarkGray);

            #endregion

            #region Bucket backgrounds

            if (bucket.Values.Length > 0)
            {
                for (int i = 0; i < bucket.Values.Length; i++)
                {
                    var bucketFFTIndexes = fft.GetIndexesByFreq(bucket.Values[i].FreqLow, bucket.Values[i].FreqHigh);
                    if (bucketFFTIndexes.Length == 0) continue;
                    var firstIndex = bucketFFTIndexes.First();
                    var lastIndex = bucketFFTIndexes.Last();

                    float x1 = xCoordsByFFTIndex[firstIndex] - halfIndexSize;
                    float x2 = xCoordsByFFTIndex[lastIndex] + halfIndexSize;
                    //float yb = this.Height / 255f * (float)bucket.Values[i].Value;
                    float yp = this.Height - (this.Height / 255f * Program.ServerContext.Packet.FFT_Bins[i]);

                    var bucketColor = hsv2rgb(i / 15f * 0.85f, 1f, 1f);
                    var bucketColorBG = new SolidBrush(Color.FromArgb(64, bucketColor));
                    var bucketColorValue = new SolidBrush(Color.FromArgb(64, bucketColor));
                    e.Graphics.FillRectangle(bucketColorBG, x1, 0, x2 - x1, this.Height);
                    e.Graphics.FillRectangle(bucketColorValue, x1, yp, x2 - x1, this.Height - yp);
                    //e.Graphics.DrawLine(new Pen(bucketColor), x1, yp, x2, yp);

                    e.Graphics.DrawRectangle(Pens.Gray, x1, 0, x2 - x1, this.Height);
                }
            }

            #endregion

            var bdIndexes = fft.GetIndexesByFreq(bd._freqLow, bd._freqHigh);

            if (bdIndexes.Length > 0)
            {
                #region Draw Beat detector bars

                for (var i = bdIndexes.First(); i <= bdIndexes.Last(); i++)
                {
                    if (i < firstDisplayedFFTindex || i > lastDisplayedFFTindex) continue;

                    float x1 = xCoordsByFFTIndex[i];
                    float y1 = scaleFFTValue(fft.Values[displayedFFTindexes[i]]);

                    e.Graphics.FillRectangle(Brushes.Green, x1 - halfIndexSize, y1, oneIndexSize, Height - y1);
                    e.Graphics.DrawRectangle(Pens.DarkGreen, x1 - halfIndexSize, y1, oneIndexSize, Height - y1);
                }

                #endregion

                #region Beatdetector history graph

                float bdX1 = xCoordsByFFTIndex[bdIndexes.First()] - halfIndexSize;
                float bdX2 = xCoordsByFFTIndex[bdIndexes.Last()] + halfIndexSize;

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

                #endregion
            }

            #region Draw FFT graph

            for (var i = 0; i < displayedFFTindexes.Length - 1; i++)
            {
                int i0 = displayedFFTindexes[i];
                int i1 = displayedFFTindexes[i + 1];
                float x1 = xCoordsByFFTIndex[i0];
                float x2 = xCoordsByFFTIndex[i1];
                float y1 = scaleFFTValue(fft.Values[i0]);
                float y2 = scaleFFTValue(fft.Values[i1]);

                e.Graphics.DrawLine(Pens.Black, x1, y1, x2, y2);
                e.Graphics.FillEllipse(Brushes.Black, x1 - 2, y1 - 2, 4, 4);
                e.Graphics.FillEllipse(Brushes.Black, x2 - 2, y2 - 2, 4, 4);

                //e.Graphics.DrawString(i0.ToString(), this.Font, Brushes.White, x1 + 2, y1 - 12);
            }

            #endregion
        }

        private Color hsv2rgb(float h, float s, float v)
        {
            Func<float, int> f = delegate (float n)
            {
                float k = (n + h * 6) % 6;
                return (int)((v - (v * s * (Math.Max(0, Math.Min(Math.Min(k, 4 - k), 1))))) * 255);
            };
            return Color.FromArgb(f(5), f(3), f(1));
        }
    }
}
