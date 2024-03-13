using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace NLOverlay.Helpers
{
    public static class Utils
    {
        public static bool IsNullOrEmpty<T>(this List<T> collection)
        {
            return collection != null && collection.Any();
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        }

        public static string Normalize(string value)
        {
            return value.Trim();
        }

        public static string ConcatStringsForMessageBox(this List<string> stringList)
        {
            var stringBuilder = new StringBuilder();
            foreach (string item in stringList)
            {
                stringBuilder.AppendLine("- " + item);
            }

            return stringBuilder.ToString();
        }

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

        public static bool IsValidOpacity(int value)
        {
            return value < 0 || value > 100;
        }

        public static string ConvertToHexWithOpacity(string colorCode, int opacity)
        {
            var color = (Color)ColorConverter.ConvertFromString(colorCode);
            var alpha = (int)Math.Round((double)opacity / 100 * 255);

            var transparentColor = Color.FromArgb((byte)alpha, color.R, color.G, color.B);
            var hexString = transparentColor.ToString();

            return hexString;
        }

        public static Brush ConvertHexStringToBrush(string value)
        {
            var converter = new BrushConverter();
            var brush = (Brush)converter.ConvertFromString(value);

            return brush;
        }

        public static double ConvertIntToXamlOpacity(int value)
        {
            return (double)value / 100;
        }
    }
}
