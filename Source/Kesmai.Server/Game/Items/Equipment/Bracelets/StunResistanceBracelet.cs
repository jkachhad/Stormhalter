using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class StunResistanceBracelet : Bracelet, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1200;

	/// <inheritdoc />
	public override int Weight => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="StunResistanceBracelet"/> class.
	/// </summary>
	public StunResistanceBracelet() : base(10)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="StunResistanceBracelet"/> class.
	/// </summary>
	public StunResistanceBracelet(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200077)); /* [You are looking at] [a jade bracelet.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250055)); /* The bracelet contains the spell of Stun Resistance. */
	}

	/// <inheritdoc />
	public override void GetTooltipDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200077)); /* a jade bracelet. */
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateModifiers(MobileEntity entity)
	{
		base.OnActivateModifiers(entity);

		if (!entity.GetStatus(typeof(StunResistanceStatus), out var resistance))
		{
			resistance = new StunResistanceStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 51 }
			};
			resistance.AddSource(new ItemSource(this));
				
			entity.AddStatus(resistance);
		}
		else
		{
			resistance.AddSource(new ItemSource(this));
		}
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateModifiers(MobileEntity entity)
	{
		base.OnInactivateModifiers(entity);

		if (entity.GetStatus(typeof(StunResistanceStatus), out var resistance))
			resistance.RemoveSource(this);
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

	/// <inheritdoc />
	public override void GetTooltipEntries(PlayerEntity beholder, Tooltip tooltip)
	{
		base.GetTooltipEntries(beholder, tooltip);

		tooltip.AddEffect(TooltipEffectTrigger.Equip, 6610000, 551); /* Grants Stun Resistance. */
	}
}
