using CustomerSegmentationML.ML.Algorithms;
using CustomerSegmentationML.ML.AutoML;
using CustomerSegmentationML.Models;
using CustomerSegmentationML.Utils;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.ML;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class TrainingForm : Form
    {
        private readonly string _dataPath;
        private readonly AutoMLTrainer _autoTrainer;
        private readonly MLContext _mlContext;
        private bool _isTraining = false;

        public TrainingForm(string dataPath, AutoMLTrainer autoTrainer)
        {
            InitializeComponent();
            _dataPath = dataPath;
            _autoTrainer = autoTrainer;
            _mlContext = new MLContext(seed: 0);

            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Model Training - Customer Segmentation";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterParent;

            // Setup algorithm options
            cmbAlgorithm.Items.AddRange(new string[]
            {
                "AutoML (Recommended)",
                "K-Means",
                "DBSCAN",
                "Hierarchical"
            });
            cmbAlgorithm.SelectedIndex = 0;

            // Setup progress tracking
            progressOverall.Style = ProgressBarStyle.Continuous;
            progressAlgorithm.Style = ProgressBarStyle.Continuous;

            UpdateStatus("Sẵn sàng để bắt đầu training");
        }

        private async void btnStartTraining_Click(object sender, EventArgs e)
        {
            if (_isTraining) return;

            try
            {
                _isTraining = true;
                btnStartTraining.Enabled = false;
                btnCancel.Enabled = true;

                UpdateStatus("Đang tải dữ liệu...");

                // Load and split data
                var (trainData, testData) = await LoadAndSplitData();

                if (cmbAlgorithm.SelectedItem.ToString().StartsWith("AutoML"))
                {
                    await RunAutoML(trainData, testData);
                }
                else
                {
                    await RunSingleAlgorithm(trainData, testData);
                }

                UpdateStatus("Training hoàn thành!");
                MessageBox.Show("Training đã hoàn thành thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong quá trình training: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Training thất bại");
            }
            finally
            {
                _isTraining = false;
                btnStartTraining.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        private async Task<(IDataView trainData, IDataView testData)> LoadAndSplitData()
        {
            return await Task.Run(() =>
            {
                var dataHelper = new CustomerSegmentationML.Utils.DataHelper()  ;
                return dataHelper.LoadAndSplitEnhancedData(_dataPath, 0.2f);
            });
        }

        private async Task RunAutoML(IDataView trainData, IDataView testData)
        {
            var progress = new Progress<AutoMLProgress>(UpdateAutoMLProgress);

            UpdateStatus("Đang chạy AutoML...");
            var result = await _autoTrainer.FindBestModelAsync(trainData, testData, progress);

            // Display results
            DisplayAutoMLResults(result);

            // Save best model
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var modelPath = $"Results/AutoML_BestModel_{timestamp}.zip";
            Directory.CreateDirectory("Results");

            _mlContext.Model.Save(result.BestResult.Model, trainData.Schema, modelPath);

            // Save detailed report
            SaveAutoMLReport(result, modelPath);
        }

        private async Task RunSingleAlgorithm(IDataView trainData, IDataView testData)
        {
                IClusteringAlgorithm algorithm;
            switch (cmbAlgorithm.SelectedItem.ToString())
            {
                case "K-Means":
                    algorithm = new KMeansClusterer();
                    break;
                default:
                    algorithm = new KMeansClusterer(); // Default fallback
                    break;
            }

            var progress = new Progress<string>(msg => UpdateStatus(msg));

            var result = await algorithm.TrainAsync(trainData, progress);
            var metrics = algorithm.Evaluate(testData);

            DisplaySingleAlgorithmResults(algorithm.Name, result, metrics);

            // Save model
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var modelPath = $"Results/{algorithm.Name}_Model_{timestamp}.zip";
            Directory.CreateDirectory("Results");
            algorithm.SaveModel(modelPath);
        }

        private void UpdateAutoMLProgress(AutoMLProgress progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<AutoMLProgress>(UpdateAutoMLProgress), progress);
                return;
            }

            lblCurrentAlgorithm.Text = $"Thuật toán: {progress.CurrentAlgorithm}";
            lblProgressMessage.Text = progress.Message;

            progressOverall.Value = Math.Min(100, (int)progress.OverallProgress);
            progressAlgorithm.Value = Math.Min(100, (int)progress.AlgorithmProgress);

            if (progress.HasError)
            {
                rtbLog.AppendText($"[ERROR] {progress.Message}\n");
                rtbLog.ScrollToCaret();
            }
            else
            {
                rtbLog.AppendText($"[INFO] {progress.Message}\n");
                rtbLog.ScrollToCaret();
            }
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), message);
                return;
            }

            lblStatus.Text = $"Trạng thái: {message}";
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtbLog.ScrollToCaret();
        }

        private void DisplayAutoMLResults(AutoMLResult result)
        {
            var resultsText = $@"
🎯 AUTOML RESULTS SUMMARY
========================
Tổng số cấu hình đã thử: {result.TotalAlgorithmsTested}
Thời gian training: {result.TotalTimeSpent:F1} giây

🏆 MÔ HÌNH TỐT NHẤT:
Thuật toán: {result.BestResult.AlgorithmName}
Silhouette Score: {result.BestResult.Metrics.SilhouetteScore:F3}
Davies-Bouldin Index: {result.BestResult.Metrics.DaviesBouldinIndex:F3}
Average Distance: {result.BestResult.Metrics.AverageDistance:F3}
Số clusters: {result.BestResult.Metrics.NumberOfClusters}

📊 TOP 3 MÔ HÌNH:
";

            var topResults = result.AllResults
                .OrderByDescending(r => r.OverallScore)
                .Take(3)
                .ToList();

            for (int i = 0; i < topResults.Count; i++)
            {
                var r = topResults[i];
                resultsText += $"{i + 1}. {r.AlgorithmName} - Score: {r.OverallScore:F3}\n";
            }

            rtbResults.Text = resultsText;
        }

        private void DisplaySingleAlgorithmResults(string algorithmName, ClusteringResult result, ClusteringMetrics metrics)
        {
            var resultsText = $@"
🎯 {algorithmName.ToUpper()} RESULTS
========================
Silhouette Score: {metrics.SilhouetteScore:F3}
Davies-Bouldin Index: {metrics.DaviesBouldinIndex:F3}
Average Distance: {metrics.AverageDistance:F3}
Số clusters: {metrics.NumberOfClusters}
Thời gian training: {result.TrainingDuration.TotalSeconds:F1} giây

📊 PHÂN TÍCH SEGMENTS:
";

            foreach (var segment in result.Segments.Values.OrderBy(s => s.SegmentId))
            {
                resultsText += $@"
Segment {segment.SegmentId}: {segment.CustomerCount} khách hàng ({segment.Percentage:F1}%)
- {segment.Description}
- {segment.BusinessInsight}
";
            }

            rtbResults.Text = resultsText;
        }

        private void SaveAutoMLReport(AutoMLResult result, string modelPath)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var reportPath = $"Results/AutoML_Report_{timestamp}.txt";

            var report = $@"AUTOML TRAINING REPORT
