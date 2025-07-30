using System;
using System.Collections.Generic;
using CustomerSegmentationML.ML.Algorithms;

namespace CustomerSegmentationML.Models
{
    public class ModelEvaluation
    {
        public ClusteringMetrics Metrics { get; set; }
        public string Notes { get; set; }
        public Dictionary<string, double> AdditionalScores { get; set; }
        public DateTime EvaluationTime { get; set; }
    }
}
