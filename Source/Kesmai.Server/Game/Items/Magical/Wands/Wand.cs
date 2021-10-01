using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public abstract partial class Wand : MeleeWeapon, IEmpowered, ICharged
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000097;
		
		/// <inheritdoc />
		public override uint BasePrice => 500;
		
		/// <inheritdoc />
		public override int Weight => 300;

		/// <inheritdoc />
		public override int Category => 3;
		
		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 2;
		
		/// <inheritdoc />
		public override Skill Skill => Skill.Staff;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Piercing;

		/// <summary>
		/// Gets the target flags utilized by this <see cref="Wand"/>.
		/// </summary>
		public virtual TargetFlags TargetFlags => TargetFlags.Path;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Wand"/> class.
		/// </summary>
		protected Wand(int wandId) : this(wandId, 5)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Wand"/> class.
		/// </summary>
		protected Wand(int wandId, int charges) : base(wandId)
		{
			_chargesCurrent = _chargesMax = charges;
		}

		#region ICharged

		private int _chargesMax;
		private int _chargesCurrent;

		/// <summary>
		/// Gets the current charges available.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public int ChargesCurrent
		{
			get => _chargesCurrent;
			set => _chargesCurrent = value;
		}

		/// <summary>
		/// Gets the maximum charges available.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public int ChargesMax
		{
			get => _chargesMax;
			set => _chargesMax = value;
		}
		
		#endregion

		#region IEmpowered

		/// <inheritdoc />
		public abstract Type ContainedSpell { get; }
		
		public abstract Spell GetSpell();
		
		/// <inheritdoc />
		public bool ContainsSpell(out Spell spell)
		{
			spell = default(Spell);

			if (ChargesCurrent > 0)
				spell = GetSpell();

			return (spell != default(Spell));
		}
		
		#endregion
		
		/// <inheritdoc />
		public override ActionType GetAction()
		{
			/* This item can be used from either hands. */
			var container = Container;

			if (container is Hands)
				return ActionType.Use;
			
			return base.GetAction();
		}
		
		/// <inheritdoc />
		public override bool HandleInteraction(MobileEntity entity, ActionType action)
		{
			if (action != ActionType.Use)
				return base.HandleInteraction(entity, action);

			if (ChargesCurrent > 0)
			{
				entity.Target = new InternalTarget(this, TargetFlags);
				return true;
			}

			return false;
		}

		protected virtual void OnTarget(MobileEntity source, Point2D location)
		{
		}
		
		protected virtual void OnTarget(MobileEntity source, MobileEntity target)
		{
		}

		private class InternalTarget : Target
		{
			private Wand _wand;
			
			public InternalTarget(Wand wand, TargetFlags flags) : base(3, flags | TargetFlags.Harmful)
			{
				_wand = wand;
			}

			protected override void OnTarget(MobileEntity source, object target)
			{
				base.OnTarget(source, target);

				if (target is MobileEntity mobile && mobile != source && mobile.IsAlive)
				{
					if (_wand != null && _wand.ChargesCurrent > 0)
					{
						_wand.OnTarget(source, mobile);
						_wand.ChargesCurrent--;
					}
				}
			}

			protected override void OnPath(MobileEntity source, List<Direction> path)
			{
				var segment = source.Segment;
				var target = source.Location;

				foreach (var direction in path)
				{
					/* We continue adding a direction until our target is out of LOS. */
					if (!source.InLOS(target))
						break;
				
					var next = target + direction;
					var segmentTile = segment.FindTile(next);
				
					/* We've extended beyond the max visibility, fizzle the spell. 
					 * If the next tile does not allow pathing through 
					 * we interrupt the path. This will cause the spell the fizzle. */
					if (source.GetDistanceToMax(next) > 3 || !segmentTile.AllowsSpellPath(source))
						return;

					target = next;
				}

				if (_wand != null && _wand.ChargesCurrent > 0)
				{
					_wand.OnTarget(source, target);
					_wand.ChargesCurrent--;
				}
			}
		}
	}
}