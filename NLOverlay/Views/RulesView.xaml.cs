using NLOverlay.Enums;
using NLOverlay.Helpers;
using NLOverlay.Models;
using NLOverlay.Services;
using NLOverlay.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using TextBox = System.Windows.Controls.TextBox;
using Visibility = System.Windows.Visibility;

namespace NLOverlay.Views
{
    /// <summary>
    /// Interaction logic for RulesView.xaml
    /// </summary>
    public partial class RulesView : UserControl
    {
        private readonly IRuleService _ruleService;
        private readonly ObservableCollection<RuleViewModel> _ruleViewModels;
        private readonly Settings settings;

        private RuleViewModel selectedRule;

        public RulesView()
        {
            InitializeComponent();

            settings = new Settings();
            _ruleService = new RuleService();

            // Startup
            selectedRule = null;
            settings.Load();
            ScrollViewer.Visibility = Visibility.Collapsed;
            _ruleViewModels = _ruleService.GetRules(settings);
            RuleNamesListView.ItemsSource = _ruleViewModels;
            if (RuleNamesListView.Items.Count > 0)
            {
                RuleNamesListView.SelectedIndex = 0;
            }
            InfoBar.IsOpen = false;
        }

        private void RuleNamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RuleNamesListView.SelectedItem != null)
            {
                ScrollViewer.Visibility = Visibility.Visible;
                selectedRule = (RuleViewModel)RuleNamesListView.SelectedItem;
                InfoBar.IsOpen = false;
            }
            else
            {
                ScrollViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void RulesSaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedRule == null)
            {
                return;
            }

            var areAllInputsValid = true;

            var userInputs = new Dictionary<(ToggleSwitch, TextBox), ThresholdTypes>
            {
                { (RuleHighlightThresholdSwitch, RuleHighlightThresholdValue), ThresholdTypes.Highlight },
                { (RuleDisableThresholdSwitch, RuleDisableThresholdValue), ThresholdTypes.Disable},
            };

            var enabledInputs = userInputs.Where(u => (bool)u.Key.Item1.IsChecked);
            var disabledInputs = userInputs.Where(u => (bool)u.Key.Item1.IsChecked == false);

            ValidateInputs(enabledInputs, ref areAllInputsValid);

            if (areAllInputsValid)
            {
                settings.InsertOrDeleteRulesOnOverlayRecord(selectedRule.Id, (bool)RuleOnOverlaySwitch.IsChecked);
                UpdateSettings(enabledInputs, disabledInputs);
                
                settings.Save();
                
                Utils.ModifyInfoBar(InfoBar, selectedRule.RuleFor, "saved", InfoBarSeverity.Success, true);
            }
            else
            {
                Utils.ModifyInfoBar(InfoBar, "Error", "Some inputs are invalid", InfoBarSeverity.Error, true);
            }
        }

        private void ValidateInputs(IEnumerable<KeyValuePair<(ToggleSwitch, TextBox), ThresholdTypes>> inputs, ref bool areAllInputsValid)
        {
            foreach (var input in inputs)
            {
                if (!IsTextBoxValueValid(input.Key.Item1, input.Key.Item2, 0, 60000))
                {
                    Utils.HighlightValidTextBoxValue(input.Key.Item2);
                }
                else
                {
                    Utils.HighlightInvalidTextBoxValue(input.Key.Item2);
                    areAllInputsValid = false;
                }
            }
        }

        private void UpdateSettings(IEnumerable<KeyValuePair<(ToggleSwitch, TextBox), ThresholdTypes>> enabledInputs, IEnumerable<KeyValuePair<(ToggleSwitch, TextBox), ThresholdTypes>> disabledInputs)
        {
            foreach (var input in enabledInputs)
            {
                var thresholdType = input.Value;
                var ruleId = selectedRule.Id;

                if (thresholdType == ThresholdTypes.Highlight)
                {
                    var highlightThreshold = GetKeyValuePairToSave(selectedRule, input.Key.Item2);
                    settings.InsertOrUpdateThresholdRecord(settings.HighlightThresholds, highlightThreshold);
                }
                else if (thresholdType == ThresholdTypes.Disable)
                {
                    var disableThreshold = GetKeyValuePairToSave(selectedRule, input.Key.Item2);
                    settings.InsertOrUpdateThresholdRecord(settings.DisableThresholds, disableThreshold);
                }

                settings.InsertOrDeleteRulesOnOverlayRecord(ruleId, (bool)RuleOnOverlaySwitch.IsChecked);
            }

            foreach (var input in disabledInputs)
            {
                var thresholdType = input.Value;
                var ruleId = selectedRule.Id;

                if (thresholdType == ThresholdTypes.Highlight)
                {
                    settings.RemoveThresholdRecord(settings.HighlightThresholds, ruleId);
                }
                else if (thresholdType == ThresholdTypes.Disable)
                {
                    settings.RemoveThresholdRecord(settings.DisableThresholds, ruleId);
                }
            }
        }

        private bool IsTextBoxValueValid(ToggleSwitch toggleSwitch, TextBox textBox, int minValue, int maxValue)
        {
            if (toggleSwitch.IsEnabled)
            {
                if (int.TryParse(textBox.Text, out int intTextValue))
                {
                    return intTextValue < minValue || intTextValue > maxValue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private Tuple<string, int> GetKeyValuePairToSave(RuleViewModel ruleViewModel, TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int intTextValue)) {
                return Tuple.Create(ruleViewModel.Id, intTextValue);
            }
            else
            {
                return null;
            }
        }
    }
}
