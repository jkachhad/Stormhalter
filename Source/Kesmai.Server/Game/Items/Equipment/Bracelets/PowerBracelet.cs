using System;
using System.Collections.Generic;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class PowerBracelet : Bracelet, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 5000;

	/// <inheritdoc />
	public override int Weight => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="PowerBracelet"/> class.
	/// </summary>
	public PowerBracelet() : this(ItemQuality.Common)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="PowerBracelet"/> class.
	/// </summary>
	public PowerBracelet(ItemQuality quality) : base(135)
	{
		Quality = quality;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PowerBracelet"/> class.
	/// </summary>
	public PowerBracelet(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200377)); /* [You are looking at] [a golden bracelet embued with sapphires.] */
	}

	public override bool CanUse(MobileEntity entity)
	{
		if (entity is PlayerEntity player)
			return player.Profession.RestrictSpellcast; /* Thief, Thaum, Wizard. */
			
		return false;
	}
		
	private int GetMagicDamageDealtIncrease()
	{
		/*
			Poor = 5
			Common = 5
			Uncommon = 6
			Rare = 9
			Epic = 13
			Legendary = 21
			Artifact = 30
			Mythical = 41
		*/
		var bonus = 5;

		if (Quality > ItemQuality.Common)
			bonus += (int)Math.Pow(Quality.Value, 2);

		return bonus;
	}
		
	protected override bool OnEquip(MobileEntity entity)
	{
		if (!base.OnEquip(entity))
			return false;
			
		if (CanUse(entity))
		{
			var magicDamageDealtIncrease = GetMagicDamageDealtIncrease();

			if (magicDamageDealtIncrease > 0)
				entity.Stats[EntityStat.MagicDamageDealtIncrease].Add(+magicDamageDealtIncrease, ModifierType.Constant);
		}

		return true;
	}
		
	protected override bool OnUnequip(MobileEntity entity)
	{
		if (!base.OnUnequip(entity))
			return false;
			
		if (CanUse(entity))
		{
			var magicDamageDealtIncrease = GetMagicDamageDealtIncrease();

			if (magicDamageDealtIncrease > 0)
				entity.Stats[EntityStat.MagicDamageDealtIncrease].Remove(+magicDamageDealtIncrease, ModifierType.Constant);
		}

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