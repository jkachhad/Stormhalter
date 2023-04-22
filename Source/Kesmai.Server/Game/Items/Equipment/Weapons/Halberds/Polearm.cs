public class Polearm : Halberd, ITreasure
    {
        /// <inheritdoc />
        public override int LabelNumber => 6000037;

        /// <inheritdoc />
        public override uint BasePrice => 10000;

        /// <inheritdoc />
        public override int Weight => 1800;

        /// <inherit />
        public override Skill Skill => Skill.Halberd;

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
		public override WeaponFlags Flags => WeaponFlags.Bashing;

        private int _weaponLevel;
        private poleArmWeapon _weaponStats;
        /// <summary>
        /// Initializes a new instance of the <see cref="Polearm"/> class.
        /// </summary>
        public Polearm(int weaponLevel) : base(944)
        {
           _weaponLevel = weaponLevel;
           _weaponStats = GetWeaponStats(weaponLevel);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Polearm"/> class.
        /// </summary>
        public Polearm(Serial serial) : base(serial)
        {
            
        }

        /// <inheritdoc />
        public override void GetDescription(List<LocalizationEntry> entries)
        {
            entries.Add(new LocalizationEntry(6200000, 6200357)); /* [You are looking at] [a light poleArm with dozens of serrated spikes, seemingly ready to draw blood.] */
        }

        /// <inheritdoc />
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);

            writer.Write((short)1); /* version */
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
                        break;
                    }
            }
        }

        public override TimeSpan GetSwingDelay(MobileEntity entity)
        {
            return entity.GetRoundDelay(_weaponStats.WeaponSpeed);
        }

        public PoleArmWeapon GetWeaponStats()
        {
            PoleArmWeapon poleArm = new PoleArmWeapon();

            switch (_weaponLevel)
            {
                case 1:
                    poleArm = SetWeaponStats(2, 8, 1, 0, 1.00);
                    break;
                case 2:
                    poleArm = SetWeaponStats(2, 9, 2, 0, 1.00);
                    break;
                case 3:
                    poleArm = SetWeaponStats(3, 10, 3, 1, 1.00);
                    break;
                case 4:
                    poleArm = SetWeaponStats(3, 11, 4, 1, 1.00);
                    break;
                case 5:
                    poleArm = SetWeaponStats(4, 12, 5, 2, 1.00);
                    break;
                case 6:
                    poleArm = SetWeaponStats(4, 13, 6, 2, 1.00);
                    break;
                case 7:
                    poleArm = SetWeaponStats(5, 14, 7, 3, 1.00);
                    break;
                default:
                    poleArm = SetWeaponStats(2, 8, 1, 0, 1.00);
                    break;
            }

            return poleArm;
        }

        public PoleArmWeapon SetWeaponStats(int minimumDamage, int maximumDamage, int baseAttackBonus, int baseDefenseBonus, double weaponSpeed)
        {
            return new PoleArmWeapon()
            {
                MinimumDamage = minimumDamage,
                MaximumDamage = maximumDamage,
                BaseAttackBonus = baseAttackBonus,
                BaseArmorBonus = baseDefenseBonus,
                WeaponSpeed = weaponSpeed
            };
        }

        public class PoleArmWeapon
        {
            public int MinimumDamage { get; set; }
            public int MaximumDamage { get; set; }
            public int BaseAttackBonus { get; set; }
            public int BaseArmorBonus { get; set; }
            public double WeaponSpeed { get; set; }
        }
       
    }