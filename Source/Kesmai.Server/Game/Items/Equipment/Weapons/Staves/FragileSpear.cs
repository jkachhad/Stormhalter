using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
    public partial class FragileSpear : Spear, ITreasure, IFragile
    {
        private int _durabilityCurrent;
        private int _durabilityMax;

        // not sure how to implement variable properties, commenting out the possible alternative
        private int _minimumDamage;
        private int _maximumDamage;
        // private int _baseArmorBonus;
        // private int _baseAttackBonus;

        [WorldForge]
        [CommandProperty(AccessLevel.GameMaster)]
        public int DurabilityCurrent
        {
            get => _durabilityCurrent;
            set => _durabilityCurrent = value.Clamp(0, _durabilityMax);
        }
		
        [WorldForge]
        [CommandProperty(AccessLevel.GameMaster)]
        public int DurabilityMax
        {
            get => _durabilityMax;
            set => _durabilityMax = value;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FragileSpear"/> class.
        /// </summary>
        protected FragileSpear(int durability = 1) : base(124)
        {
            _durabilityCurrent = durability;
            _durabilityMax = durability;
            _minimumDamage = Utility.RandomBetween(1, 5);
            _maximumDamage = Utility.RandomBetween(6, 15);
            // _baseArmorBonus = Utility.RandomBetween(1, 5);
            // _baseAttackBonus = Utility.RandomBetween(1, 5);
        }
        /// <inheritdoc />
        public override int MinimumDamage => _minimumDamage;
        
        /// <inheritdoc />
        public override int MaximumDamage => _maximumDamage;

        /// <inheritdoc />
        public override int BaseArmorBonus => Utility.RandomBetween(1, 5);

        /// <inheritdoc />
        public override int BaseAttackBonus => Utility.RandomBetween(1, 5);
        
        // /// <inheritdoc />
        // public override int BaseArmorBonus => _baseArmorBonus;
        //
        // /// <inheritdoc />
        // public override int BaseAttackBonus => _baseAttackBonus;

        /// <inheritdoc />
        public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Piercing | WeaponFlags.Bashing | WeaponFlags.Silver;

        public override void OnStrip(Corpse corpse)
        {
            /* Only reduce durability if the item was stripped when on paperdoll or rings. (Should this be changed to held?) */
            if (Container is EquipmentContainer)
            {
                if (_durabilityCurrent > 0)
                    _durabilityCurrent--;

                if (_durabilityCurrent == 0)
                    Delete();
            }
        }
    }
}