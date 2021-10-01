using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class RazRHammer : Mace, IReturningWeapon, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 2560;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 4;

		/// <inheritdoc />
		public override int MaximumDamage => 14;

		/// <inheritdoc />
		public override int BaseArmorBonus => 4;

		/// <inheritdoc />
		public override int BaseAttackBonus => 7;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.BlueGlowing | WeaponFlags.Returning | WeaponFlags.Throwable
											 | WeaponFlags.Slashing | WeaponFlags.Lawful;

		/// <inheritdoc />
		public override bool CanBind => true;

		public override void OnHit(MobileEntity attacker, MobileEntity defender)
		{
			if (Utility.RandomDouble() <= 0.25)
			{
				var spell = new ConcussionSpell()
				{
					SkillLevel = 12,
					Cost = 0,
				};
				spell.Warm(attacker);
				spell.CastAt(defender);
			}
			base.OnHit(attacker, defender);
		}

		public override bool CanUse(MobileEntity entity)
		{

			var flags = Flags;

			if (entity.LeftHand != null && flags.HasFlag(WeaponFlags.TwoHanded))
				return false; */


			if (!CanUse(entity.Alignment))
				return false;

			if (entity is PlayerEntity player && player.Profession is Knight)
				return true;

			return false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazRHammer"/> class.
		/// </summary>
		public RazRHammer() : base(75)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500028)); /* [You are looking at] [a hammer that rumbles like a storm cloud.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500032)); /* The combat adds for this weapon are +7.*/
		}
	}
}