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
	protected Mace(IEntityProvider entityProvider, EntityDependencies entityDependencies, int maceID) : base(entityProvider, entityDependencies, maceID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Mace"/> class.
	/// </summary>

	protected Mace(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
