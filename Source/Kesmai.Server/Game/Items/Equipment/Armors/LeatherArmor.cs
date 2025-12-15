using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class LeatherArmor : Armor
{
	/// <inheritdoc />
	public override uint BasePrice => 25;

	/// <inheritdoc />
	public override int Weight => 1500;

	/// <inheritdoc />
	public override int Hindrance => 1;

	/// <inheritdoc />
	public override int SlashingProtection => 1;

	/// <inheritdoc />
	public override int BashingProtection => 1;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherArmor"/> class.
	/// </summary>
	public LeatherArmor() : base(242)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LeatherArmor"/> class.
	/// </summary>
	public LeatherArmor(Serial serial) : base(serial)
	{
	}
	
	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		entity.Stats[EntityStat.MeleeDamageMitigation].Add(+1, ModifierType.Constant);
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);
        
		entity.Stats[EntityStat.MeleeDamageMitigation].Remove(+1, ModifierType.Constant);
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200002)); /* [You are looking at] [a suit of leather armor.] */
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