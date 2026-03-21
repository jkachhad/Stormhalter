using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class IceDragonScaleArmor : Armor, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000076; /* scales */

	/// <inheritdoc />
	public override uint BasePrice => 3200;

	/// <inheritdoc />
	public override int Weight => 1600;

	/// <inheritdoc />
	public override int Hindrance => 2;

	/// <inheritdoc />
	public override int SlashingProtection => 5;

	/// <inheritdoc />
	public override int PiercingProtection => 5;

	/// <inheritdoc />
	public override int BashingProtection => 5;

	/// <inheritdoc />
	public override int ProjectileProtection => 5;


	/// <inheritdoc />
	public override int ProtectionFromDaze => 30;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 15;

	/// <inheritdoc />
	public override int ProtectionFromIce => 15;

	/// <summary>
	/// Initializes a new instance of the <see cref="IceDragonScaleArmor"/> class.
	/// </summary>
	public IceDragonScaleArmor() : base(219)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="IceDragonScaleArmor"/> class.
	/// </summary>
	public IceDragonScaleArmor(Serial serial) : base(serial)
	{
	}
	
	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		entity.Stats[EntityStat.MeleeDamageMitigation].Add(+5, ModifierType.Constant);
		entity.Stats[EntityStat.RangedDamageMitigation].Add(+5, ModifierType.Constant);
		entity.Stats[EntityStat.ProjectileDamageMitigation].Add(+5, ModifierType.Constant);
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);
        
		entity.Stats[EntityStat.MeleeDamageMitigation].Remove(+5, ModifierType.Constant);
		entity.Stats[EntityStat.RangedDamageMitigation].Remove(+5, ModifierType.Constant);
		entity.Stats[EntityStat.ProjectileDamageMitigation].Remove(+5, ModifierType.Constant);
	}
	
	/// <inheritdoc />
	public override void AddProperties(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		tooltip.AddMitigations(5, 5, 5);

		base.AddProperties(tooltip, beholder);
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200181); /* [a vest made from the milky white scales of an ice dragon.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250096); /* The armor appears to have some magical properties. */
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
