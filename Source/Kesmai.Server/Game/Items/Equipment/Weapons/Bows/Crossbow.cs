using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class Crossbow : ProjectileWeapon
{
	/// <inheritdoc />
	public override int LabelNumber => 6000031;

	/// <inheritdoc />
	public override uint BasePrice => 40;

	/// <inheritdoc />
	public override int Weight => 1200;

	/// <inheritdoc />
	public override int Category => 2;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;

	/// <inheritdoc />
	public override int MinimumDamage => 1;

	/// <inheritdoc />
	public override int MaximumDamage => 8;

	/// <inheritdoc />
	public override Skill Skill => Skill.Bow;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Crossbow;

	/// <inheritdoc />
	public override int NockSound => 75;

	/// <inheritdoc />
	public override int NockedID => 111;

	/// <summary>
	/// Initializes a new instance of the <see cref="Crossbow"/> class.
	/// </summary>
	public Crossbow() : base(230)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Crossbow"/> class.
	/// </summary>
	public Crossbow(int crossbowId) : base(crossbowId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Crossbow"/> class.
	/// </summary>
	public Crossbow(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200036)); /* [You are looking at] [a crossbow.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250028)); /* The crossbow appears quite ordinary. */
	}
		
	/// <inheritdoc />
	/// <remarks>
	/// All two-handed weapons will break hide, but crossbows are an exception.
	/// We override the behavior from <see cref="Weapon"/>
	/// </remarks>
	public override bool BreaksHide(MobileEntity entity) => false;
	
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