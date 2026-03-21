using System.Collections.Generic;
using System.Drawing;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game.Items.Magical;

public class Spellbook : ItemEntity, ITreasure
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000011;
		
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 0;
		
	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 300;
		
	/// <summary>
	/// Gets the item category.
	/// </summary>
	public override int Category => 3;

	/// <inheritdoc />
	public override bool CanBind => true;
	
	/// <inheritdoc />
	public override bool RequiresIdentification => true;

	public Profession Profession
	{
		get
		{
			if (Owner is PlayerEntity player)
				return player.Profession;

			return default(Profession);
		}
	}
		
	public Spellbook(PlayerEntity owner) : base(152)
	{
		Identified = true;

		Bind(owner);
	}
	
	public Spellbook(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200216); /* a small leather-bound book, with mystic runes engraved on the cover. */
	}
	
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

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