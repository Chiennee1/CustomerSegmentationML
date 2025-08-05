using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomerSegmentationML.Models;
using CustomerSegmentationML.ML.Algorithms;
using CustomerSegmentationML.Utils;
using Microsoft.ML;

namespace CustomerSegmentationML.Forms
{
    public partial class PredictionForm : Form
    {
        private IClusteringAlgorithm _trainedModel;
        private Dictionary<uint, SegmentAnalysis> _segments;
        private MLContext _mlContext;

        // Controls for single prediction
        private TabControl tabControl;
        private TabPage tabSinglePrediction;
        private TabPage tabBatchPrediction;
        private TabPage tabResults;

        // Single prediction controls
        private GroupBox grpCustomerInfo;
        private NumericUpDown numAge;
        private NumericUpDown numIncome;
        private NumericUpDown numSpendingScore;
        private ComboBox cmbGender;
        private ComboBox cmbEducation;
        private ComboBox cmbProfession;
        private NumericUpDown numWorkExperience;
        private NumericUpDown numFamilySize;
        private ComboBox cmbCity;
        private NumericUpDown numOnlineShoppingFreq;
        private NumericUpDown numBrandLoyalty;
        private NumericUpDown numSocialMediaUsage;
        private ComboBox cmbPreferredChannel;

        private Button btnPredict;
        private GroupBox grpPredictionResult;
        private RichTextBox rtbPredictionResult;

        // Batch prediction controls
        private DataGridView dgvBatchInput;
        private Button btnAddCustomer;
        private Button btnRemoveCustomer;
        private Button btnPredictBatch;
        private Button btnImportCSV;
        private Button btnExportResults;

        // Results controls
        private DataGridView dgvResults;
        private Panel chartPanel;

        public PredictionForm()
        {
            InitializeComponent();
            InitializeCustomUI();
            LoadTrainedModel();
        }

        private void PredictionForm_Load(object sender, EventArgs e)
        {
            InitializeCustomUI();
            LoadTrainedModel();
            // Add any initialization logic here if needed
        }

        private void InitializeCustomUI()
        {
            this.Text = "🎯 Dự đoán phân cụm khách hàng";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.WindowState = FormWindowState.Maximized;

            _mlContext = new MLContext(seed: 0);

            // Create tab control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            this.Controls.Add(tabControl);

            CreateSinglePredictionTab();
            CreateBatchPredictionTab();
            CreateResultsTab();

            tabControl.SelectedIndex = 0;
        }

        private void CreateSinglePredictionTab()
        {
            tabSinglePrediction = new TabPage("🎯 Dự đoán đơn lẻ");
            tabControl.TabPages.Add(tabSinglePrediction);

            // Customer info group
            grpCustomerInfo = new GroupBox
            {
                Text = "📝 Thông tin khách hàng",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(550, 450)
            };

            // Create input controls in a more organized layout
            CreateCustomerInputControls();

            // Prediction button
            btnPredict = new Button();
            btnPredict.Text = "🚀 Dự đoán phân cụm";
            btnPredict.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnPredict.BackColor = Color.LightGreen;    
            btnPredict.Location = new Point(20, 480);
            btnPredict.Size = new Size(200, 40);
            btnPredict.Click += BtnPredict_Click;

            // Prediction result group
            grpPredictionResult = new GroupBox
            {
                Text = "📊 Kết quả dự đoán"
            };
            grpPredictionResult.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grpPredictionResult.Location = new Point(590, 20);
            grpPredictionResult.Size = new Size(580, 500);

            rtbPredictionResult = new RichTextBox();
            rtbPredictionResult.Location = new Point(15, 25);
            rtbPredictionResult.Size = new Size(550, 460);
            rtbPredictionResult.Font = new Font("Segoe UI", 9);
            rtbPredictionResult.ReadOnly = true;
            grpPredictionResult.Controls.Add(rtbPredictionResult);

            tabSinglePrediction.Controls.AddRange(new Control[] {
                grpCustomerInfo, btnPredict, grpPredictionResult
            });
        }

