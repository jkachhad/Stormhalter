using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public partial class BlueStaffRaiseDead : BlueStaff, IEmpowered, ICharged
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BlueStaffRaiseDead"/> class.
		/// </summary>
		public BlueStaffRaiseDead() : this(5)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BlueStaffRaiseDead"/> class.
		/// </summary>
		public BlueStaffRaiseDead(int charges)
		{
			_chargesCurrent = _chargesMax = charges;
		}

		public override void GetDescription(List<LocalizationEntry> entries)
		{
			base.GetDescription(entries);
			
			if (Identified)
				entries.Add(new LocalizationEntry(6250082)); /* The staff contains the spell of Raise Dead. */
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

		public Type ContainedSpell => typeof(RaiseDeadSpell);
		
		/// <inheritdoc />
		public bool ContainsSpell(out Spell spell)
		{
			spell = default(Spell);

			if (ChargesCurrent > 0)
			{
				spell = new RaiseDeadSpell()
				{
					Item = this,
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
		
		protected void OnTarget(MobileEntity source, Corpse corpse)
		{
			if (ContainsSpell(out var spell) && spell is RaiseDeadSpell raiseDead)
			{
				raiseDead.Warm(source);
				raiseDead.CastAt(corpse);
			}
		}

		private class InternalTarget : Target
		{
			private BlueStaffRaiseDead _staff;

			public InternalTarget(BlueStaffRaiseDead staff) : base(3, TargetFlags.Items)
			{
				_staff = staff;
			}

			protected override void OnTarget(MobileEntity source, object target)
			{
				base.OnTarget(source, target);

				if (target is Corpse corpse)
				{
					if (_staff != null && _staff.ChargesCurrent > 0)
					{
						_staff.OnTarget(source, corpse);
						_staff.ChargesCurrent--;
					}
				}
			}
		}
	}
}