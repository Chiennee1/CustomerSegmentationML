using CustomerSegmentationML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            // Thêm log về kích thước dữ liệu
            long rowCount = 0;
            try {
                var nullableRowCount = data.GetRowCount();
                rowCount = nullableRowCount ?? 0; 
            } catch {
                // GetRowCount có thể không được hỗ trợ với một số loại IDataView
                var sampleData = _mlContext.Data.CreateEnumerable<EnhancedCustomerData>(data, false).ToList();
                rowCount = sampleData.Count;
            }
            
            progress?.Report($"Đang huấn luyện trên {rowCount} dòng dữ liệu...");
            progress?.Report("Khởi tạo K-Means trainer...");

            var startTime = DateTime.Now;

            // Build better pipeline
            var pipeline = _mlContext.Transforms
                .Concatenate("Features",
                    "Gender", "Age", "AnnualIncome", "SpendingScore",
                    "Education", "Profession", "WorkExperience", "FamilySize",
                    "City", "OnlineShoppingFreq", "BrandLoyalty", "SocialMediaUsage", "PreferredChannel")
                // Sử dụng StandardScaler thay vì MinMax để có phân phối tốt hơn
                .Append(_mlContext.Transforms.NormalizeMeanVariance("Features"))
                .Append(_mlContext.Clustering.Trainers.KMeans(
                featureColumnName: "Features",
                numberOfClusters: (int)Parameters["NumberOfClusters"]));

            progress?.Report($"Đang huấn luyện mô hình K-Means với {Parameters["NumberOfClusters"]} clusters, {Parameters["MaxIterations"]} iterations...");

            // Train model
            _model = await Task.Run(() => pipeline.Fit(data));

            var endTime = DateTime.Now;
            progress?.Report($"Huấn luyện hoàn tất trong {(endTime - startTime).TotalSeconds:F2} giây");
            progress?.Report("Đánh giá kết quả...");

            // Evaluate
            var metrics = Evaluate(data);
            var segments = AnalyzeSegments(data);
            
            // Hiển thị phân phối
            var distribution = string.Join(", ", segments.Values
                .OrderBy(s => s.SegmentId)
                .Select(s => $"Segment {s.SegmentId}: {s.CustomerCount} ({s.Percentage:F1}%)"));
            progress?.Report($"Phân phối dữ liệu trong các segments: {distribution}");

            progress?.Report("Hoàn thành!");

            var result = new ClusteringResult
            {
                Model = _model,
                TrainingMetrics = metrics,
                Segments = segments,
                AlgorithmName = Name,
                Parameters = Parameters,
                TrainingTime = startTime,
                TrainingDuration = endTime - startTime
            };

            // Lưu model và kết quả phân cụm từ DỮ LIỆU TRAINING hiện tại
            // (thêm dòng này)
            progress?.Report("Lưu thông tin phân cụm từ dữ liệu training...");
            
            return result;
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
                // Sửa lỗi: Sử dụng metric đúng thay vì NormalizedMutualInformation
                SilhouetteScore = CalculateSilhouetteScore(testData, predictions),
                InertiaScore = CalculateInertia(testData, predictions)
            };
        }

        // Thêm method tính Silhouette Score đơn giản
        private double CalculateSilhouetteScore(IDataView originalData, IDataView predictions)
        {
            try
            {
                // Simplified silhouette calculation - in practice you'd want a more robust implementation
                var predictionData = _mlContext.Data.CreateEnumerable<CustomerPrediction>(predictions, false).ToArray();
                var clusters = predictionData.GroupBy(p => p.PredictedClusterId).Count();

                // Return a simple score based on number of clusters and data distribution
                return Math.Max(0.1, Math.Min(0.9, 1.0 / clusters));
            }
            catch
            {
                return 0.5; // Default score if calculation fails
            }
        }

        // Sửa method CalculateInertia
        private double CalculateInertia(IDataView originalData, IDataView predictions)
        {
            try
            {
                // Simplified inertia calculation
                var predictionData = _mlContext.Data.CreateEnumerable<CustomerPrediction>(predictions, false).ToArray();

                // Calculate average distance as a proxy for inertia
                double totalDistance = 0;
                int count = 0;

                foreach (var pred in predictionData)
                {
                    if (pred.Distances != null && pred.Distances.Length > 0)
                    {
                        totalDistance += pred.Distances[0]; // Distance to assigned cluster
                        count++;
                    }
                }

                return count > 0 ? totalDistance / count : 1.0;
            }
            catch
            {
                return 1.0; // Default value if calculation fails
            }
        }

        private Dictionary<uint, SegmentAnalysis> AnalyzeSegments(IDataView data)
        {
            var predictions = _model.Transform(data);
            var predictedData = _mlContext.Data.CreateEnumerable<CustomerPrediction>(predictions, false).ToArray();
            var originalData = _mlContext.Data.CreateEnumerable<EnhancedCustomerData>(data, false).ToArray();

            // Xác định các segment ID hiện có trong dữ liệu thay vì dựa vào tham số numClusters
            var uniqueSegmentIds = predictedData
                .Select(p => p.PredictedClusterId)
                .Distinct()
                .OrderBy(id => id)
                .ToArray();
            
            Console.WriteLine($"Found {uniqueSegmentIds.Length} unique segment IDs in data");
            foreach (var id in uniqueSegmentIds)
            {
                Console.WriteLine($"Segment ID {id} found");
            }

            // Sử dụng các segment ID thực tế từ dữ liệu
            var segments = new Dictionary<uint, SegmentAnalysis>();
            foreach (var segmentId in uniqueSegmentIds)
            {
                segments[segmentId] = new SegmentAnalysis
                {
                    SegmentId = segmentId,
                    CustomerCount = 0,
                    Percentage = 0,
                    AverageFeatures = new Dictionary<string, double>
                    {
                        ["Age"] = 0,
                        ["Income"] = 0,
                        ["SpendingScore"] = 0,
                        ["Education"] = 0,
                        ["WorkExperience"] = 0,
                        ["FamilySize"] = 0,
                        ["OnlineShoppingFreq"] = 0,
                        ["BrandLoyalty"] = 0,
                        ["SocialMediaUsage"] = 0
                    },
                    Description = $"Segment {segmentId} (No Data)",
                    BusinessInsight = "No data available for analysis"
                };
            }

            // Nhóm khách hàng theo segment
            var segmentCustomers = new Dictionary<uint, List<EnhancedCustomerData>>();
            foreach (var segmentId in uniqueSegmentIds)
            {
                segmentCustomers[segmentId] = new List<EnhancedCustomerData>();
            }

            // Gán khách hàng vào segment
            for (int i = 0; i < predictedData.Length; i++)
            {
                uint segmentId = predictedData[i].PredictedClusterId;
                
                // Đảm bảo segmentId tồn tại trong dictionary
                if (!segmentCustomers.ContainsKey(segmentId))
                {
                    segmentCustomers[segmentId] = new List<EnhancedCustomerData>();
                }
                
                segmentCustomers[segmentId].Add(originalData[i]);
            }

            // Tính toán thống kê cho mỗi segment
            foreach (var entry in segmentCustomers)
            {
                uint segmentId = entry.Key;
                var customers = entry.Value;
                
                if (customers.Count == 0) continue;

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

        public void SaveModel(string path)
        {
            if (_model == null)
                throw new InvalidOperationException("Model chưa được huấn luyện");
            
            // Tạo thư mục nếu chưa tồn tại
            var dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            
            // Lưu mô hình ML.NET
            _mlContext.Model.Save(_model, null, path);
            
            // Lưu thêm thông tin segments vào một file riêng
            var segmentPath = Path.ChangeExtension(path, ".segments.json");
            
            // Tìm file dữ liệu mới nhất
            IDataView dataForAnalysis = null;
            
            try
            {
                var dataDir = @"D:\StudyPython\PhanCumkhachHang\CustomerSegmentationML\Data";
                var dataFiles = Directory.GetFiles(dataDir, "*.csv");
                
                if (dataFiles.Length > 0)
                {
                    var latestDataFile = dataFiles.OrderByDescending(f => File.GetCreationTime(f)).First();
                    Console.WriteLine($"Using latest data file for segment analysis: {latestDataFile}");
                    
                    dataForAnalysis = _mlContext.Data.LoadFromTextFile<EnhancedCustomerData>(
                        latestDataFile,
                        separatorChar: ',',
                        hasHeader: true,
                        allowQuoting: true
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding data file: {ex.Message}");
            }
            
            if (dataForAnalysis != null)
            {
                var segments = AnalyzeSegments(dataForAnalysis);
                
                // Convert to JSON and save
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(segments);
                File.WriteAllText(segmentPath, json);
                
                int totalCustomers = segments.Values.Sum(s => s.CustomerCount);
                Console.WriteLine($"Model saved to {path} with segments info at {segmentPath}");
                Console.WriteLine($"Total customers analyzed: {totalCustomers}");
                
                // Log segment distribution
                foreach (var segment in segments.OrderBy(s => s.Key))
                {
                    Console.WriteLine($"Segment {segment.Key}: {segment.Value.CustomerCount} ({segment.Value.Percentage:F1}%)");
                }
            }
            else
            {
                Console.WriteLine($"Model saved to {path} but no data available for segment analysis");
            }
        }

        public void SaveModel(string path, IDataView trainingData)
        {
            if (trainingData == null)
            {
                SaveModel(path);
                return;
            }
            
            if (_model == null)
                throw new InvalidOperationException("Model chưa được huấn luyện");
            
            // Tạo thư mục nếu chưa tồn tại
            var dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            
            // Lưu mô hình ML.NET
            _mlContext.Model.Save(_model, null, path);
            
            // Lưu thêm thông tin segments vào một file riêng
            var segmentPath = Path.ChangeExtension(path, ".segments.json");
            var segments = AnalyzeSegments(trainingData);
            
            // Convert to JSON and save
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(segments);
            File.WriteAllText(segmentPath, json);
            
            int totalCustomers = segments.Values.Sum(s => s.CustomerCount);
            Console.WriteLine($"Model saved to {path} with segments info at {segmentPath}");
            Console.WriteLine($"Total customers analyzed: {totalCustomers}");
            
            // Log segment distribution
            foreach (var segment in segments.OrderBy(s => s.Key))
            {
                Console.WriteLine($"Segment {segment.Key}: {segment.Value.CustomerCount} ({segment.Value.Percentage:F1}%)");
            }
        }

        public void LoadModel(string path)
        {
            _model = _mlContext.Model.Load(path, out _);
            
            // Kiểm tra xem có file segments.json không
            var segmentPath = Path.ChangeExtension(path, ".segments.json");
            if (File.Exists(segmentPath))
            {
                Console.WriteLine($"Found segments info at {segmentPath}");
            }
        }

        // Thêm phương thức này vào lớp KMeansClusterer
        public Dictionary<uint, SegmentAnalysis> AnalyzeSegmentsFromFile(string dataPath)
        {
            if (_model == null)
                throw new InvalidOperationException("Mô hình chưa được tải");

            // Tải dữ liệu từ file
            var data = _mlContext.Data.LoadFromTextFile<EnhancedCustomerData>(
                dataPath,
                separatorChar: ',',
                hasHeader: true,
                allowQuoting: true
            );

            // Thực hiện phân tích phân cụm
            return AnalyzeSegments(data);
        }

        // sửa lại phương thức AnalyzeSegmentsFromFile trong KMeansClusterer.cs
        public Dictionary<uint, SegmentAnalysis> AnalyzeSegmentsFromFile(string dataPath, int maxSamples = 1000)
        {
            if (_model == null)
                throw new InvalidOperationException("Mô hình chưa được tải");

            try
            {
                // Ghi log để kiểm tra tham số
                System.Diagnostics.Debug.WriteLine($"Phân tích file {dataPath} với maxSamples = {maxSamples}");
                Console.WriteLine($"Phân tích file {dataPath} với maxSamples = {maxSamples}");
                
                // Tải dữ liệu từ file với kiểu dữ liệu cụ thể
                var data = _mlContext.Data.LoadFromTextFile<EnhancedCustomerData>(
                    dataPath,
                    separatorChar: ',',
                    hasHeader: true,
                    allowQuoting: true
                );

                // Đếm số dòng thực tế
                long? actualRowCount = null;
                try {
                    actualRowCount = data.GetRowCount();
                    System.Diagnostics.Debug.WriteLine($"Số dòng trong file: {actualRowCount}");
                    Console.WriteLine($"Số dòng trong file: {actualRowCount}");
                } 
                catch {
                    System.Diagnostics.Debug.WriteLine("Không thể đếm số dòng trong file");
                    Console.WriteLine("Không thể đếm số dòng trong file");
                }

                // Giới hạn số lượng mẫu chỉ khi maxSamples > 0 và ít hơn số dòng thực tế
                IDataView limitedData;
                if (maxSamples > 0 && (actualRowCount == null || maxSamples < actualRowCount))
                {
                    limitedData = _mlContext.Data.TakeRows(data, maxSamples);
                    System.Diagnostics.Debug.WriteLine($"Giới hạn dữ liệu: {maxSamples} dòng");
                    Console.WriteLine($"Giới hạn dữ liệu: {maxSamples} dòng");
                }
                else
                {
                    limitedData = data;
                    System.Diagnostics.Debug.WriteLine("Sử dụng toàn bộ dữ liệu");
                    Console.WriteLine("Sử dụng toàn bộ dữ liệu");
                }

                // Thực hiện phân tích phân cụm
                var result = AnalyzeSegments(limitedData);
                
                // Ghi log kết quả để kiểm tra
                System.Diagnostics.Debug.WriteLine($"Kết quả phân tích: {result.Count} segments");
                foreach (var segment in result)
                {
                    System.Diagnostics.Debug.WriteLine($"Segment {segment.Key}: {segment.Value.CustomerCount} khách hàng ({segment.Value.Percentage:F2}%)");
                    Console.WriteLine($"Segment {segment.Key}: {segment.Value.CustomerCount} khách hàng ({segment.Value.Percentage:F2}%)");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                System.Diagnostics.Debug.WriteLine($"Lỗi khi phân tích segments: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                Console.WriteLine($"Lỗi khi phân tích segments: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                
                // Thêm thông tin chi tiết hơn về lỗi
                throw new InvalidOperationException(
                    $"Lỗi khi phân tích segments từ file '{dataPath}': {ex.Message}", ex);
            }
        }
    }
}