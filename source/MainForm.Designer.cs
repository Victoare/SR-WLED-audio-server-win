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
            btnSettings = new Button();
            lblPPS = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            label6 = new Label();
            txtFFTUpper = new TextBox();
            label5 = new Label();
            label4 = new Label();
            txtFFTLower = new TextBox();
            txtLocalIpAddress = new TextBox();
            btnSetStartupGUI = new ButtonWithCheckbox();
            label2 = new Label();
            btnSetAutoRun = new ButtonWithCheckbox();
            txtUdpPort = new TextBox();
            label1 = new Label();
            ddlAudioDevices = new ComboBox();
            fftDisplay2 = new FFTDisplay();
            groupBox2 = new GroupBox();
            lblCapturing = new Label();
            label3 = new Label();
            toolTip1 = new ToolTip(components);
            grpSettings = new GroupBox();
            label7 = new Label();
            tmrUpdateStats = new System.Windows.Forms.Timer(components);
            grpBottomPanel.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBox2.SuspendLayout();
            grpSettings.SuspendLayout();
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
            grpBottomPanel.Controls.Add(btnSettings);
            grpBottomPanel.Controls.Add(lblPPS);
            grpBottomPanel.Controls.Add(btnExitApplication);
            grpBottomPanel.Location = new Point(4, 203);
            grpBottomPanel.Name = "grpBottomPanel";
            grpBottomPanel.Size = new Size(643, 42);
            grpBottomPanel.TabIndex = 5;
            grpBottomPanel.TabStop = false;
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
            lblPPS.Location = new Point(259, 17);
            lblPPS.Name = "lblPPS";
            lblPPS.Size = new Size(122, 15);
            lblPPS.TabIndex = 4;
            lblPPS.Text = "Packet per second : --";
            lblPPS.TextAlign = ContentAlignment.MiddleRight;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(label6, 4, 0);
            tableLayoutPanel1.Controls.Add(txtFFTUpper, 3, 0);
            tableLayoutPanel1.Controls.Add(label5, 2, 0);
            tableLayoutPanel1.Controls.Add(label4, 0, 0);
            tableLayoutPanel1.Controls.Add(txtFFTLower, 1, 0);
            tableLayoutPanel1.Location = new Point(219, 13);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(204, 29);
            tableLayoutPanel1.TabIndex = 16;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Fill;
            label6.Location = new Point(179, 0);
            label6.Name = "label6";
            label6.Size = new Size(22, 29);
            label6.TabIndex = 16;
            label6.Text = "Hz";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // txtFFTUpper
            // 
            txtFFTUpper.Dock = DockStyle.Fill;
            txtFFTUpper.Location = new Point(132, 3);
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
            label5.Location = new Point(114, 0);
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
            label4.Size = new Size(58, 29);
            label4.TabIndex = 0;
            label4.Text = "FFT range";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(label4, "Low and high end of the analyzed FFT spectrum");
            // 
            // txtFFTLower
            // 
            txtFFTLower.Dock = DockStyle.Fill;
            txtFFTLower.Location = new Point(67, 3);
            txtFFTLower.MaxLength = 5;
            txtFFTLower.Name = "txtFFTLower";
            txtFFTLower.Size = new Size(41, 23);
            txtFFTLower.TabIndex = 0;
            txtFFTLower.Text = "20";
            txtFFTLower.TextAlign = HorizontalAlignment.Center;
            // 
            // txtLocalIpAddress
            // 
            txtLocalIpAddress.Location = new Point(541, 19);
            txtLocalIpAddress.MaxLength = 15;
            txtLocalIpAddress.Name = "txtLocalIpAddress";
            txtLocalIpAddress.Size = new Size(91, 23);
            txtLocalIpAddress.TabIndex = 1;
            txtLocalIpAddress.Text = "192.168.100.100";
            // 
            // btnSetStartupGUI
            // 
            btnSetStartupGUI.CheckboxChecked = false;
            btnSetStartupGUI.Location = new Point(7, 47);
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
            label2.Location = new Point(495, 52);
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
            txtUdpPort.Location = new Point(541, 49);
            txtUdpPort.MaxLength = 5;
            txtUdpPort.Name = "txtUdpPort";
            txtUdpPort.Size = new Size(40, 23);
            txtUdpPort.TabIndex = 2;
            txtUdpPort.Text = "65535";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(494, 23);
            label1.Name = "label1";
            label1.Size = new Size(48, 15);
            label1.TabIndex = 10;
            label1.Text = "Local IP";
            toolTip1.SetToolTip(label1, "IP address of the local machine (if needed)");
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
            // grpSettings
            // 
            grpSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSettings.Controls.Add(tableLayoutPanel1);
            grpSettings.Controls.Add(btnSetStartupGUI);
            grpSettings.Controls.Add(txtLocalIpAddress);
            grpSettings.Controls.Add(btnSetAutoRun);
            grpSettings.Controls.Add(label2);
            grpSettings.Controls.Add(label1);
            grpSettings.Controls.Add(txtUdpPort);
            grpSettings.Controls.Add(label7);
            grpSettings.Location = new Point(4, 239);
            grpSettings.Name = "grpSettings";
            grpSettings.Size = new Size(643, 79);
            grpSettings.TabIndex = 0;
            grpSettings.TabStop = false;
            grpSettings.Visible = false;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label7.ForeColor = SystemColors.ControlDark;
            label7.Location = new Point(593, 64);
            label7.Name = "label7";
            label7.Size = new Size(49, 13);
            label7.TabIndex = 17;
            label7.Text = "Victoare";
            label7.TextAlign = ContentAlignment.BottomRight;
            // 
            // tmrUpdateStats
            // 
            tmrUpdateStats.Tick += tmrUpdateStats_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(651, 324);
            Controls.Add(groupBox2);
            Controls.Add(fftDisplay2);
            Controls.Add(grpBottomPanel);
            Controls.Add(grpSettings);
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
            grpSettings.ResumeLayout(false);
            grpSettings.PerformLayout();
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
        private Label label1;
        private TextBox txtUdpPort;
        private ComboBox ddlAudioDevices;
        private GroupBox groupBox2;
        private Label label3;
        private Label lblCapturing;
        private ToolTip toolTip1;
        private TextBox txtLocalIpAddress;
        private TextBox txtFFTLower;
        private Label label4;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label6;
        private TextBox txtFFTUpper;
        private Label label5;
        private Button btnSettings;
        private GroupBox grpSettings;
        private Label label7;
        private System.Windows.Forms.Timer tmrUpdateStats;
    }
}