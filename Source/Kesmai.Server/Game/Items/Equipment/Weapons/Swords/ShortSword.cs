using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class ShortSword : Sword
{
	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 1200;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Medium;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 6;
		
	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Piercing;
		
	/// <inheritdoc />
	public override int AttackSound => 45;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ShortSword"/> class.
	/// </summary>
	public ShortSword() : this(88)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ShortSword"/> class.
	/// </summary>
	public ShortSword(int swordId) : base(swordId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ShortSword"/> class.
	/// </summary>
	public ShortSword(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200024); /* [an iron shortsword.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250020); /* The shortsword appears quite ordinary. */
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
