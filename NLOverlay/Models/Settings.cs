using NLOverlay.Enums;
using NLOverlay.Helpers;
using NLOverlay.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Brush = System.Windows.Media.Brush;

namespace NLOverlay.Models
{
    public class Settings
    {
        /// <summary>
        /// List of IDs of enabled rules (for overlay)
        /// </summary>
        public List<string> RulesOnOverlay { get; set; }

        /// <summary>
        /// API Polling Rate (ms)
        /// </summary>
        public int ApiPollingRate { get; set; }

        /// <summary>
        /// Placement of overlay window
        /// </summary>
        public OverlayPlacements OverlayPlacement { get; set; }

        /// <summary>
        /// Text background color HEX value (Ex: #FFFFFFF)
        /// </summary>
        public string OverlayTextColor { get; set; }

        /// <summary>
        /// Overlay font size
        /// </summary>
        public OverlayFontSizes OverlayFontSize { get; set; }

        /// <summary>
        /// Overlay font weight
        /// </summary>
        public OverlayFontWeights OverlayFontWeight { get; set; }

        /// <summary>
        /// Threshold reached color HEX value (Ex: #FFFFFFF)s
        /// </summary>
        public string OverlayTextThresholdReachColor { get; set; }

        /// <summary>
        /// Text background color HEX value (Ex: #FFFFFFF)
        /// </summary>
        public string OverlayBackgroundColor { get; set; }

        /// <summary>
        /// Text background opacity
        /// </summary>
        public int OverlayBackgroundOpacity { get; set; }

        /// <summary>
        /// Overlay text color as brush type
        /// </summary>
        [JsonIgnore]
        public Brush OverlayTextColorBrush { get; set; }

        /// <summary>
        /// Overlay text threshold reached color as brush type
        /// </summary>
        [JsonIgnore]
        public Brush OverlayTextThresholdReachColorBrush { get; set; }

        /// <summary>
        /// Overlay background color as brush type
        /// </summary>
        [JsonIgnore]
        public Brush OverlayBackgroundBrush { get; set; }

        /// <summary>
        /// Dictionnary(ruleId, seconds) of rules that will be change colors after a certain amount of time (s)
        /// </summary>
        public Dictionary<string, int> HighlightThresholds { get; set; }

        /// <summary>
        /// Dictionnary(ruleId, seconds) of rules that will be auto-disabled after a certain amount of time (s)
        /// </summary>
        public Dictionary<string, int> DisableThresholds { get; set; }

        public Settings() {
            RulesOnOverlay = new List<string>();
            ApiPollingRate = Utils.ConvertStringToInt(Properties.Resources.DefaultApiPollingRate.ToString());
            OverlayPlacement = (OverlayPlacements)Enum.Parse(typeof(OverlayPlacements), Properties.Resources.DefaultOverlayPlacementValue.ToString());
            HighlightThresholds = new Dictionary<string, int>();
            DisableThresholds = new Dictionary<string, int>();
            OverlayTextColor = Properties.Resources.DefaultOverlayTextColor.ToString();
            OverlayFontSize = OverlayFontSizes.Large;
            OverlayFontWeight = OverlayFontWeights.Normal;
            OverlayTextThresholdReachColor = Properties.Resources.DefaultOverlayThresholdReachedColor.ToString();
            OverlayBackgroundColor = Properties.Resources.DefaultOverlayBackgroundColor.ToString();
            OverlayBackgroundOpacity = Utils.ConvertStringToInt(Properties.Resources.DefaultOverlayBackgroundOpacity.ToString());

            // Brush
            OverlayTextColorBrush = Utils.ConvertHexColorToBrush(OverlayTextColor);
            OverlayTextThresholdReachColorBrush = Utils.ConvertHexColorToBrush(OverlayTextThresholdReachColor);
            OverlayBackgroundBrush = Utils.ConvertHexColorToBrush(OverlayBackgroundColor);
        }

