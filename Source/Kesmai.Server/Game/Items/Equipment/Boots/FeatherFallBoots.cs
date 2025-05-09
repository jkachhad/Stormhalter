using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class FeatherFallBoots : Boots, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1500;

	/// <inheritdoc />
	public override int Weight => 480;

	/// <inheritdoc />
	public override int Hindrance => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="FeatherFallBoots"/> class.
	/// </summary>
	public FeatherFallBoots() : base(131)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="FeatherFallBoots"/> class.
	/// </summary>
	public FeatherFallBoots(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200039)); /* [You are looking at] [a pair of gray boots rimmed with white fur.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250029)); /* The boots contain the spell of Feather Fall. */
	}

	/// <inheritdoc />
	protected override bool OnEquip(MobileEntity entity)
	{
		var onEquip = base.OnEquip(entity);

		if (!entity.GetStatus(typeof(FeatherFallStatus), out var status))
		{
			status = new FeatherFallStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 14 }
			};
			status.AddSource(new ItemSource(this));
				
			entity.AddStatus(status);
		}
		else
		{
			status.AddSource(new ItemSource(this));
		}

		return onEquip;
	}

	/// <inheritdoc />
	protected override bool OnUnequip(MobileEntity entity)
	{
		if (!base.OnUnequip(entity))
			return false;

		if (entity.GetStatus(typeof(FeatherFallStatus), out var status))
			status.RemoveSource(this);

		return true;
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