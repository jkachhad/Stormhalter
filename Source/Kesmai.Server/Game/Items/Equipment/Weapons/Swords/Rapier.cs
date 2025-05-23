using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class Rapier : Sword
{
	/// <inheritdoc />
	public override int LabelNumber => 6000071;

	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 1150;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 6;

	/// <inheritdoc />
	public override int BaseArmorBonus => 1;

	/// <inheritdoc />
	public override Skill Skill => Skill.Rapier;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Piercing;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Rapier"/> class.
	/// </summary>
	public Rapier() : this(157)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Rapier"/> class.
	/// </summary>
	public Rapier(int rapierId) : base(rapierId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Rapier"/> class.
	/// </summary>
	public Rapier(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200013)); /* [You are looking at] [a shiny steel rapier.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250012)); /* The rapier appears quite ordinary. */
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