using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class CrocodileBoots : Boots, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 500;

	/// <inheritdoc />
	public override int Weight => 480;

	/// <inheritdoc />
	public override int Hindrance => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="CrocodileBoots"/> class.
	/// </summary>
	public CrocodileBoots() : base(122)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="CrocodileBoots"/> class.
	/// </summary>
	public CrocodileBoots(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200207)); /* [You are looking at] [a pair of boots made from crocodile hide.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250103)); /* The boots contain the spell of Breathe Water. */
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		if (!entity.GetStatus(typeof(BreatheWaterStatus), out var status))
		{
			status = new BreatheWaterStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 4 }
			};
			status.AddSource(new ItemSource(this));
				
			entity.AddStatus(status);
		}
		else
		{
			status.AddSource(new ItemSource(this));
		}
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);

		if (entity.GetStatus(typeof(BreatheWaterStatus), out var status))
			status.RemoveSource(this);
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