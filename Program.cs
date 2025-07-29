using CsvHelper;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CustomerSegmentationML
{
    // Model dữ liệu đầu vào
    public class CustomerData
    {
        [LoadColumn(0)]
        public float CustomerID { get; set; }

        [LoadColumn(1)]
        public float Gender { get; set; } // 0: Female, 1: Male

        [LoadColumn(2)]
        public float Age { get; set; }

        [LoadColumn(3)]
        public float AnnualIncome { get; set; }

        [LoadColumn(4)]
        public float SpendingScore { get; set; }
    }

    // Model dự đoán
    public class CustomerPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedClusterId { get; set; }

        [ColumnName("Score")]
        public float[] Distances { get; set; }
    }

    // Model cho CSV - Khớp với tên cột trong file CSV thực tế
    public class CustomerCSV
    {
        public int CustomerID { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public int AnnualIncome { get; set; }
        public int SpendingScore { get; set; }
    }

    class Program
    {
        private static MLContext _mlContext;
        private static ITransformer _model;
        private static string _dataPath = "Data/Mall_Customers.csv";
        private static string _modelPath = "CustomerSegmentationModel.zip";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== CUSTOMER SEGMENTATION USING MACHINE LEARNING ===");
            Console.WriteLine();

            // Khởi tạo ML Context
            _mlContext = new MLContext(seed: 0);

            try
            {
                // Kiểm tra file dữ liệu
                if (!File.Exists(_dataPath))
                {
                    Console.WriteLine($"Không tìm thấy file dữ liệu: {_dataPath}");
                    Console.WriteLine("Vui lòng tải dataset Mall Customers và đặt vào thư mục Data/");
                    Console.ReadKey();
                    return;
                }

                // Bước 1: Đọc và xử lý dữ liệu
                var data = LoadData();

                // Bước 2: Hiển thị thông tin dataset
                DisplayDatasetInfo(data);

                // Bước 3: Huấn luyện mô hình
                TrainModel(data);

                while (true)
                {
                    Console.WriteLine("\nChọn chức năng:");
                    Console.WriteLine("1. Thêm khách hàng mới và phân cụm");
                    Console.WriteLine("2. Kiểm tra chi tiết mô hình và báo cáo");
                    Console.WriteLine("3. Thoát");
                    Console.Write("Lựa chọn: ");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                        AddAndPredictNewCustomer();
                    else if (choice == "2")
                        CheckModelDetails();
                    else if (choice == "3")
                        break;
                    else
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                }
                while (true)
                {
                    Console.WriteLine("\nChọn chức năng:");
                    Console.WriteLine("1. Thêm khách hàng mới và phân cụm");
                    Console.WriteLine("2. Kiểm tra chi tiết mô hình và báo cáo");
                    Console.WriteLine("3. Thoát");
                    Console.Write("Lựa chọn: ");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                        AddAndPredictNewCustomer();
                    else if (choice == "2")
                        CheckModelDetails();
                    else if (choice == "3")
                        break;
                    else
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                }
                while (true)
                {
                    Console.WriteLine("\nChọn chức năng:");
                    Console.WriteLine("1. Thêm khách hàng mới và phân cụm");
                    Console.WriteLine("2. Kiểm tra chi tiết mô hình và báo cáo");
                    Console.WriteLine("3. Thoát");
                    Console.Write("Lựa chọn: ");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                        AddAndPredictNewCustomer();
                    else if (choice == "2")
                        CheckModelDetails();
                    else if (choice == "3")
                        break;
                    else
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                }
                while (true)
                {
                    Console.WriteLine("\nChọn chức năng:");
                    Console.WriteLine("1. Thêm khách hàng mới và phân cụm");
                    Console.WriteLine("2. Kiểm tra chi tiết mô hình và báo cáo");
                    Console.WriteLine("3. Thoát");
                    Console.Write("Lựa chọn: ");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                        AddAndPredictNewCustomer();
                    else if (choice == "2")
                        CheckModelDetails();
                    else if (choice == "3")
                        break;
                    else
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                }
                while (true)
                {
                    Console.WriteLine("\nChọn chức năng:");
                    Console.WriteLine("1. Thêm khách hàng mới và phân cụm");
                    Console.WriteLine("2. Kiểm tra chi tiết mô hình và báo cáo");
                    Console.WriteLine("3. Thoát");
                    Console.Write("Lựa chọn: ");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                        AddAndPredictNewCustomer();
                    else if (choice == "2")
                        CheckModelDetails();
                    else if (choice == "3")
                        break;
                    else
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                }
                while (true)
                {
                    Console.WriteLine("\nChọn chức năng:");
                    Console.WriteLine("1. Thêm khách hàng mới và phân cụm");
                    Console.WriteLine("2. Kiểm tra chi tiết mô hình và báo cáo");
                    Console.WriteLine("3. Thoát");
                    Console.Write("Lựa chọn: ");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                        AddAndPredictNewCustomer();
                    else if (choice == "2")
                        CheckModelDetails();
                    else if (choice == "3")
                        break;
                    else
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                }
                }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.StackTrace}");
                Console.ReadKey();
            }
        }

        static IDataView LoadData()
        {
            Console.WriteLine("Đọc dữ liệu từ file CSV...");

            // Đọc CSV và chuyển đổi
            var customerList = new List<CustomerData>();

            var reader = new StringReader(File.ReadAllText(_dataPath));
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CustomerCSV>().ToList();

            foreach (var record in records)
            {
                customerList.Add(new CustomerData
                {
                    CustomerID = record.CustomerID,
                    Gender = record.Gender == "Male" ? 1 : 0,
                    Age = record.Age,
                    AnnualIncome = record.AnnualIncome,
                    SpendingScore = record.SpendingScore
                });
            }

            Console.WriteLine($"Đã đọc {customerList.Count} khách hàng từ dataset thực tế.");

            // Chuyển đổi thành IDataView
            return _mlContext.Data.LoadFromEnumerable(customerList);
        }

        static void DisplayDatasetInfo(IDataView dataView)
        {
            Console.WriteLine("\n=== THÔNG TIN DATASET ===");

            var customerData = _mlContext.Data.CreateEnumerable<CustomerData>(dataView, false).ToArray();

            Console.WriteLine($"Tổng số khách hàng: {customerData.Length}");
            Console.WriteLine($"Tuổi: {customerData.Min(c => c.Age)} - {customerData.Max(c => c.Age)} (trung bình: {customerData.Average(c => c.Age):F1})");
            Console.WriteLine($"Thu nhập: {customerData.Min(c => c.AnnualIncome)}k$ - {customerData.Max(c => c.AnnualIncome)}k$ (trung bình: {customerData.Average(c => c.AnnualIncome):F1}k$)");
            Console.WriteLine($"Điểm chi tiêu: {customerData.Min(c => c.SpendingScore)} - {customerData.Max(c => c.SpendingScore)} (trung bình: {customerData.Average(c => c.SpendingScore):F1})");
            Console.WriteLine($"Tỷ lệ giới tính - Nam: {customerData.Count(c => c.Gender == 1)} ({customerData.Count(c => c.Gender == 1) * 100.0 / customerData.Length:F1}%), Nữ: {customerData.Count(c => c.Gender == 0)} ({customerData.Count(c => c.Gender == 0) * 100.0 / customerData.Length:F1}%)");
        }

        static void TrainModel(IDataView dataView)
        {
            Console.WriteLine("\nBắt đầu huấn luyện mô hình K-Means Clustering...\n");

            // Định nghĩa pipeline với nhiều số cluster để tìm ra số tối ưu
            var pipeline = _mlContext.Transforms
                .Concatenate("Features", "Gender", "Age", "AnnualIncome", "SpendingScore")
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                // Có thể t đổi số lượng cluster để tìm ra số tối ưu
                .Append(_mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 5));

            // Huấn luyện mô hình
            _model = pipeline.Fit(dataView);

            Console.WriteLine("Mô hình đã được huấn luyện thành công với 5 clusters!");

            // Lưu mô hình
            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
            Console.WriteLine($"Mô hình đã được lưu tại: {_modelPath}");
        }

        static void EvaluateModel(IDataView dataView)
        {
            Console.WriteLine("\n=== ĐÁNH GIÁ MÔ HÌNH ===");

            var predictions = _model.Transform(dataView);
            var metrics = _mlContext.Clustering.Evaluate(predictions);

            Console.WriteLine($"Average Distance: {metrics.AverageDistance:F2}");
            Console.WriteLine($"Davies Bouldin Index: {metrics.DaviesBouldinIndex:F2}");
            Console.WriteLine("(Davies Bouldin Index càng thấp càng tốt - các cluster tách biệt rõ ràng)");
        }

        static void PredictSample()
        {
            Console.WriteLine("\n=== DỰ ĐOÁN CHO KHÁCH HÀNG MẪU ===");

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<CustomerData, CustomerPrediction>(_model);

            // Khách hàng mẫu dựa trên dữ liệu thực tế
            var sampleCustomers = new[]
            {
                new CustomerData { CustomerID = 999, Gender = 0, Age = 25, AnnualIncome = 70, SpendingScore = 80 }, // Nữ trẻ, thu nhập trung bình, chi tiêu cao
                new CustomerData { CustomerID = 998, Gender = 1, Age = 45, AnnualIncome = 120, SpendingScore = 30 }, // Nam trung niên, thu nhập cao, chi tiêu thấp
                new CustomerData { CustomerID = 997, Gender = 0, Age = 35, AnnualIncome = 50, SpendingScore = 50 }, // Nữ trung niên, thu nhập và chi tiêu trung bình
                new CustomerData { CustomerID = 996, Gender = 1, Age = 22, AnnualIncome = 30, SpendingScore = 90 }, // Nam trẻ, thu nhập thấp, chi tiêu cao
                new CustomerData { CustomerID = 995, Gender = 0, Age = 55, AnnualIncome = 100, SpendingScore = 85 } // Nữ trung niên, thu nhập cao, chi tiêu cao
            };

            foreach (var customer in sampleCustomers)
            {
                var prediction = predictionEngine.Predict(customer);
                string genderText = customer.Gender == 1 ? "Nam" : "Nữ";

                Console.WriteLine($"Khách hàng {customer.CustomerID} ({genderText}):");
                Console.WriteLine($"  - Tuổi: {customer.Age}, Thu nhập: {customer.AnnualIncome}k$, Điểm chi tiêu: {customer.SpendingScore}");
                Console.WriteLine($"  - Segment dự đoán: {prediction.PredictedClusterId}");
                Console.WriteLine($"  - Khoảng cách đến các centroid: [{string.Join(", ", prediction.Distances.Select(d => d.ToString("F2")))}]");
                Console.WriteLine();
            }
        }

        static void AnalyzeSegments(IDataView dataView)
        {
            Console.WriteLine("=== PHÂN TÍCH CHI TIẾT CÁC SEGMENT ===");

            var predictions = _model.Transform(dataView);
            var predictedData = _mlContext.Data.CreateEnumerable<CustomerPrediction>(predictions, false).ToArray();
            var originalData = _mlContext.Data.CreateEnumerable<CustomerData>(dataView, false).ToArray();

            // Nhóm theo segment
            var segments = new Dictionary<uint, List<(CustomerData Customer, CustomerPrediction Prediction)>>();

            for (int i = 0; i < originalData.Length; i++)
            {
                var clusterId = predictedData[i].PredictedClusterId;
                if (!segments.ContainsKey(clusterId))
                    segments[clusterId] = new List<(CustomerData, CustomerPrediction)>();

                segments[clusterId].Add((originalData[i], predictedData[i]));
            }

            // Phân tích từng segment
            foreach (var segment in segments.OrderBy(s => s.Key))
            {
                var customers = segment.Value.Select(s => s.Customer).ToList();

                Console.WriteLine($"\n📊 SEGMENT {segment.Key}: {customers.Count} khách hàng ({customers.Count * 100.0 / originalData.Length:F1}%)");
                Console.WriteLine($"   ├─ Tuổi: {customers.Min(c => c.Age)}-{customers.Max(c => c.Age)} (TB: {customers.Average(c => c.Age):F1})");
                Console.WriteLine($"   ├─ Thu nhập: {customers.Min(c => c.AnnualIncome)}-{customers.Max(c => c.AnnualIncome)}k$ (TB: {customers.Average(c => c.AnnualIncome):F1}k$)");
                Console.WriteLine($"   ├─ Điểm chi tiêu: {customers.Min(c => c.SpendingScore)}-{customers.Max(c => c.SpendingScore)} (TB: {customers.Average(c => c.SpendingScore):F1})");
                Console.WriteLine($"   ├─ Giới tính: Nam {customers.Count(c => c.Gender == 1)} ({customers.Count(c => c.Gender == 1) * 100.0 / customers.Count:F1}%), Nữ {customers.Count(c => c.Gender == 0)} ({customers.Count(c => c.Gender == 0) * 100.0 / customers.Count:F1}%)");

                // Đặc điểm segment
                var avgIncome = customers.Average(c => c.AnnualIncome);
                var avgSpending = customers.Average(c => c.SpendingScore);
                var avgAge = customers.Average(c => c.Age);

                string description = GetSegmentDescription(avgIncome, avgSpending, avgAge);
                Console.WriteLine($"   └─ 💡 {description}");
            }
        }

        static string GetSegmentDescription(double avgIncome, double avgSpending, double avgAge)
        {
            // Phân tích dựa trên dataset Mall Customers thực tế
            if (avgIncome > 70 && avgSpending > 70)
                return "🌟 VIP CUSTOMERS - Thu nhập cao, chi tiêu nhiều (Target cho sản phẩm premium)";
            else if (avgIncome > 70 && avgSpending < 40)
                return "💎 POTENTIAL CUSTOMERS - Thu nhập cao nhưng chi tiêu ít (Cần chiến lược kích thích mua sắm)";
            else if (avgIncome < 40 && avgSpending > 70)
                return "❤️ LOYAL CUSTOMERS - Thu nhập thấp nhưng chi tiêu nhiều (Khách hàng trung thành)";
            else if (avgSpending < 30)
                return "🔒 CAREFUL CUSTOMERS - Chi tiêu ít (Nhóm thận trọng trong mua sắm)";
            else if (avgAge > 50)
                return "👥 MATURE CUSTOMERS - Khách hàng trưởng thành";
            else
                return "⚖️ STANDARD CUSTOMERS - Cân bằng thu nhập và chi tiêu";
        }

        static void SaveResults(IDataView dataView)
        {
            Console.WriteLine("\n=== LUU KẾT QUẢ PHÂN TÍCH ===");

            // Tạo thư mục Results
            Directory.CreateDirectory("Results");

            var predictions = _model.Transform(dataView);
            var predictedData = _mlContext.Data.CreateEnumerable<CustomerPrediction>(predictions, false).ToArray();
            var originalData = _mlContext.Data.CreateEnumerable<CustomerData>(dataView, false).ToArray();

            // Tạo file kết quả với thông tin chi tiết
            var results = new List<object>();
            for (int i = 0; i < originalData.Length; i++)
            {
                var avgIncome = originalData.Where((_, idx) => predictedData[idx].PredictedClusterId == predictedData[i].PredictedClusterId)
                                           .Average(c => c.AnnualIncome);
                var avgSpending = originalData.Where((_, idx) => predictedData[idx].PredictedClusterId == predictedData[i].PredictedClusterId)
                                            .Average(c => c.SpendingScore);
                var avgAge = originalData.Where((_, idx) => predictedData[idx].PredictedClusterId == predictedData[i].PredictedClusterId)
                                       .Average(c => c.Age);

                results.Add(new
                {
                    CustomerID = originalData[i].CustomerID,
                    Gender = originalData[i].Gender == 1 ? "Male" : "Female",
                    Age = originalData[i].Age,
                    AnnualIncome = originalData[i].AnnualIncome,
                    SpendingScore = originalData[i].SpendingScore,
                    PredictedSegment = predictedData[i].PredictedClusterId,
                    SegmentDescription = GetSegmentDescription(avgIncome, avgSpending, avgAge)
                });
            }

            // Lưu vào CSV
            var writer = new StringWriter();
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(results);
            File.WriteAllText("Results/customer_segments.csv", writer.ToString());

            Console.WriteLine("✅ Kết quả chi tiết: Results/customer_segments.csv");

            // Tạo báo cáo tóm tắt chi tiết
            var summary = new StringBuilder();
            summary.AppendLine("CUSTOMER SEGMENTATION ANALYSIS REPORT");
            summary.AppendLine("=====================================");
            summary.AppendLine($"Dataset: Mall Customers");
            summary.AppendLine($"Ngày phân tích: {DateTime.Now:dd/MM/yyyy HH:mm}");
            summary.AppendLine($"Tổng số khách hàng: {originalData.Length}");
            summary.AppendLine($"Số segment: {predictedData.Select(p => p.PredictedClusterId).Distinct().Count()}");
            summary.AppendLine($"Thuật toán: K-Means Clustering");
            summary.AppendLine();

            summary.AppendLine("PHÂN BỘ SEGMENT:");
            summary.AppendLine("================");
            var segments = predictedData.GroupBy(p => p.PredictedClusterId);
            foreach (var segment in segments.OrderBy(s => s.Key))
            {
                var customers = originalData.Where((_, idx) => predictedData[idx].PredictedClusterId == segment.Key).ToList();
                var avgIncome = customers.Average(c => c.AnnualIncome);
                var avgSpending = customers.Average(c => c.SpendingScore);
                var avgAge = customers.Average(c => c.Age);

                summary.AppendLine($"Segment {segment.Key}: {segment.Count()} khách hàng ({segment.Count() * 100.0 / originalData.Length:F1}%)");
                summary.AppendLine($"  - Thu nhập TB: {avgIncome:F1}k$");
                summary.AppendLine($"  - Chi tiêu TB: {avgSpending:F1}");
                summary.AppendLine($"  - Tuổi TB: {avgAge:F1}");
                summary.AppendLine($"  - Mô tả: {GetSegmentDescription(avgIncome, avgSpending, avgAge)}");
                summary.AppendLine();
            }

            File.WriteAllText("Results/analysis_report.txt", summary.ToString());
            Console.WriteLine("📄 Báo cáo tóm tắt: Results/analysis_report.txt");

            Console.WriteLine("\n Hoàn thành phân tích Customer Segmentation với dataset thực tế!");
        }

        static void AddAndPredictNewCustomer()
        {
            Console.WriteLine("\n=== THÊM VÀ PHÂN CỤM KHÁCH HÀNG MỚI ===");
            Console.Write("Nhập giới tính (Male/Female): ");
            string gender = Console.ReadLine();
            Console.Write("Nhập tuổi: ");
            int age = int.Parse(Console.ReadLine());
            Console.Write("Nhập thu nhập (k$): ");
            int income = int.Parse(Console.ReadLine());
            Console.Write("Nhập điểm chi tiêu (1-100): ");
            int score = int.Parse(Console.ReadLine());

            // Tạo CustomerData mới
            var newCustomer = new CustomerData
            {
                CustomerID = DateTime.Now.Ticks % 1000000, // ID tạm thời
                Gender = gender == "Male" ? 1 : 0,
                Age = age,
                AnnualIncome = income,
                SpendingScore = score
            };

            // Dự đoán phân cụm
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<CustomerData, CustomerPrediction>(_model);
            var prediction = predictionEngine.Predict(newCustomer);

            Console.WriteLine($"Khách hàng mới được phân vào segment: {prediction.PredictedClusterId}");
            Console.WriteLine($"Khoảng cách đến các centroid: [{string.Join(", ", prediction.Distances.Select(d => d.ToString("F2")))}]");

            // Ghi vào file CSV
            using (var sw = new StreamWriter(_dataPath, true))
            {
                sw.WriteLine($"{newCustomer.CustomerID},{gender},{age},{income},{score}");
            }
            Console.WriteLine("Đã thêm khách hàng mới vào file dữ liệu.");
        }

        static void CheckModelDetails()
        {
            var data = LoadData();
            AnalyzeSegments(data);
            SaveResults(data);
            Console.WriteLine("Đã cập nhật báo cáo chi tiết và file kết quả.");
        }
    }
}