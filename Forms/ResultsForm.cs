using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class ResultsForm : Form
    {
        public ResultsForm()
        {
            InitializeComponent1();
        }

        private void InitializeComponent1()
        {
            this.Text = "K?t qu? phân tích";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var label = new Label();
            label.Text = "Chức năng đang phát triển...";
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(20, 20);

            this.Controls.Add(label);
        }
    }
}