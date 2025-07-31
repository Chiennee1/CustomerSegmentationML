using System;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace CustomerSegmentationML.Utils
{
    public class ConfigHelper
    {
        private static readonly string ConfigPath = "config.json";
        private static AppConfig _config;

        public static AppConfig GetConfig()
        {
            if (_config == null)
            {
                LoadConfig();
            }
            return _config;
        }

        private static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    _config = JsonConvert.DeserializeObject<AppConfig>(json);
                }
                else
                {
                    _config = CreateDefaultConfig();
                    SaveConfig();
                }
            }
            catch (Exception)
            {
                _config = CreateDefaultConfig();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        private static AppConfig CreateDefaultConfig()
        {
            return new AppConfig
            {
                DefaultAlgorithm = "AutoML",
                MaxTrainingTimeMinutes = 30,
                DefaultNumberOfClusters = 5,
                TestDataPercentage = 0.2f,
                EnableAutoSave = true,
                ChartColors = new string[]
                {
                    "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4",
                    "#FFEAA7", "#DDA0DD", "#98D8C8", "#F7DC6F"
                },
                ExportFormats = new string[] { "CSV", "Excel", "JSON" },
                Language = "vi",
                Theme = "Light"
            };
        }
    }

    public class AppConfig
    {
        public string DefaultAlgorithm { get; set; }
        public int MaxTrainingTimeMinutes { get; set; }
        public int DefaultNumberOfClusters { get; set; }
        public float TestDataPercentage { get; set; }
        public bool EnableAutoSave { get; set; }
        public string[] ChartColors { get; set; }
        public string[] ExportFormats { get; set; }
        public string Language { get; set; }
        public string Theme { get; set; }
    }
}