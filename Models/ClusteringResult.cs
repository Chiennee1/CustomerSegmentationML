using System;
using System.Collections.Generic;
using Microsoft.ML;
using CustomerSegmentationML.ML.Algorithms;

namespace CustomerSegmentationML.Models
{
    public class ClusteringResult
    {
        public ITransformer Model { get; set; }
        public ClusteringMetrics TrainingMetrics { get; set; }
        public Dictionary<uint, SegmentAnalysis> Segments { get; set; }
        public string AlgorithmName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public DateTime TrainingTime { get; set; }
        public TimeSpan TrainingDuration { get; set; }
    }
}
