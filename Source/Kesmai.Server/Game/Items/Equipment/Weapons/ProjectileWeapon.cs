using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Entity;
using Kesmai.Server.Targeting;

using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public abstract class ProjectileWeapon : Weapon
{
	private bool _isNocked;
		
	/// <summary>
	/// Gets the nocked item ID.
	/// </summary>
	public abstract int NockedID { get; }

	/// <summary>
	/// Gets the nock sound.
	/// </summary>
	public abstract int NockSound { get; }

	/// <summary>
	/// Gets or sets the container that hold this item.
	/// </summary>
	public bool IsNocked
	{
		get => _isNocked;
		set
		{
			var oldValue = _isNocked;
			var newValue = value;

			if (oldValue == newValue)
				return;

			_isNocked = value;
				
			Delta(ItemDelta.UpdateIcon);
			Delta(ItemDelta.UpdateAction);
		}
	}

	/// <summary>
	/// Gets or sets the item identifier.
	/// </summary>
	public override int ItemId => IsNocked ? NockedID : base.ItemId;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProjectileWeapon"/> class.
	/// </summary>
	protected ProjectileWeapon(int projectileWeaponID) : base(projectileWeaponID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ProjectileWeapon"/> class.
	/// </summary>
	protected ProjectileWeapon(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		if (Container is Hands && ReferenceEquals(source.RightHand, this))
		{
			if (IsNocked)
			{
				entries.Add(ShootProjectileInteraction.Instance);
				entries.Add(UnnockProjectileInteraction.Instance);
			}
			else
			{
				entries.Add(NockProjectileInteraction.Instance);
			}
		}
		
		entries.Add(InteractionSeparator.Instance);
		
		base.GetInteractions(source, entries);
	}

	/// <inheritdoc />
	public override ActionType GetAction()
	{
		var parent = Parent;

		if (parent is MobileEntity mobile && mobile.RightHand == this)
			return IsNocked ? ActionType.Shoot : ActionType.Nock;

		return base.GetAction();
	}

	/// <inheritdoc />
	public override bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Nock && action != ActionType.Shoot)
			return base.HandleInteraction(entity, action);

		if (action == ActionType.Nock && !IsNocked)
		{
			if (!entity.CanPerformAction)
				return false;
				
			if (!entity.HasFreeHand(out var slot))
			{
				entity.SendLocalizedMessage(Color.Red, 6100020); /* You must have a free hand to do that. */
				return false;
			}

			Nock();

			var nockSound = NockSound;

			if (nockSound > 0)
				entity.EmitSound(nockSound, 3, 6);

			entity.QueueRoundTimer();
			return true;
		}
			
		if (action == ActionType.Shoot && IsNocked)
		{
			entity.Target = new ShootItemTarget(this);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Nocks this weapon.
	/// </summary>
	public void Nock()
	{
		IsNocked = true;
	}

	/// <summary>
	/// Unnocks this weapon.
	/// </summary>
	public void Unnock()
	{
		IsNocked = false;
	}

	/* Projectile weapons take two turns to use. Once to Nock, and the second to shoot.\
	 * The skill gain should be double. */

	public override double GetSkillMultiplier()
	{
		return 2.0;
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */

		writer.Write(IsNocked);
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
				IsNocked = reader.ReadBoolean();

				break;
			}
		}
	}
}

public class ShootItemTarget : MobileTarget
{
	private ProjectileWeapon _weapon;

	/// <summary>
	/// Initializes a new instance of the <see cref="ShootItemTarget"/> class.
	/// </summary>
	public ShootItemTarget(ProjectileWeapon weapon) : base(flags: TargetFlags.Harmful)
	{
		_weapon = weapon;
	}

	/// <summary>
	/// Called when a target is acquired.
	/// </summary>
	protected override void OnTarget(MobileEntity source, MobileEntity target)
	{
		if (_weapon != null)
		{
			if (source != target && source.CheckShoot(_weapon))
			{
				if (target != null && source.Shoot(target))
				{
					if (!source.HasStatus(typeof(RapidfireStatus)))
						source.QueueRoundTimer();
				}
			}
			else
			{
				source.SendLocalizedMessage(Color.Red, 6100009); /* You can't do that here. */
			}
		}
	}
}

public class NockProjectileInteraction : InteractionEntry
{
	public static readonly NockProjectileInteraction Instance = new NockProjectileInteraction();

	private NockProjectileInteraction() : base("Nock", range: 0)
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not ProjectileWeapon weapon || weapon.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}
		
		if (!source.HasFreeHand(out var slot))
		{
			source.SendLocalizedMessage(Color.Red, 6100020); /* You must have a free hand to do that. */
			return;
		}

		// Must be wielded in right hand to nock.
		if (source.RightHand != weapon)
			return;

		if (weapon.IsNocked)
			return;

		weapon.Nock();

		var nockSound = weapon.NockSound;

		if (nockSound > 0)
			source.EmitSound(nockSound, 3, 6);

		source.QueueRoundTimer();
	}
}

public class UnnockProjectileInteraction : InteractionEntry
{
	public static readonly UnnockProjectileInteraction Instance = new UnnockProjectileInteraction();

	private UnnockProjectileInteraction() : base("Unnock", range: 0)
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not ProjectileWeapon weapon || weapon.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}
		
		// Must be wielded in right hand to unnock.
		if (source.RightHand != weapon)
			return;
		
		if (!weapon.IsNocked)
			return;
		
		weapon.Unnock();
		source.QueueRoundTimer();
	}
}

public class ShootProjectileInteraction : InteractionEntry
{
	public static readonly ShootProjectileInteraction Instance = new ShootProjectileInteraction();

	private ShootProjectileInteraction() : base("Shoot", range: 0)
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not ProjectileWeapon weapon || weapon.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		// Must be wielded in right hand and nocked to shoot.
		if (source.RightHand != weapon || !weapon.IsNocked)
			return;

		source.Target = new ShootItemTarget(weapon);
	}
}
