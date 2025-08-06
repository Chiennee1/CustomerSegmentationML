using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using System.Globalization;
using CustomerSegmentationML.Models;

namespace CustomerSegmentationML.ML.DataPreprocessing
{
    public class DatasetGenerator
    {
        private static readonly Random _random = new Random(42);

        private static readonly string[] _educationLevels = { "High School", "Bachelor", "Master", "PhD" };
        private static readonly string[] _professions = { "Student", "Healthcare", "Engineer", "Artist", "Lawyer", "Doctor", "Marketing", "Entertainment" };
        private static readonly string[] _cities = { "HaNoi", "HCM", "DaNang", "Others" };
        private static readonly string[] _channels = { "Online", "Offline", "Both" };

        public static void GenerateEnhancedDataset(string outputPath, int customerCount = 1000)
        {
            var customers = new List<CustomerCSVEnhanced>();

            for (int i = 1; i <= customerCount; i++)
            {
                customers.Add(GenerateRandomCustomer(i));
            }

            // Write to CSV
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(customers);
                File.WriteAllText(outputPath, writer.ToString());
            }
        }

        private static CustomerCSVEnhanced GenerateRandomCustomer(int id)
        {
            var age = _random.Next(18, 70);
            var income = GenerateRealisticIncome(age);
            var education = GenerateEducationByAge(age);
            var profession = GenerateProfessionByEducation(education);
            var workExp = Math.Max(0, age - 22 - _random.Next(0, 5));

            return new CustomerCSVEnhanced
            {
                CustomerID = id,
                Gender = _random.NextDouble() > 0.52 ? "Female" : "Male", // Vietnam demographics
                Age = age,
                AnnualIncome = income,
                SpendingScore = GenerateSpendingScore(income, age),
                Education = education,
                Profession = profession,
                WorkExperience = workExp,
                FamilySize = _random.Next(1, 6),
                City = _cities[_random.Next(_cities.Length)],
                OnlineShoppingFreq = _random.Next(0, 20),
                BrandLoyalty = _random.Next(1, 11),
                SocialMediaUsage = (float)(_random.NextDouble() * 8), // 0-8 hours
                PreferredChannel = _channels[_random.Next(_channels.Length)]
            };
        }

        private static int GenerateRealisticIncome(int age)
        {
            // Base income by age groups (Vietnam context)
            int baseIncome;
            if (age < 25) baseIncome = _random.Next(8, 20); // Fresh graduates
            else if (age < 35) baseIncome = _random.Next(15, 40); // Early career
            else if (age < 45) baseIncome = _random.Next(25, 70); // Mid career
            else if (age < 55) baseIncome = _random.Next(30, 100); // Senior positions
            else baseIncome = _random.Next(20, 80); // Pre-retirement

            return baseIncome;
        }

        private static int GenerateSpendingScore(int income, int age)
        {
            // Spending correlates with income but also age patterns
            double baseScore = (income / 100.0) * 50; // Income factor

            // Age factor
            if (age < 30) baseScore += _random.Next(10, 30); 
            else if (age < 50) baseScore += _random.Next(0, 20); 
            else baseScore += _random.Next(-10, 10);

            return Math.Max(1, Math.Min(100, (int)baseScore + _random.Next(-15, 15)));
        }

        private static string GenerateEducationByAge(int age)
        {
            if (age < 22) return "High School";

            var rand = _random.NextDouble();
            if (age < 30)
            {
                if (rand < 0.6) return "Bachelor";
                if (rand < 0.85) return "High School";
                return "Master";
            }
            else if (age < 40)
            {
                if (rand < 0.45) return "Bachelor";
                if (rand < 0.7) return "High School";
                if (rand < 0.95) return "Master";
                return "PhD";
            }
            else
            {
                if (rand < 0.4) return "Bachelor";
                if (rand < 0.7) return "High School";
                if (rand < 0.9) return "Master";
                return "PhD";
            }
        }

        private static string GenerateProfessionByEducation(string education)
        {
            switch (education)
            {
                case "High School":
                    return new[] { "Student", "Marketing", "Entertainment" }[_random.Next(3)];
                case "Bachelor":
                    return new[] { "Healthcare", "Engineer", "Marketing", "Entertainment" }[_random.Next(4)];
                case "Master":
                    return new[] { "Engineer", "Healthcare", "Lawyer", "Marketing" }[_random.Next(4)];
                case "PhD":
                    return new[] { "Doctor", "Engineer", "Lawyer" }[_random.Next(3)];
                default:
                    return "Student";
            }
        }

        // Generate Vietnam-specific e-commerce dataset
        public static void GenerateVietnamEcommerceData(string outputPath)
        {
            var products = new[] { "Electronics", "Fashion", "Food", "Books", "Home", "Beauty", "Sports", "Travel" };
            var brands = new[] { "Samsung", "Apple", "Xiaomi", "Zara", "H&M", "Nike", "Adidas" };

            var transactions = new List<object>();

            for (int i = 1; i <= 5000; i++)
            {
                transactions.Add(new
                {
                    TransactionID = i,
                    CustomerID = _random.Next(1, 1001),
                    Product = products[_random.Next(products.Length)],
                    Brand = brands[_random.Next(brands.Length)],
                    Amount = _random.Next(50, 2000) * 1000, // VND
                    Quantity = _random.Next(1, 5),
                    Date = DateTime.Now.AddDays(-_random.Next(1, 365)).ToString("yyyy-MM-dd"),
                    Platform = new[] { "Shopee", "Lazada", "Tiki", "Website" }[_random.Next(4)],
                    PaymentMethod = new[] { "COD", "Banking", "MoMo", "ZaloPay" }[_random.Next(4)]
                });
            }

            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(transactions);
                File.WriteAllText(outputPath, writer.ToString());
            }
        }
    }
}