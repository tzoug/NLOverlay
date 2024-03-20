using NLOverlay.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NLOverlay.Converters
{
    public class FontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OverlayFontWeights overlayFontWeight)
            {
                switch (overlayFontWeight)
                {
                    case OverlayFontWeights.Light:
                        return FontWeights.Light;
                    case OverlayFontWeights.Normal:
                        return FontWeights.Normal;
                    case OverlayFontWeights.SemiBold:
                        return FontWeights.SemiBold;
                    case OverlayFontWeights.Bold:
                        return FontWeights.Bold;
                    case OverlayFontWeights.ExtraBold:
                        return FontWeights.ExtraBold;
                    default:
                        return FontWeights.Normal;
                }
            }

            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
