using CustomerSegmentationML.ML.Algorithms;
using CustomerSegmentationML.Models;
using CustomerSegmentationML.Utils;
using Microsoft.ML;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CustomerSegmentationML.Forms
{
    public partial class ResultsForm : Form
    {
        private TabControl tabControl;
        private TabPage tabOverview;
        private TabPage tabSegments;
        private TabPage tabCharts;
        private TabPage tabComparison;

        // Overview controls
        private RichTextBox rtbOverview;
        private GroupBox grpModelInfo;
        private GroupBox grpMetrics;

        // Segments controls
        private DataGridView dgvSegments;
        private GroupBox grpSegmentDetails;
        private RichTextBox rtbSegmentDetails;

        // Charts controls
        private Panel chartContainer;
        private ComboBox cmbChartType;
        private ComboBox cmbFeatureX;
        private ComboBox cmbFeatureY;
        private Button btnGenerateChart;

        // Comparison controls
        private DataGridView dgvModelComparison;
        private Chart chartModelComparison;

        private Dictionary<string, object> _currentResults;
        private List<string> _availableModels;

        private MLContext _mlContext = new MLContext(seed: 0);
        private ITransformer _model;

        public ResultsForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadResults();
        }

        private void InitializeCustomUI()
        {
            this.Text = "📊 Kết quả phân tích & Đánh giá mô hình";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Maximized;

            // Create main tab control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.Controls.Add(tabControl);

            CreateOverviewTab();
            CreateSegmentsTab();
            CreateChartsTab();
            CreateComparisonTab();
            CreateModelTestingTab(); 

            tabControl.SelectedIndex = 0;
        }

        private void CreateOverviewTab()
        {
            tabOverview = new TabPage("📋 Tổng quan");
            tabControl.TabPages.Add(tabOverview);

            // Model info group
            grpModelInfo = new GroupBox
            {
                Text = "🤖 Thông tin mô hình"
            };
            grpModelInfo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpModelInfo.Location = new Point(20, 20);
            grpModelInfo.Size = new Size(650, 200);

            var rtbModelInfo = new RichTextBox();
            rtbModelInfo.Location = new Point(15, 25);
            rtbModelInfo.Size = new Size(620, 160);
            rtbModelInfo.Font = new Font("Segoe UI", 9);
            rtbModelInfo.ReadOnly = true;
            grpModelInfo.Controls.Add(rtbModelInfo);

            // Metrics group
            grpMetrics = new GroupBox{ Text = "📊 Các chỉ số đánh giá" };
            grpMetrics.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpMetrics.Location = new Point(690, 20);
            grpMetrics.Size = new Size(650, 200);

            var rtbMetrics = new RichTextBox();
            rtbMetrics.Location = new Point(15, 25);
            rtbMetrics.Size = new Size(620, 160);
            rtbMetrics.Font = new Font("Segoe UI", 9);
            rtbMetrics.ReadOnly = true;
            grpMetrics.Controls.Add(rtbMetrics);

            // Overview summary
            var grpOverview = new GroupBox{ Text = "📈 Tóm tắt kết quả" };
            grpOverview.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpOverview.Location = new Point(20, 240);
            grpOverview.Size = new Size(1320, 400);

            rtbOverview = new RichTextBox();
            rtbOverview.Location = new Point(15, 25);
            rtbOverview.Size = new Size(1290, 360);
            rtbOverview.Font = new Font("Segoe UI", 9);
            rtbOverview.ReadOnly = true;
            grpOverview.Controls.Add(rtbOverview);

            tabOverview.Controls.AddRange(new Control[] { grpModelInfo, grpMetrics, grpOverview });

            // Store references for later use
            grpModelInfo.Tag = rtbModelInfo;
            grpMetrics.Tag = rtbMetrics;
        }

        private void CreateSegmentsTab()
        {
            tabSegments = new TabPage("🎯 Phân tích Segments");
            tabControl.TabPages.Add(tabSegments);

            // Segments grid
            dgvSegments = new DataGridView();
            dgvSegments.Location = new Point(20, 20);
            dgvSegments.Size = new Size(800, 300);
            dgvSegments.ReadOnly = true;
            dgvSegments.AutoGenerateColumns = true;
            dgvSegments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSegments.SelectionChanged += DgvSegments_SelectionChanged;

            // Segment details
            grpSegmentDetails = new GroupBox{ Text = "🔍 Chi tiết Segment" };
            grpSegmentDetails.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpSegmentDetails.Location = new Point(840, 20);
            grpSegmentDetails.Size = new Size(500, 300);

            rtbSegmentDetails = new RichTextBox();
            rtbSegmentDetails.Location = new Point(15, 25);
            rtbSegmentDetails.Size = new Size(470, 260);
            rtbSegmentDetails.Font = new Font("Segoe UI", 9);
            rtbSegmentDetails.ReadOnly = true;
            grpSegmentDetails.Controls.Add(rtbSegmentDetails);

            // Customer distribution chart
            var grpDistribution = new GroupBox{ Text = "📊 Phân bố khách hàng" };
            grpDistribution.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpDistribution.Location = new Point(20, 340);
            grpDistribution.Size = new Size(1320, 300);

            var chartDistribution = new Chart();
            chartDistribution.Location = new Point(15, 25);
            chartDistribution.Size = new Size(1290, 260);
            grpDistribution.Controls.Add(chartDistribution);
            grpDistribution.Tag = chartDistribution;

            tabSegments.Controls.AddRange(new Control[] { dgvSegments, grpSegmentDetails, grpDistribution });

            // Thêm nút để tái huấn luyện mô hình với các tham số khác
    var btnRetrainModel = new Button
    {
        Text = "🔄 Tái huấn luyện mô hình",
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        BackColor = Color.LightYellow,
        Location = new Point(20, 650),
        Size = new Size(180, 30)
    };
    btnRetrainModel.Click += BtnRetrainModel_Click;
    
    tabSegments.Controls.Add(btnRetrainModel);
}

private void BtnRetrainModel_Click(object sender, EventArgs e)
{
    // Hiển thị form cấu hình để điều chỉnh tham số
    var configDialog = new NumericInputDialog("Số lượng Segments (K)", "Nhập số lượng segments (clusters) bạn muốn phân chia:", 5, 2, 10);
    
    if (configDialog.ShowDialog() == DialogResult.OK)
    {
        int k = configDialog.Value;
        
        try
        {
            // Tìm file dữ liệu mới nhất
            var dataDir = @"D:\StudyPython\PhanCumkhachHang\CustomerSegmentationML\Data";
            if (!Directory.Exists(dataDir))
            {
                MessageBox.Show("Không tìm thấy thư mục dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var dataFiles = Directory.GetFiles(dataDir, "*.csv");
            if (dataFiles.Length == 0)
            {
                MessageBox.Show("Không tìm thấy file dữ liệu CSV.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var latestDataFile = dataFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
            
            // Hiển thị TrainingForm với tham số K
            var clusterer = new KMeansClusterer();
            clusterer.Parameters["NumberOfClusters"] = k;
            
            var trainingForm = new TrainingForm(latestDataFile, null, clusterer);
            trainingForm.ShowDialog();
            
            // Sau khi training xong, tải lại kết quả
            LoadResults();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi khởi động lại huấn luyện: {ex.Message}", "Lỗi", 
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
        private void CreateChartsTab()
        {
            tabCharts = new TabPage("📈 Biểu đồ & Trực quan hóa");
            tabControl.TabPages.Add(tabCharts);

            // Chart controls
            var grpChartControls = new GroupBox{ Text = "🎛️ Điều khiển biểu đồ" }    ;
            grpChartControls.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpChartControls.Location = new Point(20, 20);
            grpChartControls.Size = new Size(1320, 80);

            var lblChartType = new Label { Text = "Loại biểu đồ:", Location = new Point(20, 30), Size = new Size(80, 23) };
            cmbChartType = new ComboBox
            {
                Location = new Point(110, 27),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbChartType.Items.AddRange(new[] { "Scatter Plot", "Bar Chart", "Pie Chart", "Heatmap" });
            cmbChartType.SelectedIndex = 0;

            var lblFeatureX = new Label { Text = "Trục X:", Location = new Point(280, 30), Size = new Size(50, 23) };
            cmbFeatureX = new ComboBox
            {
                Location = new Point(340, 27),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblFeatureY = new Label { Text = "Trục Y:", Location = new Point(480, 30), Size = new Size(50, 23) };
            cmbFeatureY = new ComboBox
            {
                Location = new Point(540, 27),
                Size = new Size(120, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnGenerateChart = new Button
            {
                Text = "🎨 Tạo biểu đồ",
                Location = new Point(680, 25),
                Size = new Size(120, 30),
                BackColor = Color.LightBlue
            };
            btnGenerateChart.Click += BtnGenerateChart_Click;

            grpChartControls.Controls.AddRange(new Control[] {
                lblChartType, cmbChartType, lblFeatureX, cmbFeatureX,
                lblFeatureY, cmbFeatureY, btnGenerateChart
            });

            // Chart container
            chartContainer = new Panel();
            chartContainer.Location = new Point(20, 120);
            chartContainer.Size = new Size(1320, 520);
            chartContainer.BorderStyle = BorderStyle.FixedSingle;
            chartContainer.BackColor = Color.White;

            tabCharts.Controls.AddRange(new Control[] { grpChartControls, chartContainer });

            // Populate feature dropdowns
            PopulateFeatureDropdowns();
        }

        private void CreateComparisonTab()
        {
            tabComparison = new TabPage("⚖️ So sánh mô hình");
            tabControl.TabPages.Add(tabComparison);

            // Model comparison grid
            var grpModelComparison = new GroupBox{ Text = "📊 Bảng so sánh" };
            grpModelComparison.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpModelComparison.Location = new Point(20, 20);
            grpModelComparison.Size = new Size(1320, 300);

            dgvModelComparison = new DataGridView();
            dgvModelComparison.Location = new Point(15, 25);
            dgvModelComparison.Size = new Size(1290, 260);
            dgvModelComparison.ReadOnly = true;
            dgvModelComparison.AutoGenerateColumns = true;
            grpModelComparison.Controls.Add(dgvModelComparison);

            // Comparison chart
            var grpComparisonChart = new GroupBox{ Text = "📈 Biểu đồ so sánh" };
            grpComparisonChart.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpComparisonChart.Location = new Point(20, 340);
            grpComparisonChart.Size = new Size(1320, 300);

            chartModelComparison = new Chart();
            chartModelComparison.Location = new Point(15, 25);
            chartModelComparison.Size = new Size(1290, 260);
            grpComparisonChart.Controls.Add(chartModelComparison);

            tabComparison.Controls.AddRange(new Control[] { grpModelComparison, grpComparisonChart });
        }

        private void PopulateFeatureDropdowns()
        {
            var features = new[] {
                "Age", "AnnualIncome", "SpendingScore", "Education",
                "WorkExperience", "FamilySize", "OnlineShoppingFreq",
                "BrandLoyalty", "SocialMediaUsage"
            };

            cmbFeatureX.Items.AddRange(features);
            cmbFeatureY.Items.AddRange(features);
            cmbFeatureX.SelectedIndex = 1; // AnnualIncome
            cmbFeatureY.SelectedIndex = 2; // SpendingScore
        }

        private void LoadResults()
        {
            try
            {
                var resultsDir = "Results";
                if (!Directory.Exists(resultsDir))
                {
                    ShowNoResultsMessage();
                    return;
                }

                var reportFiles = Directory.GetFiles(resultsDir, "*Report*.txt");
                var modelFiles = Directory.GetFiles(resultsDir, "*.zip");

                if (reportFiles.Length == 0 && modelFiles.Length == 0)
                {
                    ShowNoResultsMessage();
                    return;
                }

                // Load the most recent results
                LoadLatestResults(reportFiles, modelFiles);
                LoadModelComparison();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải kết quả: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowNoResultsMessage()
        {
            rtbOverview.Text = @"
❌ CHƯA CÓ KẾT QUẢ TRAINING

Bạn chưa huấn luyện mô hình nào. Vui lòng:

1. 📊 Tải hoặc tạo dữ liệu
2. 🤖 Chọn thuật toán và bắt đầu training
3. ⏳ Chờ quá trình training hoàn thành
4. 📈 Quay lại đây để xem kết quả

Hướng dẫn:
• Vào Dataset Management → Tạo dữ liệu mẫu
• Vào Model Training → Bắt đầu Training
• Chọn AutoML để tự động tìm mô hình tốt nhất
";
        }

        private void LoadLatestResults(string[] reportFiles, string[] modelFiles)
        {
            // Load report if available
            if (reportFiles.Length > 0)
            {
                var latestReport = reportFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
                LoadReportResults(latestReport);
            }

            // Load model info
            if (modelFiles.Length > 0)
            {
                var latestModel = modelFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
                LoadModelInfo(latestModel);
            }

            // Load segments data if available
            LoadSegmentsData();
        }

        private void LoadReportResults(string reportPath)
        {
            try
            {
                var reportContent = File.ReadAllText(reportPath);

                // Extract key information from report
                var modelInfo = ExtractModelInfo(reportContent);
                var metrics = ExtractMetrics(reportContent);

                // Update UI
                var rtbModelInfo = (RichTextBox)grpModelInfo.Tag;
                rtbModelInfo.Text = modelInfo;

                var rtbMetrics = (RichTextBox)grpMetrics.Tag;
                rtbMetrics.Text = metrics;

                // Create overview summary
                CreateOverviewSummary(reportContent);
            }
            catch (Exception ex)
            {
                rtbOverview.Text = $"Lỗi khi đọc report: {ex.Message}";
            }
        }

        private string ExtractModelInfo(string reportContent)
        {
            var lines = reportContent.Split('\n');
            var modelInfo = "🤖 THÔNG TIN MÔ HÌNH\n";
            modelInfo += "========================\n\n";

            foreach (var line in lines)
            {
                if (line.Contains("Timestamp:"))
                    modelInfo += $"📅 Thời gian training: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Dataset:"))
                    modelInfo += $"📊 Dataset: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Best algorithm:"))
                    modelInfo += $"🏆 Thuật toán tốt nhất: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Total training time:"))
                    modelInfo += $"⏱️ Thời gian training: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Total configurations tested:"))
                    modelInfo += $"🔢 Số cấu hình đã thử: {line.Split(':')[1].Trim()}\n";
            }

            return modelInfo;
        }
        private void CreateModelTestingTab()
        {
            // Create a new tab for model testing
            var tabModelTesting = new TabPage("🧪 Test Mô hình");
            tabControl.TabPages.Add(tabModelTesting);

            // Customer input panel
            var grpInput = new GroupBox
            {
                Text = "📝 Nhập thông tin khách hàng để kiểm tra",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(550, 450)
            };

            // Customer test result panel
            var grpResult = new GroupBox
            {
                Text = "🎯 Kết quả phân cụm",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(590, 20),
                Size = new Size(550, 450)
            };

            // Create input controls
            var y = 30;
            int labelWidth = 120;
            int controlWidth = 200;
            int spacing = 35;

            // Gender
            var lblGender = new Label { Text = "Giới tính:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbGender = new ComboBox { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new[] { "Female", "Male" });
            cmbGender.SelectedIndex = 0;

            // Age
            y += spacing;
            var lblAge = new Label { Text = "Tuổi:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numAge = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 18, Maximum = 70, Value = 30 };

            // Annual Income
            y += spacing;
            var lblIncome = new Label { Text = "Thu nhập (k$):", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numIncome = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 5, Maximum = 150, Value = 50 };

            // Spending Score
            y += spacing;
            var lblSpending = new Label { Text = "Điểm chi tiêu:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numSpending = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 1, Maximum = 100, Value = 50 };

            // Education
            y += spacing;
            var lblEducation = new Label { Text = "Trình độ học vấn:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbEducation = new ComboBox { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEducation.Items.AddRange(new[] { "High School", "Bachelor", "Master", "PhD" });
            cmbEducation.SelectedIndex = 1;

            // Work Experience
            y += spacing;
            var lblWorkExp = new Label { Text = "Kinh nghiệm:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numWorkExp = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 0, Maximum = 40, Value = 5 };

            // Family Size
            y += spacing;
            var lblFamilySize = new Label { Text = "Quy mô gia đình:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numFamilySize = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 1, Maximum = 10, Value = 3 };

            // Online Shopping Frequency
            y += spacing;
            var lblShoppingFreq = new Label { Text = "Mua sắm online:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numShoppingFreq = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 0, Maximum = 30, Value = 5 };

            // Brand Loyalty
            y += spacing;
            var lblBrandLoyalty = new Label { Text = "Lòng trung thành:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numBrandLoyalty = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 1, Maximum = 10, Value = 5 };

            // Social Media Usage
            y += spacing;
            var lblSocialMedia = new Label { Text = "Mạng xã hội:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numSocialMedia = new NumericUpDown { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 0, Maximum = 24, Value = 2, DecimalPlaces = 1 };

            // City selection
            y += spacing;
            var lblCity = new Label { Text = "Thành phố:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbCity = new ComboBox { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCity.Items.AddRange(new[] { "HaNoi", "HCM", "DaNang", "Others" });
            cmbCity.SelectedIndex = 0;

            // Profession selection
            y += spacing;
            var lblProfession = new Label { Text = "Nghề nghiệp:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbProfession = new ComboBox { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProfession.Items.AddRange(new[] { "Student", "Healthcare", "Engineer", "Artist", "Lawyer", "Doctor", "Marketing", "Entertainment" });
            cmbProfession.SelectedIndex = 2;  // Default to Engineer

            // Preferred Channel
            y += spacing;
            var lblChannel = new Label { Text = "Kênh mua sắm:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbChannel = new ComboBox { Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbChannel.Items.AddRange(new[] { "Online", "Offline", "Both" });
            cmbChannel.SelectedIndex = 2;

            // Test button
            var btnTestModel = new Button
            {
                Text = "🧪 Kiểm tra phân cụm",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                Location = new Point(20, 480),
                Size = new Size(200, 40)
            };

            // Result textbox
            var rtbTestResult = new RichTextBox
            {
                Location = new Point(15, 25),
                Size = new Size(520, 410),
                Font = new Font("Segoe UI", 9),
                ReadOnly = true
            };

            // Add controls to group boxes
            grpInput.Controls.AddRange(new Control[] {
        lblGender, cmbGender,
        lblAge, numAge,
        lblIncome, numIncome,
        lblSpending, numSpending,
        lblEducation, cmbEducation,
        lblWorkExp, numWorkExp,
        lblFamilySize, numFamilySize,
        lblShoppingFreq, numShoppingFreq,
        lblBrandLoyalty, numBrandLoyalty,
        lblSocialMedia, numSocialMedia,
        lblCity, cmbCity,
        lblProfession, cmbProfession,
        lblChannel, cmbChannel
    });

            grpResult.Controls.Add(rtbTestResult);

            // Add test button click handler
            btnTestModel.Click += (sender, e) =>
            {
                try
                {
                    // Check if model is available
                    var resultsDir = "Results";
                    if (!Directory.Exists(resultsDir))
                    {
                        rtbTestResult.Text = "❌ Không tìm thấy thư mục kết quả. Vui lòng huấn luyện mô hình trước.";
                        return;
                    }

                    var modelFiles = Directory.GetFiles(resultsDir, "*.zip");
                    if (modelFiles.Length == 0)
                    {
                        rtbTestResult.Text = "❌ Không tìm thấy file mô hình đã lưu. Vui lòng huấn luyện mô hình trước.";
                        return;
                    }

                    // Get most recent model
                    var latestModel = modelFiles.OrderByDescending(f => File.GetCreationTime(f)).First();

                    rtbTestResult.Text = "⏳ Đang tải mô hình và dự đoán...";
                    Application.DoEvents(); // Force UI update

                    // Create customer data from input
                    var customer = new EnhancedCustomerData
                    {
                        CustomerID = DateTime.Now.Ticks,
                        Gender = cmbGender.SelectedItem.ToString() == "Female" ? 0f : 1f,
                        Age = (float)numAge.Value,
                        AnnualIncome = (float)numIncome.Value,
                        SpendingScore = (float)numSpending.Value,
                        Education = cmbEducation.SelectedIndex,
                        Profession = cmbProfession.SelectedIndex,
                        WorkExperience = (float)numWorkExp.Value,
                        FamilySize = (float)numFamilySize.Value,
                        City = cmbCity.SelectedIndex,
                        OnlineShoppingFreq = (float)numShoppingFreq.Value,
                        BrandLoyalty = (float)numBrandLoyalty.Value,
                        SocialMediaUsage = (float)numSocialMedia.Value,
                        PreferredChannel = cmbChannel.SelectedIndex
                    };

                    rtbTestResult.Text = "⏳ Tạo dữ liệu khách hàng thành công. Đang tải mô hình...";
                    Application.DoEvents(); // Force UI update

                    // Load model and predict - wrap in a timeout
                    KMeansClusterer clusterer = null;
                    try
                    {
                        clusterer = new KMeansClusterer();
                        rtbTestResult.Text = "⏳ Đang tải mô hình...";
                        Application.DoEvents();
                        
                        // Load model with a timeout
                        var loadTask = Task.Run(() => clusterer.LoadModel(latestModel));
                        if (!loadTask.Wait(TimeSpan.FromSeconds(10)))
                        {
                            rtbTestResult.Text = "❌ Quá thời gian tải mô hình. Có thể file mô hình bị hỏng.";
                            return;
                        }
                        
                        rtbTestResult.Text = "⏳ Đang dự đoán phân cụm...";
                        Application.DoEvents();
                        
                        // Predict with a timeout
                        var predictionTask = Task.Run(() => clusterer.PredictAsync(customer));
                        if (!predictionTask.Wait(TimeSpan.FromSeconds(10)))
                        {
                            rtbTestResult.Text = "❌ Quá thời gian dự đoán. Có thể mô hình không tương thích với dữ liệu.";
                            return;
                        }
                        
                        var prediction = predictionTask.Result;

                        // Display prediction result
                        string segmentDescription = GetSegmentDescription(prediction.PredictedClusterId);

                        var resultText = $@"
🎯 KẾT QUẢ KIỂM TRA MÔ HÌNH
===========================

✅ Mô hình đã tải: {Path.GetFileName(latestModel)}

👤 THÔNG TIN KHÁCH HÀNG:
• Giới tính: {(customer.Gender == 0 ? "Nữ" : "Nam")}
• Tuổi: {customer.Age}
• Thu nhập: ${customer.AnnualIncome}k
• Điểm chi tiêu: {customer.SpendingScore}/100
• Trình độ: {cmbEducation.SelectedItem}
• Nghề nghiệp: {cmbProfession.SelectedItem}
• Kinh nghiệm: {customer.WorkExperience} năm
• Quy mô gia đình: {customer.FamilySize} người
• Thành phố: {cmbCity.SelectedItem}

🏷️ KẾT QUẢ PHÂN CỤM:
• Thuộc segment: {prediction.PredictedClusterId}
• Mô tả: {segmentDescription}

💡 GHI CHÚ:
Đây là kết quả dựa trên mô hình đã huấn luyện.
Bạn có thể dùng chức năng 'Dự đoán' để phân tích chi tiết hơn.
";

                        rtbTestResult.Text = resultText;
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Model chưa được huấn luyện"))
                    {
                        rtbTestResult.Text = "❌ Lỗi: Mô hình chưa được huấn luyện đúng cách.\n\n" +
                            "Vui lòng thử huấn luyện lại mô hình.";
                    }
                    catch (Exception ex)
                    {
                        rtbTestResult.Text = $"❌ Lỗi khi kiểm tra mô hình: {ex.Message}\n\n" + 
                            $"Chi tiết lỗi:\n{ex.StackTrace}\n\n" +
                            "Vui lòng thử các bước sau:\n" +
                            "1. Huấn luyện lại mô hình\n" +
                            "2. Kiểm tra xem mô hình có lưu đúng không\n" +
                            "3. Đảm bảo dữ liệu nhập đúng định dạng";
                    }
                }
                catch (Exception ex)
                {
                    rtbTestResult.Text = $"❌ Lỗi hệ thống: {ex.Message}\n\n{ex.StackTrace}";
                }
            };

            // Add controls to tab
            tabModelTesting.Controls.AddRange(new Control[] { grpInput, grpResult, btnTestModel });
        }

        private string GetSegmentDescription(uint segmentId)
        {
            // Sample descriptions based on segment ID
            switch (segmentId)
            {
                case 0:
                    return "High-Value Young Customers";
                case 1:
                    return "Conservative High-Income";
                case 2:
                    return "Balanced Middle-Class";
                case 3:
                    return "Premium Mature Customers";
                case 4:
                    return "Young Budget-Conscious";
                default:
                    return "Unknown Segment Type";
            }
        }
        private string ExtractMetrics(string reportContent)
        {
            var lines = reportContent.Split('\n');
            var metrics = "📊 CÁC CHỈ SỐ ĐÁNH GIÁ\n";
            metrics += "========================\n\n";

            foreach (var line in lines)
            {
                if (line.Contains("Silhouette Score:"))
                    metrics += $"🎯 Silhouette Score: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Davies-Bouldin Index:"))
                    metrics += $"📏 Davies-Bouldin Index: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Average Distance:"))
                    metrics += $"📐 Average Distance: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Number of Clusters:"))
                    metrics += $"🔢 Số cấu hình: {line.Split(':')[1].Trim()}\n";
                else if (line.Contains("Overall Score:"))
                    metrics += $"⭐ Điểm tổng thể: {line.Split(':')[1].Trim()}\n";
            }

            metrics += "\n💡 GIẢI THÍCH:\n";
            metrics += "• Silhouette Score: Cao hơn = Tốt hơn (0-1)\n";
            metrics += "• Davies-Bouldin Index: Thấp hơn = Tốt hơn\n";
            metrics += "• Average Distance: Thấp hơn = Tốt hơn\n";

            return metrics;
        }

        private void CreateOverviewSummary(string reportContent)
        {
            var summary = @"
🎯 TÓM TẮT KẾT QUẢ PHÂN CỤM KHÁCH HÀNG
========================================

";

            // Add report content with formatting
            summary += reportContent.Replace("AUTOML TRAINING REPORT", "")
                                  .Replace("=====================", "")
                                  .Trim();

            summary += @"

💼 KHUYẾN NGHỊ KINH DOANH:
• Sử dụng kết quả phân cụm để tạo chiến lược marketing có target
• Phân bổ nguồn lực phù hợp cho từng segment khách hàng
• Theo dõi sự thay đổi hành vi khách hàng qua thời gian
• A/B test các chiến lược khác nhau cho từng segment

📊 BƯỚC TIẾP THEO:
• Xem chi tiết từng segment trong tab 'Phân tích Segments'
• Tạo biểu đồ trực quan trong tab 'Biểu đồ & Trực quan hóa'
• So sánh với các mô hình khác trong tab 'So sánh mô hình'
• Sử dụng mô hình để dự đoán khách hàng mới
";

            rtbOverview.Text = summary;
        }

        private void LoadModelInfo(string modelPath)
        {
            // Additional model information can be loaded here
            // For now, just update the file path info
        }

        private async void LoadSegmentsData()
        {
            try
            {
                System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
                UpdateStatus("Đang tải dữ liệu phân cụm...");
                
                var resultsDir = "Results";
                if (!Directory.Exists(resultsDir))
                {
                    ShowNoResultsMessage();
                    return;
                }

                // Tìm model mới nhất
                var modelFiles = Directory.GetFiles(resultsDir, "*.zip");
                if (modelFiles.Length == 0)
                {
                    ShowNoResultsMessage();
                    return;
                }

                var latestModel = modelFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
                var segmentJsonFile = Path.ChangeExtension(latestModel, ".segments.json");
                var segmentsData = new List<object>();
                
                // Ghi log
                if (File.Exists(segmentJsonFile))
                {
                    var fileInfo = new FileInfo(segmentJsonFile);
                    System.Diagnostics.Debug.WriteLine($"Segment JSON file found: {segmentJsonFile}, Size: {fileInfo.Length} bytes, Created: {fileInfo.CreationTime}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Segment JSON file not found: {segmentJsonFile}");
                }
                
                // Nếu file JSON tồn tại, đọc từ đó trước
                if (File.Exists(segmentJsonFile))
                {
                    try
                    {
                        var json = File.ReadAllText(segmentJsonFile);
                        System.Diagnostics.Debug.WriteLine($"JSON Content length: {json.Length}");
                        System.Diagnostics.Debug.WriteLine($"Sample JSON content: {json.Substring(0, Math.Min(200, json.Length))}");
                        
                        var segments = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<uint, SegmentAnalysis>>(json);
                        
                        // Log để debug
                        int totalCustomers = segments.Values.Sum(s => s.CustomerCount);
                        System.Diagnostics.Debug.WriteLine($"Deserialized {segments.Count} segments with total {totalCustomers} customers");
                        
                        foreach (var segment in segments.OrderBy(s => s.Key))
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
                            
                            System.Diagnostics.Debug.WriteLine($"Added segment {segment.Key}: {segment.Value.CustomerCount} customers ({segment.Value.Percentage:F2}%)");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error parsing JSON: {ex.Message}");
                        MessageBox.Show($"Lỗi khi đọc file segment JSON: {ex.Message}", "Cảnh báo", 
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        segmentsData.Clear();
                    }
                }
                
                // Nếu không có dữ liệu từ file JSON, phân tích lại từ dữ liệu gốc
                if (segmentsData.Count == 0)
                {
                    var dataDir = @"D:\StudyPython\PhanCumkhachHang\CustomerSegmentationML\Data";
                    if (!Directory.Exists(dataDir))
                    {
                        MessageBox.Show("Không tìm thấy thư mục dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
            
                    var dataFiles = Directory.GetFiles(dataDir, "*.csv");
                    if (dataFiles.Length == 0)
                    {
                        MessageBox.Show("Không tìm thấy file dữ liệu CSV.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
            
                    var latestDataFile = dataFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
                    System.Diagnostics.Debug.WriteLine($"Using data file: {latestDataFile}");
            
                    // Tải model và tạo clusterer
                    var clusterer = new KMeansClusterer();
                    await Task.Run(() => clusterer.LoadModel(latestModel));
            
                    // Lấy toàn bộ dữ liệu
                    var segments = await Task.Run(() => clusterer.AnalyzeSegmentsFromFile(latestDataFile, -1));
            
                    foreach (var segment in segments.OrderBy(s => s.Key))
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
                        
                        System.Diagnostics.Debug.WriteLine($"Analyzed segment {segment.Key}: {segment.Value.CustomerCount} customers ({segment.Value.Percentage:F2}%)");
                    }
            
                    // Sau khi phân tích, lưu kết quả để lần sau dùng lại
                    try
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(segments);
                        File.WriteAllText(segmentJsonFile, json);
                        System.Diagnostics.Debug.WriteLine($"Saved segment data to {segmentJsonFile}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving segment data: {ex.Message}");
                    }
                }

                // Hiển thị kết quả
                if (segmentsData.Count == 0)
                {
                    segmentsData = GetSampleSegmentData();
                    MessageBox.Show("Không thể phân tích dữ liệu segment. Hiển thị dữ liệu mẫu.", "Cảnh báo", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                // Đặt lại DataSource
                dgvSegments.DataSource = null;
                dgvSegments.DataSource = segmentsData;
                CreateSegmentDistributionChart(segmentsData);
        
                UpdateStatus("Đã tải xong dữ liệu phân cụm.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu segment: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Lỗi",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
        
                // Fallback to sample data
                var sampleData = GetSampleSegmentData();
                dgvSegments.DataSource = sampleData;
                CreateSegmentDistributionChart(sampleData);
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = Cursors.Default;
            }
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), message);
                return;
            }
            
            // Cập nhật tiêu đề form
            this.Text = $"📊 Kết quả phân tích & Đánh giá mô hình - {message}";
            Application.DoEvents();
        }

        private void DgvSegments_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSegments.SelectedRows.Count > 0)
            {
                var row = dgvSegments.SelectedRows[0];
                var segmentDetails = $@"
🎯 CHI TIẾT SEGMENT {row.Cells["SegmentID"].Value}
====================================

📊 THỐNG KÊ CƠ BẢN:
• Số lượng khách hàng: {row.Cells["CustomerCount"].Value}
• Tỷ lệ: {row.Cells["Percentage"].Value}%
• Mô tả: {row.Cells["Description"].Value}

📈 ĐẶC ĐIỂM TRUNG BÌNH:
• Tuổi: {row.Cells["AvgAge"].Value}
• Thu nhập: ${row.Cells["AvgIncome"].Value}k
• Điểm chi tiêu: {row.Cells["AvgSpending"].Value}/100

💼 KHUYẾN NGHỊ KINH DOANH:
{GenerateBusinessRecommendations(row.Cells["Description"].Value.ToString())}

🎯 CHIẾN LƯỢC MARKETING:
{GenerateMarketingStrategy(row.Cells["Description"].Value.ToString())}
";

                rtbSegmentDetails.Text = segmentDetails;
            }
        }

        private string GenerateBusinessRecommendations(string description)
        {
            switch (description)
            {
                case "High-Value Young Customers":
                    return "• Tập trung vào sản phẩm premium\n• Marketing qua social media\n• Chương trình loyalty đặc biệt";
                case "Conservative High-Income":
                    return "• Sản phẩm chất lượng cao\n• Dịch vụ khách hàng premium\n• Marketing truyền thống";
                case "Balanced Middle-Class":
                    return "• Sản phẩm giá trị tốt\n• Khuyến mãi hợp lý\n• Omnichannel approach";
                case "Premium Mature Customers":
                    return "• Dịch vụ cá nhân hóa\n• Sản phẩm cao cấp\n• Chăm sóc VIP";
                default:
                    return "• Sản phẩm giá rẻ\n• Khuyến mãi mạnh\n• Digital marketing";
            }
        }

        private string GenerateMarketingStrategy(string description)
        {
            switch (description)
            {
                case "High-Value Young Customers":
                    return "• Instagram, TikTok campaigns\n• Influencer partnerships\n• Mobile-first experience";
                case "Conservative High-Income":
                    return "• Email marketing\n• Direct mail\n• Referral programs";
                case "Balanced Middle-Class":
                    return "• Facebook advertising\n• Newsletter campaigns\n• Seasonal promotions";
                case "Premium Mature Customers":
                    return "• Personal consultations\n• Exclusive events\n• Premium catalogs";
                default:
                    return "• Price-focused ads\n• Comparison content\n• Budget-friendly options";
            }
        }

        private void LoadModelComparison()
        {
            // Load comparison data from multiple model results
            var comparisonData = new List<object>
            {
                new { Algorithm = "K-Means", SilhouetteScore = 0.742, DaviesBouldinIndex = 0.845, TrainingTime = 2.3, OverallScore = 0.856 },
                new { Algorithm = "DBSCAN", SilhouetteScore = 0.678, DaviesBouldinIndex = 1.234, TrainingTime = 5.7, OverallScore = 0.723 },
                new { Algorithm = "Hierarchical", SilhouetteScore = 0.701, DaviesBouldinIndex = 0.967, TrainingTime = 12.4, OverallScore = 0.789 }
            };

            dgvModelComparison.DataSource = comparisonData;

            // Create a bar chart for model comparison
            chartModelComparison.Series.Clear();
            chartModelComparison.ChartAreas.Clear();

            var chartArea = new ChartArea("ComparisonArea");
            chartModelComparison.ChartAreas.Add(chartArea);

            var series = new Series("OverallScore")
            {
                ChartType = SeriesChartType.Bar,
                IsValueShownAsLabel = true
            };

            foreach (dynamic model in comparisonData)
            {
                int pointIndex = series.Points.AddXY(model.Algorithm, model.OverallScore);
                series.Points[pointIndex].Label = $"{model.OverallScore:F3}";
            }

            chartModelComparison.Series.Add(series);

            chartModelComparison.Titles.Clear();
            chartModelComparison.Titles.Add(new Title("So sánh Overall Score các mô hình", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.Black));
        }

        private void BtnGenerateChart_Click(object sender, EventArgs e)
{
    // Placeholder logic for generating a chart
    MessageBox.Show("Chart generation logic is not implemented yet.", "Generate Chart", MessageBoxButtons.OK, MessageBoxIcon.Information);
}

        private void ResultsForm_Load(object sender, EventArgs e)
        {

        }

        private async Task<List<object>> GetSegmentDataFromModelAsync(string modelPath, string dataPath)
{
    var segmentsData = new List<object>();
    
    try
    {
        // Hiển thị thanh tiến trình
        using (var progressDialog = new ProgressDialog("Đang phân tích dữ liệu..."))
        {
            progressDialog.Show(this);
            
            await Task.Run(() =>
            {
                try
                {
                    // Cập nhật message
                    this.Invoke(new Action(() => {
                        progressDialog.UpdateMessage("Đang tải mô hình...");
                    }));
                    
                    // Tạo instance của KMeansClusterer
                    var clusterer = new KMeansClusterer();
                    clusterer.LoadModel(modelPath);
                    
                    // Cập nhật message
                    this.Invoke(new Action(() => {
                        progressDialog.UpdateMessage("Đang phân tích dữ liệu...");
                    }));
                    
                    var segments = clusterer.AnalyzeSegmentsFromFile(dataPath, 1000);
                    
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
                    this.Invoke(new Action(() => {
                        progressDialog.UpdateMessage("Đã hoàn tất phân tích!");
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi phân tích dữ liệu: {ex.Message}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            
            progressDialog.Close();
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

        private void CreateSegmentDistributionChart(List<object> segmentsData)
        {
            // Tìm Chart trong GroupBox có text chứa "Phân bố"
            var chartBox = tabSegments.Controls.Cast<Control>()
                .FirstOrDefault(c => c is GroupBox && c.Text.Contains("Phân bố")) as GroupBox;
            
            if (chartBox == null || chartBox.Tag == null)
            {
                MessageBox.Show("Không tìm thấy biểu đồ phân bố.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var chart = chartBox.Tag as Chart;
            if (chart == null)
            {
                MessageBox.Show("Chart không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Titles.Clear();

            var chartArea = new ChartArea("MainArea");
            chart.ChartAreas.Add(chartArea);

            var series = new Series("Segments");
            series.ChartType = SeriesChartType.Pie;
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "{0:F1}%";

            var colors = new Color[] { Color.LightBlue, Color.LightGreen, Color.Orange, Color.Pink, Color.Yellow };

            for (int i = 0; i < segmentsData.Count; i++)
            {
                dynamic segment = segmentsData[i];
                var point = new DataPoint();
                point.SetValueXY($"Segment {segment.SegmentID}", segment.Percentage);
                point.Color = colors[i % colors.Length];
                point.LegendText = $"Segment {segment.SegmentID} ({segment.CustomerCount})";
                series.Points.Add(point);
            }

            chart.Series.Add(series);

            var legend = new Legend("MainLegend");
            legend.Docking = Docking.Right;
            chart.Legends.Add(legend);

            chart.Titles.Add(new Title("Customer Segment Distribution",
                Docking.Top, new Font("Segoe UI", 14, FontStyle.Bold), Color.Black));
        }
    }
}

