using System.Net;

namespace WledSRServer
{
    internal class GuiContext : ApplicationContext
    {
        private MainForm? _mainForm;
        private NotifyIcon? _notifyIcon;
        private bool _showIconInfo = true;
        private System.ComponentModel.IContainer? _components;

        public void Init()
        {
            _components = new System.ComponentModel.Container();

            var menuStrip = new ContextMenuStrip(_components);
            var smf = menuStrip.Items.Add("Show form", null, new EventHandler((s, o) => ShowMainForm()));
            smf.Font = new Font(smf.Font, FontStyle.Bold);
            menuStrip.Items.Add("Exit", null, new EventHandler((s, o) => ExitApp()));

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "WLED SR Server";
            _notifyIcon.Icon = Properties.Resources.NotifIcon;
            _notifyIcon.ContextMenuStrip = menuStrip;
            _notifyIcon.DoubleClick += (s, o) => ShowMainForm();
            _notifyIcon.Visible = true;

            if (!IsLocalIPSettingCorrect())
                ShowMainForm(showSettings: true);

            if (!Properties.Settings.Default.StartWithoutGUI)
                ShowMainForm();
            else
                _showIconInfo = false;
        }

        private bool IsLocalIPSettingCorrect()
        {
            var localIpSetting = Properties.Settings.Default.LocalIPToBind;
            if (string.IsNullOrEmpty(localIpSetting)) return true;
            if (IPAddress.TryParse(localIpSetting, out var localIp) && NetworkManager.TestLocalIP(localIp, out var _)) return true;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }

            base.Dispose(disposing);
        }

        public void ExitApp()
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = false;
            Application.Exit();
        }

        private void ShowMainForm(bool showSettings = false)
        {
            if (_mainForm?.Visible == true)
            {
                _mainForm.Activate();
                _mainForm.WindowState = FormWindowState.Normal;
            }
            else
            {
                _mainForm ??= new MainForm();
                _mainForm.Show();
            }
            if (showSettings)
                _mainForm.ShowSettings();
        }

        internal void FormClosed(CloseReason closeReason)
        {
            if (closeReason == CloseReason.UserClosing && _showIconInfo)
            {
                _notifyIcon.ShowBalloonTip(10000, Program.MboxTitle, "The server keeps running in the backgorund. You can find it in the system tray.", ToolTipIcon.Info);
                _showIconInfo = false;
            }

            if (_mainForm != null)
            {
                _mainForm.Dispose();
                _mainForm = null;
            }
        }
    }
}
