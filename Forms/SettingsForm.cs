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
            this.Text = "Cài ??t";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var label = new Label();
            label.Text = "Cài ??t h? th?ng ?ang ???c phát tri?n...";
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(20, 20);

            this.Controls.Add(label);
        }
    }
}