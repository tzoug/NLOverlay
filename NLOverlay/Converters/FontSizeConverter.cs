using NLOverlay.Enums;
using System.Globalization;
using System;
using System.Windows.Data;

namespace NLOverlay.Converters
{
    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OverlayFontSizes overlayFontSize)
            {
                switch (overlayFontSize)
                {
                    case OverlayFontSizes.Small:
                        return 12.0;
                    case OverlayFontSizes.Medium:
                        return 16.0;
                    case OverlayFontSizes.Large:
                        return 20.0;
                    case OverlayFontSizes.ExtraLarge:
                        return 24.0;
                    default:
                        return 20.0;
                }
            }

            return 20.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
