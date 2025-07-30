using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class PredictionForm : Form
    {
        public PredictionForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.Text = "D? ?oán khách hàng m?i";
            this.Size = new System.Drawing.Size(600, 400);
        }

        private void InitializeUI()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            // TODO: Thêm các control nh?p thông tin khách hàng, nút d? ?oán, hi?n th? k?t qu?
        }
    }
}
