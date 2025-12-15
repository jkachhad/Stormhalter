using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Kesmai.WorldForge;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI.Converters;

[ValueConversion(typeof(IComponentProvider), typeof(Brush))]
public sealed class ComponentProviderToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            null => Brushes.Transparent,
            TerrainComponent => Brushes.Black,
            SegmentComponent => Brushes.OrangeRed,
            SegmentBrush => Brushes.MediumSeaGreen,
            SegmentTemplate => Brushes.SteelBlue,
            _ => Brushes.Transparent
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
