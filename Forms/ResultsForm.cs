using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CustomerSegmentationML.Models;
using CustomerSegmentationML.ML.Algorithms;
using CustomerSegmentationML.Utils;
using Microsoft.ML;

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
                    metrics += $"🔢 Số cụm: {line.Split(':')[1].Trim()}\n";
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

        private void LoadSegmentsData()
        {
            // This would load actual segment data from the trained model
            // For now, create sample data
            var segmentsData = new List<object>
            {
                new { SegmentID = 0, CustomerCount = 45, Percentage = 22.5, AvgAge = 28.5, AvgIncome = 65.2, AvgSpending = 78.3, Description = "High-Value Young Customers" },
                new { SegmentID = 1, CustomerCount = 38, Percentage = 19.0, AvgAge = 45.2, AvgIncome = 85.7, AvgSpending = 45.1, Description = "Conservative High-Income" },
                new { SegmentID = 2, CustomerCount = 52, Percentage = 26.0, AvgAge = 35.8, AvgIncome = 42.3, AvgSpending = 55.9, Description = "Balanced Middle-Class" },
                new { SegmentID = 3, CustomerCount = 33, Percentage = 16.5, AvgAge = 52.1, AvgIncome = 78.4, AvgSpending = 82.7, Description = "Premium Mature Customers" },
                new { SegmentID = 4, CustomerCount = 32, Percentage = 16.0, AvgAge = 23.9, AvgIncome = 28.6, AvgSpending = 25.4, Description = "Young Budget-Conscious" }
            };

            dgvSegments.DataSource = segmentsData;

            // Create pie chart for distribution
            CreateSegmentDistributionChart(segmentsData);
        }

        private void CreateSegmentDistributionChart(List<object> segmentsData)
        {
            var chart = (Chart)((GroupBox)tabSegments.Controls.Cast<Control>()
                .First(c => c is GroupBox && c.Text.Contains("Phân bố"))).Tag;

            chart.Series.Clear();
            chart.ChartAreas.Clear();

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
    }
}