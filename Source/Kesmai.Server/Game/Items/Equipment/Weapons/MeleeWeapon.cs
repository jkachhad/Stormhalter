using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Items;

public abstract partial class MeleeWeapon : Weapon
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MeleeWeapon"/> class.
	/// </summary>
	protected MeleeWeapon(int meleeWeaponID) : base(meleeWeaponID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="MeleeWeapon"/> class.
	/// </summary>
	protected MeleeWeapon(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)0); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 0:
			{
				break;
			}
		}
	}
}