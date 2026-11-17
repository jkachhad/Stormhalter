using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class DexterityRing : Ring, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1500;

	/// <inheritdoc />
	public override int Weight => 20;

	/// <summary>
	/// The dexterity bonus provided by this ring.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int BonusDexterity => 2;

	/// <summary>
	/// Initializes a new instance of the <see cref="DexterityRing"/> class.
	/// </summary>
	public DexterityRing() : this(19)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="DexterityRing"/> class.
	/// </summary>
	public DexterityRing(int ringId) : base(ringId)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DexterityRing"/> class.
	/// </summary>
	public DexterityRing(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200056)); /* [You are looking at] [a wide platinum band engraved with many tiny runes.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250044)); /* The ring greatly increases dexterity. */
	}

	/// <inheritdoc />
	public override void GetTooltipDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200056)); /* a wide platinum band engraved with many tiny runes. */
	}
		
	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	/// <remarks>
	/// The Dexterity comes from <see cref="AgileStatus"/>, with this ring as one of its sources, so that the rings a
	/// player wears add at most +2 Dexterity together.
	/// </remarks>
	protected override void OnActivateModifiers(MobileEntity entity)
	{
		base.OnActivateModifiers(entity);

		if (entity.GetStatus(typeof(AgileStatus), out var status))
		{
			status.AddSource(new ItemSource(this));
		}
		else
		{
			status = new AgileStatus(entity);
			status.AddSource(new ItemSource(this));

			entity.AddStatus(status);
		}
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateModifiers(MobileEntity entity)
	{
		base.OnInactivateModifiers(entity);

		if (entity.GetStatus(typeof(AgileStatus), out var status))
			status.RemoveSource(this);
	}

	/// <inheritdoc />
	public override void GetTooltipEntries(TooltipContext context, Tooltip tooltip)
	{
		base.GetTooltipEntries(context, tooltip);

		/* The Dexterity comes from AgileStatus, not from this ring's own modifiers, so the tooltip's bonus lines can't
		 * show it. This line stands in, in the same color as those lines, until statuses show their bonuses on
		 * tooltips. The bonus is passed as signed text, since an int argument is read as a localization number. */
		if (BonusDexterity > 0)
			tooltip.AddLocalizedText(6600064, Color.FromArgb(180, 220, 255), TooltipTextStyle.Normal, -15000,
				$"{BonusDexterity:+0;-0}"); /* {0} Dexterity */
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
