using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BlackBroadsword : Sword, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000019;

		/// <inheritdoc />
		/// <remarks>
		/// The individual components give 1700. If the item were to be broken by a player,
		/// it would award better experience.
		/// </remarks>
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int Weight => 4160;
		
		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 2;

		/// <inheritdoc />
		public override int MaximumDamage => 12;

		/// <inheritdoc />
		public override int BaseArmorBonus => 3;

		/// <inheritdoc />
		public override int BaseAttackBonus => 4;

		/// <inheritdoc />
		public override Skill Skill => Skill.Greatsword;

		/// <inheritdoc />
		public override WeaponFlags Flags
		{
			get
			{
				var parent = Parent;
				var flags = WeaponFlags.TwoHanded | WeaponFlags.BlueGlowing | WeaponFlags.Slashing | WeaponFlags.Lawful;

				/* Remove 2H requirement */
				if (parent is PlayerEntity player && player.Stats[EntityStat.StrengthAdds].Value > 5 && player.RightHand == this)
					flags &= ~WeaponFlags.TwoHanded;

				return flags;
			}
		}

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlackBroadsword"/> class.
		/// </summary>
		public BlackBroadsword() : base(195)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200004)); /* [You are looking at] [a black broadsword. The sword is emitting a faint blue glow. The weapon is lawful.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
		}
	}
}