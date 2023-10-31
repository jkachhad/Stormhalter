using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract partial class Equipment : ItemEntity
{
	/// <summary>
	/// Gets the hindrance penalty for this <see cref="Equipment"/>.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Hindrance => 0;

	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromDaze => 0;
		
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromFire => 0;
		
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromIce => 0;
		
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromConcussion => 0;
		
	/// <summary>
	/// Gets the health regeneration provided by this <see cref="Equipment"/>
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int HealthRegeneration => 0;
		
	/// <summary>
	/// Gets the stamina regeneration provided by this <see cref="Equipment"/>
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int StaminaRegeneration => 0;
		
	/// <summary>
	/// Gets the mana regeneration provided by this <see cref="Equipment"/>
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ManaRegeneration => 0;

	/// <summary>
	/// Gets a value indicating if this instance restricts spell casting for certain professions.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool RestrictSpellcast => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="Equipment"/> class.
	/// </summary>
	protected Equipment(int equipmentId) : base(equipmentId)
	{
	}

	protected override bool OnEquip(MobileEntity entity)
	{
		if (!base.OnEquip(entity))
			return false;
			
		if (CanUse(entity))
		{
			if (ProtectionFromFire > 0)
				entity.Stats[EntityStat.FireProtection].Add(+ProtectionFromFire, ModifierType.Constant);
				
			if (ProtectionFromIce > 0)
				entity.Stats[EntityStat.IceProtection].Add(+ProtectionFromIce, ModifierType.Constant);
				
			if (ProtectionFromDaze > 0)
				entity.Stats[EntityStat.DazeProtection].Add(+ProtectionFromDaze, ModifierType.Constant);

			if (HealthRegeneration > 0)
				entity.Stats[EntityStat.HealthRegenerationRate].Add(+HealthRegeneration, ModifierType.Constant);

			if (StaminaRegeneration > 0)
				entity.Stats[EntityStat.StaminaRegenerationRate].Add(+StaminaRegeneration, ModifierType.Constant);

			if (ManaRegeneration > 0)
				entity.Stats[EntityStat.ManaRegenerationRate].Add(+ManaRegeneration, ModifierType.Constant);
		}

		return true;
	}
		
	protected override bool OnUnequip(MobileEntity entity)
	{
		if (!base.OnUnequip(entity))
			return false;
			
		if (CanUse(entity))
		{
			if (ProtectionFromFire > 0)
				entity.Stats[EntityStat.FireProtection].Remove(+ProtectionFromFire, ModifierType.Constant);
				
			if (ProtectionFromIce > 0)
				entity.Stats[EntityStat.IceProtection].Remove(+ProtectionFromIce, ModifierType.Constant);
				
			if (ProtectionFromDaze > 0)
				entity.Stats[EntityStat.DazeProtection].Remove(+ProtectionFromDaze, ModifierType.Constant);

			if (HealthRegeneration > 0)
				entity.Stats[EntityStat.HealthRegenerationRate].Remove(+HealthRegeneration, ModifierType.Constant);

			if (StaminaRegeneration > 0)
				entity.Stats[EntityStat.StaminaRegenerationRate].Remove(+StaminaRegeneration, ModifierType.Constant);
				
			if (ManaRegeneration > 0)
				entity.Stats[EntityStat.ManaRegenerationRate].Remove(+ManaRegeneration, ModifierType.Constant);
		}

		return true;
	}
	
	/// <inheritdoc />
	protected override void OnBreak(MobileEntity source)
	{
		base.OnBreak(source);

		if (this is not ITreasure)
			return;

		// Only players get experience when breaking items.
		if (source is not PlayerEntity player)
			return;

		var value = 0;

		// Conjured (recall rings) and purchased items do not provide a bonus.
		if (IsConjured || IsPurchased)
			return;

		var multiplier = 10;

		if (IsBound)
			multiplier += 5;

		value = Utility.RandomRange((int)(ActualPrice * multiplier), 0.5, 0.9);

		if (Owner != null && Owner != player)
		{
			if (player.Alignment.IsAny(Alignment.Lawful))
				player.Alignment = Alignment.Neutral;
		}
				
		if (value > 0)
			player.AwardExperience(value);
	}
}