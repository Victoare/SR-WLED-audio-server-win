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
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var fftBytes = Program.ServerContext.Packet.fftResult;

            if (DesignMode)
                new Random().NextBytes(fftBytes);

            var rectanglesFull = new RectangleF[fftBytes.Length];
            var rectanglesBar = new RectangleF[fftBytes.Length];
            var padding = 4;
            var width = (float)(this.Width - 1 + padding) / fftBytes.Length - padding;
            var fullHeight = (float)this.Height - 1;

            for (int i = 0; i < fftBytes.Length; i++)
            {
                rectanglesFull[i] = new RectangleF(i * (width + padding), 0, width, fullHeight);
                var barHeight = fullHeight * fftBytes[i] / 255;
                rectanglesBar[i] = new RectangleF(i * (width + padding), fullHeight - barHeight, width, barHeight);
            }

            var bgBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));
            var borderPen = new Pen(Color.Silver);
            e.Graphics.FillRectangles(bgBrush, rectanglesFull);     // bg
            //e.Graphics.FillRectangles(Brushes.Blue, rectanglesBar); // bar

            for (int i = 0; i < fftBytes.Length; i++)
            {
                var barColor = new SolidBrush(hsv2rgb(i / 15f * 0.85f, 1f, 1f));
                e.Graphics.FillRectangle(barColor, rectanglesBar[i]); // bar
            }

            e.Graphics.DrawRectangles(borderPen, rectanglesFull);   // border
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
