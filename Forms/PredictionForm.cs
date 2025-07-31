using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class PredictionForm : Form
    {
        public PredictionForm()
        {
            InitializeComponent1();
        }

        private void InitializeComponent1()
        {
            this.Text = "Dự đoán khách hàng";
            this.Size = new System.Drawing.Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            var label = new Label();
            label.Text = "Chức năng dự đoán đang được phát triển...";
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(20, 20);

            this.Controls.Add(label);
        }
    }
}