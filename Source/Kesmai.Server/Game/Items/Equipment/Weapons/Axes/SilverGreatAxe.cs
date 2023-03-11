using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SilverGreatAxe : Axe, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 5760;

		/// <inheritdoc />
		public override int LabelNumber => 6000044;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 4;

		/// <inheritdoc />
		public override int MaximumDamage => 14;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int BaseAttackBonus => 4;

		/// <inheritdoc />
		public override Skill Skill => Skill.Greatsword;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Silver | WeaponFlags.Slashing 
		                                     | WeaponFlags.Lawful | WeaponFlags.BlueGlowing;

        /// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="SilverGreatAxe"/> class.
		/// </summary>
		public SilverGreatAxe() : base(125)
		{
		}

		/// <summary>
		/// Calculates the fumble chance as a percent.
		/// </summary>
		public override double CalculateFumbleChance(MobileEntity entity)
		{
			// Skill Level = 3 =>  ((3 + 1)^2) * 10 = 160 => 1 / 160;
			// Skill Level = 4 =>  ((4 + 1)^2) * 10 = 250 => 1 / 250;
			var fumblePercentage = 1 / (10 * Math.Pow(entity.GetSkillLevel(Skill) + 1, 2));

			if(entity is PlayerEntity player)
			{
				if(player.LeftHand != null)
				{
					fumblePercentage = 1.0;
				}
			}
		
			return fumblePercentage;
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200145)); /* [You are looking at] [a huge battle axe with shining silver blades. The axe is emitting a faint blue glow. The weapon is lawful.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
		}
	}
}