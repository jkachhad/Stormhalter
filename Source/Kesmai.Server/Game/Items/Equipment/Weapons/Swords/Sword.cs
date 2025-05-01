using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract partial class Sword : MeleeWeapon
{
	/// <inheritdoc />
	public override int LabelNumber => 6000090;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;

	/// <inheritdoc />
	public override int Category => 2;
		
	/// <inheritdoc />
	public override Skill Skill => Skill.Sword;

	/// <summary>
	/// Initializes a new instance of the <see cref="Sword" /> class.
	/// </summary>
	protected Sword(int swordID) : base(swordID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Sword" /> class.
	/// </summary>
	protected Sword(Serial serial) : base(serial)
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