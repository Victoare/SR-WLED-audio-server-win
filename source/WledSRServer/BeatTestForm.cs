using WledSRServer.Audio;
using WledSRServer.Audio.AudioProcessor.FFT;
using WledSRServer.Properties;
using WledSRServer.UserControls;

namespace WledSRServer
{
    public partial class BeatTestForm : Form
    {
        public BeatTestForm()
        {
            InitializeComponent();
            rbDisplayRange_Low100.CheckedChanged += (s, e) => SetRange();
            rbDisplayRange_Beat.CheckedChanged += (s, e) => SetRange();
            rbDisplayRange_Settings.CheckedChanged += (s, e) => SetRange();
            rbDisplayRange_Full.CheckedChanged += (s, e) => SetRange();
            SetRange();

            cbShowBeat.CheckedChanged += (s, e) => ShowBeat();
            ShowBeat();
        }

        private void ShowBeat()
        {
            fftGraph1.BeatFlash = cbShowBeat.Checked;
        }

        private void SetRange()
        {
            if (rbDisplayRange_Low100.Checked)
            {
                fftGraph1.MinFreq = 0;
                fftGraph1.MaxFreq = 100;
            }
            if (rbDisplayRange_Beat.Checked)
            {
                fftGraph1.MinFreq = 10;
                fftGraph1.MaxFreq = 1000;
            }
            if (rbDisplayRange_Settings.Checked)
            {
                fftGraph1.MinFreq = Settings.Default.FFTLow;
                fftGraph1.MaxFreq = Settings.Default.FFTHigh;
            }
            if (rbDisplayRange_Full.Checked)
            {
                var fft = AudioCaptureManager.ActiveChain?.GetContext<FFTData>();
                fftGraph1.MinFreq = fft?.Frequencies.FirstOrDefault(10) ?? 10;
                fftGraph1.MaxFreq = fft?.Frequencies.LastOrDefault(10000) ?? 10000;
            }

            lblFreqMin.Text = fftGraph1.MinFreq.ToString("# ##0.##") + " Hz";
            lblFreqMax.Text = fftGraph1.MaxFreq.ToString("# ##0.##") + " Hz";
        }
    }
}
