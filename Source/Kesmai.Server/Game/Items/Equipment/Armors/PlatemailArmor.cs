using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class PlatemailArmor : Armor
{
	/// <inheritdoc />
	public override uint BasePrice => 100;

	/// <inheritdoc />
	public override int Weight => 4000;

	/// <inheritdoc />
	public override int Hindrance => 2;

	/// <inheritdoc />
	public override int SlashingProtection => 2;

	/// <inheritdoc />
	public override int PiercingProtection => 2;

	/// <inheritdoc />
	public override int BashingProtection => 2;

	/// <inheritdoc />
	public override int ProjectileProtection => 1;

	/// <inheritdoc />
	public override bool RestrictSpellcast => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlatemailArmor"/> class.
	/// </summary>
	public PlatemailArmor() : base(241)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PlatemailArmor"/> class.
	/// </summary>
	public PlatemailArmor(Serial serial) : base(serial)
	{
	}
	
	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		entity.Stats[EntityStat.MeleeDamageMitigation].Add(+2, ModifierType.Constant);
		entity.Stats[EntityStat.RangedDamageMitigation].Add(+2, ModifierType.Constant);
		entity.Stats[EntityStat.ProjectileDamageMitigation].Add(+1, ModifierType.Constant);
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);
        
		entity.Stats[EntityStat.MeleeDamageMitigation].Remove(+2, ModifierType.Constant);
		entity.Stats[EntityStat.RangedDamageMitigation].Remove(+2, ModifierType.Constant);
		entity.Stats[EntityStat.ProjectileDamageMitigation].Remove(+1, ModifierType.Constant);
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200026)); /* [You are looking at] [an iron breastplate and greaves.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250022)); /* The armor appears quite ordinary. */
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