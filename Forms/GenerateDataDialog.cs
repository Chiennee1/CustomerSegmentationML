using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class GenerateDataDialog : Form
    {
        public int CustomerCount { get; private set; } = 1000;
        public GenerateDataDialog() { InitializeComponent(); }
        private void InitializeComponent() { this.Text = "T?o d? li?u m?u"; this.Size = new System.Drawing.Size(350, 180); }
    }
}
