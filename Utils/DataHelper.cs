using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using CustomerSegmentationML.Models;
using CsvHelper;
using System.Globalization;

namespace CustomerSegmentationML.Utils
{
    public class DataHelper
    {
        private readonly MLContext _mlContext;

        public DataHelper()
        {
            _mlContext = new MLContext(seed: 0);
        }

        /// <summary>
        /// Load và split dữ liệu enhanced với preprocessing
        /// </summary>
        public (IDataView trainData, IDataView testData) LoadAndSplitEnhancedData(string dataPath, float testRatio = 0.2f)
        {
            // Đọc file CSV và convert sang Enhanced format
            var enhancedData = LoadEnhancedDataFromCSV(dataPath);

            // Tạo IDataView từ enhanced data
            var dataView = _mlContext.Data.LoadFromEnumerable(enhancedData);

            // Split data
            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: testRatio, seed: 0);

            return (split.TrainSet, split.TestSet);
        }

        /// <summary>
        /// Load dữ liệu từ CSV và convert sang EnhancedCustomerData
        /// </summary>
        private List<EnhancedCustomerData> LoadEnhancedDataFromCSV(string csvPath)
        {
            var result = new List<EnhancedCustomerData>();

            using (var reader = new StringReader(File.ReadAllText(csvPath)))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CustomerCSVEnhanced>().ToList();

                foreach (var record in records)
                {
                    result.Add(ConvertToEnhancedData(record));
                }
            }