=====================
Timestamp: {DateTime.Now}
Dataset: {Path.GetFileName(_dataPath)}
Model Path: {modelPath}

SUMMARY:
- Total configurations tested: {result.TotalAlgorithmsTested}
- Total training time: {result.TotalTimeSpent:F1} seconds
- Best algorithm: {result.BestResult.AlgorithmName}

BEST MODEL METRICS:
- Silhouette Score: {result.BestResult.Metrics.SilhouetteScore:F4}
- Davies-Bouldin Index: {result.BestResult.Metrics.DaviesBouldinIndex:F4}
- Average Distance: {result.BestResult.Metrics.AverageDistance:F4}
- Number of Clusters: {result.BestResult.Metrics.NumberOfClusters}
- Overall Score: {result.BestResult.OverallScore:F4}

PARAMETERS:
";

            foreach (var param in result.BestResult.Parameters)
            {
                report += $"- {param.Key}: {param.Value}\n";
            }

            report += "\nALL RESULTS:\n";
            foreach (var r in result.AllResults.OrderByDescending(x => x.OverallScore))
            {
                report += $"{r.AlgorithmName}: Score={r.OverallScore:F4}, Time={r.TrainingDuration.TotalSeconds:F1}s\n";
            }

            File.WriteAllText(reportPath, report);

            UpdateStatus($"Đã lưu báo cáo: {reportPath}");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void TrainingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isTraining)
            {
                var result = MessageBox.Show("Training đang chạy. Bạn có chắc muốn hủy?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
