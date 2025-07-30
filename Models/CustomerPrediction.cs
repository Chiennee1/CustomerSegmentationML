using Microsoft.ML.Data;

namespace CustomerSegmentationML.Models
{
    public class CustomerPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedClusterId { get; set; }

        [ColumnName("Score")]
        public float[] Distances { get; set; }
    }
}
