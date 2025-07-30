using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;

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

        [Name("Annual Income (k$)")]
        public int AnnualIncome { get; set; }

        [Name("Spending Score (1-100)")]
        public int SpendingScore { get; set; } 
    }

    class Program
    {
        private static MLContext _mlContext;
        private static ITransformer _model;
        private static string _dataPath = "D:\\StudyPython\\PhanCumkhachHang\\CustomerSegmentationML\\Data\\Mall_Customers.csv";
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


                // Bước 1: Đọc và phân chia dữ liệu
                var (trainData, testData) = LoadDataSplit();
                DisplayDatasetInfo(trainData);
                DisplayDatasetInfo(testData);
                // Bước 2: Huấn luyện mô hình
                TrainModel(trainData);

                while (true)
                {
                    Console.WriteLine("\nChọn chức năng:");
                    Console.WriteLine("1. Xem mẫu dữ liệu trong file CSV");
                    Console.WriteLine("2. Kiểm tra mô hình trên dữ liệu TRAIN");
                    Console.WriteLine("3. Kiểm tra mô hình trên dữ liệu TEST");
                    Console.WriteLine("4. Xem mô hình đã lưu");
                    Console.WriteLine("5. Thêm khách hàng mới và phân cụm");
                    Console.WriteLine("6. Xem chi tiết kết quả phân tích");
                    Console.WriteLine("7. Xem báo cáo tóm tắt");
                    Console.WriteLine("8. Dự đoán phân cụm cho một số khách hàng trong bộ TEST");
                    Console.WriteLine("9. Xuất báo cáo phân khúc ra file TXT");
                    Console.WriteLine("10. Xuất báo cáo phân khúc ra file Excel");
                    Console.WriteLine("11. Thoát");
                    Console.Write("Lựa chọn: ");
                    var choice = Console.ReadLine();
                    if (choice == "1")
                        ShowSampleCSVData();
                    else if (choice == "5")
                        AddAndPredictNewCustomer();
                    else if (choice == "2")
                        TestModelOnTrain(trainData);
                    else if (choice == "3")
                        TestModelOnTest(testData);
                    else if (choice == "4")
                        ViewSavedModel();
                    else if (choice == "6")
                        ViewAnalysisDetails();
                    else if (choice == "7")
                        ViewSummaryReport();
                    else if (choice == "7")
                        PredictSample(testData);
                    else if (choice == "8")
                        ExportSegmentReportToTxt();
                    else if (choice == "10")
                        ExportToExcel();
                    else if (choice == "11")
                        break;

                    else
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                }

                TestModelOnTrain(trainData);
                TestModelOnTest(testData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.StackTrace}");
                Console.ReadKey();
            }
        }

        static (IDataView trainData, IDataView testData) LoadDataSplit(float testFraction = 0.2f)
{
    Console.WriteLine("Đọc dữ liệu từ file CSV...");

    var customerList = new List<CustomerData>();

    var reader = new StringReader(File.ReadAllText(_dataPath));
    var csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
    {
        MissingFieldFound = null
    };
    var csv = new CsvReader(reader, csvConfig);

    var records = csv.GetRecords<CustomerCSV>()
        .Where(r => r.SpendingScore != 0)
        .ToList();

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

    // Shuffle dữ liệu để chia ngẫu nhiên
    var rnd = new Random(0);
    var shuffled = customerList.OrderBy(x => rnd.Next()).ToList();

    int testCount = (int)(shuffled.Count * testFraction);
    var testList = shuffled.Take(testCount).ToList();
    var trainList = shuffled.Skip(testCount).ToList();

    var trainData = _mlContext.Data.LoadFromEnumerable(trainList);
    var testData = _mlContext.Data.LoadFromEnumerable(testList);

    Console.WriteLine($"Train: {trainList.Count} - Test: {testList.Count}");

    return (trainData, testData);
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

       
        static void PredictSample(IDataView testData)
        {
            Console.WriteLine("\n=== DỰ ĐOÁN PHÂN CỤM CHO KHÁCH HÀNG TRONG BỘ TEST ===");

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<CustomerData, CustomerPrediction>(_model);

            // Lấy danh sách khách hàng từ bộ test
            var testCustomers = _mlContext.Data.CreateEnumerable<CustomerData>(testData, reuseRowObject: false).ToList();

            // Dự đoán cho 5 khách hàng đầu tiên trong bộ test
            foreach (var customer in testCustomers.Take(5))
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
                Console.WriteLine($"   ├─ Giới tính: Nam {customers.Count(c => c.Gender == 1)} ({customers.Count(c => c.Gender == 1) * 100.0 / customers.Count:F1}%), Nữ {customers.Count(c => c.Gender == 0)} " +
                    $"({customers.Count(c => c.Gender == 0) * 100.0 / customers.Count:F1}%)");

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

        static void CheckModelDetails(IDataView dataView)
        {
            AnalyzeSegments(dataView);
            SaveResults(dataView);
            Console.WriteLine("Đã cập nhật báo cáo chi tiết và file kết quả.");
        }

        static void ViewSavedModel()
        {
            if (!File.Exists(_modelPath))
            {
                Console.WriteLine($"Không tìm thấy file mô hình: {_modelPath}");
                return;
            }

            // Nạp lại mô hình từ file zip
            var loadedModel = _mlContext.Model.Load(_modelPath, out var inputSchema);

            Console.WriteLine("=== THÔNG TIN MÔ HÌNH ĐÃ LƯU ===");
            Console.WriteLine($"Đường dẫn: {_modelPath}");
            Console.WriteLine($"Schema đầu vào:");
            foreach (var col in inputSchema)
            {
                Console.WriteLine($" - {col.Name}: {col.Type}");
            }
            Console.WriteLine("Bạn có thể dùng mô hình này để dự đoán hoặc phân tích lại dữ liệu.");
        }

        static void ViewAnalysisDetails()
        {
            string resultPath = "Results/customer_segments.csv";
            if (!File.Exists(resultPath))
            {
                Console.WriteLine("Chưa có file kết quả phân tích. Hãy chạy phân tích trước.");
                return;
            }

            Console.WriteLine("=== KẾT QUẢ PHÂN TÍCH CHI TIẾT ===");
            var lines = File.ReadAllLines(resultPath);
            for (int i = 0; i < Math.Min(10, lines.Length); i++)
            {
                Console.WriteLine(lines[i]);
            }
            if (lines.Length > 10)
                Console.WriteLine($"... (Tổng cộng {lines.Length - 1} khách hàng)");
        }

        static void ViewSummaryReport()
        {
            string reportPath = "Results/analysis_report.txt";
            if (!File.Exists(reportPath))
            {
                Console.WriteLine("Chưa có báo cáo tóm tắt. Hãy chạy phân tích trước.");
                return;
            }

            Console.WriteLine("=== BÁO CÁO TÓM TẮT ===");
            string report = File.ReadAllText(reportPath);
            Console.WriteLine(report);
        }

        static void TestModelOnTrain(IDataView trainData)
        {
            Console.WriteLine("\n=== ĐÁNH GIÁ MÔ HÌNH TRÊN DỮ LIỆU TRAIN ===");
            EvaluateModel(trainData);
            AnalyzeSegments(trainData);
        }

        static void TestModelOnTest(IDataView testData)
        {
            Console.WriteLine("\n=== ĐÁNH GIÁ MÔ HÌNH TRÊN DỮ LIỆU TEST ===");
            EvaluateModel(testData);
            AnalyzeSegments(testData);
        }

        static void ExportSegmentReportToTxt()
        {
            string resultPath = "Results/customer_segments.csv";
            string segmentReportPath = "Results/segment_report.txt";
            if (!File.Exists(resultPath))
            {
                Console.WriteLine("Chưa có file kết quả phân tích. Hãy chạy phân tích trước.");
                return;
            }

            var lines = File.ReadAllLines(resultPath);
            var segments = new Dictionary<string, List<string>>();

            // Bỏ dòng tiêu đề
            for (int i = 1; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                var segment = cols[5]; // PredictedSegment
                if (!segments.ContainsKey(segment))
                    segments[segment] = new List<string>();
                segments[segment].Add(lines[i]);
            }

            var sb = new StringBuilder();
            sb.AppendLine("BÁO CÁO PHÂN KHÚC KHÁCH HÀNG");
            sb.AppendLine("============================");
            foreach (var kv in segments)
            {
                sb.AppendLine($"Phân khúc {kv.Key}: {kv.Value.Count} khách hàng");
                foreach (var line in kv.Value.Take(5)) // Hiển thị 5 khách hàng đầu
                    sb.AppendLine("  " + line);
                sb.AppendLine();
            }

            File.WriteAllText(segmentReportPath, sb.ToString());
            Console.WriteLine($"✅ Đã xuất báo cáo phân khúc: {segmentReportPath}");
        }

        // Xuất báo cáo phân khúc ra file Excel
        static void ExportToExcel()
        {
            string csvPath = "Results/customer_segments.csv";
            string excelPath = "Results/customer_segments.xlsx";
            if (!File.Exists(csvPath))
            {
                Console.WriteLine("Chưa có file kết quả phân tích.");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Segments");

            for (int i = 0; i < lines.Length; i++)
            {
                var cols = lines[i].Split(',');
                for (int j = 0; j < cols.Length; j++)
                    ws.Cell(i + 1, j + 1).Value = cols[j];
            }

            wb.SaveAs(excelPath);
            Console.WriteLine($"✅ Đã xuất file Excel: {excelPath}");
        }

        static void ShowSampleCSVData(int sampleCount = 10)
{
    if (!File.Exists(_dataPath))
    {
        Console.WriteLine($"Không tìm thấy file dữ liệu: {_dataPath}");
        return;
    }

    Console.WriteLine("\n=== MỘT SỐ DÒNG DỮ LIỆU TỪ FILE CSV ===");
    using (var reader = new StreamReader(_dataPath))
    {
        int lineNum = 0;
        string line;
        while ((line = reader.ReadLine()) != null && lineNum < sampleCount + 1)
        {
            Console.WriteLine(line);
            lineNum++;
        }
        if (lineNum <= 1)
            Console.WriteLine("File không có dữ liệu!");
        else if (reader.ReadLine() != null)
            Console.WriteLine("... (Dữ liệu còn nữa, chỉ hiển thị một phần)");
    }
}
    }
}