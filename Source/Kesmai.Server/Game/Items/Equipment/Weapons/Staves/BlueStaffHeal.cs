using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public partial class BlueStaffHeal : BlueStaff, IEmpowered, ICharged
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlueStaffHeal"/> class.
		/// </summary>
		public BlueStaffHeal() : this(5)
		{
			Hue = Color.Yellow;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BlueStaffHeal"/> class.
		/// </summary>
		public BlueStaffHeal(int charges)
		{
			_chargesCurrent = _chargesMax = charges;
		}

		public override void GetDescription(List<LocalizationEntry> entries)
		{
			base.GetDescription(entries);
			
			if (Identified)
				entries.Add(new LocalizationEntry(6250110)); /* The staff contains the spell of Heal. */
		}
		
		#region ICharged

		private int _chargesMax;
		private int _chargesCurrent;

		public int ChargesCurrent
		{
			get => _chargesCurrent;
			set => _chargesCurrent = value;
		}

		public int ChargesMax
		{
			get => _chargesMax;
			set => _chargesMax = value;
		}
		
		#endregion

		public Type ContainedSpell => typeof(HealSpell);
		
		/// <inheritdoc />
		public bool ContainsSpell(out Spell spell)
		{
			spell = default(Spell);

			if (ChargesCurrent > 0)
			{
				spell = new HealSpell()
				{
					Item = this,
					SkillLevel = 10,
				
					Cost = 0,
				};
			}

			return (spell != default(Spell));
		}
		
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
				entity.Target = new InternalTarget(this);
				return true;
			}

			return false;
		}
		
		protected void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (ContainsSpell(out var spell) && spell is HealSpell cure)
			{
				cure.Warm(source);
				cure.CastAt(target);
			}
		}

		private class InternalTarget : MobileTarget
		{
			private BlueStaffHeal _staff;

			public InternalTarget(BlueStaffHeal staff) : base(flags: TargetFlags.Beneficial)
			{
				_staff = staff;
			}

			protected override void OnTarget(MobileEntity source, MobileEntity target)
			{
				base.OnTarget(source, target);

				if (target.IsAlive)
				{
					if (_staff != null && _staff.ChargesCurrent > 0)
					{
						_staff.OnTarget(source, target);
						_staff.ChargesCurrent--;
					}
				}
			}
		}
	}
}