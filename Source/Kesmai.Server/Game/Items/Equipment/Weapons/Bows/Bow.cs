using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Bow : ProjectileWeapon
{
	/// <inheritdoc />
	public override int LabelNumber => 6000015;
		
	/// <inheritdoc />
	public override int Category => 2;

	/// <inheritdoc />
	public override Skill Skill => Skill.Bow;

	/// <inheritdoc />
	public override int NockSound => 76;
		
	/// <inheritdoc />
	public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bow;

	/// <summary>
	/// Initializes a new instance of the <see cref="Bow"/> class.
	/// </summary>
	protected Bow(int bowID) : base(bowID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Bow"/> class.
	/// </summary>
	protected Bow(Serial serial) : base(serial)
	{
	}

	public override void OnDropped()
	{
		/* When the bow is dropped, either from death, fumble, or moved, it will unnock. */
		if (IsNocked)
			Unnock();
			
		base.OnDropped();
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