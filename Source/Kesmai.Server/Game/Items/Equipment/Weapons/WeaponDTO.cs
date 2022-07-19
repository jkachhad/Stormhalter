using System;
using Kesmai.Server.Game;
using System.Collections.Generic;

    public class WeaponDTO 
    {
        public class BaseWeaponProperties
        {
            public WeaponTypes _WeaponTypes {get;set;} = WeaponTypes.Axe; 
            public int WeaponLevel {get;set;} = 1;
            public override uint BasePrice {get;set;} = 20;

            /// <inheritdoc />
            public override int Weight {get;set;} = 1200;

            /// <inheritdoc />
            public override ShieldPenetration Penetration {get;set;} = ShieldPenetration.Medium;

            /// <inheritdoc />
            public override int MinimumDamage {get;set;} = 1;

            /// <inheritdoc />
            public override int MaximumDamage {get;set;} = 6;

            /// <inheritdoc />
            public override int BaseAttackBonus => 0; 

            /// <inheritdoc />
            public override int BaseArmorBonus => 0; 
            
            /// <inheritdoc />
            public override WeaponFlags Flags {get;set;} = WeaponFlags.Piercing;
        }
        
        #region 
        /* Stormhalter charts https://docs.google.com/spreadsheets/d/16CxfCJLUUMPGuWnDws4UgOnKPOrr-2vzlaG_5F5y0uc/edit#gid=0 */
        public Dictionary<string, BaseWeaponProperties> SetupAxeWeaponDTOProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Axe 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Axe,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Axe 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Axe,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Axe 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Axe,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Axe 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Axe,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Axe 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Axe,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Axe 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Axe,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Axe 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Axe,
                WeaponLevel = 7,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 14,
                BaseAttackBonus = 7,
                BaseArmorBonus = 3
             } );

             return weaponProps;
        }
        #endregion
        public enum WeaponTypes
        {
            Axe,
            Crossbow,
            Dagger,
            Flail,
            Greataxe,
            Halberd,
            Longbow,
            Longsword,
            Pike,
            Rapier,
            Shortsword,
            Spear,
            Staff,
            ThrowingHammer,
            TwoHandFlail,
            Warhammer
        }
        
    }