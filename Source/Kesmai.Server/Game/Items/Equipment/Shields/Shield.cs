using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract partial class Shield : ItemEntity, IArmored, IWieldable
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000082;

	/// <summary>
	/// Gets the base armor bonus provided by this <see cref="Armor"/>.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int BaseArmorBonus { get { return 0; } }

	/// <summary>
	/// Gets the protection provided against slashing attacks.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int SlashingProtection { get { return 0; } }

	/// <summary>
	/// Gets the protection provided against peircing attacks.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int PiercingProtection { get { return 0; } }

	/// <summary>
	/// Gets the protection provided against bashing attacks.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int BashingProtection { get { return 0; } }

	/// <summary>
	/// Gets the protection provided against projectile attacks.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProjectileProtection { get { return 0; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="Shield"/> class.
	/// </summary>
	protected Shield(int shieldID) : base(shieldID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Shield"/> class.
	/// </summary>
	protected Shield(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Shields only provide a blocking bonus against weapons when equipped in the left-hand.
	/// </remarks>
	public override int CalculateBlockingBonus(ItemEntity item)
	{
		if (item is IWeapon weapon && weapon.Flags.HasFlag(WeaponFlags.Projectile))
			return ProjectileProtection;

		return BaseArmorBonus;
	}
	
	/// <inheritdoc />
	public virtual void OnBlock(MobileEntity attacker)
	{
	}
		
	public virtual void OnWield(MobileEntity entity)
	{
	}
		
	public virtual void OnUnwield(MobileEntity entity)
	{
	}
	
	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
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