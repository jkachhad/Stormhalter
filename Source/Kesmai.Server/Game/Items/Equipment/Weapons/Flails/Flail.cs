using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

/* Nunchakus, morning stars and flails are included in the flail weapon type. */
/* These weapons are popular with Martial Artists and Thieves because of their compactness. */
public abstract class Flail : MeleeWeapon
{
	/// <inheritdoc />
	public override int Category => 2;
		
	/// <inheritdoc />
	public override int LabelNumber => 6000037;

	/// <inheritdoc />
	public override Skill Skill => Skill.Flail;
		
	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Medium;

	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.Bashing;
		
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Flail"/> class.
	/// </summary>
	protected Flail(int flailID) : base(flailID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Flail"/> class.
	/// </summary>
	protected Flail(Serial serial) : base(serial)
	{
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