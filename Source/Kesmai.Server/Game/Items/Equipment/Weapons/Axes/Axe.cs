using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract partial class Axe : MeleeWeapon
{
	/// <inheritdoc />
	public override int LabelNumber => 6000005;
		
	/// <inheritdoc />
	public override int Category => 2;

	/// <inheritdoc />
	public override Skill Skill => Skill.Mace;

	/// <inheritdoc />
	public override ShieldPenetration Penetration => ShieldPenetration.Light;

	/// <summary>
	/// Initializes a new instance of the <see cref="Axe"/> class.
	/// </summary>
	protected Axe(int axeID) : base(axeID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Axe"/> class.
	/// </summary>
	protected Axe(Serial serial) : base(serial)
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