            return result;
        }

        /// <summary>
        /// Convert CustomerCSVEnhanced sang EnhancedCustomerData với encoding
        /// </summary>
        private EnhancedCustomerData ConvertToEnhancedData(CustomerCSVEnhanced csvData)
        {
            return new EnhancedCustomerData
            {
                CustomerID = csvData.CustomerID,
                Gender = EncodeGender(csvData.Gender),
                Age = csvData.Age,
                AnnualIncome = csvData.AnnualIncome,
                SpendingScore = csvData.SpendingScore,
                Education = EncodeEducation(csvData.Education),
                Profession = EncodeProfession(csvData.Profession),
                WorkExperience = csvData.WorkExperience,
                FamilySize = csvData.FamilySize,
                City = EncodeCity(csvData.City),
                OnlineShoppingFreq = csvData.OnlineShoppingFreq,
                BrandLoyalty = csvData.BrandLoyalty,
                SocialMediaUsage = csvData.SocialMediaUsage,
                PreferredChannel = EncodePreferredChannel(csvData.PreferredChannel)
            };
        }

        /// <summary>
        /// Validate dữ liệu CSV trước khi load
        /// </summary>
        public DataValidationResult ValidateCSVFile(string csvPath)
        {
            var result = new DataValidationResult
            {
                IsValid = true,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            try
            {
                if (!File.Exists(csvPath))
                {
                    result.IsValid = false;
                    result.Errors.Add("File không tồn tại");
                    return result;
                }

                var fileInfo = new FileInfo(csvPath);
                if (fileInfo.Length == 0)
                {
                    result.IsValid = false;
                    result.Errors.Add("File rỗng");
                    return result;
                }

                // Đọc và validate headers
                using (var reader = new StringReader(File.ReadAllText(csvPath)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    var headers = csv.HeaderRecord;

                    var requiredHeaders = new[]
                    {
                        "CustomerID", "Gender", "Age", "AnnualIncome", "SpendingScore",
                        "Education", "Profession", "WorkExperience", "FamilySize",
                        "City", "OnlineShoppingFreq", "BrandLoyalty",
                        "SocialMediaUsage", "PreferredChannel"
                    };

                    var missingHeaders = requiredHeaders.Where(h => !headers.Contains(h)).ToList();
                    if (missingHeaders.Any())
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Thiếu các cột: {string.Join(", ", missingHeaders)}");
                    }

                    // Đếm số dòng dữ liệu
                    int rowCount = 0;
                    while (csv.Read())
                    {
                        rowCount++;
                    }

                    result.RowCount = rowCount;

                    if (rowCount < 10)
                    {
                        result.Warnings.Add("Dữ liệu có ít hơn 10 dòng, có thể không đủ để training");
                    }
                    else if (rowCount > 100000)
                    {
                        result.Warnings.Add("Dữ liệu lớn, quá trình training có thể mất nhiều thời gian");
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Lỗi đọc file: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Get thông tin tổng quan về dataset
        /// </summary>
        public DatasetSummary GetDatasetSummary(string csvPath)
        {
            var summary = new DatasetSummary();

            try
            {
                using (var reader = new StringReader(File.ReadAllText(csvPath)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<CustomerCSVEnhanced>().ToList();

                    summary.TotalRecords = records.Count;
                    summary.AverageAge = records.Average(r => r.Age);
                    summary.AverageIncome = records.Average(r => r.AnnualIncome);
                    summary.AverageSpendingScore = records.Average(r => r.SpendingScore);

                    summary.GenderDistribution = records
                        .GroupBy(r => r.Gender)
                        .ToDictionary(g => g.Key, g => (double)g.Count() / records.Count * 100);

                    summary.EducationDistribution = records
                        .GroupBy(r => r.Education)
                        .ToDictionary(g => g.Key, g => (double)g.Count() / records.Count * 100);

                    summary.CityDistribution = records
                        .GroupBy(r => r.City)
                        .ToDictionary(g => g.Key, g => (double)g.Count() / records.Count * 100);

                    summary.ProfessionDistribution = records
                        .GroupBy(r => r.Profession)
                        .ToDictionary(g => g.Key, g => (double)g.Count() / records.Count * 100);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi phân tích dataset: {ex.Message}");
            }

            return summary;
        }

        // Encoder methods
        private float EncodeGender(string gender)
        {
            if (gender?.ToLower() == "female") return 0f;
            if (gender?.ToLower() == "male") return 1f;
            return 0f;
        }

        private float EncodeEducation(string education)
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

        private float EncodeProfession(string profession)
        {
            switch (profession)
            {
                case "Student": return 0f;
                case "Healthcare": return 1f;
                case "Engineer": return 2f;
                case "Artist": return 3f;
                case "Lawyer": return 4f;
                case "Doctor": return 5f;
                case "Marketing": return 6f;
                case "Entertainment": return 7f;
                default: return 0f;
            }
        }

        private float EncodeCity(string city)
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

        private float EncodePreferredChannel(string channel)
        {
            switch (channel)
            {
                case "Online": return 0f;
                case "Offline": return 1f;
                case "Both": return 2f;
                default: return 2f;
            }
        }

        /// <summary>
        /// Export predictions ra file CSV
        /// </summary>
        public void ExportPredictions(List<EnhancedCustomerData> customers,
            List<CustomerPrediction> predictions, string outputPath)
        {
            var results = customers.Select((customer, index) => new
            {
                CustomerID = (int)customer.CustomerID,
                Age = (int)customer.Age,
                AnnualIncome = (int)customer.AnnualIncome,
                SpendingScore = (int)customer.SpendingScore,
                PredictedSegment = predictions[index].PredictedClusterId,
                DistanceToCluster = predictions[index].Distances?[0] ?? 0f
            }).ToList();

            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(results);
                File.WriteAllText(outputPath, writer.ToString());
            }
        }
    }

    public class DataValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public int RowCount { get; set; }
    }

    public class DatasetSummary
    {
        public int TotalRecords { get; set; }
        public double AverageAge { get; set; }
        public double AverageIncome { get; set; }
        public double AverageSpendingScore { get; set; }
        public Dictionary<string, double> GenderDistribution { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> EducationDistribution { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> CityDistribution { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> ProfessionDistribution { get; set; } = new Dictionary<string, double>();
    }
}