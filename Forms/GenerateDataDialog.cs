using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class GenerateDataDialog : Form
    {
        public int CustomerCount { get; private set; } = 1000;

        public GenerateDataDialog()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Tạo dữ liệu mẫu";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            numCustomerCount.Value = 1000;
            numCustomerCount.Minimum = 100;
            numCustomerCount.Maximum = 10000;
            numCustomerCount.Increment = 100;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            CustomerCount = (int)numCustomerCount.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Show the new data generation test form
            var testForm = new DataGenerationTestForm();
            testForm.ShowDialog();
        }
    }
}