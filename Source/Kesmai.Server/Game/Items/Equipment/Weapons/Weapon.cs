using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract partial class Weapon : ItemEntity, IWeapon, IArmored, IWieldable
{
	/// <summary>
	/// Gets the attack bonus provided by this <see cref="IWeapon" /> for <see cref="MobileEntity" />.
	/// </summary>
	/// <remarks>
	/// Attack bonus provided by weapons is dependent on weapon skill, <see cref="PlayerEntity.Dexterity"/>, 
	/// and <see cref="BaseAttackBonus"/>.
	/// </remarks>
	public virtual double GetAttackBonus(MobileEntity attacker, MobileEntity defender)
	{
		var attackBonus = BaseAttackBonus;

		/* Enchanted weapons from a Knight have additional attack bonus based on the level of the knight. */
		if (IsEnchanted)
		{
			if (attacker is PlayerEntity player)
				attackBonus += ((player.Level - 5) / 3).Clamp(1, 5);
			else
				attackBonus += 1;
		}

		/*
		 * "The Black Broadsword (BBS for short) is the best weapon in the game. It hits hard and blocks well.
		 * 	Though the Silver Greataxe can hit harder at times, the BBS has a harder hitting average than the
		 * 	Greataxe. The BBS has two advantages. First, it blocks well, and the BBS is "lawful" so it gains
		 * 	one extra damage add against evil crits like dragons and drakes."
		 * 
		 */
		if (defender != null)
		{
			if (Flags.HasFlag(WeaponFlags.Lawful) && defender.Alignment == Alignment.Evil)
				attackBonus += 1;
		}

		return attackBonus;
	}
		
	/// <inheritdoc/>
	/// <remarks>
	/// Weapons only provide a blocking bonus against other weapons. Two-handed weapons
	/// should only provide a benefit if equipped in the right-hand.
	/// </remarks>
	public override int CalculateBlockingBonus(ItemEntity item)
	{
		if (item is IWeapon weapon && weapon.Flags.HasFlag(WeaponFlags.Projectile))
			return ProjectileProtection;

		return BaseArmorBonus;
	}
		
	/// <summary>
	/// Calculates the fumble chance as a percent.
	/// </summary>
	public override double CalculateFumbleChance(MobileEntity entity)
	{
		// Skill Level = 3 =>  ((3 + 1)^2) * 10 = 160 => 1 / 160;
		// Skill Level = 4 =>  ((4 + 1)^2) * 10 = 250 => 1 / 250;
		return 1 / (10 * Math.Pow(entity.GetSkillLevel(Skill) + 1, 2));
	}

	public virtual void OnWield(MobileEntity entity)
	{
		if (CanUse(entity))
		{
			if (HealthRegeneration > 0)
				entity.Stats[EntityStat.HealthRegenerationRate].Add(+HealthRegeneration, ModifierType.Constant);

			if (StaminaRegeneration > 0)
				entity.Stats[EntityStat.StaminaRegenerationRate].Add(+StaminaRegeneration, ModifierType.Constant);
				
			if (ManaRegeneration > 0)
				entity.Stats[EntityStat.ManaRegenerationRate].Add(+ManaRegeneration, ModifierType.Constant);
		}
	}
		
	public virtual void OnUnwield(MobileEntity entity)
	{
		if (CanUse(entity))
		{
			if (HealthRegeneration > 0)
				entity.Stats[EntityStat.HealthRegenerationRate].Remove(+HealthRegeneration, ModifierType.Constant);

			if (StaminaRegeneration > 0)
				entity.Stats[EntityStat.StaminaRegenerationRate].Remove(+StaminaRegeneration, ModifierType.Constant);
				
			if (ManaRegeneration > 0)
				entity.Stats[EntityStat.ManaRegenerationRate].Remove(+ManaRegeneration, ModifierType.Constant);
		}
	}

	/// <inheritdoc />
	public override bool BreaksHide(MobileEntity entity)
	{
		return Flags.HasFlag(WeaponFlags.TwoHanded);
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
	
	/// <inheritdoc />
	public virtual void OnBlock(MobileEntity attacker)
	{
	}
}