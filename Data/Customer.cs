using Microsoft.ML.Data;

namespace CustomerSegmentationML.Data
{
    public class Customer
    {
        [LoadColumn(0)]
        public float CustomerID { get; set; }

        [LoadColumn(1)]
        public string Gender { get; set; }

        [LoadColumn(2)]
        public float Age { get; set; }

        [LoadColumn(3)]
        public float AnnualIncome { get; set; }

        [LoadColumn(4)]
        public float SpendingScore { get; set; }
    }

    public class CustomerClusterPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedClusterId { get; set; }

        [ColumnName("Score")]
        public float[] Distances { get; set; }
    }
}