        private void CreateCustomerInputControls()
        {
            int y = 30;
            int labelWidth = 120;
            int controlWidth = 200;
            int spacing = 35;

            // Gender
            AddLabelAndControl("Giới tính:", y, labelWidth, controlWidth,
                cmbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
            cmbGender.Items.AddRange(new[] { "Female", "Male" });
            cmbGender.SelectedIndex = 0;

            // Age
            y += spacing;
            AddLabelAndControl("Tuổi:", y, labelWidth, controlWidth,
                numAge = new NumericUpDown { Minimum = 18, Maximum = 70, Value = 30 });

            // Annual Income
            y += spacing;
            AddLabelAndControl("Thu nhập (k$):", y, labelWidth, controlWidth,
                numIncome = new NumericUpDown { Minimum = 5, Maximum = 150, Value = 50 });

            // Spending Score
            y += spacing;
            AddLabelAndControl("Điểm chi tiêu:", y, labelWidth, controlWidth,
                numSpendingScore = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 50 });

            // Education
            y += spacing;
            AddLabelAndControl("Trình độ học vấn:", y, labelWidth, controlWidth,
                cmbEducation = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
            cmbEducation.Items.AddRange(new[] { "High School", "Bachelor", "Master", "PhD" });
            cmbEducation.SelectedIndex = 1;

            // Profession
            y += spacing;
            AddLabelAndControl("Nghề nghiệp:", y, labelWidth, controlWidth,
                cmbProfession = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
            cmbProfession.Items.AddRange(new[] {
                "Student", "Healthcare", "Engineer", "Artist",
                "Lawyer", "Doctor", "Marketing", "Entertainment"
            });
            cmbProfession.SelectedIndex = 2;

            // Work Experience
            y += spacing;
            AddLabelAndControl("Kinh nghiệm (năm):", y, labelWidth, controlWidth,
                numWorkExperience = new NumericUpDown { Minimum = 0, Maximum = 40, Value = 5 });

            // Family Size
            y += spacing;
            AddLabelAndControl("Quy mô gia đình:", y, labelWidth, controlWidth,
                numFamilySize = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 3 });

            // City
            y += spacing;
            AddLabelAndControl("Thành phố:", y, labelWidth, controlWidth,
                cmbCity = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
            cmbCity.Items.AddRange(new[] { "HaNoi", "HCM", "DaNang", "Others" });
            cmbCity.SelectedIndex = 0;

            // Online Shopping Frequency
            y += spacing;
            AddLabelAndControl("Mua sắm online/tháng:", y, labelWidth, controlWidth,
                numOnlineShoppingFreq = new NumericUpDown { Minimum = 0, Maximum = 30, Value = 5 });

            // Brand Loyalty
            y += spacing;
            AddLabelAndControl("Lòng trung thành (1-10):", y, labelWidth, controlWidth,
                numBrandLoyalty = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 5 });

            // Social Media Usage
            y += spacing;
            AddLabelAndControl("Sử dụng mạng xã hội (h/ngày):", y, labelWidth, controlWidth,
                numSocialMediaUsage = new NumericUpDown { Minimum = 0, Maximum = 24, Value = 2, DecimalPlaces = 1 });

            // Preferred Channel
            y += spacing;
            AddLabelAndControl("Kênh ưa thích:", y, labelWidth, controlWidth,
                cmbPreferredChannel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList });
            cmbPreferredChannel.Items.AddRange(new[] { "Online", "Offline", "Both" });
            cmbPreferredChannel.SelectedIndex = 2;
        }

        private void AddLabelAndControl(string labelText, int y, int labelWidth, int controlWidth, Control control)
        {
            var label = new Label();
            label.Text = labelText;
            label.Location = new Point(15, y);
            label.Size = new Size(labelWidth, 23);
            label.Font = new Font("Segoe UI", 9);

            control.Location = new Point(15 + labelWidth + 10, y);
            control.Size = new Size(controlWidth, 23);

            grpCustomerInfo.Controls.AddRange(new Control[] { label, control });
        }

