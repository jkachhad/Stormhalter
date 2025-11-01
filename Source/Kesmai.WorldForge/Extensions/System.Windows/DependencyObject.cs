using System.Windows.Media;

namespace System.Windows;

public static class DependencyObjectExtensions
{
	public static T FindAncestor<T>(this DependencyObject current) where T : DependencyObject
	{
		while (current != null)
		{
			if (current is T dependencyObject)
				return dependencyObject;
			
			current = VisualTreeHelper.GetParent(current);
		}
		
		return null;
	}
}