using CustomerSegmentationML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerSegmentationML.ML.Algorithms
{
    public class KMeansClusterer : IClusteringAlgorithm
    {
        private MLContext _mlContext;
        private ITransformer _model;

        public string Name => "K-Means";
        public string Description => "Phân cụm dựa trên khoảng cách Euclidean, phù hợp cho dữ liệu có hình dạng cầu";

        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>
        {
            ["NumberOfClusters"] = 5,
            ["MaxIterations"] = 100,
            ["InitializationAlgorithm"] = "KMeansPlusPlus"
        };

        public KMeansClusterer()
        {
            _mlContext = new MLContext(seed: 0);
        }

        public async Task<ClusteringResult> TrainAsync(IDataView data, IProgress<string> progress = null)
        {
            progress?.Report("Khởi tạo K-Means trainer...");

            var startTime = DateTime.Now;

            // Build pipeline
            var pipeline = _mlContext.Transforms
                .Concatenate("Features",
                    "Gender", "Age", "AnnualIncome", "SpendingScore",
                    "Education", "Profession", "WorkExperience", "FamilySize",
                    "City", "OnlineShoppingFreq", "BrandLoyalty", "SocialMediaUsage", "PreferredChannel")
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Clustering.Trainers.KMeans("Features",
                    numberOfClusters: (int)Parameters["NumberOfClusters"],
                    maximumNumberOfIterations: (int)Parameters["MaxIterations"]));

            progress?.Report("Đang huấn luyện mô hình K-Means...");

            // Train model
            _model = await Task.Run(() => pipeline.Fit(data));

            var endTime = DateTime.Now;
            progress?.Report("Đánh giá kết quả...");

            // Evaluate
            var metrics = Evaluate(data);
            var segments = AnalyzeSegments(data);

            progress?.Report("Hoàn thành!");

            return new ClusteringResult
            {
                Model = _model,
                TrainingMetrics = metrics,
                Segments = segments,
                AlgorithmName = Name,
                Parameters = Parameters,
                TrainingTime = startTime,
                TrainingDuration = endTime - startTime
            };
        }

        public async Task<CustomerPrediction> PredictAsync(EnhancedCustomerData customer)
        {
            if (_model == null)
                throw new InvalidOperationException("Model chưa được huấn luyện");

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<EnhancedCustomerData, CustomerPrediction>(_model);
            return await Task.Run(() => predictionEngine.Predict(customer));
        }

        public ClusteringMetrics Evaluate(IDataView testData)
        {
            if (_model == null)
                throw new InvalidOperationException("Model chưa được huấn luyện");

            var predictions = _model.Transform(testData);
            var metrics = _mlContext.Clustering.Evaluate(predictions);

            return new ClusteringMetrics
            {
                AverageDistance = metrics.AverageDistance,
                DaviesBouldinIndex = metrics.DaviesBouldinIndex,
                NumberOfClusters = (int)Parameters["NumberOfClusters"],
                SilhouetteScore = metrics.NormalizedMutualInformation, 
                InertiaScore = CalculateInertia(testData, predictions)
            };
        }

        private Dictionary<uint, SegmentAnalysis> AnalyzeSegments(IDataView data)
        {
            var predictions = _model.Transform(data);
            var predictedData = _mlContext.Data.CreateEnumerable<CustomerPrediction>(predictions, false).ToArray();
            var originalData = _mlContext.Data.CreateEnumerable<EnhancedCustomerData>(data, false).ToArray();

            var segments = new Dictionary<uint, SegmentAnalysis>();
            var groupedData = predictedData
                .Select((pred, index) => new { Prediction = pred, Original = originalData[index] })
                .GroupBy(x => x.Prediction.PredictedClusterId);

            foreach (var group in groupedData)
            {
                var customers = group.Select(x => x.Original).ToList();
                var segmentId = group.Key;

                segments[segmentId] = new SegmentAnalysis
                {
                    SegmentId = segmentId,
                    CustomerCount = customers.Count,
                    Percentage = (double)customers.Count / originalData.Length * 100,
                    AverageFeatures = new Dictionary<string, double>
                    {
                        ["Age"] = customers.Average(c => c.Age),
                        ["Income"] = customers.Average(c => c.AnnualIncome),
                        ["SpendingScore"] = customers.Average(c => c.SpendingScore),
                        ["Education"] = customers.Average(c => c.Education),
                        ["WorkExperience"] = customers.Average(c => c.WorkExperience),
                        ["FamilySize"] = customers.Average(c => c.FamilySize),
                        ["OnlineShoppingFreq"] = customers.Average(c => c.OnlineShoppingFreq),
                        ["BrandLoyalty"] = customers.Average(c => c.BrandLoyalty),
                        ["SocialMediaUsage"] = customers.Average(c => c.SocialMediaUsage)
                    },
                    Description = GenerateSegmentDescription(customers),
                    BusinessInsight = GenerateBusinessInsight(customers)
                };
            }

            return segments;
        }

        private string GenerateSegmentDescription(List<EnhancedCustomerData> customers)
        {
            var avgAge = customers.Average(c => c.Age);
            var avgIncome = customers.Average(c => c.AnnualIncome);
            var avgSpending = customers.Average(c => c.SpendingScore);
            var avgEducation = customers.Average(c => c.Education);

            if (avgIncome > 60 && avgSpending > 70)
                return "🌟 High-Value Customers: Thu nhập cao, chi tiêu nhiều";
            else if (avgIncome > 60 && avgSpending < 40)
                return "💎 Potential Customers: Thu nhập cao, chi tiêu thấp";
            else if (avgAge < 30 && avgSpending > 60)
                return "🔥 Young Spenders: Trẻ tuổi, thích chi tiêu";
            else if (avgEducation > 2 && avgIncome > 40)
                return "🎓 Educated Professionals: Trình độ cao, thu nhập ổn định";
            else if (avgSpending < 30)
                return "🔒 Conservative Customers: Thận trọng trong chi tiêu";
            else
                return "⚖️ Balanced Customers: Cân bằng thu nhập và chi tiêu";
        }

        private string GenerateBusinessInsight(List<EnhancedCustomerData> customers)
        {
            var avgIncome = customers.Average(c => c.AnnualIncome);
            var avgSpending = customers.Average(c => c.SpendingScore);
            var avgOnlineShopping = customers.Average(c => c.OnlineShoppingFreq);

            if (avgIncome > 60 && avgSpending > 70)
                return "Target cho sản phẩm premium, chương trình VIP, dịch vụ cá nhân hóa";
            else if (avgIncome > 60 && avgSpending < 40)
                return "Cần chiến lược kích thích mua sắm: khuyến mãi, thử nghiệm miễn phí";
            else if (avgOnlineShopping > 10)
                return "Tập trung marketing online, mobile app, social commerce";
            else
                return "Phát triển omnichannel, cải thiện trải nghiệm mua sắm";
        }

        private double CalculateOverallScore(ClusteringMetrics metrics)
        {
            // Weighted score combining multiple metrics
            var silhouetteWeight = 0.4;
            var daviesBouldinWeight = 0.3; // Lower is better, so invert
            var averageDistanceWeight = 0.3; // Lower is better, so invert

            var normalizedSilhouette = Math.Max(0, metrics.SilhouetteScore);
            var normalizedDaviesBouldin = Math.Max(0, 1.0 / (1.0 + metrics.DaviesBouldinIndex));
            var normalizedDistance = Math.Max(0, 1.0 / (1.0 + metrics.AverageDistance));

            return (silhouetteWeight * normalizedSilhouette) +
                   (daviesBouldinWeight * normalizedDaviesBouldin) +
                   (averageDistanceWeight * normalizedDistance);
        }

        public void SaveModel(string path)
        {
            if (_model == null)
                throw new InvalidOperationException("Model chưa được huấn luyện");
            _mlContext.Model.Save(_model, null, path);
        }

        public void LoadModel(string path)
        {
            _model = _mlContext.Model.Load(path, out _);
        }
    }

    public class AutoMLResult
    {
        public AlgorithmResult BestResult { get; set; }
        public List<AlgorithmResult> AllResults { get; set; }
        public double TotalTimeSpent { get; set; }
        public int TotalAlgorithmsTested { get; set; }
        public string Summary => $"Tested {TotalAlgorithmsTested} configurations in {TotalTimeSpent:F1}s. Best: {BestResult?.AlgorithmName}";
    }

    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public ClusteringMetrics Metrics { get; set; }
        public TimeSpan TrainingDuration { get; set; }
        public ITransformer Model { get; set; }
        public double OverallScore => CalculateScore();

        private double CalculateScore()
        {
            return Math.Max(0, Metrics.SilhouetteScore) - (Metrics.DaviesBouldinIndex * 0.1);
        }
    }

    public class AutoMLProgress
    {
        public string CurrentAlgorithm { get; set; }
        public double AlgorithmProgress { get; set; }
        public double OverallProgress { get; set; }
        public string Message { get; set; }
        public bool HasError { get; set; }
    }
}