        public void RequestSave(ObservableCollection<RuleViewModel> ruleViewModel)
        {
            RulesOnOverlay = ruleViewModel.Where(r => r.ShowOnOverlay).Select(r => r.Id).ToList();
            HighlightThresholds = ruleViewModel.Where(r => r.HighlightThresholdEnabled).ToDictionary(r => r.Id, r => r.HighlightThreshold);
            DisableThresholds = ruleViewModel.Where(r => r.DisableThresholdEnabled).ToDictionary(r => r.Id, r => r.DisableThreshold);
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

                    RulesOnOverlay = data.RulesOnOverlay ?? new List<string>();
                    ApiPollingRate = LoadPollingRate(data);
                    OverlayPlacement = data.OverlayPlacement;
                    HighlightThresholds = LoadThresholds(data.HighlightThresholds, 0, 60000, 0);
                    DisableThresholds = LoadThresholds(data.DisableThresholds, 0, 60000, 0);
                    OverlayTextColor = LoadHexColor(data.OverlayTextColor, Properties.Resources.DefaultOverlayTextColor);
                    OverlayFontSize = data.OverlayFontSize;
                    OverlayFontWeight = data.OverlayFontWeight;
                    OverlayTextThresholdReachColor = LoadHexColor(data.OverlayTextThresholdReachColor, Properties.Resources.DefaultOverlayThresholdReachedColor);
                    OverlayBackgroundColor = LoadHexColor(data.OverlayBackgroundColor, Properties.Resources.DefaultOverlayBackgroundColor);
                    OverlayBackgroundOpacity = LoadOpacity(data.OverlayBackgroundOpacity, Properties.Resources.DefaultOverlayBackgroundOpacity);
                    
                    // Brush
                    OverlayTextColorBrush = Utils.ConvertHexColorToBrush(OverlayTextColor);
                    OverlayTextThresholdReachColorBrush = Utils.ConvertHexColorToBrush(OverlayTextThresholdReachColor);
                    OverlayBackgroundBrush = Utils.ConvertHexColorToBrush(OverlayBackgroundColor);
                }
            }
            catch (Exception)
            {
                // Catch error while reading file
            }
        }

        private int LoadPollingRate(Settings data)
        {
            var defaultPollingRate = int.Parse(Properties.Resources.DefaultApiPollingRate);
            if (data == null) return defaultPollingRate;

            var pollingRate = data.ApiPollingRate;
            var minPollingRate = int.Parse(Properties.Resources.MinApiPollingRate);
            var maxPollingRate = int.Parse(Properties.Resources.MaxApiPollingRate);

            return pollingRate >= minPollingRate && pollingRate <= maxPollingRate 
                ? pollingRate 
                : defaultPollingRate;
        }

        private Dictionary<string, int> LoadThresholds(Dictionary<string, int> thresholds, int minValue, int maxValue, int defaultValue)
        {
            Dictionary<string, int> updatedThresholds = new Dictionary<string, int>();

            foreach (var kvp in thresholds)
            {
                if (kvp.Value < minValue || kvp.Value > maxValue)
                {
                    updatedThresholds[kvp.Key] = defaultValue;
                }
                else
                {
                    updatedThresholds[kvp.Key] = kvp.Value;
                }
            }

            return updatedThresholds;
        }

        private string LoadHexColor(string hexColor, string defaultValue)
        {
            return Utils.IsValidHex(hexColor) ? hexColor : defaultValue;
        }

        private int LoadOpacity(int opacity, string defaultTextValue)
        {
            var defaultValue = Utils.ConvertStringToInt(defaultTextValue);
            return IsOpacityValid(opacity) ? opacity : defaultValue;
        }

        public void InsertOrDeleteRulesOnOverlayRecord(string ruleId, bool shouldAdd)
        {
            if (ruleId == null)
            {
                return;
            }

            if (shouldAdd)
            {
                if (!RulesOnOverlay.Contains(ruleId))
                {
                    RulesOnOverlay.Add(ruleId);
                }
            }
            else
            {
                RulesOnOverlay.Remove(ruleId);
            }
        }

        public void InsertOrUpdateThresholdRecord(Dictionary<string, int> dictToEdit, Tuple<string, int> keyValuePair)
        {
            if (keyValuePair == null)
            {
                return;
            }

            if (dictToEdit.ContainsKey(keyValuePair.Item1))
            {
                dictToEdit[keyValuePair.Item1] = keyValuePair.Item2;
            }
            else
            {
                dictToEdit.Add(keyValuePair.Item1, keyValuePair.Item2);
            }
        }

        public void RemoveThresholdRecord(Dictionary<string, int> dictToEdit, string ruleId)
        {
            if (dictToEdit.ContainsKey(ruleId))
            {
                dictToEdit.Remove(ruleId);
            }
        }

        public bool IsTextPollingRateValid(string pollingRate)
        {
            if (pollingRate == null)
            {
                return false;
            }

            if (int.TryParse(pollingRate, out int pollingRateValue))
            {
                var minPollingRate = int.Parse(Properties.Resources.MinApiPollingRate);
                var maxPollingRate = int.Parse(Properties.Resources.MaxApiPollingRate);

                return minPollingRate <= pollingRateValue && maxPollingRate >= pollingRateValue;
            }
            else
            {
                return false;
            }
        }

        private bool IsOpacityValid(int opacity)
        {
            return opacity >= 0 && opacity <= 100;
        }

        public bool IsTextOpacityValid(string opacity)
        {
            if (opacity == null)
            {
                return false;
            }

            if (int.TryParse(opacity, out int opacityValue))
            {
                return IsOpacityValid(opacityValue);
            }
            else
            {
                return false;
            }
        }

        public static string GetFilePath()
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
