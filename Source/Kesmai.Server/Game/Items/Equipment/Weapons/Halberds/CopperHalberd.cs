using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class CopperHalberd : Halberd, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 50;

	/// <inheritdoc />
	public override int Weight => 5000;

	/// <inheritdoc />
	public override int MinimumDamage => 2;

	/// <inheritdoc />
	public override int MaximumDamage => 14;

	/// <inheritdoc />
	public override int BaseArmorBonus => 2;

	/// <inheritdoc />
	public override int BaseAttackBonus => 5;
		
	/// <inheritdoc />
	public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Lawful;

	/// <inheritdoc />
	public override bool CanBind => true;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="CopperHalberd"/> class.
	/// </summary>
	public CopperHalberd() : base(187)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="CopperHalberd"/> class.
	/// </summary>
	public CopperHalberd(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200160); /* [a heavy halberd.  The pole is perfect ash and the blade is forged from a golden copper colored alloy.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250002); /* The combat adds for this weapon are +5. */
	}

	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <inheritdoc />
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}
}
