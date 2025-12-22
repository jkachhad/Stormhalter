using System;
using System.Collections.Generic;
using System.Drawing;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public abstract class Weapon : ItemEntity, IWeapon, IArmored, IWieldable
{
	protected Poison _poison;

	/// <summary>
	/// Gets the skill utilized by this <see cref="IWeapon"/> during combat.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public abstract Skill Skill { get; }

	/// <summary>
	/// Gets the base attack bonus value for this <see cref="Weapon"/>.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int BaseAttackBonus => 0;

	/// <summary>
	/// Gets the penetration value for this <see cref="Weapon"/>.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual ShieldPenetration Penetration => ShieldPenetration.None;

	/// <summary>
	/// Gets the weapon flags.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual WeaponFlags Flags => WeaponFlags.None;

	/// <summary>
	/// Gets the minimum damage for this <see cref="IWeapon"/>.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int MinimumDamage => 0;

	/// <summary>
	/// Gets the maximum damage for this <see cref="IWeapon"/>.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int MaximumDamage => 0;

	/// <summary>
	/// Gets the base armor bonus provided by this <see cref="IArmored"/>.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int BaseArmorBonus => 0;

	/// <summary>
	/// Gets the protection provided against slashing attacks.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int SlashingProtection => 0;

	/// <summary>
	/// Gets the protection provided against peircing attacks.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int PiercingProtection => 0;

	/// <summary>
	/// Gets the protection provided against bashing attacks.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int BashingProtection => 0;

	/// <summary>
	/// Gets the protection provided against projectile attacks.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProjectileProtection => 0;

	/// <summary>
	/// Gets the maximum range at which this weapon can be used.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int MaxRange => 0;

	/// <summary>
	/// Gets the poison applied to this <see cref="Weapon"/>.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual Poison Poison
	{
		get => _poison;
		set
		{
			if (_poison != value)
				Delta(ItemDelta.UpdateIcon);

			_poison = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsPoisoned => _poison != null;

	/// <summary>
	/// Gets the health regeneration provided by this <see cref="Weapon"/>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int HealthRegeneration => 0;

	/// <summary>
	/// Gets the stamina regeneration provided by this <see cref="Weapon"/>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int StaminaRegeneration => 0;

	/// <summary>
	/// Gets the mana regeneration provided by this <see cref="Weapon"/>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ManaRegeneration => 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="Weapon"/> class.
	/// </summary>
	protected Weapon(int weaponID) : base(weaponID)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Weapon"/> class.
	/// </summary>
	protected Weapon(Serial serial) : base(serial)
	{
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		base.GetInteractions(source, entries);

		entries.Add(InteractionSeparator.Instance);

		if (Container is Hands || Container is Belt || (Container is Backpack && Container.GetSlot(this) < 5))
		{
			// Only add the throw interaction if the weapon is throwable and is in a valid container.
			// Use AddOrReplace to remove previously added throw interactions from base.GetInteractions.
			if ((Flags & WeaponFlags.Throwable) != 0)
				entries.AddOrReplace(ThrowItemInteraction.Instance);
		}

		if (Container is Hands)
			entries.Add(UnwieldWeaponInteraction.Instance);
		else
			entries.Add(WieldWeaponInteraction.Instance);
	}

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
			if (Flags.HasFlag(WeaponFlags.Lawful) && defender.Alignment == Alignment.Evil)
				attackBonus += 1;

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
			if (player.Alignment.IsAny(Alignment.Lawful))
				player.Alignment = Alignment.Neutral;

		if (value > 0)
			player.AwardExperience(value);
	}

	/// <inheritdoc />
	public virtual void OnBlock(MobileEntity attacker)
	{
	}

	/// <inheritdoc />
	public override int GetFumbleLocalization()
	{
		return 6300016; /* The weapon leaps out of your hand. */
	}

	/// <inheritdoc />
	public override ActionType GetAction()
	{
		var container = Container;

		if ((Flags & WeaponFlags.Throwable) != 0)
			if (container is Hands || container is Belt || (container is Backpack && container.GetSlot(this) < 5))
				return ActionType.Throw;

		return base.GetAction();
	}

	/// <inheritdoc />
	public override bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Throw)
			return base.HandleInteraction(entity, action);

		var container = Container;

		if ((Flags & WeaponFlags.Throwable) != 0)
			if (container is Hands || container is Belt || (container is Backpack && container.GetSlot(this) < 5))
			{
				entity.Target = new ThrowItemTarget(this);
				return true;
			}

		return false;
	}

	/// <summary>
	/// Overridable. Determines whether the specified instance can use this item.
	/// </summary>
	public override bool CanUse(MobileEntity entity)
	{
		if (!base.CanUse(entity))
			return false;

		/* I thought I recalled information about being unable to swing two handed weapons with left hand. */
/*			if (entity.LeftHand != null && flags.HasFlag(WeaponFlags.TwoHanded))
				return false;*/

		/* We prevent the weapon from being beneficial if alignment values do not match. */
		var flags = Flags;
		var alignment = entity.Alignment;

		if ((flags.HasFlag(WeaponFlags.Lawful) && alignment != Alignment.Lawful) ||
		    (flags.HasFlag(WeaponFlags.Neutral) && alignment != Alignment.Neutral) ||
		    (flags.HasFlag(WeaponFlags.Chaotic) && alignment != Alignment.Chaotic && alignment != Alignment.Evil))
			return false;

		return true;
	}

	/// <summary>
	/// Gets the swing delay for this <see cref="Weapon"/> for <see cref="MobileEntity"/>.
	/// </summary>
	public virtual TimeSpan GetSwingDelay(MobileEntity entity)
	{
		return entity.GetRoundDelay();
	}

	/// <summary>
	/// Gets the multiplier for skill gain awarded per weapon swing.
	/// </summary>
	public virtual double GetSkillMultiplier()
	{
		return 1.0;
	}

	public virtual void OnHit(MobileEntity attacker, MobileEntity defender)
	{
		if (Poison != null)
		{
			if (defender.IsAlive)
				defender.Poison(attacker, Poison);

			if (attacker.Alignment == Alignment.Lawful)
				attacker.Alignment = Alignment.Neutral;

			Poison = null;
		}
	}

	public override bool DropToLocation(Point2D location)
	{
		/* Remove enchantment if dropped to the world (death, moved, etc.) */
		if (IsEnchanted)
			IsEnchanted = false;

		return base.DropToLocation(location);
	}

	private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
	{
		if (setIf)
			flags |= toSet;
	}

	private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
	{
		return (flags & toGet) != 0;
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)2); /* version */

		var flags = SaveFlag.None;

		SetSaveFlag(ref flags, SaveFlag.IsEnchanted, IsEnchanted);
		SetSaveFlag(ref flags, SaveFlag.Envenomed, _poison != null);

		writer.Write((int)flags);

		if (GetSaveFlag(flags, SaveFlag.Envenomed))
		{
			writer.Write(_poison.Delay);
			writer.Write(_poison.Potency);
		}
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
			case 2:
			{
				var flags = (SaveFlag)reader.ReadInt32();

				if (GetSaveFlag(flags, SaveFlag.IsEnchanted))
					IsEnchanted = true;

				if (GetSaveFlag(flags, SaveFlag.Envenomed))
				{
					var delay = reader.ReadTimeSpan();
					var potency = reader.ReadInt32();

					if (potency > 0)
						Poison = new Venom(potency);
				}

				goto case 1;
			}
			case 1:
			{
				break;
			}
		}
	}

	[Flags]
	private enum SaveFlag
	{
		None = 0x00000000,

		Envenomed = 0x00000010,
		IsEnchanted = 0x00000020
	}
}

