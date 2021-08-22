using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge
{
	public class Tool : ObservableObject
	{
		public static Tool Default = new ArrowTool();
		
		private bool _isActive;

		private string _name;
		private BitmapImage _icon;
		protected Cursor _cursor;
		
		protected Vector2F _position;
		
		public string Name => _name;
		public BitmapImage Icon => _icon;

		public Cursor Cursor => _cursor;
		
		public bool IsActive
		{
			get => _isActive;
			set => SetProperty(ref _isActive, value);
		}

		public Tool(string name, string icon)
		{
			_name = name;
			_icon = new BitmapImage(new Uri(@$"pack://application:,,,/Kesmai.WorldForge;component/Resources/{icon}.png"));

			_cursor = Cursors.Arrow;
			
			IsActive = false;
		}
		
		public virtual void OnHandleInput(PresentationTarget target, IInputService inputService)
		{
			if (inputService.IsMouseOrTouchHandled)
				return;
			
			_position = inputService.MousePosition;
		}

		public virtual void OnRender(RenderContext context)
		{
		}

		public virtual void OnActivate()
		{
		}

		public virtual void OnDeactivate()
		{
		}
	}
}