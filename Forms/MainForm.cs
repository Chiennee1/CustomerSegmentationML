using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CustomerSegmentationML.ML.DataPreprocessing;
using CustomerSegmentationML.ML.AutoML;
using CustomerSegmentationML.Utils;
using System.Drawing;
using System.Collections.Generic;
using CustomerSegmentationML.ML.Algorithms;

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
            // Đường dẫn tuyệt đối đến thư mục Data trong dự án
            string dataDir = @"D:\StudyPython\PhanCumkhachHang\CustomerSegmentationML\Data";

            // Kiểm tra xem thư mục có tồn tại không
            if (!Directory.Exists(dataDir))
            {
                cmbDataset.Items.Clear();
                btnViewData.Enabled = false;
                btnStartTraining.Enabled = false;
                UpdateStatus("Không tìm thấy thư mục dữ liệu");
                lblDatasetCount.Text = "Có 0 dataset";
                return;
            }

            var dataFiles = Directory.GetFiles(dataDir, "*.csv");
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
                        string projectPath = @"D:\StudyPython\PhanCumkhachHang\CustomerSegmentationML";

                        // Đảm bảo thư mục Data tồn tại
                        string dataDir = Path.Combine(projectPath, "Data");
                        if (!Directory.Exists(dataDir))
                        {
                            Directory.CreateDirectory(dataDir);
                        }

                        // Đường dẫn tuyệt đối đến các file dữ liệu
                        string enhancedFilePath = Path.Combine(dataDir, "Enhanced_Customers.csv");
                        string ecommFilePath = Path.Combine(dataDir, "Vietnam_Ecommerce.csv");

                        await Task.Run(() =>
                        {
                            ((IProgress<string>)progress).Report("Đang tạo Enhanced Dataset...");
                            DatasetGenerator.GenerateEnhancedDataset(enhancedFilePath, dialog.CustomerCount);

                            ((IProgress<string>)progress).Report("Đang tạo Vietnam E-commerce Data...");
                            DatasetGenerator.GenerateVietnamEcommerceData(ecommFilePath);
                        }).ConfigureAwait(false);

                        // UI updates must be on UI thread
                        this.Invoke(new Action(() =>
                        {
                            CheckDataFiles();
                            UpdateStatus("Tạo dữ liệu thành công!");

                            // Hiển thị đường dẫn file đầy đủ trong thông báo
                            MessageBox.Show($"Đã tạo thành công dữ liệu mới tại:\n{enhancedFilePath}",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            var filePath = Path.Combine(@"D:\StudyPython\PhanCumkhachHang\CustomerSegmentationML\Data", fileName);

            var dataForm = new DataViewForm(filePath);
            dataForm.ShowDialog();
        }

        private void btnStartTraining_Click(object sender, EventArgs e)
        {
            if (cmbDataset.SelectedItem == null) return;

            var fileName = cmbDataset.SelectedItem.ToString();
            _currentDataPath = Path.Combine(@"D:\StudyPython\PhanCumkhachHang\CustomerSegmentationML\Data", fileName);

            var trainingForm = new TrainingForm(_currentDataPath, _autoTrainer);
            trainingForm.ShowDialog();

            // Refresh results if training completed
            CheckForResults();
        }

        private void btnViewResults_Click(object sender, EventArgs e)
        {
            var resultsForm = new ResultsForm();
            resultsForm.Show();
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

        private void InitializeModelTestButton()
        {
            var btnTestModel = new Button
            {
                Text = "🧪 Kiểm tra Mô hình",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(500, 500),
                Size = new Size(200, 50),
                BackColor = Color.LightYellow
            };

            btnTestModel.Click += (sender, e) =>
            {
                var resultsForm = new ResultsForm();
                resultsForm.ShowDialog();

                // Automatically select the testing tab
                var tabControl = resultsForm.Controls[0] as TabControl;
                if (tabControl != null && tabControl.TabPages.Count >= 5)
                {
                    tabControl.SelectedIndex = 4; // Select the model testing tab
                }
            };

            this.Controls.Add(btnTestModel);
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

        private async Task<List<object>> GetSegmentDataFromModelAsync(string modelPath, string dataPath)
        {
            var segmentsData = new List<object>();

            try
            {
                // Tạo và hiển thị ProgressDialog
                ProgressDialog progressDialog = new ProgressDialog("Đang phân tích dữ liệu...");
                // KHÔNG sử dụng "this" làm owner
                progressDialog.Show(); // Không truyền owner

                try
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            // Cập nhật message
                            if (!progressDialog.IsDisposed)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    if (!progressDialog.IsDisposed)
                                        progressDialog.UpdateMessage("Đang tải mô hình...");
                                }));
                            }

                            // Tạo instance của KMeansClusterer
                            var clusterer = new KMeansClusterer();
                            clusterer.LoadModel(modelPath);

                            // Cập nhật message
                            if (!progressDialog.IsDisposed)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    if (!progressDialog.IsDisposed)
                                        progressDialog.UpdateMessage("Đang phân tích dữ liệu...");
                                }));
                            }

                            var segments = clusterer.AnalyzeSegmentsFromFile(dataPath, 500); // Giảm xuống 500 mẫu để cải thiện hiệu suất

                            // Chuyển đổi Dictionary sang danh sách đối tượng
                            foreach (var segment in segments)
                            {
                                segmentsData.Add(new
                                {
                                    SegmentID = segment.Key,
                                    CustomerCount = segment.Value.CustomerCount,
                                    Percentage = segment.Value.Percentage,
                                    AvgAge = segment.Value.AverageFeatures["Age"],
                                    AvgIncome = segment.Value.AverageFeatures["Income"],
                                    AvgSpending = segment.Value.AverageFeatures["SpendingScore"],
                                    Description = segment.Value.Description
                                });
                            }

                            // Cập nhật message
                            if (!progressDialog.IsDisposed)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    if (!progressDialog.IsDisposed)
                                        progressDialog.UpdateMessage("Đã hoàn tất phân tích!");
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi phân tích dữ liệu: {ex.Message}", "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
                finally
                {
                    // Đóng progress dialog khi đã xong
                    if (!progressDialog.IsDisposed)
                    {
                        progressDialog.Invoke(new Action(() =>
                        {
                            if (!progressDialog.IsDisposed)
                                progressDialog.Close();
                        }));
                    }
                }

                return segmentsData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi trích xuất dữ liệu từ mô hình: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return GetSampleSegmentData();
            }
        }
        private List<object> GetSampleSegmentData()
        {
            // Sample data as fallback
            return new List<object>
    {
        new { SegmentID = 0, CustomerCount = 45, Percentage = 22.5, AvgAge = 28.5, AvgIncome = 65.2, AvgSpending = 78.3, Description = "High-Value Young Customers" },
        new { SegmentID = 1, CustomerCount = 38, Percentage = 19.0, AvgAge = 45.2, AvgIncome = 85.7, AvgSpending = 45.1, Description = "Conservative High-Income" },
        new { SegmentID = 2, CustomerCount = 52, Percentage = 26.0, AvgAge = 35.8, AvgIncome = 42.3, AvgSpending = 55.9, Description = "Balanced Middle-Class" },
        new { SegmentID = 3, CustomerCount = 33, Percentage = 16.5, AvgAge = 52.1, AvgIncome = 78.4, AvgSpending = 82.7, Description = "Premium Mature Customers" },
        new { SegmentID = 4, CustomerCount = 32, Percentage = 16.0, AvgAge = 23.9, AvgIncome = 28.6, AvgSpending = 25.4, Description = "Young Budget-Conscious" }
    };
        }
    }
}