using NAudio.MediaFoundation;
using System.Windows.Forms;

namespace WledSRServer
{
    public partial class FFTDisplay : UserControl
    {
        public FFTDisplay()
        {
            InitializeComponent();

            timer1.Interval = 20;
            timer1.Tick += (s, o) => { if (!DesignMode) { Invalidate(); } };
            //timer1.Enabled = !DesignMode;
            //if (!DesignMode)
            timer1.Start();

            RecalculateRectangles();
            this.Resize += (s, e) => RecalculateRectangles();

            this.MouseMove += FFTDisplay_MouseMove;
        }

        private void FFTDisplay_MouseMove(object? sender, MouseEventArgs e)
        {
            var mouseX = e.Location.X;
            var undexRextIdx = _rectanglesFull.Select((r, idx) => new { x0 = r.X, x1 = r.X + r.Width, idx }).FirstOrDefault(r => r.x0 <= mouseX && r.x1 >= mouseX)?.idx;
            toolTip1.SetToolTip(this, (undexRextIdx == null || AudioCapture.FFTfreqBands == null) ? null : AudioCapture.FFTfreqBands[undexRextIdx.Value]);
        }

        private void ToolTip1_Draw(object? sender, DrawToolTipEventArgs e)
        {
            throw new NotImplementedException();
        }

        private const int PADDING = 4;
        private RectangleF[] _rectanglesFull;
        private RectangleF[] _rectanglesBar;
        private Brush[] _barColor;
        private Brush _barBG;
        private Pen _barBorder;

        private void RecalculateRectangles()
        {
            var fftBytes = Program.ServerContext.Packet.fftResult;
            _rectanglesFull = new RectangleF[fftBytes.Length];
            _rectanglesBar = new RectangleF[fftBytes.Length];
            var width = (float)(this.Width - 1 + PADDING) / fftBytes.Length - PADDING;
            var fullHeight = (float)this.Height - 1;

            for (int i = 0; i < fftBytes.Length; i++)
            {
                _rectanglesFull[i] = new RectangleF(i * (width + PADDING), 0, width, fullHeight);
                var barHeight = fullHeight * fftBytes[i] / 255;
                _rectanglesBar[i] = new RectangleF(i * (width + PADDING), fullHeight - barHeight, width, barHeight);
            }

            _barColor = new Brush[fftBytes.Length];
            for (int i = 0; i < fftBytes.Length; i++)
                _barColor[i] = new SolidBrush(hsv2rgb(i / 15f * 0.85f, 1f, 1f));

            _barBG = new SolidBrush(Color.FromKnownColor(KnownColor.Control));
            _barBorder = new Pen(Color.Silver);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var fftBytes = Program.ServerContext.Packet.fftResult;

            if (DesignMode)
                new Random().NextBytes(fftBytes);

            var fullHeight = (float)this.Height - 1;
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
