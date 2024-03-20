using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace NLOverlay.Helpers
{
    public static class Utils
    {
        public static int ConvertStringToInt(this string value)
        {
            if (int.TryParse(value, out int result)) return result;
            return 0;
        }

        public static bool IsValidHex(string value)
        {
            // Regular expression to match hexadecimal string without #
            string pattern = @"^#[0-9a-fA-F]{6}$";
            return Regex.IsMatch(value, pattern);
        }

        public static Brush ConvertColorAndOpacityToBrush(string colorCode, int opacity)
        {
            if (!IsValidHex(colorCode)) return null;

            var combinedColorAndOpacity = CombineHexColorAndOpacity(colorCode, opacity); 
            return ConvertHexColorToBrush(combinedColorAndOpacity);
        }

        public static string CombineHexColorAndOpacity(string colorCode, int opacity)
        {
            if (!IsValidHex(colorCode)) return null;

            var color = (Color)ColorConverter.ConvertFromString(colorCode);
            var alpha = Math.Round((double)opacity / 100 * 255);

            var transparentColor = Color.FromArgb((byte)alpha, color.R, color.G, color.B);
            //var hexString = transparentColor.ToString();
            var hexString = $"#{transparentColor.A:X2}{transparentColor.R:X2}{transparentColor.G:X2}{transparentColor.B:X2}";

            return hexString;
        }

        public static Brush ConvertHexColorToBrush(string colorCode)
        {
            if (!IsValidHex(colorCode)) return null;

            var converter = new BrushConverter();
            return (Brush)converter.ConvertFromString(colorCode);
        }
        
        public static void HighlightValidTextBoxValue(System.Windows.Controls.TextBox textBox)
        {
            textBox.BorderBrush = Brushes.Transparent;
        }

        public static void HighlightInvalidTextBoxValue(System.Windows.Controls.TextBox textBox)
        {
            textBox.BorderBrush = Brushes.Red;
        }

        public static void ModifyInfoBar(InfoBar infoBar, string title, string message, InfoBarSeverity severity, bool isOpen)
        {
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.Severity = severity;
            infoBar.IsOpen = isOpen;
        }
    }
}
