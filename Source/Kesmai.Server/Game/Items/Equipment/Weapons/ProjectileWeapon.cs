using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kesmai.Server.Entity;
using Kesmai.Server.Targeting;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class ProjectileWeapon : Weapon
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
						source.QueueRoundTimer();
				}
				else
				{
					source.SendLocalizedMessage(Color.Red, 6100009); /* You can't do that here. */
				}
			}
		}
	}
}
