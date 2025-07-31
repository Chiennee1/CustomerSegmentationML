using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace CustomerSegmentationML.Utils
{
    public class ExportHelper
    {
        public void ExportDataGridViewToExcel(DataGridView dgv, string filePath)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Add headers
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = dgv.Columns[i].HeaderText;
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Add data
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    var cellValue = dgv.Rows[i].Cells[j].Value?.ToString() ?? "";
                    worksheet.Cell(i + 2, j + 1).Value = cellValue;
                }
            }

            // Auto-fit columns
            worksheet.ColumnsUsed().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        public void ExportDataGridViewToCSV(DataGridView dgv, string filePath)
        {
            var csv = new StringBuilder();

            // Add headers
            var headers = new string[dgv.Columns.Count];
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                headers[i] = dgv.Columns[i].HeaderText;
            }
            csv.AppendLine(string.Join(",", headers));

            // Add data
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                var values = new string[dgv.Columns.Count];
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    var cellValue = dgv.Rows[i].Cells[j].Value?.ToString() ?? "";
                    values[j] = cellValue.Contains(",") ? $"\"{cellValue}\"" : cellValue;
                }
                csv.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }
    }
}