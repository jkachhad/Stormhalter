using System;

namespace DigitalRune.Game.UI.Controls;

public static class MenuExtensions
{
	public static MenuItem Create(this Menu menu, string title, EventHandler<EventArgs> handler)
	{
		var menuItem = new MenuButton()
		{
			Content = new TextBlock()
			{
				Text = title,
				Font = "Tahoma", FontSize = 10
			},
			
		};
		menuItem.Click += handler;

		return menuItem;
	}

	public static MenuItem CreateSeparator(this Menu menu)
	{
		return new MenuSeparator();
	}
}