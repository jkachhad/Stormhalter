using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge;

public static class RelayCommandExtensions
{
	public static void DependsOn<T>(this RelayCommand command, params Expression<Func<T>>[] propertyExpressions)
	{
		var propertyNames = propertyExpressions.Select(GetPropertyName);

		WeakReferenceMessenger.Default.Register<RelayCommand,
			PropertyChangedMessage<T>>(
			command,
			(r, m) =>
			{
				if (propertyNames.Contains(m.PropertyName))
					r.NotifyCanExecuteChanged();
			});
	}
		
	public static void DependsOn<V, T>(this RelayCommand<V> command, params Expression<Func<T>>[] propertyExpressions)
	{
		var propertyNames = propertyExpressions.Select(GetPropertyName);

		WeakReferenceMessenger.Default.Register<RelayCommand<V>,
			PropertyChangedMessage<T>>(
			command,
			(r, m) =>
			{
				if (propertyNames.Contains(m.PropertyName))
					r.NotifyCanExecuteChanged();
			});
	}

	private static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
	{
		if (propertyExpression == null)
			throw new ArgumentNullException(nameof(propertyExpression));

		if (propertyExpression.Body is not MemberExpression body)
			throw new ArgumentException("Invalid argument", nameof(propertyExpression));
 
		var property = body.Member as PropertyInfo;
			
		if (property == null)
			throw new ArgumentException("Argument is not a property", nameof(propertyExpression));
 
		return property.Name;
	}
}