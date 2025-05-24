using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Kesmai.WorldForge.Editor.MVP.Models.World.Components; // for TilePrefabComponent

namespace Kesmai.WorldForge.UI
{
    public class PrefabOnlyVisibilityConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return value is TilePrefabComponent ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
}