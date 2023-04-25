public class MorningStar : Flail, ITreasure
    {
        /// <inheritdoc />
        public override int LabelNumber => 6000037;

        /// <inheritdoc />
        public override uint BasePrice => 10000;

        /// <inheritdoc />
        public override uint Category => 2;

        /// <inheritdoc />
        public override int Weight => 1800;

        /// <inherit />
        public override Skill Skill => Skill.Flail;

        /// <inheritdoc />
        public override int MinimumDamage => _weaponStats.MinimumDamage;

        /// <inheritdoc />
        public override int MaximumDamage => _weaponStats.MaximumDamage;

        /// <inheritdoc />
        public override int BaseArmorBonus => _weaponStats.BaseArmorBonus;

        /// <inheritdoc />
        public override int BaseAttackBonus => _weaponStats.BaseAttackBonus;
        
        /// <inheritdoc />
        public override bool CanDisintegrate => false;
        
        /// <inheritdoc />
        public override bool CanBind => true;

        /// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Bashing | WeaponFlags.BlueGlowing;

        private int _weaponLevel;
        private FlailWeapon _weaponStats;
        /// <summary>
        /// Initializes a new instance of the <see cref="MorningStar"/> class.
        /// </summary>
        public MorningStar(int weaponLevel) : base(944)
        {
           _weaponLevel = weaponLevel;
           _weaponStats = GetWeaponStats(weaponLevel);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MorningStar"/> class.
        /// </summary>
        public MorningStar(Serial serial) : base(serial)
        {
            
        }

        /// <inheritdoc />
        public override void GetDescription(List<LocalizationEntry> entries)
        {
            entries.Add(new LocalizationEntry(6200000, 6200357)); /* [You are looking at] [a light flail with dozens of serrated spikes, seemingly ready to draw blood.] */
        }

        /// <inheritdoc />
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write((short)1); /* version */

            writer.Write((int)_weaponLevel); /* weapon level */
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
                        _weaponLevel = reader.ReadInt32();
                        break;
                    }
            }
        }

        public override TimeSpan GetSwingDelay(MobileEntity entity)
        {
            return entity.GetRoundDelay(_weaponStats.WeaponSpeed);
        }

        public FlailWeapon GetWeaponStats()
        {
            FlailWeapon flail = new FlailWeapon();

            switch (_weaponLevel)
            {
                case 1:
                    flail = SetWeaponStats(2, 8, 1, 0, 1.05);
                    break;
                case 2:
                    flail = SetWeaponStats(2, 9, 2, 0, 1.1);
                    break;
                case 3:
                    flail = SetWeaponStats(3, 10, 3, 1, 1.15);
                    break;
                case 4:
                    flail = SetWeaponStats(3, 11, 4, 1, 1.2);
                    break;
                case 5:
                    flail = SetWeaponStats(4, 12, 5, 2, 1.25);
                    break;
                case 6:
                    flail = SetWeaponStats(4, 13, 6, 2, 1.30);
                    break;
                case 7:
                    flail = SetWeaponStats(5, 14, 7, 3, 1.35);
                    break;
                default:
                    flail = SetWeaponStats(2, 8, 1, 0, 1.05);
                    break;
            }

            return flail;
        }

        public FlailWeapon SetWeaponStats(int minimumDamage, int maximumDamage, int baseAttackBonus, int baseDefenseBonus, double weaponSpeed)
        {
            return new FlailWeapon()
            {
                MinimumDamage = minimumDamage,
                MaximumDamage = maximumDamage,
                BaseAttackBonus = baseAttackBonus,
                BaseArmorBonus = baseDefenseBonus,
                WeaponSpeed = weaponSpeed
            };
        }

        public class FlailWeapon
        {
            public int MinimumDamage { get; set; }
            public int MaximumDamage { get; set; }
            public int BaseAttackBonus { get; set; }
            public int BaseArmorBonus { get; set; }
            public double WeaponSpeed { get; set; }
        }
    }