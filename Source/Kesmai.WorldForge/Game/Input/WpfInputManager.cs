using DigitalRune;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kesmai.WorldForge
{
	public class WpfInputManager : InputManager
	{
		private WpfKeyboard _keyboard;
		private WpfMouse _mouse;

		public override bool IsTouchEnabled => false;

		public WpfInputManager(WpfKeyboard keyboard, WpfMouse mouse)
		{
			_keyboard = keyboard;
			_mouse = mouse;
		}

		public override KeyboardState GetKeyboardState()
		{
			return _keyboard.GetState();
		}

		public override MouseState GetMouseState()
		{
			return _mouse.GetState();
		}
	}
}
