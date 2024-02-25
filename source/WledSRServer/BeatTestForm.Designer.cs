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
            fftGraph1 = new UserControls.BeatDetectorGraph();
            SuspendLayout();
            // 
            // fftGraph1
            // 
            fftGraph1.Dock = DockStyle.Fill;
            fftGraph1.Location = new Point(0, 0);
            fftGraph1.Name = "fftGraph1";
            fftGraph1.Size = new Size(704, 346);
            fftGraph1.TabIndex = 0;
            // 
            // BeatTestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 346);
            Controls.Add(fftGraph1);
            Name = "BeatTestForm";
            Text = "Beat detector test form";
            ResumeLayout(false);
        }

        #endregion

        private UserControls.BeatDetectorGraph fftGraph1;
    }
}