using System.Collections.Generic;
using System.IO;
using System.Linq;
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

	public override bool CanBind => true;

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
		Bind(owner);
	}
	
	public Spellbook(Serial serial) : base(serial)
	{
	}
	
	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200216)); /* [You are looking at] [a small leather-bound book, with mystic runes engraved on the cover.] */

		if (Identified && Owner != null)
			entries.Add(new LocalizationEntry(6300341, Profession.Info.Name.ToLower())); /* This book belongs to a {0}. */
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