using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Items
{
	public abstract partial class MeleeWeapon : Weapon
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MeleeWeapon"/> class.
		/// </summary>
		protected MeleeWeapon(int meleeWeaponID) : base(meleeWeaponID)
		{
		}
	}
}
