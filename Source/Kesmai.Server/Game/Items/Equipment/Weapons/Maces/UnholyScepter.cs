using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
    public partial class UnholyScepter : Mace
    {

        /// <inheritdoc />
        public override uint BasePrice => 10000;

        /// <inheritdoc />
        public override int Weight => 2000;

        /// <inheritdoc />
        public override int Category => 2;

        /// <inheritdoc />
        public override int MinimumDamage => 2;

        /// <inheritdoc />
        public override int MaximumDamage => 10;

        /// <inheritdoc />
        public override ShieldPenetration Penetration => ShieldPenetration.Light;

        /// <inheritdoc />
        public override int BaseArmorBonus => 3;

        /// <inheritdoc />
        public override int BaseAttackBonus => 3;

        /// <inheritdoc />
        public override int ManaRegeneration => 1;

        /// <inheritdoc />
        public override Skill Skill => Skill.Mace;

        /// <inheritdoc />
        public override WeaponFlags Flags => WeaponFlags.Bashing | WeaponFlags.BlueGlowing | WeaponFlags.Lawful;

        /// <inheritdoc />
		public override bool CanBind => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnholyScepter"/> class.
        /// </summary>
        public UnholyScepter() : base(965)
        {
        }

        /// <inheritdoc />
		public override bool CanEquip(MobileEntity entity)
        {
            if (entity is PlayerEntity player)
                return player.Profession == Profession.Thaumaturge || player.Profession == Profession.Wizard;

            return false;
        }

        /// <inheritdoc />
        public override void GetDescription(List<LocalizationEntry> entries)
        {
            entries.Add(new LocalizationEntry(6200000, 6200382)); /* [You are looking at] [an unholy scepter that has the markings of a king. The weapon emanates power. The weapon is lawful.] */

            if (Identified)
                entries.Add(new LocalizationEntry(6250134)); /* The scepter seems to have some magical properties. */
        }
    }


}
