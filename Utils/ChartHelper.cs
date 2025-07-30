using CustomerSegmentationML.ML.Algorithms;
using CustomerSegmentationML.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CustomerSegmentationML.Utils
{
    public static class ChartHelper
    {
        public static Chart CreateSegmentDistributionChart(Dictionary<uint, SegmentAnalysis> segments)
        {
            var chart = new Chart();
            chart.Size = new Size(500, 400);

            var chartArea = new ChartArea("MainArea");
            chart.ChartAreas.Add(chartArea);

            var series = new Series("Segments");
            series.ChartType = SeriesChartType.Pie;
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "{0:F1}%";

            var colors = new Color[]
            {
                Color.LightBlue, Color.LightGreen, Color.Orange,
                Color.Pink, Color.Yellow, Color.LightCoral,
                Color.LightGray, Color.Cyan
            };

            int colorIndex = 0;
            foreach (var segment in segments.Values)
            {
                var point = new DataPoint();
                point.SetValueXY($"Segment {segment.SegmentId}", segment.Percentage);
                point.Color = colors[colorIndex % colors.Length];
                point.LegendText = $"Segment {segment.SegmentId} ({segment.CustomerCount})";
                series.Points.Add(point);
                colorIndex++;
            }

            chart.Series.Add(series);

            var legend = new Legend("MainLegend");
            legend.Docking = Docking.Right;
            chart.Legends.Add(legend);

            chart.Titles.Add(new Title("Customer Segment Distribution", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.Black));

            return chart;
        }

        public static Chart CreateFeatureComparisonChart(Dictionary<uint, SegmentAnalysis> segments, string feature)
        {
            var chart = new Chart();
            chart.Size = new Size(600, 400);

            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "Segments";
            chartArea.AxisY.Title = feature;
            chartArea.AxisX.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chartArea.AxisY.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chart.ChartAreas.Add(chartArea);

            var series = new Series(feature);
            series.ChartType = SeriesChartType.Column;
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "{0:F1}";

            foreach (var segment in segments.Values.OrderBy(s => s.SegmentId))
            {
                if (segment.AverageFeatures.ContainsKey(feature))
                {
                    var point = new DataPoint();
                    point.SetValueXY($"Segment {segment.SegmentId}", segment.AverageFeatures[feature]);
                    point.Color = GetSegmentColor((int)segment.SegmentId);
                    series.Points.Add(point);
                }
            }

            chart.Series.Add(series);

            chart.Titles.Add(new Title($"{feature} by Segment", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.Black));

            return chart;
        }

        public static Chart CreateScatterPlot(List<EnhancedCustomerData> customers,
            List<CustomerPrediction> predictions, string xFeature, string yFeature)
        {
            var chart = new Chart();
            chart.Size = new Size(600, 500);

            var chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = xFeature;
            chartArea.AxisY.Title = yFeature;
            chartArea.AxisX.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chartArea.AxisY.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            chart.ChartAreas.Add(chartArea);

            // Group by cluster
            var clusteredData = customers
                .Select((customer, index) => new { Customer = customer, Cluster = predictions[index].PredictedClusterId })
                .GroupBy(x => x.Cluster);

            foreach (var cluster in clusteredData)
            {
                var series = new Series($"Segment {cluster.Key}");
                series.ChartType = SeriesChartType.Point;
                series.MarkerStyle = MarkerStyle.Circle;
                series.MarkerSize = 8;
                series.Color = GetSegmentColor((int)cluster.Key);

                foreach (var item in cluster)
                {
                    var xValue = GetFeatureValue(item.Customer, xFeature);
                    var yValue = GetFeatureValue(item.Customer, yFeature);
                    series.Points.AddXY(xValue, yValue);
                }

                chart.Series.Add(series);
            }

            var legend = new Legend("MainLegend");
            legend.Docking = Docking.Right;
            chart.Legends.Add(legend);

            chart.Titles.Add(new Title($"{xFeature} vs {yFeature}", Docking.Top,
                new Font("Segoe UI", 14, FontStyle.Bold), Color.Black));

            return chart;
        }

        private static Color GetSegmentColor(int segmentId)
        {
            var colors = new Color[]
            {
                Color.Red, Color.Blue, Color.Green, Color.Orange,
                Color.Purple, Color.Brown, Color.Pink, Color.Gray
            };
            return colors[segmentId % colors.Length];
        }

        private static double GetFeatureValue(EnhancedCustomerData customer, string feature)
        {
            switch (feature)
            {
                case "Age": return customer.Age;
                case "AnnualIncome": return customer.AnnualIncome;
                case "SpendingScore": return customer.SpendingScore;
                case "Education": return customer.Education;
                case "WorkExperience": return customer.WorkExperience;
                case "FamilySize": return customer.FamilySize;
                case "OnlineShoppingFreq": return customer.OnlineShoppingFreq;
                case "BrandLoyalty": return customer.BrandLoyalty;
                case "SocialMediaUsage": return customer.SocialMediaUsage;
                default: return 0;
            }
        }
    }
}