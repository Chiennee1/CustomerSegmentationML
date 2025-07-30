using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class ResultsForm : Form
    {
        public ResultsForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.Text = "K?t qu? phân tích";
            this.Size = new System.Drawing.Size(900, 600);
        }

        private void InitializeUI()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            // TODO: Thêm các control hi?n th? k?t qu?, bi?u ??, b?ng phân tích segment
        }
    }
}
