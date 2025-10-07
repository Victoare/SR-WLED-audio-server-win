namespace WledSRServer
{
    partial class BeatTestForm
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
            fftGraph1 = new WledSRServer.UserControls.BeatDetectorGraph();
            groupBox1 = new GroupBox();
            panel1 = new Panel();
            lblFreqMax = new Label();
            lblFreqMin = new Label();
            groupBox2 = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            rbDisplayRange_Low100 = new RadioButton();
            rbDisplayRange_Beat = new RadioButton();
            rbDisplayRange_Settings = new RadioButton();
            rbDisplayRange_Full = new RadioButton();
            cbShowBeat = new CheckBox();
            label1 = new Label();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // fftGraph1
            // 
            fftGraph1.BeatFlash = true;
            fftGraph1.Dock = DockStyle.Fill;
            fftGraph1.Location = new Point(3, 19);
            fftGraph1.MaxFreq = 0D;
            fftGraph1.MinFreq = 0D;
            fftGraph1.Name = "fftGraph1";
            fftGraph1.Size = new Size(958, 356);
            fftGraph1.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(fftGraph1);
            groupBox1.Controls.Add(panel1);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(964, 392);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "FFT data";
            // 
            // panel1
            // 
            panel1.Controls.Add(lblFreqMax);
            panel1.Controls.Add(lblFreqMin);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(3, 375);
            panel1.Name = "panel1";
            panel1.Size = new Size(958, 14);
            panel1.TabIndex = 1;
            // 
            // lblFreqMax
            // 
            lblFreqMax.AutoSize = true;
            lblFreqMax.Dock = DockStyle.Right;
            lblFreqMax.Location = new Point(896, 0);
            lblFreqMax.Name = "lblFreqMax";
            lblFreqMax.Size = new Size(62, 15);
            lblFreqMax.TabIndex = 0;
            lblFreqMax.Text = "Freq HIGH";
            // 
            // lblFreqMin
            // 
            lblFreqMin.AutoSize = true;
            lblFreqMin.Dock = DockStyle.Left;
            lblFreqMin.Location = new Point(0, 0);
            lblFreqMin.Name = "lblFreqMin";
            lblFreqMin.Size = new Size(59, 15);
            lblFreqMin.TabIndex = 0;
            lblFreqMin.Text = "Freq LOW";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(tableLayoutPanel1);
            groupBox2.Dock = DockStyle.Bottom;
            groupBox2.Location = new Point(0, 392);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(964, 60);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Settings";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 0);
            tableLayoutPanel1.Controls.Add(cbShowBeat, 2, 0);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 19);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(958, 38);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(rbDisplayRange_Low100);
            flowLayoutPanel1.Controls.Add(rbDisplayRange_Beat);
            flowLayoutPanel1.Controls.Add(rbDisplayRange_Settings);
            flowLayoutPanel1.Controls.Add(rbDisplayRange_Full);
            flowLayoutPanel1.Location = new Point(87, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(266, 31);
            flowLayoutPanel1.TabIndex = 4;
            flowLayoutPanel1.WrapContents = false;
            // 
            // rbDisplayRange_Low100
            // 
            rbDisplayRange_Low100.Appearance = Appearance.Button;
            rbDisplayRange_Low100.AutoSize = true;
            rbDisplayRange_Low100.Location = new Point(3, 3);
            rbDisplayRange_Low100.Name = "rbDisplayRange_Low100";
            rbDisplayRange_Low100.Size = new Size(60, 25);
            rbDisplayRange_Low100.TabIndex = 3;
            rbDisplayRange_Low100.TabStop = true;
            rbDisplayRange_Low100.Text = "Low 100";
            rbDisplayRange_Low100.UseVisualStyleBackColor = true;
            // 
            // rbDisplayRange_Beat
            // 
            rbDisplayRange_Beat.Appearance = Appearance.Button;
            rbDisplayRange_Beat.AutoSize = true;
            rbDisplayRange_Beat.Checked = true;
            rbDisplayRange_Beat.Location = new Point(69, 3);
            rbDisplayRange_Beat.Name = "rbDisplayRange_Beat";
            rbDisplayRange_Beat.Size = new Size(87, 25);
            rbDisplayRange_Beat.TabIndex = 2;
            rbDisplayRange_Beat.TabStop = true;
            rbDisplayRange_Beat.Text = "Beat detector";
            rbDisplayRange_Beat.UseVisualStyleBackColor = true;
            // 
            // rbDisplayRange_Settings
            // 
            rbDisplayRange_Settings.Appearance = Appearance.Button;
            rbDisplayRange_Settings.AutoSize = true;
            rbDisplayRange_Settings.Location = new Point(162, 3);
            rbDisplayRange_Settings.Name = "rbDisplayRange_Settings";
            rbDisplayRange_Settings.Size = new Size(59, 25);
            rbDisplayRange_Settings.TabIndex = 2;
            rbDisplayRange_Settings.TabStop = true;
            rbDisplayRange_Settings.Text = "Settings";
            rbDisplayRange_Settings.UseVisualStyleBackColor = true;
            // 
            // rbDisplayRange_Full
            // 
            rbDisplayRange_Full.Appearance = Appearance.Button;
            rbDisplayRange_Full.AutoSize = true;
            rbDisplayRange_Full.Location = new Point(227, 3);
            rbDisplayRange_Full.Name = "rbDisplayRange_Full";
            rbDisplayRange_Full.Size = new Size(36, 25);
            rbDisplayRange_Full.TabIndex = 2;
            rbDisplayRange_Full.TabStop = true;
            rbDisplayRange_Full.Text = "Full";
            rbDisplayRange_Full.UseVisualStyleBackColor = true;
            // 
            // cbShowBeat
            // 
            cbShowBeat.AutoSize = true;
            cbShowBeat.CheckAlign = ContentAlignment.MiddleRight;
            cbShowBeat.Checked = true;
            cbShowBeat.CheckState = CheckState.Checked;
            cbShowBeat.Dock = DockStyle.Fill;
            cbShowBeat.Location = new Point(359, 3);
            cbShowBeat.Name = "cbShowBeat";
            cbShowBeat.Size = new Size(81, 32);
            cbShowBeat.TabIndex = 4;
            cbShowBeat.Text = "Show beat";
            cbShowBeat.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(78, 38);
            label1.TabIndex = 1;
            label1.Text = "Display range";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // BeatTestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(964, 452);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            Name = "BeatTestForm";
            Text = "Beat detector test form";
            groupBox1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private UserControls.BeatDetectorGraph fftGraph1;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label label1;
        private Panel panel1;
        private Label lblFreqMax;
        private Label lblFreqMin;
        private CheckBox cbShowBeat;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private RadioButton rbDisplayRange_Low100;
        private RadioButton rbDisplayRange_Beat;
        private RadioButton rbDisplayRange_Settings;
        private RadioButton rbDisplayRange_Full;
    }
}