        private void CreateBatchPredictionTab()
        {
            tabBatchPrediction = new TabPage("📊 Dự đoán hàng loạt");
            tabControl.TabPages.Add(tabBatchPrediction);

            // Batch input grid
            dgvBatchInput = new DataGridView();
            dgvBatchInput.Location = new Point(20, 60);
            dgvBatchInput.Size = new Size(1150, 300);
            dgvBatchInput.AutoGenerateColumns = true;
            dgvBatchInput.AllowUserToAddRows = true;

            // Buttons for batch operations
            var btnPanel = new Panel();
            btnPanel.Location = new Point(20, 20);
            btnPanel.Size = new Size(1150, 35);

            btnAddCustomer = new Button { Text = "➕ Thêm khách hàng", Size = new Size(150, 30) };
            btnAddCustomer.Click += BtnAddCustomer_Click;

            btnRemoveCustomer = new Button { Text = "➖ Xóa khách hàng", Location = new Point(160, 0), Size = new Size(150, 30) };
            btnRemoveCustomer.Click += BtnRemoveCustomer_Click;

            btnImportCSV = new Button { Text = "📁 Import CSV", Location = new Point(320, 0), Size = new Size(120, 30) };
            btnImportCSV.Click += BtnImportCSV_Click;

            btnPredictBatch = new Button
            {
                Text = "🚀 Dự đoán tất cả",
                Location = new Point(450, 0),
                Size = new Size(150, 30),
                BackColor = Color.LightBlue
            };
            btnPredictBatch.Click += BtnPredictBatch_Click;

            btnExportResults = new Button { Text = "💾 Export kết quả", Location = new Point(610, 0), Size = new Size(150, 30) };
            btnExportResults.Click += BtnExportResults_Click;

            btnPanel.Controls.AddRange(new Control[] {
                btnAddCustomer, btnRemoveCustomer, btnImportCSV, btnPredictBatch, btnExportResults
            });

            tabBatchPrediction.Controls.AddRange(new Control[] { btnPanel, dgvBatchInput });

            // Initialize batch data grid
            InitializeBatchDataGrid();
        }

        private void CreateResultsTab()
        {
            tabResults = new TabPage("📈 Kết quả chi tiết");
            tabControl.TabPages.Add(tabResults);

            dgvResults = new DataGridView();
            dgvResults.Location = new Point(20, 20);
            dgvResults.Size = new Size(1150, 400);
            dgvResults.ReadOnly = true;
            dgvResults.AutoGenerateColumns = true;

            chartPanel = new Panel();
            chartPanel.Location = new Point(20, 440);
            chartPanel.Size = new Size(1150, 300);
            chartPanel.BorderStyle = BorderStyle.FixedSingle;

            tabResults.Controls.AddRange(new Control[] { dgvResults, chartPanel });
        }

        private void InitializeBatchDataGrid()
        {
            var dt = new DataTable();
            dt.Columns.Add("Gender", typeof(string));
            dt.Columns.Add("Age", typeof(int));
            dt.Columns.Add("AnnualIncome", typeof(int));
            dt.Columns.Add("SpendingScore", typeof(int));
            dt.Columns.Add("Education", typeof(string));
            dt.Columns.Add("Profession", typeof(string));
            dt.Columns.Add("WorkExperience", typeof(int));
            dt.Columns.Add("FamilySize", typeof(int));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("OnlineShoppingFreq", typeof(int));
            dt.Columns.Add("BrandLoyalty", typeof(int));
            dt.Columns.Add("SocialMediaUsage", typeof(float));
            dt.Columns.Add("PreferredChannel", typeof(string));

            dgvBatchInput.DataSource = dt;
        }