public class WieldWeaponInteraction : InteractionEntry
{
	public static readonly WieldWeaponInteraction Instance = new WieldWeaponInteraction();

	private WieldWeaponInteraction() : base("Wield", range: 0)
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not Weapon weapon || weapon.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		if (!source.HasFreeHand(out var handSlot) || !handSlot.HasValue)
		{
			source.SendLocalizedMessage(Color.Red, 6100020); /* You must have a free hand to do that. */
			return;
		}

		source.Lift(weapon, weapon.Amount, out var liftRejectReason);

		if (liftRejectReason.HasValue)
			return;

		source.DropHeld(InventoryGroup.Hands, handSlot.Value);
	}
}

public class UnwieldWeaponInteraction : InteractionEntry
{
	public static readonly UnwieldWeaponInteraction Instance = new UnwieldWeaponInteraction();

	private UnwieldWeaponInteraction() : base("Unwield")
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not Weapon weapon || weapon.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		/* Prefer belt, then backpack. If both fail, keep in hands and inform the player. */
		var destinationGroup = default(InventoryGroup?);
		var destinationSlot = default(int?);

		var belt = source.Belt;

		if (belt != null)
		{
			destinationSlot = belt.CheckHold(weapon);

			if (destinationSlot.HasValue)
				destinationGroup = InventoryGroup.Belt;
		}

		if (!destinationGroup.HasValue)
		{
			var backpack = source.Backpack;

			if (backpack != null)
			{
				destinationSlot = backpack.CheckHold(weapon);

				if (destinationSlot.HasValue)
					destinationGroup = InventoryGroup.Backpack;
			}
		}

		if (!destinationGroup.HasValue)
		{
			source.SendLocalizedMessage(Color.Red, 6300372); /* You do not have enough room to do that. */
			return;
		}

		source.Lift(weapon, weapon.Amount, out var liftRejectReason);

		if (liftRejectReason.HasValue)
			return;

		source.DropHeld(destinationGroup.Value, destinationSlot.Value);
	}
}
