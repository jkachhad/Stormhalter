using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items;

public partial class BirchWand : Wand, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 500;

	/// <inheritdoc />
	public override int Weight => 500;
		
	/// <inheritdoc />
	public override TargetFlags TargetFlags => TargetFlags.Mobiles;

	/// <inheritdoc />

	public override Type ContainedSpell => typeof(BlindSpell);
	/// <summary>
	/// Initializes a new instance of the <see cref="BirchWand"/> class.
	/// </summary>
	public BirchWand() : base(206)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BirchWand"/> class.
	/// </summary>
	public BirchWand(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200225)); /* [You are looking at] [a birch wand.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250114)); /* The wand contains the spell of Blind. */
	}
		
	public override Spell GetSpell()
	{
		return new BlindSpell()
		{
			Item = this,
				
			Intensity = 2,
			SkillLevel = 8,
				
			Cost = 0,
		};
	}
		
	protected override void OnTarget(MobileEntity source, MobileEntity target)
	{
		var spell = GetSpell();

		if (spell is BlindSpell blind)
		{
			blind.Warm(source);
			blind.CastAt(target);
		}
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