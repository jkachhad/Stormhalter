using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class BattleAxe : Axe
{
	/// <inheritdoc />
	public override uint BasePrice => 30;

	/// <inheritdoc />
	public override int Weight => 2200;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Slashing;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="BattleAxe"/> class.
	/// </summary>
	public BattleAxe() : base(146)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BattleAxe"/> class.
	/// </summary>
	public BattleAxe(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200016)); /* [You are looking at] [a large battle axe.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250015)); /* The battle axe appears quite ordinary. */
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