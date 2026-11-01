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

	public static T FindDescendant<T>(this DependencyObject current) where T : DependencyObject
	{
		for (var i = 0; i < VisualTreeHelper.GetChildrenCount(current); i++)
		{
			var child = VisualTreeHelper.GetChild(current, i);
			
			if (child is T dependencyObject)
				return dependencyObject;
			
			var descendant = child.FindDescendant<T>();
			
			if (descendant != null)
				return descendant;
		}
		
		return null;
	}
}
