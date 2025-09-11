using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CommunityToolkit.Mvvm.Messaging;

public static class WeakReferenceMessengerExtensions
{
	// send a message with delay
	public static void SendDelayed<TMessage>(this IMessenger messenger, TMessage message, TimeSpan delay) where TMessage : class
	{
		if (messenger == null)
			throw new ArgumentNullException(nameof(messenger));
		
		if (message == null)
			throw new ArgumentNullException(nameof(message));
		
		if (delay < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be non-negative.");
		
		// Use a DispatcherTimer to send the message after the specified delay
		// This ensures the message is sent on the UI thread if called from there
		// If called from a non-UI thread, the timer will still work but the message
		// will be sent on the thread pool thread.
		Task.Delay(delay).ContinueWith(_ =>
		{
			Application.Current.Dispatcher.Invoke(() => messenger.Send(message));
		});
	}
}