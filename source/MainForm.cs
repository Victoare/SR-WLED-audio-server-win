using System.Diagnostics;
using System.Net;

namespace WledSRServer
{
    public partial class MainForm : Form
    {
        private Stopwatch PPSwatch = new Stopwatch();

        public MainForm()
        {
            InitializeComponent();

            if (!DesignMode)
                this.Icon = Properties.Resources.NotifIcon;

            // Console.WriteLine("===[ packet preview ]======================================================");
            // Console.WriteLine($"sampleRaw  : {packet.sampleRaw,-20:F45}");
            // Console.WriteLine($"sampleSmth : {packet.sampleSmth,-20:F45}");
            // Console.WriteLine($"samplePeak : {packet.samplePeak,-20}");

            // Console.WriteLine($"FFT_Magnitude : {packet.FFT_Magnitude,-20:F32}");
            // Console.WriteLine($"FFT_MajorPeak : {packet.FFT_MajorPeak,10:F4} (hz)                  ");

            // V=FreqToDisplay, V0=FreqLow, V1=FreqHigh, X0-X1 control width
            // X = X0 + (X1 - X0)(log(V) - log(V0))/(log(V1) - log(V0))

            btnSetAutoRun.CheckboxChecked = AdminFunctions.GetAutoRun();
            btnSetStartupGUI.CheckboxChecked = Properties.Settings.Default.StartWithoutGUI;
            txtUdpPort.Text = Properties.Settings.Default.WledUdpMulticastPort.ToString();

            ddlAudioDevices.DataSource = AudioCapture.GetDevices();
            ddlAudioDevices.DisplayMember = nameof(AudioCapture.SimpleDeviceDescriptor.Name);
            ddlAudioDevices.ValueMember = nameof(AudioCapture.SimpleDeviceDescriptor.ID);
            ddlAudioDevices.SelectedValue = Properties.Settings.Default.AudioCaptureDeviceId;
            ddlAudioDevices.SelectedIndexChanged += ddlAudioDevices_Changed;

            txtUdpPort.AutoCompleteCustomSource.Add("11988"); // the default one
            txtUdpPort.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtUdpPort.AutoCompleteSource = AutoCompleteSource.CustomSource;

            txtLocalIpAddress.AutoCompleteCustomSource.AddRange(Network.GetLocalIPAddresses());
            txtLocalIpAddress.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtLocalIpAddress.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtLocalIpAddress.Text = Properties.Settings.Default.LocalIPToBind;
            txtLocalIpAddress.TextChanged += txtLocalIpAddress_Changed;

            txtFFTLower.Text = Properties.Settings.Default.FFTLow.ToString();
            txtFFTLower.TextChanged += txtFFTLower_TextChanged;
            txtFFTUpper.Text = Properties.Settings.Default.FFTHigh.ToString();
            txtFFTUpper.TextChanged += txtFFTUpper_TextChanged;

            var tmr = new System.Windows.Forms.Timer();
            tmr.Tick += UpdateStats;
            tmr.Interval = 50;
            tmr.Start();

            toolTip1.InitialDelay = 100;

            PPSwatch.Start();
        }

        private void UpdateStats(object? sender, EventArgs e)
        {
            if (PPSwatch.ElapsedMilliseconds > 500)
            {
                var pps = 0;
                if (Program.ServerContext.PacketCounter > 0)
                {
                    pps = (int)(Program.ServerContext.PacketCounter * 1000 / PPSwatch.ElapsedMilliseconds);
                    Program.ServerContext.PacketCounter = 0;
                    PPSwatch.Restart();
                }
                lblPPS.Text = $"Packet per second : {pps:D}";
            }
            if (Program.ServerContext.PacketSendError)
            {
                lblPPS.ForeColor = Color.Red;
                toolTip1.SetToolTip(lblPPS, "There is some problem sending out the packages.");
            }
            else
            {
                lblPPS.ForeColor = Program.ServerContext.PacketSendError ? Color.Red : Color.FromKnownColor(KnownColor.ControlText);
                toolTip1.SetToolTip(lblPPS, null);
            }

            lblCapturing.BackColor = AudioCapture.Capturing ? Color.LightGreen : Color.FromKnownColor(KnownColor.Control);
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            grpSettings.Visible = !grpSettings.Visible;
        }

