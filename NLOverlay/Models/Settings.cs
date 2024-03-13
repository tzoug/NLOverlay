using NLOverlay.Enums;
using NLOverlay.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.Json;

namespace NLOverlay.Models
{
    public class Settings
    {
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

        /// <summary>
        /// Placement of overlay window
        /// </summary>
        [DisplayName("Overlay Placement")]
        [Category("Advanced")]
        public OverlayPlacement OverlayPlacement { get; set; }

        /// <summary>
        /// Text background color HEX value (Ex: #FFFFFFF)
        /// </summary>
        [DisplayName("Text Color")]
        [Category("Advanced")]
        public string OverlayTextColor { get; set; }

        /// <summary>
        /// Text background color HEX value (Ex: #FFFFFFF)
        /// </summary>
        [DisplayName("Text Background Color")]
        [Category("Advanced")]
        public string OverlayTextBackgroundColor { get; set; }

        /// <summary>
        /// Threshold reached color HEX value (Ex: #FFFFFFF)
        /// </summary>
        [DisplayName("Threshold Reached Color")]
        [Category("Advanced")]
        public string OverlayTextThresholdReachColor { get; set; }

        /// <summary>
        /// Text background opacity
        /// </summary>
        [DisplayName("Text Background Opacity %")]
        [Category("Advanced")]
        public int OverlayTextBackgroundOpacity { get; set; }

        [Browsable(false)]
        public Dictionary<string, int> HighlightThresholds { get; set; }
        
        [Browsable(false)]
        public Dictionary<string, int> DisableThresholds { get; set; }

        public Settings() {
            RulesOnOverlay = new List<string>();
            ApiPollingRate = Properties.Resources.DefaultApiPollingRate.ToString();
            OverlayPlacement = (OverlayPlacement)Enum.Parse(typeof(OverlayPlacement), Properties.Resources.DefaultOverlayPlacementValue.ToString());
            HighlightThresholds = new Dictionary<string, int>();
            DisableThresholds = new Dictionary<string, int>();
            OverlayTextColor = "#FFFFFF";
            OverlayTextBackgroundColor = "#000000";
            OverlayTextThresholdReachColor = "#FF0000";
            OverlayTextBackgroundOpacity = 50;
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
                    var data = JsonSerializer.Deserialize<Settings>(jsonContent);

                    var validationResult = Validate();

                    if (validationResult.IsValid)
                    {
                        RulesOnOverlay = data.RulesOnOverlay; 
                        ApiPollingRate = data.ApiPollingRate;
                        OverlayPlacement = data.OverlayPlacement;
                        HighlightThresholds = data.HighlightThresholds;
                        DisableThresholds = data.DisableThresholds;
                        OverlayTextColor = data.OverlayTextColor;
                        OverlayTextBackgroundColor = data.OverlayTextBackgroundColor;
                        OverlayTextBackgroundOpacity = data.OverlayTextBackgroundOpacity;
                        OverlayTextThresholdReachColor = data.OverlayTextThresholdReachColor;
                    }
                }
            }
            catch (Exception)
            {
                // Catch error while reading file
            }
        }

        public ValidationResult Validate()
        {
            var validationResults = new List<ValidationResult>();
            
            ValidateApiPollingRate(validationResults);
            ValidateThresholds(validationResults);
            ValidateOverlayColors(validationResults);
            ValidateOpacity(validationResults);

            return new ValidationResult(validationResults);
        }

        private void ValidateApiPollingRate(ICollection<ValidationResult> results)
        {
            var minPollingRate = int.Parse(Properties.Resources.MinApiPollingRate);
            var maxPollingRate = int.Parse(Properties.Resources.MaxApiPollingRate);

            if (!int.TryParse(ApiPollingRate, out var number) || number < minPollingRate || number > maxPollingRate)
            {
                results.Add(new ValidationResult("API Polling Rate must be between 0 and 60000 ms."));
            }
        }

        private void ValidateThresholds(ICollection<ValidationResult> results)
        {
            var allThresholdValues = HighlightThresholds.Values.ToList();
            allThresholdValues.AddRange(DisableThresholds.Values);
            
            // Ensure no values that are negative
            if (allThresholdValues.Any(values => values < 0))
            {
                results.Add(new ValidationResult("Threshold values (in seconds) cannot be negative."));
                return; // Only add it to the results once.
            }
        }

        private void ValidateOverlayColors(ICollection<ValidationResult> results)
        {
            // Background color
            OverlayTextBackgroundColor = string.IsNullOrWhiteSpace(OverlayTextBackgroundColor)
                ? "#000000"
                : OverlayTextBackgroundColor;

            // Text color
            OverlayTextColor = string.IsNullOrWhiteSpace(OverlayTextColor)
            ? "#FFFFFF"
            : OverlayTextColor;

            // Threshold color
            OverlayTextThresholdReachColor = string.IsNullOrWhiteSpace(OverlayTextThresholdReachColor)
            ? "#FF0000"
            : OverlayTextThresholdReachColor;

            if (!Utils.IsValidHex(OverlayTextBackgroundColor) || 
                !Utils.IsValidHex(OverlayTextColor) ||
                !Utils.IsValidHex(OverlayTextThresholdReachColor))
            {
                results.Add(new ValidationResult("Invalid HEX color value."));
            }
        }

        private void ValidateOpacity(ICollection<ValidationResult> results)
        {
            if (Utils.IsValidOpacity(OverlayTextBackgroundOpacity))
            {
                results.Add(new ValidationResult("Opacity value must be between 0 and 100."));
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
