using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Common
{
    public class SettingsData
    {
        private const int MinOverlayOpacity = 0;
        private const int MaxOverlayOpacity = 100;
        private const int MinApiPollingRate = 0;
        private const int MaxApiPollingRate = 60000;

        private const int DefaultOverlayOpacity = 20;
        private const int DefaultApiPollingRate = 1000;

        /// <summary>
        /// List of IDs of enabled rules (for overlay)
        /// </summary>
        [Browsable(false)]
        public List<string> EnabledRuleIds { get; set; }

        /// <summary>
        /// Overlay opacity
        /// </summary>
        [Browsable(false)]
        public string OverlayOpacity { get; set; }

        /// <summary>
        /// API Polling Rate
        /// </summary>
        [DisplayName("API Polling Rate (ms)")]
        [Category("Advanced")]
        public string ApiPollingRate { get; set; }

        public SettingsData() {
            EnabledRuleIds = new List<string>();
            OverlayOpacity = DefaultOverlayOpacity.ToString();
            ApiPollingRate = DefaultApiPollingRate.ToString();
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
                        EnabledRuleIds = data.EnabledRuleIds;
                        OverlayOpacity = data.OverlayOpacity;
                        ApiPollingRate = data.ApiPollingRate;
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

            ValidateEnabledRules(validationResults);
            ValidateOverlayOpacity(validationResults);
            ValidateApiPollingRate(validationResults);

            return new ValidationResult(validationResults);
        }

        private void ValidateEnabledRules(List<ValidationResult> results)
        {
        }

        private void ValidateOverlayOpacity(List<ValidationResult> results)
        {
            if (!int.TryParse(OverlayOpacity, out var number) || number < MinOverlayOpacity || number > MaxOverlayOpacity)
            {
                results.Add(new ValidationResult("Overlay Opacity must be between 0 and 100."));
            }
        }

        private void ValidateApiPollingRate(List<ValidationResult> results)
        {
            if (!int.TryParse(ApiPollingRate, out var number) || number < MinApiPollingRate || number > MaxApiPollingRate)
            {
                results.Add(new ValidationResult("API Polling Rate must be between 0 and 60000 ms."));
            }
        }

        private string GetFilePath()
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