        private void btnExitApplication_Click(object sender, EventArgs e)
        {
            Program.GuiContext.ExitApp();
        }

        private void btnSetAutoRun_Click(object sender, EventArgs e)
        {
            var newState = !AdminFunctions.GetAutoRun();
            if (!AdminFunctions.SetAutoRun(newState))
                MessageBox.Show("Cannot set the AutoStart value", Program.MboxTitle);
            btnSetAutoRun.CheckboxChecked = AdminFunctions.GetAutoRun();
        }

        private void btnSetStartupGUI_Click(object sender, EventArgs e)
        {
            var newValue = !Properties.Settings.Default.StartWithoutGUI;
            Properties.Settings.Default.StartWithoutGUI = newValue;
            Properties.Settings.Default.Save();
            btnSetStartupGUI.CheckboxChecked = newValue;
        }

        private void txtUdpPort_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtUdpPort.Text, out var udpport) || udpport < 0 || udpport > 65535)
            {
                txtUdpPort.BackColor = Color.Salmon;
                toolTip1.SetToolTip(txtUdpPort, "Not a valid port number");
                return;
            }
            toolTip1.SetToolTip(txtUdpPort, null);
            txtUdpPort.BackColor = Color.White;

            Properties.Settings.Default.WledUdpMulticastPort = udpport;
            Properties.Settings.Default.Save();
            Network.Start();
        }

        private void txtLocalIpAddress_Changed(object? sender, EventArgs e)
        {
            var newIpAddress = txtLocalIpAddress.Text;
            if (!string.IsNullOrEmpty(newIpAddress) && (!IPAddress.TryParse(newIpAddress, out var newAddress) || !Network.TestLocalIP(newAddress)))
            {
                txtLocalIpAddress.BackColor = Color.Salmon;
                toolTip1.SetToolTip(txtLocalIpAddress, "Not valid IP address or there is a problem using it.");
                return;
            }
            toolTip1.SetToolTip(txtLocalIpAddress, null);
            txtLocalIpAddress.BackColor = Color.White;

            Properties.Settings.Default.LocalIPToBind = newIpAddress;
            Properties.Settings.Default.Save();
            Network.Start();
        }

        private void ddlAudioDevices_Changed(object sender, EventArgs e)
        {
            var selected = ddlAudioDevices.SelectedItem as AudioCapture.SimpleDeviceDescriptor;
            if (selected == null) return;
            Properties.Settings.Default.AudioCaptureDeviceId = selected.ID;
            if (AudioCapture.Start())
                Properties.Settings.Default.Save();
        }

        private void txtFFTLower_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtFFTLower.Text, out var newValue) || newValue < 1 || newValue >= Properties.Settings.Default.FFTHigh)
            {
                txtFFTLower.BackColor = Color.Salmon;
                toolTip1.SetToolTip(txtFFTLower, "Needs to be a number between 1 and the higher end of the range");
                return;
            }
            toolTip1.SetToolTip(txtFFTLower, null);
            txtFFTLower.BackColor = Color.White;

            Properties.Settings.Default.FFTLow = newValue;
            Properties.Settings.Default.Save();
            AudioCapture.Start();
        }

        private void txtFFTUpper_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(txtFFTUpper.Text, out var newValue) || newValue > 99999 || newValue <= Properties.Settings.Default.FFTLow)
            {
                txtFFTUpper.BackColor = Color.Salmon;
                toolTip1.SetToolTip(txtFFTUpper, "Needs to be a number between the lower end of the range and 99999");
                return;
            }
            toolTip1.SetToolTip(txtFFTUpper, null);
            txtFFTUpper.BackColor = Color.White;

            Properties.Settings.Default.FFTHigh = newValue;
            Properties.Settings.Default.Save();
            AudioCapture.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Program.GuiContext.FormClosed(e.CloseReason);
        }
    }
}
