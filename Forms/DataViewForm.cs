using System;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace CustomerSegmentationML.Forms
{
    public partial class DataViewForm : Form
    {
        private string _filePath;
        private DataGridView dataGridView;

        public DataViewForm(string filePath)
        {
            _filePath = filePath;
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            Controls.Add(dataGridView);

            // Load data after form is shown to ensure controls are ready
            this.Shown += (s, e) => LoadData();
        }

        private void LoadData()
        {
            try
            {
                var lines = File.ReadAllLines(_filePath);
                if (lines.Length == 0) return;

                var headers = lines[0].Split(',');
                var dt = new DataTable();

                foreach (var header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }

                for (int i = 1; i < Math.Min(lines.Length, 1000); i++)
                {
                    var values = lines[i].Split(',');
                    dt.Rows.Add(values);
                }

                dataGridView.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đọc dữ liệu: {ex.Message}", "Lỗi");
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}