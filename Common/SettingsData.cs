using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Common
{
    public class SettingsData
    {
        private const int MinApiPollingRate = 0;
        private const int MaxApiPollingRate = 60000;

        private const int DefaultApiPollingRate = 1000;

        /// <summary>
        /// List of IDs of enabled rules (for overlay)
        /// </summary>
        [Browsable(false)]
        public List<string> RulesOnOverlay { get; set; }

        /// <summary>
        /// API Polling Rate
        /// </summary>
        [DisplayName("API Polling Rate (ms)")]
        [Category("Advanced")]
        public string ApiPollingRate { get; set; }

        [Browsable(false)]
        public Dictionary<string, int> FlashThresholds { get; set; }
        
        [Browsable(false)]
        public Dictionary<string, int> DisableThresholds { get; set; }

        public SettingsData() {
            RulesOnOverlay = new List<string>();
            ApiPollingRate = DefaultApiPollingRate.ToString();
            FlashThresholds = new Dictionary<string, int>();
            DisableThresholds = new Dictionary<string, int>();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            var filePath = GetFilePath();
            File.WriteAllText(filePath, json);
        }

        public void Load()
        {
            var filePath = GetFilePath();
            try
            {
                if (File.Exists(filePath))
                {
                    var jsonContent = File.ReadAllText(filePath);
                    var data = JsonSerializer.Deserialize<SettingsData>(jsonContent);

                    var validationResult = Validate();

                    if (validationResult.IsValid)
                    {
                        // TODO Check for matching rules and no random values
                        RulesOnOverlay = data.RulesOnOverlay; 
                        ApiPollingRate = data.ApiPollingRate;
                        FlashThresholds = data.FlashThresholds;
                        DisableThresholds = data.DisableThresholds;
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch error while reading file
            }
        }

        public ValidationResult Validate()
        {
            var validationResults = new List<ValidationResult>();
            
            ValidateApiPollingRate(validationResults);
            ValidateThresholds(validationResults);

            return new ValidationResult(validationResults);
        }

        private void ValidateApiPollingRate(ICollection<ValidationResult> results)
        {
            if (!int.TryParse(ApiPollingRate, out var number) || number < MinApiPollingRate || number > MaxApiPollingRate)
            {
                results.Add(new ValidationResult("API Polling Rate must be between 0 and 60000 ms."));
            }
        }

        private void ValidateThresholds(ICollection<ValidationResult> results)
        {
            var allThresholdValues = FlashThresholds.Values.ToList();
            allThresholdValues.AddRange(DisableThresholds.Values);
            
            // Ensure no values that are negative
            if (allThresholdValues.Any(values => values < 0))
            {
                results.Add(new ValidationResult("Threshold values (in seconds) cannot be negative."));
                return; // Only add it to the results once.
            }
        }

        public string GetFilePath()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var saveFolder = Path.Combine(documentsPath, "NLOverlay");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }
            return Path.Combine(saveFolder, "settings.json");
        }
    }
}
