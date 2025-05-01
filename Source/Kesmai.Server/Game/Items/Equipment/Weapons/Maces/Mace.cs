using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Mace : MeleeWeapon
{
	/// <inheritdoc />
	public override int LabelNumber => 6000060;

	/// <inheritdoc />
	public override int Category => 2;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;

	/// <inheritdoc />
	public override Skill Skill => Skill.Mace;

	/// <summary>
	/// Initializes a new instance of the <see cref="Mace"/> class.
	/// </summary>
	protected Mace(int maceID) : base(maceID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Mace"/> class.
	/// </summary>
	protected Mace(Serial serial) : base(serial)
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