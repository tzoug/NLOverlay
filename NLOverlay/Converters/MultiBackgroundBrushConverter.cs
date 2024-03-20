using NLOverlay.Helpers;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NLOverlay.Converters
{
    public class MultiBackgroundBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is string colorCode && values[1] is int opacityPercentage)
            {
                var combinedColorAndOpacity = Utils.CombineHexColorAndOpacity(colorCode, opacityPercentage);
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(combinedColorAndOpacity));
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}