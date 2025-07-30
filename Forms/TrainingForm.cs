using System;
using System.Windows.Forms;
using CustomerSegmentationML.ML.AutoML;

namespace CustomerSegmentationML.Forms
{
    public partial class TrainingForm : Form
    {
        private string _dataPath;
        private AutoMLTrainer _autoTrainer;

        public TrainingForm(string dataPath, AutoMLTrainer autoTrainer)
        {
            _dataPath = dataPath;
            _autoTrainer = autoTrainer;
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Huấn luyện mô hình - Training";
            this.Size = new System.Drawing.Size(700, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            // TODO: Thêm các control chọn thuật toán, nút bắt đầu, progress bar, v.v.
        }
    }
}
