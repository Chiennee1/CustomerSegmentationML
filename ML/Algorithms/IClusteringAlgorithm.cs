using CustomerSegmentationML.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerSegmentationML.ML.Algorithms
{
    public interface IClusteringAlgorithm
    {
        string Name { get; }
        string Description { get; }
        Dictionary<string, object> Parameters { get; set; }

        Task<ClusteringResult> TrainAsync(IDataView data, IProgress<string> progress = null);
        Task<CustomerPrediction> PredictAsync(EnhancedCustomerData customer);
        ClusteringMetrics Evaluate(IDataView testData);
        void SaveModel(string path);
        void LoadModel(string path);
    }

    public class ClusteringMetrics
    {
        public double AverageDistance { get; set; }
        public double DaviesBouldinIndex { get; set; }
        public double SilhouetteScore { get; set; }
        public double InertiaScore { get; set; }
        public int NumberOfClusters { get; set; }
        public double CalinskyHarabaszScore { get; set; }
    }

    public class SegmentAnalysis
    {
        public uint SegmentId { get; set; }
        public int CustomerCount { get; set; }
        public double Percentage { get; set; }
        public Dictionary<string, double> AverageFeatures { get; set; }
        public string Description { get; set; }
        public string BusinessInsight { get; set; }
    }
}