using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class DataViewForm : Form
    {
        private string _csvPath;
        private DataGridView _dataGridView;

        public DataViewForm(string csvPath)
        {
            _csvPath = csvPath;
            InitializeComponent();
            InitializeUI();
            LoadCsvData();
        }

        private void InitializeUI()
        {
            this.Text = $"Xem d? li?u - {Path.GetFileName(_csvPath)}";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            _dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(_dataGridView);
        }

        private void LoadCsvData()
        {
            if (!File.Exists(_csvPath))
            {
                MessageBox.Show($"Không tìm th?y file: {_csvPath}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var dt = new DataTable();
            using (var reader = new StreamReader(_csvPath))
            {
                string headerLine = reader.ReadLine();
                if (headerLine == null) return;
                var headers = headerLine.Split(',');
                foreach (var h in headers)
                    dt.Columns.Add(h);
                int rowCount = 0;
                while (!reader.EndOfStream && rowCount < 100)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    dt.Rows.Add(values);
                    rowCount++;
                }
            }
            _dataGridView.DataSource = dt;
        }
    }
}
