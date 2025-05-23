using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class HeavyCrossbow : Crossbow, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 60;

	/// <inheritdoc />
	public override int Weight => 1400;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

	/// <inheritdoc />
	public override int MinimumDamage => 2;

	/// <inheritdoc />
	public override int MaximumDamage => 12;

	/// <inheritdoc />
	public override int BaseAttackBonus => 4;
		
	/// <inheritdoc />
	public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing;

	/// <inheritdoc />
	public override bool CanBind => true;
		
	/// <inheritdoc />
	public override int NockedID => 390;

	/// <summary>
	/// Initializes a new instance of the <see cref="HeavyCrossbow"/> class.
	/// </summary>
	public HeavyCrossbow() : base(232)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="HeavyCrossbow"/> class.
	/// </summary>
	public HeavyCrossbow(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200038)); /* [You are looking at] [a heavy crossbow with an ornately carved tiller and a peculiar black steel bow. The crossbow is emitting a soft blue glow.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
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