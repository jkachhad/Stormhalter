using System;
using System.Collections;
using System.Collections.Generic;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;

namespace Kesmai.WorldForge;

public class WpfUIManager : IUIService
{
	private IInputService _inputService;
	private readonly List<UIScreen> _sortedScreens = new List<UIScreen>();

	public object Cursor { get; set; } = null;
	public object GameForm => null;
	public KeyMap KeyMap { get; set; } = KeyMap.AutoKeyMap;

	public IInputService InputService => _inputService;

	public UIScreenCollection Screens { get; }

	public WpfUIManager(IInputService inputService)
	{
		_inputService = inputService;
		_sortedScreens = new List<UIScreen>();

		Screens = new UIScreenCollection(this);
	}

	public void Update(TimeSpan deltaTime)
	{
		if (Screens.IsDirty)
		{
			lock (((ICollection)Screens).SyncRoot)
			{
				Screens.IsDirty = false;

				_sortedScreens.Clear();

				foreach (var screen in Screens)
					_sortedScreens.Add(screen);

				_sortedScreens.Sort(UIManager.UIScreenComparer.Instance);
			}
		}

		foreach (var screen in _sortedScreens)
			screen.NewFrame();
		foreach (var screen in _sortedScreens)
			screen.Update(deltaTime);
	}
}