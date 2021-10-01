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
	//TODO look over the whole damn thing. Make sure it has charges. Get new example of charged item? Check phantasm intensity, skill level, and especially TARGETING
	[WorldForge]
	public class RazThaumSpear : Staff, ITreasure, IEmpowered, ICharged
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000086;

		/// <inheritdoc />
		public override uint BasePrice => 1500;

		/// <inheritdoc />
		public override int Weight => 1400;

		/// <inheritdoc />
		public override int MinimumDamage => 4;

		/// <inheritdoc />
		public override int MaximumDamage => 14;

		/// <inheritdoc />
		public override int BaseArmorBonus => 5;

		/// <inheritdoc />
		public override int BaseAttackBonus => 6;

		//TODO check if MaxRange property on weapons works.
		/// <inheritdoc />
		public override int MaxRange => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Piercing |  WeaponFlags.Silver | WeaponFlags.BlueGlowing;

        /// <inheritdoc />
		public override bool CanBind => true;

		/// <inheritdoc />
		public override int ManaRegeneration => 1;

		//Check context on how to make the spell work properly: Does targetting work the same for summon phantasm?
		public RazThaumSpear() : this(5)
		{
			Hue = Color.Green;
		}

		public override bool CanUse(MobileEntity entity)
		{

			var flags = Flags;

			if (entity.LeftHand != null && flags.HasFlag(WeaponFlags.TwoHanded))
				return false; */


			if (!CanUse(entity.Alignment))
				return false;

			if (entity is PlayerEntity player && player.Profession is Thaumaturge)
				return true;

			return false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazThaumSpear"/> class.
		/// </summary>
		public RazThaumSpear(int charges)
		{
			_chargesCurrent = _chargesMax = charges;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazThaumSpear"/> class.
		/// </summary>
		public RazThaumSpear(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)1); /* version */

			writer.Write((short)_chargesCurrent);
			writer.Write((short)_chargesMax);
		}

		/// <inheritdoc />
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt16();

			switch (version)
			{
				case 1:
					{
						_chargesCurrent = reader.ReadInt16();
						_chargesMax = reader.ReadInt16();

						break;
					}
			}
		}

		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500030)); /* [You are looking at] [a silver spear. Its aura gives off a blue glow, divine in purpose.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500031)); /* The spear contains the power to summon aid.*/
		}

		//TODO: Add charges?
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

		public Type ContainedSpell => typeof(SummonPhantasm);

		/// <inheritdoc />
		public bool ContainsSpell(out Spell spell)
		{
			spell = default(Spell);

			if (ChargesCurrent > 0)
			{
				spell = new SummonPhantasm()
				{
					Item = this,
					Intensity = 10,
					SkillLevel = 19,

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


		//TODO figure out the warm, cast at commands for summon phantasm
		protected void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (ContainsSpell(out var spell) && spell is SummonPhantasm summon)
			{
				summon.Warm();
				summon.CastAt(source.Location);
			}
		}

		private class InternalTarget : Target
		{
			private RazThaumSpear _staff;

			public InternalTarget(RazThaumSpear staff) : base(3, TargetFlags.Mobiles | TargetFlags.Beneficial)
			{
				_staff = staff;
			}

			protected override void OnTarget(MobileEntity source, object target)
			{
				base.OnTarget(source, target);

				if (target is MobileEntity mobile && mobile.IsAlive)
				{
					if (_staff != null && _staff.ChargesCurrent > 0)
					{
						_staff.OnTarget(source, mobile);
						_staff.ChargesCurrent--;
					}
				}
			}
		}
	}
}