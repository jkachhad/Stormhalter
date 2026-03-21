using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class YasnakiDagger : Dagger, IReturningWeapon, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1;

	/// <inheritdoc />
	public override int BaseAttackBonus => 5;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override WeaponFlags Flags => base.Flags | WeaponFlags.Silver | WeaponFlags.Returning;

	/// <inheritdoc />
	public override bool CanBind => true;

	/// <inheritdoc />
	protected override int PoisonedItemId => 314;

	/// <summary>
	/// Initializes a new instance of the <see cref="YasnakiDagger"/> class.
	/// </summary>
	public YasnakiDagger() : base(308)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="YasnakiDagger"/> class.
	/// </summary>
	public YasnakiDagger(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200164); /* [a fine throwing dagger with the symbol of the yasnaki marking the hilt.] */

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
