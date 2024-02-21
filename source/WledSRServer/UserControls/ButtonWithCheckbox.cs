namespace WledSRServer.UserControls
{
    public class ButtonWithCheckbox : Button
    {
        private CheckBox _chb;

        public ButtonWithCheckbox()
        {
            SetStyle(ControlStyles.Selectable, false);
            SuspendLayout();
            _chb = new CheckBox();
            Controls.Add(_chb);

            _chb.AutoSize = true;
            _chb.Enabled = false;
            _chb.Size = new Size(15, 14);
            _chb.Top = (Height - _chb.Height) / 2;
            _chb.Left = Width - _chb.Width - 3;
            _chb.Anchor = AnchorStyles.Right;
            _chb.UseVisualStyleBackColor = true;

            ResumeLayout(false);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            _chb.Invalidate();
        }

        public bool CheckboxChecked
        {
            get => _chb.Checked;
            set => _chb.Checked = value;
        }

    }
}
