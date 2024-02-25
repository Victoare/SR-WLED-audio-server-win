namespace WledSRServer
{
    public partial class FFTDisplay : UserControl
    {
        public FFTDisplay()
        {
            InitializeComponent();

            SetupRedrawOnNewPacket();

            RecalculateRectangles();
            this.Resize += (s, e) => RecalculateRectangles();

            this.MouseMove += FFTDisplay_MouseMove;
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

        private void FFTDisplay_MouseMove(object? sender, MouseEventArgs e)
        {
            var mouseX = e.Location.X;
            var undexRextIdx = _rectanglesFull.Select((r, idx) => new { x0 = r.X, x1 = r.X + r.Width, idx }).FirstOrDefault(r => r.x0 <= mouseX && r.x1 >= mouseX)?.idx;
            toolTip1.SetToolTip(this, (undexRextIdx == null || AudioCaptureManager.FFTfreqBands == null) ? null : AudioCaptureManager.FFTfreqBands[undexRextIdx.Value]);
        }

        private const int PADDING = 4;
        private RectangleF[] _rectanglesFull;
        private RectangleF[] _rectanglesBar;
        private Brush[] _barColor;
        private Brush _barBG;
        private Pen _barBorder;

        private void RecalculateRectangles()
        {
            var barCount = Program.ServerContext.Packet.FFT_Bins.Length;
            _rectanglesFull = new RectangleF[barCount];
            _rectanglesBar = new RectangleF[barCount];
            var width = (float)(this.Width - 1 + PADDING) / barCount - PADDING;
            var fullHeight = (float)this.Height - 1;

            for (int i = 0; i < barCount; i++)
            {
                _rectanglesFull[i] = new RectangleF(i * (width + PADDING), 0, width, fullHeight);
                _rectanglesBar[i] = new RectangleF(i * (width + PADDING), 0, width, 0);
            }

            _barColor = new Brush[barCount];
            for (int i = 0; i < barCount; i++)
                _barColor[i] = new SolidBrush(hsv2rgb(i / 15f * 0.85f, 1f, 1f));

            _barBG = new SolidBrush(Color.FromKnownColor(KnownColor.Control));
            _barBorder = new Pen(Color.Silver);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var fftBytes = Program.ServerContext.Packet.FFT_Bins;

            if (DesignMode)
                new Random().NextBytes(fftBytes);

            var fullHeight = _rectanglesFull[0].Height;
            for (int i = 0; i < fftBytes.Length; i++)
            {
                var barHeight = fullHeight * fftBytes[i] / 255;
                _rectanglesBar[i].Y = fullHeight - barHeight;
                _rectanglesBar[i].Height = barHeight;
            }

            e.Graphics.FillRectangles(_barBG, _rectanglesFull);     // bg

            for (int i = 0; i < fftBytes.Length; i++)
                e.Graphics.FillRectangle(_barColor[i], _rectanglesBar[i]); // bar

            e.Graphics.DrawRectangles(_barBorder, _rectanglesFull);   // border
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
