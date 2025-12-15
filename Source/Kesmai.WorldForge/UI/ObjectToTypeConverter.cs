using System;
using System.Windows.Data;

namespace Kesmai.WorldForge;

[ValueConversion(typeof(object), typeof(string))]
public class ObjectToTypeConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return value is null ? String.Empty : value.GetType().Name;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new InvalidOperationException();
	}
}