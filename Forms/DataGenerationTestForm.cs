using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CustomerSegmentationML.ML.DataPreprocessing;

namespace CustomerSegmentationML.Forms
{
    public partial class DataGenerationTestForm : Form
    {
        public DataGenerationTestForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        private void InitializeUI()
        {
            this.Text = "🧪 Tạo và Kiểm tra Dữ liệu Mẫu";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Main layout panel
            var panelMain = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Data generation options
            var grpOptions = new GroupBox
            {
                Text = "Tùy chọn tạo dữ liệu",
                Location = new Point(20, 20),
                Size = new Size(740, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var lblCustomerCount = new Label
            {
                Text = "Số lượng khách hàng:",
                Location = new Point(20, 30),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9)
            };

            var numCustomerCount = new NumericUpDown
            {
                Minimum = 100,
                Maximum = 10000,
                Value = 1000,
                Location = new Point(170, 30),
                Size = new Size(100, 25)
            };

            var lblOutputPath = new Label
            {
                Text = "Đường dẫn file output:",
                Location = new Point(20, 70),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9)
            };

            var txtOutputPath = new TextBox
            {
                Text = "Data/enhanced_customers.csv",
                Location = new Point(170, 70),
                Size = new Size(400, 25)
            };

            var btnBrowse = new Button
            {
                Text = "...",
                Location = new Point(580, 70),
                Size = new Size(30, 25)
            };

            // Generate button
            var btnGenerate = new Button
            {
                Text = "🧪 Tạo dữ liệu mẫu mới",
                BackColor = Color.LightGreen,
                Location = new Point(20, 150),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Preview data
            var grpPreview = new GroupBox
            {
                Text = "Xem trước dữ liệu",
                Location = new Point(20, 200),
                Size = new Size(740, 300),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var dgvPreview = new DataGridView
            {
                Location = new Point(10, 30),
                Size = new Size(720, 260),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true
            };

            // Status label
            var lblStatus = new Label
            {
                Text = "Sẵn sàng tạo dữ liệu",
                Location = new Point(20, 510),
                Size = new Size(740, 25),
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            // Add event handlers
            btnBrowse.Click += (sender, e) =>
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = "enhanced_customers.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = saveDialog.FileName;
                }
            };

            btnGenerate.Click += (sender, e) =>
            {
                try
                {
                    btnGenerate.Enabled = false;
                    lblStatus.Text = "⏳ Đang tạo dữ liệu...";
                    
                    var outputPath = txtOutputPath.Text;
                    var customerCount = (int)numCustomerCount.Value;

                    // Make sure directory exists
                    var directory = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Generate data
                    DatasetGenerator.GenerateEnhancedDataset(outputPath, customerCount);

                    // Show message
                    MessageBox.Show($"Đã tạo thành công {customerCount} khách hàng!\nFile: {outputPath}", 
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Load preview
                    LoadPreview(outputPath, dgvPreview);
                    
                    lblStatus.Text = $"✅ Đã tạo thành công dữ liệu tại {outputPath}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tạo dữ liệu: {ex.Message}", 
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "❌ Tạo dữ liệu thất bại";
                }
                finally
                {
                    btnGenerate.Enabled = true;
                }
            };

            // Add controls to form
            grpOptions.Controls.AddRange(new Control[] {
                lblCustomerCount, numCustomerCount,
                lblOutputPath, txtOutputPath, btnBrowse
            });

            grpPreview.Controls.Add(dgvPreview);

            panelMain.Controls.AddRange(new Control[] {
                grpOptions, btnGenerate, grpPreview, lblStatus
            });

            this.Controls.Add(panelMain);

            // Check if previous data exists and load preview
            if (File.Exists("Data/enhanced_customers.csv"))
            {
                LoadPreview("Data/enhanced_customers.csv", dgvPreview);
            }
        }

        private void LoadPreview(string filePath, DataGridView dgv)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                // Read first 20 lines for preview
                var lines = File.ReadAllLines(filePath);
                if (lines.Length <= 1) return;

                var headers = lines[0].Split(',');
                var data = new object[Math.Min(lines.Length - 1, 20)][];

                for (int i = 1; i <= Math.Min(lines.Length - 1, 20); i++)
                {
                    var values = lines[i].Split(',');
                    data[i - 1] = values;
                }

                // Create DataTable
                var dt = new System.Data.DataTable();
                foreach (var header in headers)
                {
                    dt.Columns.Add(header);
                }

                foreach (var row in data)
                {
                    dt.Rows.Add(row);
                }

                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể đọc file preview: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}