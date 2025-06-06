using WledSRServer.UserControls;

namespace WledSRServer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btnExitApplication = new Button();
            grpBottomPanel = new GroupBox();
            label9 = new Label();
            beatPixel1 = new BeatPixel();
            btnSettings = new Button();
            lblPPS = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            chbFFTLogFreq = new CheckBox();
            label6 = new Label();
            txtFFTUpper = new TextBox();
            label5 = new Label();
            label4 = new Label();
            txtFFTLower = new TextBox();
            btnSetStartupGUI = new ButtonWithCheckbox();
            label2 = new Label();
            btnSetAutoRun = new ButtonWithCheckbox();
            txtUdpPort = new TextBox();
            ddlAudioDevices = new ComboBox();
            fftDisplay2 = new FFTDisplay();
            groupBox2 = new GroupBox();
            lblCapturing = new Label();
            label3 = new Label();
            toolTip1 = new ToolTip(components);
            lblRelevantIP = new Label();
            label10 = new Label();
            label1 = new Label();
            groupBox3 = new GroupBox();
            btnGainSettings = new Button();
            label8 = new Label();
            ddlValueScale = new ComboBox();
            groupbox4 = new GroupBox();
            tmrUpdateStats = new System.Windows.Forms.Timer(components);
            pnlSettings = new Panel();
            tableLayoutPanel2 = new TableLayoutPanel();
            gbAdvancedNetwork = new GroupBox();
            txtRelevantIP = new TextBox();
            cbSendMode = new ComboBox();
            txtLocalIpAddress = new TextBox();
            gbGainControl = new GroupBox();
            chbAutoGainControl = new CheckBox();
            tbGainValue = new TrackBar();
            pnlSettingsMain = new Panel();
            groupBox1 = new GroupBox();
            btnAdvancedNetwork = new Button();
            grpBottomPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupbox4.SuspendLayout();
            pnlSettings.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            gbAdvancedNetwork.SuspendLayout();
            gbGainControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tbGainValue).BeginInit();
            pnlSettingsMain.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // btnExitApplication
            // 
            btnExitApplication.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExitApplication.Location = new Point(514, 13);
            btnExitApplication.Name = "btnExitApplication";
            btnExitApplication.Size = new Size(124, 24);
            btnExitApplication.TabIndex = 1;
            btnExitApplication.Text = "Stop server and exit";
            btnExitApplication.UseVisualStyleBackColor = true;
            btnExitApplication.Click += btnExitApplication_Click;
            // 
            // grpBottomPanel
            // 
            grpBottomPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpBottomPanel.Controls.Add(label9);
            grpBottomPanel.Controls.Add(beatPixel1);
            grpBottomPanel.Controls.Add(btnSettings);
            grpBottomPanel.Controls.Add(lblPPS);
            grpBottomPanel.Controls.Add(btnExitApplication);
            grpBottomPanel.Location = new Point(4, 203);
            grpBottomPanel.Name = "grpBottomPanel";
            grpBottomPanel.Size = new Size(643, 42);
            grpBottomPanel.TabIndex = 5;
            grpBottomPanel.TabStop = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BackColor = Color.Transparent;
            label9.Location = new Point(155, 17);
            label9.Name = "label9";
            label9.Size = new Size(30, 15);
            label9.TabIndex = 6;
            label9.Text = "Beat";
            // 
            // beatPixel1
            // 
            beatPixel1.Location = new Point(186, 15);
            beatPixel1.Name = "beatPixel1";
            beatPixel1.Size = new Size(18, 19);
            beatPixel1.TabIndex = 5;
            // 
            // btnSettings
            // 
            btnSettings.Location = new Point(5, 13);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(75, 24);
            btnSettings.TabIndex = 0;
            btnSettings.Text = "⚙ Settings ";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += btnSettings_Click;
            // 
            // lblPPS
            // 
            lblPPS.AutoSize = true;
            lblPPS.Location = new Point(260, 17);
            lblPPS.Name = "lblPPS";
            lblPPS.Size = new Size(122, 15);
            lblPPS.TabIndex = 4;
            lblPPS.Text = "Packet per second : --";
            lblPPS.TextAlign = ContentAlignment.MiddleRight;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 6;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(chbFFTLogFreq, 5, 0);
            tableLayoutPanel1.Controls.Add(label6, 4, 0);
            tableLayoutPanel1.Controls.Add(txtFFTUpper, 3, 0);
            tableLayoutPanel1.Controls.Add(label5, 2, 0);
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(txtFFTLower, 1, 0);
            tableLayoutPanel1.Location = new Point(6, 15);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(320, 29);
            tableLayoutPanel1.TabIndex = 16;
            // 
            // chbFFTLogFreq
            // 
            chbFFTLogFreq.AutoSize = true;
            chbFFTLogFreq.Dock = DockStyle.Fill;
            chbFFTLogFreq.Location = new Point(243, 3);
            chbFFTLogFreq.Name = "chbFFTLogFreq";
            chbFFTLogFreq.Size = new Size(74, 23);
            chbFFTLogFreq.TabIndex = 19;
            chbFFTLogFreq.Text = "LogScale";
            toolTip1.SetToolTip(chbFFTLogFreq, "Frequency distribution on logarithmic scale (instead of linear)");
            chbFFTLogFreq.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Fill;
            label6.Location = new Point(216, 0);
            label6.Name = "label6";
            label6.Size = new Size(21, 29);
            label6.TabIndex = 16;
            label6.Text = "Hz";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtFFTUpper
            // 
            txtFFTUpper.Dock = DockStyle.Fill;
            txtFFTUpper.Location = new Point(169, 3);
            txtFFTUpper.MaxLength = 5;
            txtFFTUpper.Name = "txtFFTUpper";
            txtFFTUpper.Size = new Size(41, 23);
            txtFFTUpper.TabIndex = 1;
            txtFFTUpper.Text = "24000";
            txtFFTUpper.TextAlign = HorizontalAlignment.Center;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Fill;
            label5.Location = new Point(151, 0);
            label5.Name = "label5";
            label5.Size = new Size(12, 29);
            label5.TabIndex = 15;
            label5.Text = "-";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(3, 0);
            label4.Name = "label4";
            label4.Size = new Size(95, 29);
            label4.TabIndex = 0;
            label4.Text = "Frequency range";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(label4, "Low and high end of the analyzed FFT spectrum");
            // 
            // txtFFTLower
            // 
            txtFFTLower.Dock = DockStyle.Fill;
            txtFFTLower.Location = new Point(104, 3);
            txtFFTLower.MaxLength = 5;
            txtFFTLower.Name = "txtFFTLower";
            txtFFTLower.Size = new Size(41, 23);
            txtFFTLower.TabIndex = 0;
            txtFFTLower.Text = "20";
            txtFFTLower.TextAlign = HorizontalAlignment.Center;
            // 
            // btnSetStartupGUI
            // 
            btnSetStartupGUI.CheckboxChecked = false;
            btnSetStartupGUI.Location = new Point(7, 49);
            btnSetStartupGUI.Name = "btnSetStartupGUI";
            btnSetStartupGUI.Size = new Size(137, 24);
            btnSetStartupGUI.TabIndex = 1;
            btnSetStartupGUI.Text = "Start without GUI";
            btnSetStartupGUI.TextAlign = ContentAlignment.MiddleLeft;
            btnSetStartupGUI.UseVisualStyleBackColor = true;
            btnSetStartupGUI.Click += btnSetStartupGUI_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(31, 22);
            label2.Name = "label2";
            label2.Size = new Size(45, 15);
            label2.TabIndex = 10;
            label2.Text = "SR Port";
            toolTip1.SetToolTip(label2, "The SR UDP port in the WLED settings page");
            // 
            // btnSetAutoRun
            // 
            btnSetAutoRun.CheckboxChecked = false;
            btnSetAutoRun.Location = new Point(7, 18);
            btnSetAutoRun.Name = "btnSetAutoRun";
            btnSetAutoRun.Size = new Size(137, 24);
            btnSetAutoRun.TabIndex = 0;
            btnSetAutoRun.Text = "Start with windows";
            btnSetAutoRun.TextAlign = ContentAlignment.MiddleLeft;
            btnSetAutoRun.UseVisualStyleBackColor = true;
            btnSetAutoRun.Click += btnSetAutoRun_Click;
            // 
            // txtUdpPort
            // 
            txtUdpPort.Location = new Point(79, 18);
            txtUdpPort.MaxLength = 5;
            txtUdpPort.Name = "txtUdpPort";
            txtUdpPort.Size = new Size(40, 23);
            txtUdpPort.TabIndex = 2;
            txtUdpPort.Text = "65535";
            // 
            // ddlAudioDevices
            // 
            ddlAudioDevices.BackColor = SystemColors.Window;
            ddlAudioDevices.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlAudioDevices.FormattingEnabled = true;
            ddlAudioDevices.Location = new Point(85, 13);
            ddlAudioDevices.Name = "ddlAudioDevices";
            ddlAudioDevices.Size = new Size(482, 23);
            ddlAudioDevices.TabIndex = 11;
            // 
            // fftDisplay2
            // 
            fftDisplay2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            fftDisplay2.Location = new Point(4, 38);
            fftDisplay2.Name = "fftDisplay2";
            fftDisplay2.Size = new Size(643, 170);
            fftDisplay2.TabIndex = 6;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(lblCapturing);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(ddlAudioDevices);
            groupBox2.Location = new Point(4, -5);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(643, 41);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            // 
            // lblCapturing
            // 
            lblCapturing.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCapturing.BorderStyle = BorderStyle.FixedSingle;
            lblCapturing.Location = new Point(571, 13);
            lblCapturing.Name = "lblCapturing";
            lblCapturing.Size = new Size(67, 23);
            lblCapturing.TabIndex = 13;
            lblCapturing.Text = "Capturing";
            lblCapturing.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(7, 16);
            label3.Name = "label3";
            label3.Size = new Size(72, 15);
            label3.TabIndex = 12;
            label3.Text = "Input device";
            // 
            // lblRelevantIP
            // 
            lblRelevantIP.AutoSize = true;
            lblRelevantIP.Location = new Point(10, 49);
            lblRelevantIP.Name = "lblRelevantIP";
            lblRelevantIP.Size = new Size(71, 15);
            lblRelevantIP.TabIndex = 24;
            lblRelevantIP.Text = "Target IP list";
            toolTip1.SetToolTip(lblRelevantIP, "Relevant IP settings.");
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(11, 21);
            label10.Name = "label10";
            label10.Size = new Size(67, 15);
            label10.TabIndex = 19;
            label10.Text = "Send mode";
            toolTip1.SetToolTip(label10, "Package sending mode");
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(446, 21);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.TabIndex = 10;
            label1.Text = "Local (server) IP";
            toolTip1.SetToolTip(label1, "IP address of the local machine (if needed)");
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnGainSettings);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(ddlValueScale);
            groupBox3.Controls.Add(tableLayoutPanel1);
            groupBox3.Location = new Point(158, -8);
            groupBox3.Margin = new Padding(0);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(330, 81);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            // 
            // btnGainSettings
            // 
            btnGainSettings.Location = new Point(265, 49);
            btnGainSettings.Name = "btnGainSettings";
            btnGainSettings.Size = new Size(59, 24);
            btnGainSettings.TabIndex = 20;
            btnGainSettings.Text = "🎚️ Gain";
            btnGainSettings.UseVisualStyleBackColor = true;
            btnGainSettings.Click += btnGainSettings_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(28, 52);
            label8.Name = "label8";
            label8.Size = new Size(75, 15);
            label8.TabIndex = 19;
            label8.Text = "Value scaling";
            // 
            // ddlValueScale
            // 
            ddlValueScale.DropDownStyle = ComboBoxStyle.DropDownList;
            ddlValueScale.FormattingEnabled = true;
            ddlValueScale.Location = new Point(108, 50);
            ddlValueScale.Name = "ddlValueScale";
            ddlValueScale.Size = new Size(151, 23);
            ddlValueScale.TabIndex = 18;
            // 
            // groupbox4
            // 
            groupbox4.Controls.Add(btnSetStartupGUI);
            groupbox4.Controls.Add(btnSetAutoRun);
            groupbox4.Location = new Point(0, -8);
            groupbox4.Margin = new Padding(0);
            groupbox4.Name = "groupbox4";
            groupbox4.Size = new Size(154, 81);
            groupbox4.TabIndex = 0;
            groupbox4.TabStop = false;
            // 
            // tmrUpdateStats
            // 
            tmrUpdateStats.Tick += tmrUpdateStats_Tick;
            // 
            // pnlSettings
            // 
            pnlSettings.Controls.Add(tableLayoutPanel2);
            pnlSettings.Controls.Add(pnlSettingsMain);
            pnlSettings.Location = new Point(4, 248);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.Size = new Size(645, 274);
            pnlSettings.TabIndex = 8;
            pnlSettings.Visible = false;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.AutoSize = true;
            tableLayoutPanel2.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(gbAdvancedNetwork, 0, 0);
            tableLayoutPanel2.Controls.Add(gbGainControl, 0, 1);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 73);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(645, 201);
            tableLayoutPanel2.TabIndex = 4;
            // 
            // gbAdvancedNetwork
            // 
            gbAdvancedNetwork.Controls.Add(txtRelevantIP);
            gbAdvancedNetwork.Controls.Add(lblRelevantIP);
            gbAdvancedNetwork.Controls.Add(cbSendMode);
            gbAdvancedNetwork.Controls.Add(label10);
            gbAdvancedNetwork.Controls.Add(txtLocalIpAddress);
            gbAdvancedNetwork.Controls.Add(label1);
            gbAdvancedNetwork.Dock = DockStyle.Top;
            gbAdvancedNetwork.Location = new Point(0, 0);
            gbAdvancedNetwork.Margin = new Padding(0);
            gbAdvancedNetwork.Name = "gbAdvancedNetwork";
            gbAdvancedNetwork.Size = new Size(645, 76);
            gbAdvancedNetwork.TabIndex = 6;
            gbAdvancedNetwork.TabStop = false;
            gbAdvancedNetwork.Text = "Advanced Network settings";
            gbAdvancedNetwork.Visible = false;
            // 
            // txtRelevantIP
            // 
            txtRelevantIP.Location = new Point(84, 45);
            txtRelevantIP.Name = "txtRelevantIP";
            txtRelevantIP.Size = new Size(549, 23);
            txtRelevantIP.TabIndex = 2;
            // 
            // cbSendMode
            // 
            cbSendMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbSendMode.FormattingEnabled = true;
            cbSendMode.Items.AddRange(new object[] { "LAN Broadcast (default)", "Subnet Broadcast", "Multicast", "Direct IP targeting" });
            cbSendMode.Location = new Point(84, 17);
            cbSendMode.Name = "cbSendMode";
            cbSendMode.Size = new Size(166, 23);
            cbSendMode.TabIndex = 1;
            // 
            // txtLocalIpAddress
            // 
            txtLocalIpAddress.Location = new Point(542, 17);
            txtLocalIpAddress.MaxLength = 15;
            txtLocalIpAddress.Name = "txtLocalIpAddress";
            txtLocalIpAddress.Size = new Size(91, 23);
            txtLocalIpAddress.TabIndex = 0;
            txtLocalIpAddress.Text = "192.168.100.100";
            // 
            // gbGainControl
            // 
            gbGainControl.Controls.Add(chbAutoGainControl);
            gbGainControl.Controls.Add(tbGainValue);
            gbGainControl.Dock = DockStyle.Top;
            gbGainControl.Location = new Point(0, 76);
            gbGainControl.Margin = new Padding(0);
            gbGainControl.Name = "gbGainControl";
            gbGainControl.Size = new Size(645, 66);
            gbGainControl.TabIndex = 5;
            gbGainControl.TabStop = false;
            gbGainControl.Text = "Gain control";
            gbGainControl.Visible = false;
            // 
            // chbAutoGainControl
            // 
            chbAutoGainControl.AutoSize = true;
            chbAutoGainControl.Location = new Point(13, 23);
            chbAutoGainControl.Name = "chbAutoGainControl";
            chbAutoGainControl.Size = new Size(82, 19);
            chbAutoGainControl.TabIndex = 2;
            chbAutoGainControl.Text = "Automatic";
            chbAutoGainControl.UseVisualStyleBackColor = true;
            // 
            // tbGainValue
            // 
            tbGainValue.Location = new Point(98, 18);
            tbGainValue.Maximum = 100;
            tbGainValue.Name = "tbGainValue";
            tbGainValue.Size = new Size(535, 45);
            tbGainValue.TabIndex = 3;
            // 
            // pnlSettingsMain
            // 
            pnlSettingsMain.Controls.Add(groupbox4);
            pnlSettingsMain.Controls.Add(groupBox3);
            pnlSettingsMain.Controls.Add(groupBox1);
            pnlSettingsMain.Dock = DockStyle.Top;
            pnlSettingsMain.Location = new Point(0, 0);
            pnlSettingsMain.Name = "pnlSettingsMain";
            pnlSettingsMain.Size = new Size(645, 73);
            pnlSettingsMain.TabIndex = 5;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnAdvancedNetwork);
            groupBox1.Controls.Add(txtUdpPort);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(493, -8);
            groupBox1.Margin = new Padding(0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(151, 81);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            // 
            // btnAdvancedNetwork
            // 
            btnAdvancedNetwork.Location = new Point(7, 49);
            btnAdvancedNetwork.Name = "btnAdvancedNetwork";
            btnAdvancedNetwork.Size = new Size(137, 24);
            btnAdvancedNetwork.TabIndex = 19;
            btnAdvancedNetwork.Text = "🔣 Advanced network";
            btnAdvancedNetwork.UseVisualStyleBackColor = true;
            btnAdvancedNetwork.Click += btnAdvancedNetwork_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(651, 534);
            Controls.Add(pnlSettings);
            Controls.Add(groupBox2);
            Controls.Add(fftDisplay2);
            Controls.Add(grpBottomPanel);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WLED SoundReactive Server";
            grpBottomPanel.ResumeLayout(false);
            grpBottomPanel.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupbox4.ResumeLayout(false);
            pnlSettings.ResumeLayout(false);
            pnlSettings.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            gbAdvancedNetwork.ResumeLayout(false);
            gbAdvancedNetwork.PerformLayout();
            gbGainControl.ResumeLayout(false);
            gbGainControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tbGainValue).EndInit();
            pnlSettingsMain.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button btnExitApplication;
        private FFTDisplay fftDisplay1;
        private GroupBox grpBottomPanel;
        private FFTDisplay fftDisplay2;
        private Label lblPPS;
        private ButtonWithCheckbox btnSetAutoRun;
        private ButtonWithCheckbox btnSetStartupGUI;
        private Label label2;
        private TextBox txtUdpPort;
        private ComboBox ddlAudioDevices;
        private GroupBox groupBox2;
        private Label label3;
        private Label lblCapturing;
        private ToolTip toolTip1;
        private TextBox txtFFTLower;
        private Label label4;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label6;
        private TextBox txtFFTUpper;
        private Label label5;
        private Button btnSettings;
        private GroupBox groupbox4;
        private System.Windows.Forms.Timer tmrUpdateStats;
        private ComboBox ddlValueScale;
        private CheckBox chbFFTLogFreq;
        private Panel pnlSettings;
        private GroupBox groupBox3;
        private GroupBox groupBox1;
        private Label label8;
        private BeatPixel beatPixel1;
        private Label label9;
        private Button btnAdvancedNetwork;
        private Button btnGainSettings;
        private TableLayoutPanel tableLayoutPanel2;
        private GroupBox gbGainControl;
        private TrackBar tbGainValue;
        private CheckBox chbAutoGainControl;
        private GroupBox gbAdvancedNetwork;
        private TextBox txtRelevantIP;
        private Label lblRelevantIP;
        private ComboBox cbSendMode;
        private Label label10;
        private TextBox txtLocalIpAddress;
        private Label label1;
        private Panel pnlSettingsMain;
    }
}