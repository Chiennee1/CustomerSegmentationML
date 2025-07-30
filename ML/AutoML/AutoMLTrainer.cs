using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using CustomerSegmentationML.Models;
using CustomerSegmentationML.ML.Algorithms;

namespace CustomerSegmentationML.ML.AutoML
{
    public class AutoMLTrainer
    {
        private readonly List<IClusteringAlgorithm> _algorithms;
        private readonly MLContext _mlContext;

        public AutoMLTrainer()
        {
            _mlContext = new MLContext(seed: 0);
            _algorithms = new List<IClusteringAlgorithm>
            {
                new KMeansClusterer(),
                // Add more algorithms later
            };
        }

        public async Task<AutoMLResult> FindBestModelAsync(IDataView trainData, IDataView validationData,
            IProgress<AutoMLProgress> progress = null)
        {
            var results = new List<AlgorithmResult>();
            var totalAlgorithms = _algorithms.Count;
            var currentAlgorithm = 0;

            foreach (var algorithm in _algorithms)
            {
                currentAlgorithm++;
                progress?.Report(new AutoMLProgress
                {
                    CurrentAlgorithm = algorithm.Name,
                    AlgorithmProgress = 0,
                    OverallProgress = (double)(currentAlgorithm - 1) / totalAlgorithms * 100,
                    Message = $"Đang thử nghiệm {algorithm.Name}..."
                });

                try
                {
                    var hyperparameterResults = await TryDifferentHyperparameters(algorithm, trainData, validationData, progress);
                    results.AddRange(hyperparameterResults);
                }
                catch (Exception ex)
                {
                    progress?.Report(new AutoMLProgress
                    {
                        CurrentAlgorithm = algorithm.Name,
                        Message = $"Lỗi khi thử {algorithm.Name}: {ex.Message}",
                        HasError = true
                    });
                }

                progress?.Report(new AutoMLProgress
                {
                    CurrentAlgorithm = algorithm.Name,
                    AlgorithmProgress = 100,
                    OverallProgress = (double)currentAlgorithm / totalAlgorithms * 100,
                    Message = $"Hoàn thành {algorithm.Name}"
                });
            }

            // Find best model based on multiple criteria
            var bestResult = results
                .Where(r => !double.IsNaN(r.Metrics.SilhouetteScore))
                .OrderByDescending(r => CalculateOverallScore(r.Metrics))
                .FirstOrDefault();

            return new AutoMLResult
            {
                BestResult = bestResult,
                AllResults = results,
                TotalTimeSpent = results.Sum(r => r.TrainingDuration.TotalSeconds),
                TotalAlgorithmsTested = results.Count
            };
        }

        private async Task<List<AlgorithmResult>> TryDifferentHyperparameters(IClusteringAlgorithm algorithm,
            IDataView trainData, IDataView validationData, IProgress<AutoMLProgress> progress)
        {
            var results = new List<AlgorithmResult>();

            if (algorithm is KMeansClusterer)
            {
                var clusterCounts = new[] { 3, 4, 5, 6, 7, 8 };
                var maxIterations = new[] { 50, 100, 200 };

                var totalCombinations = clusterCounts.Length * maxIterations.Length;
                var currentCombination = 0;

                foreach (var clusters in clusterCounts)
                {
                    foreach (var maxIter in maxIterations)
                    {
                        currentCombination++;
                        var combinationProgress = (double)currentCombination / totalCombinations * 100;

                        progress?.Report(new AutoMLProgress
                        {
                            CurrentAlgorithm = algorithm.Name,
                            AlgorithmProgress = combinationProgress,
                            Message = $"Testing K={clusters}, MaxIter={maxIter}"
                        });

                        try
                        {
                            var cloner = new KMeansClusterer();
                            cloner.Parameters["NumberOfClusters"] = clusters;
                            cloner.Parameters["MaxIterations"] = maxIter;

                            var startTime = DateTime.Now;
                            var result = await cloner.TrainAsync(trainData);
                            var endTime = DateTime.Now;

                            var metrics = cloner.Evaluate(validationData);

                            results.Add(new AlgorithmResult
                            {
                                AlgorithmName = algorithm.Name,
                                Parameters = new Dictionary<string, object>(cloner.Parameters),
                                Metrics = metrics,
                                TrainingDuration = endTime - startTime,
                                Model = result.Model
                            });
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with other combinations
                            progress?.Report(new AutoMLProgress
                            {
                                CurrentAlgorithm = algorithm.Name,
                                Message = $"Error with K={clusters}, MaxIter={maxIter}: {ex.Message}",
                                HasError = true
                            });
                        }
                    }
                }
            }

            return results;
        }

        private double CalculateOverallScore(ClusteringMetrics metrics)
        {
            // Weighted score combining multiple metrics
            var silhouetteWeight = 0.4;
            var daviesBouldinWeight = 0.3; // Lower is better, so invert
            var averageDistanceWeight = 0.3; // Lower is better, so invert

            var normalizedSilhouette = Math.Max(0, metrics.SilhouetteScore);
            var normalizedDaviesBouldin = Math.Max(0, 1.0 / (1.0 + metrics.DaviesBouldinIndex));
            var normalizedDistance = Math.Max(0, 1.0 / (1.0 + metrics.AverageDistance));

            return (silhouetteWeight * normalizedSilhouette) +
                   (daviesBouldinWeight * normalizedDaviesBouldin) +
                   (averageDistanceWeight * normalizedDistance);
        }
    }
}