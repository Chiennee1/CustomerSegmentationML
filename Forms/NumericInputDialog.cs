using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public class NumericInputDialog : Form
    {
        private NumericUpDown numericUpDown;
        private Button btnOK;
        private Button btnCancel;
        private Label lblPrompt;

        public int Value => (int)numericUpDown.Value;

        public NumericInputDialog(string title, string prompt, int defaultValue = 5, int min = 2, int max = 10)
        {
            this.Text = title;
            this.Size = new System.Drawing.Size(350, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblPrompt = new Label
            {
                Text = prompt,
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(300, 20),
                AutoSize = true
            };

            numericUpDown = new NumericUpDown
            {
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(100, 25),
                Minimum = min,
                Maximum = max,
                Value = defaultValue
            };

            btnOK = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(130, 100),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.OK
            };

            btnCancel = new Button
            {
                Text = "Hủy",
                Location = new System.Drawing.Point(230, 100),
                Size = new System.Drawing.Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblPrompt);
            this.Controls.Add(numericUpDown);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
    }
}