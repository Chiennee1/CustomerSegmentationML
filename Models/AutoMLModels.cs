using System;
using System.Collections.Generic;
using Microsoft.ML;
using CustomerSegmentationML.ML.Algorithms;

namespace CustomerSegmentationML.Models
{
    public class AutoMLResult
    {
        public AlgorithmResult BestResult { get; set; }
        public List<AlgorithmResult> AllResults { get; set; }
        public double TotalTimeSpent { get; set; }
        public int TotalAlgorithmsTested { get; set; }
        public string Summary => $"Tested {TotalAlgorithmsTested} configurations in {TotalTimeSpent:F1}s. Best: {BestResult?.AlgorithmName}";
    }

    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public ClusteringMetrics Metrics { get; set; }
        public TimeSpan TrainingDuration { get; set; }
        public ITransformer Model { get; set; }
        public double OverallScore => CalculateScore();

        private double CalculateScore()
        {
            if (Metrics == null) return 0;
            return Math.Max(0, Metrics.SilhouetteScore) - (Metrics.DaviesBouldinIndex * 0.1);
        }
    }

    public class AutoMLProgress
    {
        public string CurrentAlgorithm { get; set; }
        public double AlgorithmProgress { get; set; }
        public double OverallProgress { get; set; }
        public string Message { get; set; }
        public bool HasError { get; set; }
    }
}