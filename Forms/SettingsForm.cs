using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "C�i ??t";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var label = new Label();
            label.Text = "C�i ??t h? th?ng ?ang ???c ph�t tri?n...";
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(20, 20);

            this.Controls.Add(label);
        }
    }
}