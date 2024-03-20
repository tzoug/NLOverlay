using NLOverlay.Enums;
using NLOverlay.Helpers;
using NLOverlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace NLOverlay.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly Settings settings;

        public SettingsView()
        {
            InitializeComponent();

            settings = new Settings();
            settings.Load();

            DataContext = settings;
            SettingsInfoBar.IsOpen = false;
            OverlayPlacementComboBox.ItemsSource = Enum.GetValues(typeof(OverlayPlacements)).Cast<OverlayPlacements>();
            OverlayFontSizeComboBox.ItemsSource = Enum.GetValues(typeof(OverlayFontSizes)).Cast<OverlayFontSizes>();
            OverlayFontWeightComboBox.ItemsSource = Enum.GetValues(typeof(OverlayFontWeights)).Cast<OverlayFontWeights>();
        }

        private void SettingsSaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var areAllInputsValid = true;
            var overlayPlacement = OverlayPlacements.LeftCenter;
            var fontSize = OverlayFontSizes.Large;
            var fontWeight = OverlayFontWeights.Normal;

            StartPollingRateValidation(ref areAllInputsValid);
            StartOpacityValidation(ref areAllInputsValid);
            StartHexColorValidation(ref areAllInputsValid);

            if (Enum.TryParse(OverlayPlacementComboBox.SelectedValue?.ToString(), out OverlayPlacements selectedPlacement))
            {
                overlayPlacement = selectedPlacement;
            }
            else
            {
                areAllInputsValid = false;
            }

            if (Enum.TryParse(OverlayFontSizeComboBox.SelectedValue?.ToString(), out OverlayFontSizes selectedSize))
            {
                fontSize = selectedSize;
            }
            else
            {
                areAllInputsValid = false;
            }

            if (Enum.TryParse(OverlayFontWeightComboBox.SelectedValue?.ToString(), out OverlayFontWeights selectedWeight))
            {
                fontWeight = selectedWeight;
            }
            else
            {
                areAllInputsValid = false;
            }

            if (areAllInputsValid)
            {
                settings.ApiPollingRate = Utils.ConvertStringToInt(PollingRateTextBox.Text);
                settings.OverlayBackgroundColor = OverlayBackground.Text;
                settings.OverlayTextColor = OverlayTextColor.Text;
                settings.OverlayTextThresholdReachColor = OverlayThresholdReachedColor.Text;
                settings.OverlayPlacement = overlayPlacement;
                settings.OverlayFontSize = fontSize;
                settings.OverlayFontWeight = fontWeight;

                settings.Save();
                Utils.ModifyInfoBar(SettingsInfoBar, "Settings", "saved", InfoBarSeverity.Success, true);
            }
            else
            {
                Utils.ModifyInfoBar(SettingsInfoBar, "Error", "Some inputs are invalid", InfoBarSeverity.Error, true);
            }
        }

        private void StartPollingRateValidation(ref bool areAllInputsValid)
        {
            if (!settings.IsTextPollingRateValid(PollingRateTextBox.Text))
            {
                Utils.HighlightInvalidTextBoxValue(PollingRateTextBox);
                areAllInputsValid = false;
            }
            else
            {
                Utils.HighlightValidTextBoxValue(PollingRateTextBox);
            }
        }

        private void StartOpacityValidation(ref bool areAllInputsValid)
        {
            if (!settings.IsTextOpacityValid(OverlayBackgroundOpacity.Text))
            {
                Utils.HighlightInvalidTextBoxValue(OverlayBackgroundOpacity);
                areAllInputsValid = false;
            }
            else
            {
                Utils.HighlightValidTextBoxValue(OverlayBackgroundOpacity);
            }
        }

        private void StartHexColorValidation(ref bool areAllInputsValid)
        {
            var hexColorInputs = new List<System.Windows.Controls.TextBox>
            {
                OverlayBackground,
                OverlayTextColor,
                OverlayThresholdReachedColor
            };

            foreach (var input in hexColorInputs)
            {
                if (!Utils.IsValidHex(input.Text))
                {
                    Utils.HighlightInvalidTextBoxValue(input);
                    areAllInputsValid = false;
                }
                else
                {
                    Utils.HighlightValidTextBoxValue(input);
                }
            }
        }

    }
}
