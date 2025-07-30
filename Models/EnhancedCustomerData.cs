using Microsoft.ML.Data;

namespace CustomerSegmentationML.Models
{
    public class EnhancedCustomerData
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

        // Enhanced features
        [LoadColumn(5)]
        public float Education { get; set; } // 0: High School, 1: Bachelor, 2: Master, 3: PhD

        [LoadColumn(6)]
        public float Profession { get; set; } // 0: Student, 1: Healthcare, 2: Engineer, 3: Artist, 4: Lawyer, 5: Doctor, 6: Marketing, 7: Entertainment

        [LoadColumn(7)]
        public float WorkExperience { get; set; } // Years of experience

        [LoadColumn(8)]
        public float FamilySize { get; set; } // Number of family members

        [LoadColumn(9)]
        public float City { get; set; } // 0: HaNoi, 1: HCM, 2: DaNang, 3: Others

        [LoadColumn(10)]
        public float OnlineShoppingFreq { get; set; } // Times per month

        [LoadColumn(11)]
        public float BrandLoyalty { get; set; } // 1-10 scale

        [LoadColumn(12)]
        public float SocialMediaUsage { get; set; } // Hours per day

        [LoadColumn(13)]
        public float PreferredChannel { get; set; } // 0: Online, 1: Offline, 2: Both
    }

    public class CustomerCSVEnhanced
    {
        public int CustomerID { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public int AnnualIncome { get; set; }
        public int SpendingScore { get; set; }
        public string Education { get; set; }
        public string Profession { get; set; }
        public int WorkExperience { get; set; }
        public int FamilySize { get; set; }
        public string City { get; set; }
        public int OnlineShoppingFreq { get; set; }
        public int BrandLoyalty { get; set; }
        public float SocialMediaUsage { get; set; }
        public string PreferredChannel { get; set; }
    }
}