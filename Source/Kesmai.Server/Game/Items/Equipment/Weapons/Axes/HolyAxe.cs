using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class HolyAxe : Axe
	{
		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int Weight => 2200;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Slashing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="HolyAxe"/> class.
		/// </summary>
		public HolyAxe() : base(146)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200016)); /* [You are looking at] [a large battle axe.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250015)); /* The battle axe appears quite ordinary. */
		}

        public override double GetAttackBonus(MobileEntity attacker, MobileEntity defender)
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
             *     Though the Silver Greataxe can hit harder at times, the BBS has a harder hitting average than the
             *     Greataxe. The BBS has two advantages. First, it blocks well, and the BBS is "lawful" so it gains
             *     one extra damage add against evil crits like dragons and drakes."
             * 
             */
            if (defender != null)
            {
                if (Flags.HasFlag(WeaponFlags.Lawful) && defender.Alignment == Alignment.Evil)
                    attackBonus += 1;
                
                if (IsUndead(defender))
                {
                    attackBonus += 1;
                }
                
            }

            return attackBonus;
        }

        private bool IsUndead(MobileEntity mobileEntity)
        {
            var Type = mobileEntity.GetType();

            var IsUndead = typeof(IUndead).IsAssignableFrom(Type);

            return IsUndead;
        }
	}
}