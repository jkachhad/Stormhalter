using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Sword : MeleeWeapon
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
	protected Sword(IEntityProvider entityProvider, EntityDependencies entityDependencies, int swordID) : base(entityProvider, entityDependencies, swordID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Sword" /> class.
	/// </summary>

	protected Sword(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
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