        private async void BtnPredict_Click(object sender, EventArgs e)
        {
            if (_trainedModel == null)
            {
                MessageBox.Show("Không tìm thấy mô hình đã huấn luyện. Vui lòng huấn luyện mô hình trước!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnPredict.Enabled = false;
                rtbPredictionResult.Text = "Đang dự đoán...";

                var customer = CreateEnhancedCustomerFromInput();
                var prediction = await _trainedModel.PredictAsync(customer);

                DisplayPredictionResult(customer, prediction);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi dự đoán: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPredict.Enabled = true;
            }
        }

        private EnhancedCustomerData CreateEnhancedCustomerFromInput()
        {
            return new EnhancedCustomerData
            {
                CustomerID = DateTime.Now.Ticks, // Temporary ID
                Gender = cmbGender.SelectedItem.ToString() == "Female" ? 0f : 1f,
                Age = (float)numAge.Value,
                AnnualIncome = (float)numIncome.Value,
                SpendingScore = (float)numSpendingScore.Value,
                Education = (float)cmbEducation.SelectedIndex,
                Profession = (float)cmbProfession.SelectedIndex,
                WorkExperience = (float)numWorkExperience.Value,
                FamilySize = (float)numFamilySize.Value,
                City = (float)cmbCity.SelectedIndex,
                OnlineShoppingFreq = (float)numOnlineShoppingFreq.Value,
                BrandLoyalty = (float)numBrandLoyalty.Value,
                SocialMediaUsage = (float)numSocialMediaUsage.Value,
                PreferredChannel = (float)cmbPreferredChannel.SelectedIndex
            };
        }

        private void DisplayPredictionResult(EnhancedCustomerData customer, CustomerPrediction prediction)
        {
            var result = $@"🎯 KẾT QUẢ DỰ ĐOÁN PHÂN CỤM
========================================

👤 THÔNG TIN KHÁCH HÀNG:
• Giới tính: {(customer.Gender == 0 ? "Nữ" : "Nam")}
• Tuổi: {customer.Age}
• Thu nhập hàng năm: ${customer.AnnualIncome}k
• Điểm chi tiêu: {customer.SpendingScore}/100
• Trình độ: {cmbEducation.SelectedItem}
• Nghề nghiệp: {cmbProfession.SelectedItem}
• Kinh nghiệm: {customer.WorkExperience} năm
• Quy mô gia đình: {customer.FamilySize} người
• Thành phố: {cmbCity.SelectedItem}
• Mua sắm online: {customer.OnlineShoppingFreq} lần/tháng
• Lòng trung thành: {customer.BrandLoyalty}/10
• Sử dụng mạng xã hội: {customer.SocialMediaUsage} giờ/ngày
• Kênh ưa thích: {cmbPreferredChannel.SelectedItem}

🏷️ PHÂN CỤM DỰ ĐOÁN:
• Thuộc Segment: {prediction.PredictedClusterId}
• Độ tin cậy: {(prediction.Distances?[0] ?? 0):F2}

";

            // Add segment analysis if available
            if (_segments != null && _segments.ContainsKey(prediction.PredictedClusterId))
            {
                var segment = _segments[prediction.PredictedClusterId];
                result += $@"📊 THÔNG TIN SEGMENT {prediction.PredictedClusterId}:
• Mô tả: {segment.Description}
• Số lượng khách hàng: {segment.CustomerCount} ({segment.Percentage:F1}%)
• Insight kinh doanh: {segment.BusinessInsight}

📈 ĐẶC ĐIỂM TRUNG BÌNH CỦA SEGMENT:
";
                foreach (var feature in segment.AverageFeatures)
                {
                    result += $"• {feature.Key}: {feature.Value:F2}\n";
                }
            }

            result += $@"

💡 KHUYẾN NGHỊ:
{GenerateRecommendations(customer, prediction)}";

            rtbPredictionResult.Text = result;
        }

        private string GenerateRecommendations(EnhancedCustomerData customer, CustomerPrediction prediction)
        {
            var recommendations = new List<string>();

            if (customer.AnnualIncome > 60 && customer.SpendingScore > 70)
            {
                recommendations.Add("🌟 Khách hàng VIP: Ưu tiên sản phẩm cao cấp, dịch vụ cá nhân hóa");
                recommendations.Add("💎 Chương trình loyalty premium, ưu đãi độc quyền");
            }
            else if (customer.AnnualIncome > 60 && customer.SpendingScore < 40)
            {
                recommendations.Add("🎯 Khách hàng tiềm năng: Cần chiến lược kích thích mua sắm");
                recommendations.Add("🎁 Khuyến mãi hấp dẫn, chương trình thử nghiệm miễn phí");
            }

            if (customer.OnlineShoppingFreq > 10)
            {
                recommendations.Add("📱 Tập trung kênh online: mobile app, social commerce");
                recommendations.Add("🚚 Dịch vụ giao hàng express, trải nghiệm mua sắm số");
            }

            if (customer.Age < 30)
            {
                recommendations.Add("🔥 Targeting gen Z/Millennials: trend products, social media marketing");
                recommendations.Add("📸 Content marketing, influencer collaboration");
            }

            return recommendations.Count > 0 ? string.Join("\n", recommendations) :
                "Tiếp tục theo dõi hành vi khách hàng để đưa ra khuyến nghị phù hợp";
        }

        private void LoadTrainedModel()
        {
            try
            {
                var resultsDir = "Results";
                if (!Directory.Exists(resultsDir))
                {
                    rtbPredictionResult.Text = "Chưa có mô hình đã huấn luyện. Vui lòng huấn luyện mô hình trước!";
                    btnPredict.Enabled = false;
                    btnPredictBatch.Enabled = false;
                    return;
                }

                var modelFiles = Directory.GetFiles(resultsDir, "*.zip");
                if (modelFiles.Length == 0)
                {
                    rtbPredictionResult.Text = "Không tìm thấy file mô hình. Vui lòng huấn luyện mô hình trước!";
                    btnPredict.Enabled = false;
                    btnPredictBatch.Enabled = false;
                    return;
                }

                // Load the most recent model
                var latestModel = modelFiles.OrderByDescending(f => File.GetCreationTime(f)).First();

                // For now, create a new KMeans model and load
                _trainedModel = new KMeansClusterer();
                _trainedModel.LoadModel(latestModel);

                rtbPredictionResult.Text = $"✅ Đã tải mô hình: {Path.GetFileName(latestModel)}\n\n" +
                    "Nhập thông tin khách hàng và nhấn 'Dự đoán phân cụm' để bắt đầu!";
            }
            catch (Exception ex)
            {
                rtbPredictionResult.Text = $"❌ Lỗi khi tải mô hình: {ex.Message}\n\n" +
                    "Vui lòng huấn luyện lại mô hình!";
                btnPredict.Enabled = false;
                btnPredictBatch.Enabled = false;
            }
        }

        // Batch prediction methods
        private void BtnAddCustomer_Click(object sender, EventArgs e)
        {
            var dt = (DataTable)dgvBatchInput.DataSource;
            var newRow = dt.NewRow();
            newRow["Gender"] = "Female";
            newRow["Age"] = 30;
            newRow["AnnualIncome"] = 50;
            newRow["SpendingScore"] = 50;
            newRow["Education"] = "Bachelor";
            newRow["Profession"] = "Engineer";
            newRow["WorkExperience"] = 5;
            newRow["FamilySize"] = 3;
            newRow["City"] = "HaNoi";
            newRow["OnlineShoppingFreq"] = 5;
            newRow["BrandLoyalty"] = 5;
            newRow["SocialMediaUsage"] = 2.0f;
            newRow["PreferredChannel"] = "Both";
            dt.Rows.Add(newRow);
        }

        private void BtnRemoveCustomer_Click(object sender, EventArgs e)
        {
            if (dgvBatchInput.SelectedRows.Count > 0)
            {
                dgvBatchInput.Rows.RemoveAt(dgvBatchInput.SelectedRows[0].Index);
            }
        }

        private void BtnImportCSV_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Chọn file CSV để import"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var dataHelper = new DataHelper();
                    var validation = dataHelper.ValidateCSVFile(openFileDialog.FileName);

                    if (!validation.IsValid)
                    {
                        MessageBox.Show($"File CSV không hợp lệ:\n{string.Join("\n", validation.Errors)}",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Load CSV data into the grid
                    // This is a simplified version - you might want to use CsvHelper for robust parsing
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    var dt = (DataTable)dgvBatchInput.DataSource;
                    dt.Clear();

                    for (int i = 1; i < lines.Length; i++) // Skip header
                    {
                        var values = lines[i].Split(',');
                        if (values.Length >= dt.Columns.Count)
                        {
                            var row = dt.NewRow();
                            for (int j = 0; j < Math.Min(values.Length, dt.Columns.Count); j++)
                            {
                                row[j] = values[j].Trim();
                            }
                            dt.Rows.Add(row);
                        }
                    }

                    MessageBox.Show($"Đã import thành công {dt.Rows.Count} khách hàng!",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi import file: {ex.Message}",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnPredictBatch_Click(object sender, EventArgs e)
        {
            if (_trainedModel == null)
            {
                MessageBox.Show("Không tìm thấy mô hình đã huấn luyện!",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dt = (DataTable)dgvBatchInput.DataSource;
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để dự đoán!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                btnPredictBatch.Enabled = false;
                var results = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    if (row.IsNull(0)) continue; // Skip empty rows

                    var customer = CreateEnhancedCustomerFromDataRow(row);
                    var prediction = await _trainedModel.PredictAsync(customer);

                    results.Add(new
                    {
                        CustomerID = results.Count + 1,
                        Gender = row["Gender"].ToString(),
                        Age = Convert.ToInt32(row["Age"]),
                        AnnualIncome = Convert.ToInt32(row["AnnualIncome"]),
                        SpendingScore = Convert.ToInt32(row["SpendingScore"]),
                        PredictedSegment = prediction.PredictedClusterId,
                        Confidence = prediction.Distances?[0] ?? 0f,
                        Education = row["Education"].ToString(),
                        Profession = row["Profession"].ToString(),
                        City = row["City"].ToString()
                    });
                }

                // Display results
                dgvResults.DataSource = results;
                tabControl.SelectedTab = tabResults;

                MessageBox.Show($"Đã dự đoán thành công cho {results.Count} khách hàng!",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi dự đoán hàng loạt: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPredictBatch.Enabled = true;
            }
        }

        private EnhancedCustomerData CreateEnhancedCustomerFromDataRow(DataRow row)
        {
            return new EnhancedCustomerData
            {
                CustomerID = DateTime.Now.Ticks,
                Gender = row["Gender"].ToString() == "Female" ? 0f : 1f,
                Age = Convert.ToSingle(row["Age"]),
                AnnualIncome = Convert.ToSingle(row["AnnualIncome"]),
                SpendingScore = Convert.ToSingle(row["SpendingScore"]),
                Education = GetEducationCode(row["Education"].ToString()),
                Profession = GetProfessionCode(row["Profession"].ToString()),
                WorkExperience = Convert.ToSingle(row["WorkExperience"]),
                FamilySize = Convert.ToSingle(row["FamilySize"]),
                City = GetCityCode(row["City"].ToString()),
                OnlineShoppingFreq = Convert.ToSingle(row["OnlineShoppingFreq"]),
                BrandLoyalty = Convert.ToSingle(row["BrandLoyalty"]),
                SocialMediaUsage = Convert.ToSingle(row["SocialMediaUsage"]),
                PreferredChannel = GetChannelCode(row["PreferredChannel"].ToString())
            };
        }

        private float GetEducationCode(string education)
        {
            switch (education)
            {
                case "High School": return 0f;
                case "Bachelor": return 1f;
                case "Master": return 2f;
                case "PhD": return 3f;
                default: return 0f;
            }
        }

        private float GetProfessionCode(string profession)
        {
            var professions = new[] { "Student", "Healthcare", "Engineer", "Artist", "Lawyer", "Doctor", "Marketing", "Entertainment" };
            var index = Array.IndexOf(professions, profession);
            return index >= 0 ? index : 0f;
        }

        private float GetCityCode(string city)
        {
            switch (city)
            {
                case "HaNoi": return 0f;
                case "HCM": return 1f;
                case "DaNang": return 2f;
                case "Others": return 3f;
                default: return 3f;
            }
        }

        private float GetChannelCode(string channel)
        {
            switch (channel)
            {
                case "Online": return 0f;
                case "Offline": return 1f;
                case "Both": return 2f;
                default: return 2f;
            }
        }

        private void BtnExportResults_Click(object sender, EventArgs e)
        {
            if (dgvResults.DataSource == null)
            {
                MessageBox.Show("Không có kết quả để export!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                Title = "Lưu kết quả dự đoán",
                FileName = $"PredictionResults_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var exportHelper = new ExportHelper();

                    if (saveFileDialog.FilterIndex == 1) // Excel
                    {
                        exportHelper.ExportDataGridViewToExcel(dgvResults, saveFileDialog.FileName);
                    }
                    else // CSV
                    {
                        exportHelper.ExportDataGridViewToCSV(dgvResults, saveFileDialog.FileName);
                    }

                    MessageBox.Show($"Đã export thành công: {saveFileDialog.FileName}",
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi export: {ex.Message}",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}