using System;

namespace Kesmai.Server.Items;

[Flags]
public enum WeaponFlags
{
	None			= 0x000000,

	TwoHanded		= 0x000001,
	Bow				= 0x000002 | Projectile,
	Crossbow		= 0x000004 | Projectile,
	Silver			= 0x000008,
		
	BlueGlowing		= 0x000010,
	MustThrow		= 0x000020,
	QuickThrow		= 0x000040,
	Returning		= 0x000080,

	Throwable		= 0x000100,
	Piercing		= 0x000200,
	Slashing		= 0x000400,
	Bashing			= 0x000800,

	Lawful			= 0x001000,
	Chaotic			= 0x002000,
	Neutral			= 0x004000,
	Projectile		= 0x008000,

	SnakeStaff		= 0x010000,
}