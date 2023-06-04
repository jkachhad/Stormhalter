using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Kesmai.WorldForge;

public class DataGridRowToIndexConverter : MarkupExtension, IValueConverter
{
	private static DataGridRowToIndexConverter _instance;

	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is DataGridRow row)
			return row.GetIndex() + 1;
			
		return -1;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_instance == null) 
			_instance = new DataGridRowToIndexConverter();
			
		return _instance;
	}
}