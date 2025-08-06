using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CustomerSegmentationML.ML.Algorithms;
using CustomerSegmentationML.Models;
using Microsoft.ML;

namespace CustomerSegmentationML.Forms
{
    public partial class PredictionForm : Form
    {
        private TabControl tabControl;
        private TabPage tabSinglePrediction;
        private TabPage tabBatchPrediction;
        private TabPage tabDetailedResults;
        
        private KMeansClusterer _clusterer;
        private string _modelPath;
        private List<CustomerPrediction> _batchResults;

        public PredictionForm()
        {
            InitializeComponent();
            InitializeCustomUI();
        }

        private void PredictionForm_Load(object sender, EventArgs e)
        {
            LoadLatestModel();
            UpdateModelInfo();
        }

        private void InitializeCustomUI()
        {
            this.Text = "🔮 Dự đoán phân cụm khách hàng";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            // Create main tab control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.Controls.Add(tabControl);

            CreateSinglePredictionTab();
            CreateBatchPredictionTab();
            CreateDetailedResultsTab();
        }

        private void LoadLatestModel()
        {
            var resultsDir = "Results";
            if (!Directory.Exists(resultsDir))
            {
                MessageBox.Show("Không tìm thấy thư mục kết quả. Vui lòng huấn luyện mô hình trước.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var modelFiles = Directory.GetFiles(resultsDir, "*.zip");
            if (modelFiles.Length == 0)
            {
                MessageBox.Show("Không tìm thấy file mô hình đã lưu. Vui lòng huấn luyện mô hình trước.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get most recent model
            _modelPath = modelFiles.OrderByDescending(f => File.GetCreationTime(f)).First();

            // Initialize model
            _clusterer = new KMeansClusterer();
            try
            {
                Cursor = Cursors.WaitCursor;
                _clusterer.LoadModel(_modelPath);
                UpdateModelInfo();
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Lỗi khi tải mô hình: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateModelInfo()
        {
            // Find the model info label in the tabs
            var lblModelInfo = tabSinglePrediction.Controls.Find("lblModelInfo", true).FirstOrDefault() as Label;
            if (lblModelInfo != null)
            {
                lblModelInfo.Text = $"✓ Đã tải mô hình: {Path.GetFileName(_modelPath)}";
            }

            var lblBatchModelInfo = tabBatchPrediction.Controls.Find("lblBatchModelInfo", true).FirstOrDefault() as Label;
            if (lblBatchModelInfo != null)
            {
                lblBatchModelInfo.Text = $"✓ Đã tải mô hình: {Path.GetFileName(_modelPath)}";
            }
        }

        #region Single Prediction Tab
        
        private void CreateSinglePredictionTab()
        {
            tabSinglePrediction = new TabPage("🔍 Dự đoán đơn lẻ");
            tabControl.TabPages.Add(tabSinglePrediction);

            // Customer input panel
            var grpInput = new GroupBox
            {
                Text = "📝 Thông tin khách hàng",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(450, 580)
            };

            // Customer test result panel
            var grpResult = new GroupBox
            {
                Text = "🎯 Kết quả dự đoán",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(490, 20),
                Size = new Size(460, 580)
            };

            // Create input controls
            var y = 30;
            int labelWidth = 150;
            int controlWidth = 200;
            int spacing = 35;

            // Gender
            var lblGender = new Label { Text = "Giới tính:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbGender = new ComboBox { Name = "cmbGender", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new[] { "Female", "Male" });
            cmbGender.SelectedIndex = 0;

            // Age
            y += spacing;
            var lblAge = new Label { Text = "Tuổi:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numAge = new NumericUpDown { Name = "numAge", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 18, Maximum = 70, Value = 30 };

            // Annual Income
            y += spacing;
            var lblIncome = new Label { Text = "Thu nhập (k$):", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numIncome = new NumericUpDown { Name = "numIncome", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 5, Maximum = 150, Value = 50 };

            // Spending Score
            y += spacing;
            var lblSpending = new Label { Text = "Điểm chi tiêu:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numSpending = new NumericUpDown { Name = "numSpending", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 1, Maximum = 100, Value = 50 };

            // Education
            y += spacing;
            var lblEducation = new Label { Text = "Trình độ học vấn:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbEducation = new ComboBox { Name = "cmbEducation", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEducation.Items.AddRange(new[] { "High School", "Bachelor", "Master", "PhD" });
            cmbEducation.SelectedIndex = 1;

            // Profession
            y += spacing;
            var lblProfession = new Label { Text = "Nghề nghiệp:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbProfession = new ComboBox { Name = "cmbProfession", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProfession.Items.AddRange(new[] { "Student", "Healthcare", "Engineer", "Artist", "Lawyer", "Doctor", "Marketing", "Entertainment" });
            cmbProfession.SelectedIndex = 2;  // Default to Engineer

            // Work Experience
            y += spacing;
            var lblWorkExp = new Label { Text = "Kinh nghiệm (năm):", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numWorkExp = new NumericUpDown { Name = "numWorkExp", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 0, Maximum = 40, Value = 5 };

            // Family Size
            y += spacing;
            var lblFamilySize = new Label { Text = "Quy mô gia đình:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numFamilySize = new NumericUpDown { Name = "numFamilySize", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 1, Maximum = 10, Value = 3 };

            // City
            y += spacing;
            var lblCity = new Label { Text = "Thành phố:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbCity = new ComboBox { Name = "cmbCity", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCity.Items.AddRange(new[] { "HaNoi", "HCM", "DaNang", "Others" });
            cmbCity.SelectedIndex = 0;

            // Online Shopping Frequency
            y += spacing;
            var lblShoppingFreq = new Label { Text = "Mua sắm online:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numShoppingFreq = new NumericUpDown { Name = "numShoppingFreq", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 0, Maximum = 30, Value = 5 };

            // Brand Loyalty
            y += spacing;
            var lblBrandLoyalty = new Label { Text = "Lòng trung thành:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numBrandLoyalty = new NumericUpDown { Name = "numBrandLoyalty", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 1, Maximum = 10, Value = 5 };

            // Social Media Usage
            y += spacing;
            var lblSocialMedia = new Label { Text = "Mạng xã hội (giờ/ngày):", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var numSocialMedia = new NumericUpDown { Name = "numSocialMedia", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), Minimum = 0, Maximum = 24, Value = 2, DecimalPlaces = 1 };

            // Preferred Channel
            y += spacing;
            var lblChannel = new Label { Text = "Kênh mua sắm:", Location = new Point(15, y), Size = new Size(labelWidth, 23), Font = new Font("Segoe UI", 9) };
            var cmbChannel = new ComboBox { Name = "cmbChannel", Location = new Point(15 + labelWidth + 10, y), Size = new Size(controlWidth, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbChannel.Items.AddRange(new[] { "Online", "Offline", "Both" });
            cmbChannel.SelectedIndex = 2;

            // Model info label
            y += spacing;
            var lblModelInfo = new Label { Name = "lblModelInfo", Text = "⌛ Chưa tải mô hình", Location = new Point(15, y), Size = new Size(400, 23), Font = new Font("Segoe UI", 9, FontStyle.Italic) };

            // Predict button
            var btnPredict = new Button
            {
                Text = "🔮 Dự đoán phân cụm",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                Location = new Point(15, y + spacing),
                Size = new Size(200, 40)
            };

            // Result panel elements
            var rtbPredictionResult = new RichTextBox
            {
                Name = "rtbPredictionResult",
                Location = new Point(15, 25),
                Size = new Size(430, 380),
                Font = new Font("Segoe UI", 9),
                ReadOnly = true
            };

            // Segment visualization
            var lblSegmentViz = new Label { Text = "Biểu đồ phân cụm:", Location = new Point(15, 415), Size = new Size(150, 23), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            var chartSegment = new Chart
            {
                Name = "chartSegment",
                Location = new Point(15, 440),
                Size = new Size(430, 130),
                BackColor = Color.WhiteSmoke
            };
            
            // Initialize chart
            var chartArea = new ChartArea("SegmentArea");
            chartSegment.ChartAreas.Add(chartArea);
            chartSegment.Titles.Add(new Title("Vị trí khách hàng trong các phân cụm", Docking.Top, new Font("Segoe UI", 10), Color.Black));

            // Add controls to group boxes
            grpInput.Controls.AddRange(new Control[] {
                lblGender, cmbGender,
                lblAge, numAge,
                lblIncome, numIncome,
                lblSpending, numSpending,
                lblEducation, cmbEducation,
                lblProfession, cmbProfession,
                lblWorkExp, numWorkExp,
                lblFamilySize, numFamilySize,
                lblCity, cmbCity,
                lblShoppingFreq, numShoppingFreq,
                lblBrandLoyalty, numBrandLoyalty,
                lblSocialMedia, numSocialMedia,
                lblChannel, cmbChannel,
                lblModelInfo, btnPredict
            });

            grpResult.Controls.AddRange(new Control[] {
                rtbPredictionResult,
                lblSegmentViz,
                chartSegment
            });

            // Add prediction button click handler
            btnPredict.Click += async (sender, e) =>
            {
                await PredictSingleCustomer(
                    cmbGender, numAge, numIncome, numSpending, 
                    cmbEducation, cmbProfession, numWorkExp, 
                    numFamilySize, cmbCity, numShoppingFreq, 
                    numBrandLoyalty, numSocialMedia, cmbChannel,
                    rtbPredictionResult, chartSegment);
            };

            // Add controls to tab
            tabSinglePrediction.Controls.AddRange(new Control[] { grpInput, grpResult });
        }

        private async Task PredictSingleCustomer(
            ComboBox cmbGender, NumericUpDown numAge, NumericUpDown numIncome, NumericUpDown numSpending,
            ComboBox cmbEducation, ComboBox cmbProfession, NumericUpDown numWorkExp,
            NumericUpDown numFamilySize, ComboBox cmbCity, NumericUpDown numShoppingFreq,
            NumericUpDown numBrandLoyalty, NumericUpDown numSocialMedia, ComboBox cmbChannel,
            RichTextBox rtbResult, Chart chart)
        {
            try
            {
                if (_clusterer == null)
                {
                    rtbResult.Text = "❌ Mô hình chưa được tải. Vui lòng tải mô hình trước.";
                    return;
                }

                Cursor = Cursors.WaitCursor;
                rtbResult.Text = "⏳ Đang dự đoán...";
                Application.DoEvents();

                // Create customer data from input
                var customer = new EnhancedCustomerData
                {
                    CustomerID = (float)DateTime.Now.Ticks,
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

                // Predict
                var prediction = await Task.Run(() => _clusterer.PredictAsync(customer));

                // Get segment description and recommendations
                string segmentDescription = GetSegmentDescription(prediction.PredictedClusterId);
                string recommendations = GetRecommendations(prediction.PredictedClusterId);

                // Display prediction result
                var resultText = $@"
🎯 KẾT QUẢ DỰ ĐOÁN PHÂN CỤM
===========================

✅ Phân cụm dự đoán: Segment {prediction.PredictedClusterId}

📊 Mô tả phân cụm:
{segmentDescription}

👤 THÔNG TIN KHÁCH HÀNG:
• Giới tính: {(customer.Gender == 0 ? "Nữ" : "Nam")}
• Tuổi: {customer.Age}
• Thu nhập: ${customer.AnnualIncome}k
• Điểm chi tiêu: {customer.SpendingScore}/100
• Trình độ: {cmbEducation.SelectedItem}
• Nghề nghiệp: {cmbProfession.SelectedItem}
• Kinh nghiệm: {customer.WorkExperience} năm
• Quy mô gia đình: {customer.FamilySize} người

💡 KHUYẾN NGHỊ TIẾP THỊ:
{recommendations}
";

                rtbResult.Text = resultText;
                
                // Update chart
                UpdateSegmentChart(chart, prediction.PredictedClusterId);
                
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                rtbResult.Text = $"❌ Lỗi khi dự đoán: {ex.Message}\n\n{ex.StackTrace}";
            }
        }
        
        private void UpdateSegmentChart(Chart chart, uint segmentId)
        {
            chart.Series.Clear();
            
            // Create series for segments
            var series = new Series("Segments")
            {
                ChartType = SeriesChartType.Bubble
            };

            // Add points for each segment (simplified visualization)
            series.Points.Add(new DataPoint(1, 1) { MarkerSize = 3 }); // Segment 0
            series.Points.Add(new DataPoint(1, 3) { MarkerSize = 3 }); // Segment 1
            series.Points.Add(new DataPoint(3, 2) { MarkerSize = 3 }); // Segment 2
            series.Points.Add(new DataPoint(3, 4) { MarkerSize = 3 }); // Segment 3
            series.Points.Add(new DataPoint(5, 1) { MarkerSize = 3 }); // Segment 4
            
            // Set colors for all segments
            for (int i = 0; i < series.Points.Count; i++)
            {
                if (i == segmentId)
                    series.Points[i].Color = Color.Red;
                else
                    series.Points[i].Color = Color.LightBlue;
                
                series.Points[i].Label = $"S{i}";
            }
            
            chart.Series.Add(series);
            
            // Set chart properties
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = 6;
            chart.ChartAreas[0].AxisY.Minimum = 0;
            chart.ChartAreas[0].AxisY.Maximum = 5;
            chart.ChartAreas[0].AxisX.Title = "Thu nhập";
            chart.ChartAreas[0].AxisY.Title = "Chi tiêu";
        }
        #endregion
        
        #region Batch Prediction Tab
        
        private void CreateBatchPredictionTab()
        {
            tabBatchPrediction = new TabPage("📊 Dự đoán hàng loạt");
            tabControl.TabPages.Add(tabBatchPrediction);

            // File input panel
            var grpFileInput = new GroupBox
            {
                Text = "📂 Dữ liệu đầu vào",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(940, 120)
            };

            // Results panel
            var grpResults = new GroupBox
            {
                Text = "📊 Kết quả dự đoán",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 150),
                Size = new Size(940, 450)
            };
            
            // File input controls
            var lblFilePath = new Label { Text = "Đường dẫn file CSV:", Location = new Point(15, 30), Size = new Size(120, 23), Font = new Font("Segoe UI", 9) };
            var txtFilePath = new TextBox { Name = "txtFilePath", Location = new Point(140, 30), Size = new Size(600, 23), ReadOnly = true };
            var btnBrowse = new Button { Text = "Chọn file...", Location = new Point(750, 30), Size = new Size(100, 23) };
            
            // Model info and predict button
            var lblBatchModelInfo = new Label { Name = "lblBatchModelInfo", Text = "⌛ Chưa tải mô hình", Location = new Point(15, 70), Size = new Size(400, 23), Font = new Font("Segoe UI", 9, FontStyle.Italic) };
            var btnBatchPredict = new Button 
            { 
                Text = "🔮 Dự đoán hàng loạt", 
                Location = new Point(750, 65), 
                Size = new Size(150, 30),
                BackColor = Color.LightGreen,
                Enabled = false
            };
            
            // Results grid
            var dgvResults = new DataGridView
            {
                Name = "dgvResults",
                Location = new Point(15, 25),
                Size = new Size(910, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            
            // Export button
            var btnExport = new Button
            {
                Text = "📥 Xuất kết quả",
                Location = new Point(800, 390),
                Size = new Size(125, 30),
                Enabled = false
            };
            
            // Summary label
            var lblSummary = new Label { Name = "lblSummary", Text = "", Location = new Point(15, 390), Size = new Size(600, 40), Font = new Font("Segoe UI", 9) };
            
            // Add controls to group boxes
            grpFileInput.Controls.AddRange(new Control[] {
                lblFilePath, txtFilePath, btnBrowse,
                lblBatchModelInfo, btnBatchPredict
            });
            
            grpResults.Controls.AddRange(new Control[] {
                dgvResults, btnExport, lblSummary
            });
            
            // Browse button click handler
            btnBrowse.Click += (sender, e) =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Chọn file dữ liệu khách hàng"
                };
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                    btnBatchPredict.Enabled = true;
                }
            };
            
            // Batch predict button click handler
            btnBatchPredict.Click += async (sender, e) =>
            {
                await PredictBatch(txtFilePath.Text, dgvResults, lblSummary, btnExport);
            };
            
            // Export button click handler
            btnExport.Click += (sender, e) =>
            {
                ExportBatchResults(dgvResults);
            };
            
            // Add controls to tab
            tabBatchPrediction.Controls.AddRange(new Control[] { grpFileInput, grpResults });
        }
        
        private async Task PredictBatch(string filePath, DataGridView dgvResults, Label lblSummary, Button btnExport)
        {
            try
            {
                if (_clusterer == null)
                {
                    MessageBox.Show("Mô hình chưa được tải. Vui lòng tải mô hình trước.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    MessageBox.Show("Vui lòng chọn file CSV hợp lệ.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Cursor = Cursors.WaitCursor;
                lblSummary.Text = "⏳ Đang xử lý dự đoán hàng loạt...";
                Application.DoEvents();

                // Load and process the CSV file
                var customers = await Task.Run(() => LoadCustomersFromCSV(filePath));
                
                // Do predictions
                _batchResults = new List<CustomerPrediction>();
                var results = new List<BatchResultRow>();
                
                foreach (var customer in customers)
                {
                    var prediction = await Task.Run(() => _clusterer.PredictAsync(customer));
                    _batchResults.Add(prediction);
                    
                    results.Add(new BatchResultRow
                    {
                        CustomerID = (long)customer.CustomerID,
                        Gender = customer.Gender == 0 ? "Female" : "Male",
                        Age = customer.Age,
                        Income = customer.AnnualIncome,
                        SpendingScore = customer.SpendingScore,
                        Segment = prediction.PredictedClusterId,
                        SegmentDescription = GetSegmentDescription(prediction.PredictedClusterId)
                    });
                }
                
                // Display results in grid
                dgvResults.DataSource = results;
                
                // Update summary
                var segmentCounts = results.GroupBy(r => r.Segment)
                    .Select(g => new { Segment = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Segment)
                    .ToList();
                
                var summaryText = $"✅ Đã phân tích {results.Count} khách hàng. Phân bố cụm: ";
                summaryText += string.Join(", ", segmentCounts.Select(s => $"S{s.Segment}: {s.Count}"));
                
                lblSummary.Text = summaryText;
                btnExport.Enabled = true;
                
                Cursor = Cursors.Default;
                
                // Update detailed results tab
                UpdateDetailedResultsTab(results);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Lỗi khi dự đoán hàng loạt: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private List<EnhancedCustomerData> LoadCustomersFromCSV(string filePath)
        {
            var customers = new List<EnhancedCustomerData>();
            
            // This is a simplified implementation
            // In a real application, you would use a proper CSV parser
            var lines = File.ReadAllLines(filePath);
            var headers = lines[0].Split(',');
            
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                if (values.Length < 10) continue; // Skip invalid rows
                
                try
                {
                    var customer = new EnhancedCustomerData
                    {
                        CustomerID = long.Parse(values[0]),
                        Gender = values[1].Trim().ToLower() == "female" ? 0f : 1f,
                        Age = float.Parse(values[2]),
                        AnnualIncome = float.Parse(values[3]),
                        SpendingScore = float.Parse(values[4]),
                        Education = ParseEducation(values[5]),
                        Profession = ParseProfession(values[6]),
                        WorkExperience = float.Parse(values[7]),
                        FamilySize = float.Parse(values[8]),
                        City = ParseCity(values[9]),
                        OnlineShoppingFreq = values.Length > 10 ? float.Parse(values[10]) : 5f,
                        BrandLoyalty = values.Length > 11 ? float.Parse(values[11]) : 5f,
                        SocialMediaUsage = values.Length > 12 ? float.Parse(values[12]) : 2f,
                        PreferredChannel = values.Length > 13 ? ParseChannel(values[13]) : 2f
                    };
                    
                    customers.Add(customer);
                }
                catch
                {
                    // Skip rows with parsing errors
                }
            }
            
            return customers;
        }
        
        private float ParseEducation(string education)
        {
            education = education.Trim().ToLower();
            if (education.Contains("high school")) return 0;
            if (education.Contains("bachelor")) return 1;
            if (education.Contains("master")) return 2;
            if (education.Contains("phd")) return 3;
            return 1; // Default to Bachelor
        }
        
        private float ParseProfession(string profession)
        {
            profession = profession.Trim().ToLower();
            if (profession.Contains("student")) return 0;
            if (profession.Contains("healthcare")) return 1;
            if (profession.Contains("engineer")) return 2;
            if (profession.Contains("artist")) return 3;
            if (profession.Contains("lawyer")) return 4;
            if (profession.Contains("doctor")) return 5;
            if (profession.Contains("marketing")) return 6;
            if (profession.Contains("entertainment")) return 7;
            return 2; // Default to Engineer
        }
        
        private float ParseCity(string city)
        {
            city = city.Trim().ToLower();
            if (city.Contains("hanoi")) return 0;
            if (city.Contains("hcm")) return 1;
            if (city.Contains("danang")) return 2;
            return 3; // Default to Others
        }
        
        private float ParseChannel(string channel)
        {
            channel = channel.Trim().ToLower();
            if (channel.Contains("online")) return 0;
            if (channel.Contains("offline")) return 1;
            return 2; // Default to Both
        }
        
        private void ExportBatchResults(DataGridView dgvResults)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Title = "Xuất kết quả dự đoán"
            };
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    
                    if (saveFileDialog.FileName.EndsWith(".csv"))
                    {
                        // Export to CSV
                        using (var writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            // Write headers
                            var headers = new List<string>();
                            foreach (DataGridViewColumn column in dgvResults.Columns)
                            {
                                headers.Add(column.HeaderText);
                            }
                            writer.WriteLine(string.Join(",", headers));
                            
                            // Write rows
                            foreach (DataGridViewRow row in dgvResults.Rows)
                            {
                                var cells = new List<string>();
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    cells.Add(cell.Value?.ToString() ?? "");
                                }
                                writer.WriteLine(string.Join(",", cells));
                            }
                        }
                    }
                    else
                    {
                        // For Excel export, you would typically use a library like EPPlus or NPOI
                        MessageBox.Show("Chức năng xuất file Excel sẽ được bổ sung trong phiên bản sau.",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    
                    Cursor = Cursors.Default;
                    MessageBox.Show("Xuất kết quả thành công!",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show($"Lỗi khi xuất kết quả: {ex.Message}",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private class BatchResultRow
        {
            public long CustomerID { get; set; }
            public string Gender { get; set; }
            public float Age { get; set; }
            public float Income { get; set; }
            public float SpendingScore { get; set; }
            public uint Segment { get; set; }
            public string SegmentDescription { get; set; }
        }
        #endregion
        
        #region Detailed Results Tab
        
        private void CreateDetailedResultsTab()
        {
            tabDetailedResults = new TabPage("📈 Kết quả chi tiết");
            tabControl.TabPages.Add(tabDetailedResults);
            
            // Create main layout
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };
            
            // Top panel - Segment overview
            var grpSegmentOverview = new GroupBox
            {
                Text = "📊 Tổng quan phân cụm",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            
            // Bottom panel - Segment details
            var grpSegmentDetails = new GroupBox
            {
                Text = "🔍 Chi tiết phân cụm",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            
            // Segment overview chart
            var chartOverview = new Chart
            {
                Name = "chartOverview",
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };
            
            // Initialize chart
            var chartArea = new ChartArea("OverviewArea");
            chartOverview.ChartAreas.Add(chartArea);
            chartOverview.Titles.Add(new Title("Phân bố khách hàng theo phân cụm", Docking.Top, new Font("Segoe UI", 12), Color.Black));
            
            // Segment details controls
            var lblSelectSegment = new Label
            {
                Text = "Chọn phân cụm:",
                Location = new Point(15, 25),
                Size = new Size(100, 23),
                Font = new Font("Segoe UI", 9)
            };
            
            var cmbSegment = new ComboBox
            {
                Name = "cmbSegment",
                Location = new Point(120, 25),
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Thêm dòng này để tránh lỗi khi không có dữ liệu
            if (_batchResults == null || _batchResults.Count == 0)
            {
                cmbSegment.Items.Add("Chưa có dữ liệu phân cụm");
                cmbSegment.Enabled = false; // Vô hiệu hóa combobox nếu không có dữ liệu
            }
            else
            {
                cmbSegment.Items.AddRange(new[] { "Segment 0", "Segment 1", "Segment 2", "Segment 3", "Segment 4" });
                cmbSegment.SelectedIndex = 0;
            }

            var lblSegmentStats = new Label
            {
                Name = "lblSegmentStats",
                Location = new Point(300, 25),
                Size = new Size(600, 23),
                Font = new Font("Segoe UI", 9)
            };
            
            // Detail charts panel
            var pnlDetailCharts = new Panel
            {
                Location = new Point(15, 60),
                Size = new Size(910, 270),
                BorderStyle = BorderStyle.None
            };
            
            // Create detail charts
            var chartAgeIncome = new Chart
            {
                Name = "chartAgeIncome",
                Location = new Point(0, 0),
                Size = new Size(450, 270),
                BackColor = Color.WhiteSmoke
            };
            
            var chartAreaAgeIncome = new ChartArea("AgeIncomeArea");
            chartAgeIncome.ChartAreas.Add(chartAreaAgeIncome);
            chartAgeIncome.Titles.Add(new Title("Tuổi vs Thu nhập", Docking.Top, new Font("Segoe UI", 10), Color.Black));
            
            var chartSpendingLoyalty = new Chart
            {
                Name = "chartSpendingLoyalty",
                Location = new Point(460, 0),
                Size = new Size(450, 270),
                BackColor = Color.WhiteSmoke
            };
            
            var chartAreaSpendingLoyalty = new ChartArea("SpendingLoyaltyArea");
            chartSpendingLoyalty.ChartAreas.Add(chartAreaSpendingLoyalty);
            chartSpendingLoyalty.Titles.Add(new Title("Chi tiêu vs Lòng trung thành", Docking.Top, new Font("Segoe UI", 10), Color.Black));
            
            // Add charts to panel
            pnlDetailCharts.Controls.AddRange(new Control[] { chartAgeIncome, chartSpendingLoyalty });
            
            // Add controls to group boxes
            grpSegmentOverview.Controls.Add(chartOverview);
            grpSegmentDetails.Controls.AddRange(new Control[] { 
                lblSelectSegment, cmbSegment, lblSegmentStats, pnlDetailCharts 
            });
            
            // Add group boxes to split container
            splitContainer.Panel1.Controls.Add(grpSegmentOverview);
            splitContainer.Panel2.Controls.Add(grpSegmentDetails);
            
            // Add split container to tab
            tabDetailedResults.Controls.Add(splitContainer);
            
            // Add segment selection handler
            cmbSegment.SelectedIndexChanged += (sender, e) =>
            {
                if (_batchResults != null && _batchResults.Count > 0)
                {
                    UpdateDetailCharts(
                        cmbSegment.SelectedIndex,
                        chartAgeIncome,
                        chartSpendingLoyalty,
                        lblSegmentStats
                    );
                }
            };
        }
        
        private void UpdateDetailedResultsTab(List<BatchResultRow> results)
        {
            // Get the overview chart
            var chartOverview = tabDetailedResults.Controls[0].Controls[0].Controls[0] as Chart;
            if (chartOverview != null)
            {
                UpdateOverviewChart(chartOverview, results);
            }
            
            // Get the segment dropdown and select the first segment
            var cmbSegment = tabDetailedResults.Controls[0].Controls[1].Controls.Find("cmbSegment", true).FirstOrDefault() as ComboBox;
            if (cmbSegment != null)
            {
                cmbSegment.SelectedIndex = 0;
            }
        }
        
        void UpdateOverviewChart(Chart chart, List<BatchResultRow> results)
        {
            chart.Series.Clear();

            // Create series for pie chart
            var pieSeries = new Series("Segments")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "{0:P1}"
            };

            // Group results by segment
            var segmentGroups = results.GroupBy(r => r.Segment)
                .Select(g => new { Segment = g.Key, Count = g.Count() })
                .OrderBy(x => x.Segment)
                .ToList();

            // Set colors for segments
            var colors = new Color[] { Color.LightBlue, Color.LightGreen, Color.Orange, Color.Pink, Color.Yellow };

            // Add data points
            for (int i = 0; i < segmentGroups.Count; i++)
            {
                var group = segmentGroups[i];
                int dataPoint = pieSeries.Points.AddY(group.Count);
                pieSeries.Points[dataPoint].LegendText = $"Segment {group.Segment} ({group.Count})";
                pieSeries.Points[dataPoint].Label = $"{(double)group.Count / results.Count:P1}";
                pieSeries.Points[dataPoint].Color = colors[i % colors.Length];
            }

            chart.Series.Add(pieSeries);

            // Add legend
            chart.Legends.Clear();
            var legend = new Legend("MainLegend") { Docking = Docking.Right };
            chart.Legends.Add(legend);
        }
        
        private void UpdateDetailCharts(int segmentIndex, Chart chartAgeIncome, Chart chartSpendingLoyalty, Label lblStats)
        {
            if (_batchResults == null || _batchResults.Count == 0) return;
            
            // Get customers in the selected segment
            var segmentCustomers = _batchResults
                .Where(r => r.PredictedClusterId == segmentIndex)
                .ToList();
            
            // Update segment stats label
            lblStats.Text = $"Số lượng khách hàng trong phân cụm: {segmentCustomers.Count} " +
                            $"({(double)segmentCustomers.Count / _batchResults.Count:P1} của tổng số)";
            
            // Update Age vs Income chart
            chartAgeIncome.Series.Clear();
            
            var seriesAgeIncome = new Series("AgeIncome")
            {
                ChartType = SeriesChartType.Point,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                Color = GetSegmentColor(segmentIndex)
            };
            
            // Assuming we have access to the original customer data
            // In a real implementation, you might need to store this data
            // For now, we'll generate some random points
            Random rand = new Random(segmentIndex);
            for (int i = 0; i < 50; i++)
            {
                double age = 20 + rand.NextDouble() * 40; // Age between 20-60
                double income = 20 + rand.NextDouble() * 80; // Income between 20-100k
                seriesAgeIncome.Points.AddXY(age, income);
            }
            
            chartAgeIncome.Series.Add(seriesAgeIncome);
            chartAgeIncome.ChartAreas[0].AxisX.Title = "Tuổi";
            chartAgeIncome.ChartAreas[0].AxisY.Title = "Thu nhập (k$)";
            
            // Update Spending vs Loyalty chart
            chartSpendingLoyalty.Series.Clear();
            
            var seriesSpendingLoyalty = new Series("SpendingLoyalty")
            {
                ChartType = SeriesChartType.Point,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                Color = GetSegmentColor(segmentIndex)
            };
            
            for (int i = 0; i < 50; i++)
            {
                double spending = 10 + rand.NextDouble() * 90; // Spending between 10-100
                double loyalty = 1 + rand.NextDouble() * 9; // Loyalty between 1-10
                seriesSpendingLoyalty.Points.AddXY(spending, loyalty);
            }
            
            chartSpendingLoyalty.Series.Add(seriesSpendingLoyalty);
            chartSpendingLoyalty.ChartAreas[0].AxisX.Title = "Điểm chi tiêu";
            chartSpendingLoyalty.ChartAreas[0].AxisY.Title = "Lòng trung thành";
        }
        
        private Color GetSegmentColor(int segmentIndex)
        {
            var colors = new Color[] { Color.LightBlue, Color.LightGreen, Color.Orange, Color.Pink, Color.Yellow };
            return colors[segmentIndex % colors.Length];
        }
        #endregion

        #region Helper Methods
        
        private string GetSegmentDescription(uint segmentId)
        {
            switch (segmentId)
            {
                case 0:
                    return "High-Value Young Customers: Thu nhập cao, chi tiêu nhiều, độ tuổi 25-35.";
                case 1:
                    return "Conservative High-Income: Thu nhập cao, chi tiêu thận trọng, độ tuổi 40+.";
                case 2:
                    return "Balanced Middle-Class: Thu nhập và chi tiêu trung bình, độ tuổi đa dạng.";
                case 3:
                    return "Premium Mature Customers: Thu nhập cao, chi tiêu cao, độ tuổi 45+.";
                case 4:
                    return "Young Budget-Conscious: Thu nhập thấp, chi tiêu thấp, độ tuổi dưới 30.";
                default:
                    return "Unknown Segment Type";
            }
        }
        
        private string GetRecommendations(uint segmentId)
        {
            switch (segmentId)
            {
                case 0: // High-Value Young
                    return "• Tiếp thị sản phẩm cao cấp qua kênh social media\n" +
                           "• Chương trình loyalty với trải nghiệm độc đáo\n" +
                           "• Truyền thông nhấn mạnh đến thiết kế và trải nghiệm";
                case 1: // Conservative High-Income
                    return "• Tập trung vào giá trị bền vững của sản phẩm\n" +
                           "• Chương trình VIP với dịch vụ cá nhân hóa\n" +
                           "• Tiếp thị qua kênh truyền thống và email";
                case 2: // Balanced Middle-Class
                    return "• Nhấn mạnh tính giá trị của sản phẩm\n" +
                           "• Chương trình khuyến mãi linh hoạt\n" +
                           "• Sử dụng đa kênh để tiếp cận";
                case 3: // Premium Mature
                    return "• Dịch vụ VIP với tư vấn cá nhân hóa\n" +
                           "• Tiếp thị nhấn mạnh chất lượng và độc quyền\n" +
                           "• Tổ chức các sự kiện đặc biệt cho nhóm khách hàng này";
                case 4: // Young Budget-Conscious
                    return "• Tập trung vào các chương trình giảm giá và trả góp\n" +
                           "• Tiếp thị qua social media với nội dung về giá trị\n" +
                           "• Phát triển các dòng sản phẩm giá rẻ nhưng thời trang";
                default:
                    return "Chưa có khuyến nghị cho phân cụm này.";
            }
        }
        #endregion
    }
}