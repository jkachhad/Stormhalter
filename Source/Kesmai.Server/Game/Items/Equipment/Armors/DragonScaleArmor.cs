using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class DragonScaleArmor : Armor, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000076; /* scales */

	/// <inheritdoc />
	public override uint BasePrice => 800;

	/// <inheritdoc />
	public override int Weight => 3000;

	/// <inheritdoc />
	public override int Hindrance => 2;

	/// <inheritdoc />
	public override int SlashingProtection => 3;

	/// <inheritdoc />
	public override int PiercingProtection => 3;

	/// <inheritdoc />
	public override int BashingProtection => 3;

	/// <inheritdoc />
	public override int ProjectileProtection => 3;

	/// <inheritdoc />
	public override int ProtectionFromFire => 5;

	/// <inheritdoc />
	public override int ProtectionFromIce => 5;

	/// <summary>
	/// Initializes a new instance of the <see cref="DragonScaleArmor"/> class.
	/// </summary>
	public DragonScaleArmor() : base(243)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="DragonScaleArmor"/> class.
	/// </summary>
	public DragonScaleArmor(Serial serial) : base(serial)
	{
	}
	
	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		entity.Stats[EntityStat.MeleeDamageMitigation].Add(+3, ModifierType.Constant);
		entity.Stats[EntityStat.RangedDamageMitigation].Add(+3, ModifierType.Constant);
		entity.Stats[EntityStat.ProjectileDamageMitigation].Add(+3, ModifierType.Constant);
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);
        
		entity.Stats[EntityStat.MeleeDamageMitigation].Remove(+3, ModifierType.Constant);
		entity.Stats[EntityStat.RangedDamageMitigation].Remove(+3, ModifierType.Constant);
		entity.Stats[EntityStat.ProjectileDamageMitigation].Remove(+3, ModifierType.Constant);
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200173)); /* [You are looking at] [a vest made of dragon scales.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250096)); /* The armor appears to have some magical properties. */
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