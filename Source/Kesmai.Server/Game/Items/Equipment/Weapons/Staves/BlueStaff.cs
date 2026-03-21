using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class BlueStaff : Staff, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1500;

	/// <inheritdoc />
	public override int Weight => 500;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override int BaseArmorBonus => 2;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="BlueStaff"/> class.
	/// </summary>
	public BlueStaff() : base(38)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BlueStaff"/> class.
	/// </summary>
	public BlueStaff(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200140); /* [a long, surprisingly light staff composed of a bluish metal.] */
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
