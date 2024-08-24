using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class MainGauche : Dagger, ITreasure
{
		
	public override int LabelNumber => 6000106; /*gauche*/
	/// <inheritdoc />
	public override int Weight => 20;

	/// <inheritdoc />
	public override uint BasePrice => 100;

	/// <inherit />
	public override Skill Skill => Skill.Dagger;

	/// <inherit />
	public override int BaseArmorBonus => 4;
		
	/// <inheritdoc />
	public override int BaseAttackBonus => 2;
		
	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;
		
	/// <inheritdoc />
	protected override int PoisonedItemId => 901;
		
	/// <inherit />
	public override WeaponFlags Flags => WeaponFlags.Piercing | WeaponFlags.Silver | WeaponFlags.Neutral;

	/// <inherit />
	public override bool CanBind => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="MainGauche"/> class.
	/// </summary>
	public MainGauche() : base(900)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="MainGauche"/> class.
	/// </summary>
	public MainGauche(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200341)); /* [You are looking at] [an antique Lengian parrying dagger with a gold heron inscribed in the smooth blue gilted metal. The weapon feels exceptionally well balanced. The weapon is neutral.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250080)); /* The combat adds for this weapon are +2. */
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