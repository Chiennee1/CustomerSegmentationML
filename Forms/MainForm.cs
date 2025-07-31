using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CustomerSegmentationML.ML.DataPreprocessing;
using CustomerSegmentationML.ML.AutoML;
using CustomerSegmentationML.Utils;

namespace CustomerSegmentationML.Forms
{
    public partial class MainForm : Form
    {
        private AutoMLTrainer _autoTrainer;
        private string _currentDataPath;

        public MainForm()
        {
            InitializeComponent();
            _autoTrainer = new AutoMLTrainer();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Customer Segmentation ML - Machine Learning Tool";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Update status
            UpdateStatus("Sẵn sàng để bắt đầu phân tích");
            CheckDataFiles();
        }

        private void CheckDataFiles()
        {
            var dataFiles = Directory.GetFiles("Data", "*.csv");
            cmbDataset.Items.Clear();

            foreach (var file in dataFiles)
            {
                cmbDataset.Items.Add(Path.GetFileName(file));
            }

            if (cmbDataset.Items.Count > 0)
            {
                cmbDataset.SelectedIndex = 0;
                btnViewData.Enabled = true;
                btnStartTraining.Enabled = true;
            }
            else
            {
                btnViewData.Enabled = false;
                btnStartTraining.Enabled = false;
                UpdateStatus("Không tìm thấy file dữ liệu");
            }

            lblDatasetCount.Text = $"Có {cmbDataset.Items.Count} dataset";
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = $"Trạng thái: {message}";
            statusStrip.Refresh();
        }

        private async void btnGenerateData_Click(object sender, EventArgs e)
        {
            using (var dialog = new GenerateDataDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var progress = new Progress<string>(msg => UpdateStatus(msg));
                    btnGenerateData.Enabled = false;

                    try
                    {
                        await Task.Run(() =>
                        {
                            ((IProgress<string>)progress).Report("Đang tạo Enhanced Dataset...");
                            DatasetGenerator.GenerateEnhancedDataset("Data/Enhanced_Customers.csv", dialog.CustomerCount);

                            ((IProgress<string>)progress).Report("Đang tạo Vietnam E-commerce Data...");
                            DatasetGenerator.GenerateVietnamEcommerceData("Data/Vietnam_Ecommerce.csv");
                        }).ConfigureAwait(false);

                        // UI updates must be on UI thread
                        this.Invoke(new Action(() =>
                        {
                            CheckDataFiles();
                            UpdateStatus("Tạo dữ liệu thành công!");
                            MessageBox.Show("Đã tạo thành công dữ liệu mới!", "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show($"Lỗi tạo dữ liệu: {ex.Message}", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    finally
                    {
                        this.Invoke(new Action(() =>
                        {
                            btnGenerateData.Enabled = true;
                        }));
                    }
                }
            }
        }

        private void btnViewData_Click(object sender, EventArgs e)
        {
            if (cmbDataset.SelectedItem == null) return;

            var fileName = cmbDataset.SelectedItem.ToString();
            var filePath = Path.Combine("Data", fileName);

            var dataForm = new DataViewForm(filePath);
            dataForm.ShowDialog();
        }

        private async void btnStartTraining_Click(object sender, EventArgs e)
        {
            if (cmbDataset.SelectedItem == null) return;

            var fileName = cmbDataset.SelectedItem.ToString();
            _currentDataPath = Path.Combine("Data", fileName);

            var trainingForm = new TrainingForm(_currentDataPath, _autoTrainer);
            trainingForm.ShowDialog();

            // Refresh results if training completed
            CheckForResults();
        }

        private void btnViewResults_Click(object sender, EventArgs e)
        {
            var resultsForm = new ResultsForm();
            resultsForm.ShowDialog();
        }

        private void btnPredictCustomer_Click(object sender, EventArgs e)
        {
            var predictionForm = new PredictionForm();
            predictionForm.ShowDialog();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        private void CheckForResults()
        {
            var hasResults = Directory.Exists("Results") &&
                           Directory.GetFiles("Results", "*.csv").Length > 0;
            btnViewResults.Enabled = hasResults;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckForResults();

            // Show welcome message
            var welcome = new WelcomeDialog();
            welcome.ShowDialog();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            var helpText = @"
HƯỚNG DẪN SỬ DỤNG CUSTOMER SEGMENTATION ML

1. TẠO DỮ LIỆU:
   - Click 'Tạo dữ liệu mẫu' để tạo dataset
   - Chọn số lượng khách hàng muốn tạo

2. XEM DỮ LIỆU:
   - Chọn dataset từ dropdown
   - Click 'Xem dữ liệu' để preview

3. HUẤN LUYỆN MÔ HÌNH:
   - Click 'Bắt đầu Training'
   - Chọn thuật toán hoặc AutoML
   - Theo dõi progress

4. XEM KẾT QUẢ:
   - Click 'Xem kết quả' sau khi training
   - Phân tích các segment

5. DỰ ĐOÁN:
   - Click 'Dự đoán khách hàng'
   - Nhập thông tin khách hàng mới

FEATURES:
✓ Multiple ML algorithms (K-Means, DBSCAN, etc.)
✓ AutoML tự động tìm mô hình tốt nhất
✓ Enhanced dataset với nhiều features
✓ Visualization và analysis tools
✓ Export results to Excel/CSV
";

            MessageBox.Show(helpText, "Hướng dẫn sử dụng",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void grpDataset_Enter(object sender, EventArgs e)
        {

        }

        private void MainForm_Load_1(object sender, EventArgs e)
        {

        }

        private void menuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}