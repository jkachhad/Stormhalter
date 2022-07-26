using System;
using Kesmai.Server.Game;
using System.Collections.Generic;

namespace Kesmai.Server.Items
{
   public class WeaponDTO 
    {
        public class BaseWeaponProperties
        {
            public WeaponTypes WeaponType {get;set;} = WeaponTypes.Axe; 
            public int weaponLevel {get;set;} = 1;
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
            public override int BaseAttackBonus {get;set;} = 0; 

            /// <inheritdoc />
            public override int BaseArmorBonus {get;set;} = 0; 
            
            /// <inheritdoc />
            public override WeaponFlags Flags {get;set;} = WeaponFlags.Piercing;
        }
        
        /* Stormhalter charts https://docs.google.com/spreadsheets/d/16CxfCJLUUMPGuWnDws4UgOnKPOrr-2vzlaG_5F5y0uc/edit#gid=0 */
        #region 
        public Dictionary<string, BaseWeaponProperties> SetupAxeProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Axe 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Axe 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Axe 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Axe 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Axe 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Axe 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Axe 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
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

        public Dictionary<string, BaseWeaponProperties> SetupCrossbowProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Crossbow 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 4,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 5,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 5", new BaseWeaponProperties(){
               WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 6,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 8,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Heavy,
                MinimumDamage = 5,
                MaximumDamage = 14,
                BaseAttackBonus = 9,
                BaseArmorBonus = 0
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupDaggerProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Dagger 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 4,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Dagger 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 5,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Dagger 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 6,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Dagger 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 7,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Dagger 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Dagger 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Dagger 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 7,
                BaseArmorBonus = 3
             } );

             return weaponProps;
        }
        
        public Dictionary<string, BaseWeaponProperties> SetupFlailProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Flail 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Flail 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Flail 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Flail 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Flail 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Flail 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Flail 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 5,
                MaximumDamage = 14,
                BaseAttackBonus = 7,
                BaseArmorBonus = 3
             } );

             return weaponProps;
        }
        
        public Dictionary<string, BaseWeaponProperties> SetupGreatAxeProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Greataxe 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greataxe 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greataxe 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greataxe 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greataxe 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greataxe 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greataxe 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 5,
                MaximumDamage = 14,
                BaseAttackBonus = 7,
                BaseArmorBonus = 3
             } );

             return weaponProps;
        }
        
        public Dictionary<string, BaseWeaponProperties> SetupGreatSwordProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Greatsword 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greatsword 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greatsword 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greatsword 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 13,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greatsword 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 14,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greatsword 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 15,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greatsword 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 5,
                MaximumDamage = 16,
                BaseAttackBonus = 7,
                BaseArmorBonus = 3
             } );

             return weaponProps;
        }
        
        public Dictionary<string, BaseWeaponProperties> SetupHalberdProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Halberd 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Halberd 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Halberd 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Halberd 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Halberd 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Halberd 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Halberd 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 5,
                MaximumDamage = 14,
                BaseAttackBonus = 7,
                BaseArmorBonus = 3
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupLongbowProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Longbow 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 3,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 14,
                BaseAttackBonus = 7,
                BaseArmorBonus = 0
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupLongswordProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Longsword 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Longsword 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Longsword 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 10,
                BaseAttackBonus = 2,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Longsword 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 3,
                BaseArmorBonus = 3
             } );

             weaponProps.Add("Longsword 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 12,
                BaseAttackBonus = 4,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Longsword 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 13,
                BaseAttackBonus = 4,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Longsword 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 3,
                MaximumDamage = 14,
                BaseAttackBonus = 5,
                BaseArmorBonus = 5
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupPikeProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Pike 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Pike 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Pike 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 2,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Pike 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Pike 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 4,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Pike 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 4,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Pike 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Heavy,
                MinimumDamage = 5,
                MaximumDamage = 14,
                BaseAttackBonus = 5,
                BaseArmorBonus = 3
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupRapierProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Rapier 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 6,
                BaseAttackBonus = 1,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Rapier 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 7,
                BaseAttackBonus = 2,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Rapier 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 3,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Rapier 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 4,
                BaseArmorBonus = 3
             } );

             weaponProps.Add("Rapier 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 5,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Rapier 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 6,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Rapier 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 7,
                BaseArmorBonus = 5
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupShortswordProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Shortsword 1", new BaseWeaponProperties(){
                WeaponTypes = WeaponTypes.Shortsword,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 6,
                BaseAttackBonus = 0,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Shortsword 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 7,
                BaseAttackBonus = 0,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Shortsword 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 1,
                MaximumDamage = 7,
                BaseAttackBonus = 1,
                BaseArmorBonus = 3
             } );

             weaponProps.Add("Shortsword 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Shortsword 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 2,
                BaseArmorBonus = 5
             } );

             weaponProps.Add("Shortsword 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 6
             } );

             weaponProps.Add("Shortsword 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Heavy,
                MinimumDamage = 3,
                MaximumDamage = 9,
                BaseAttackBonus = 3,
                BaseArmorBonus = 7
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupSpearProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Spear 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Spear 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 1,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Spear 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 2,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Spear 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 3,
                BaseArmorBonus = 3
             } );

             weaponProps.Add("Spear 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 4,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Spear 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 3,
                MaximumDamage = 13,
                BaseAttackBonus = 4,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Spear 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 14,
                BaseAttackBonus = 5,
                BaseArmorBonus = 5
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupStaffProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Staff 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 4,
                BaseAttackBonus = 0,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Staff 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 5,
                BaseAttackBonus = 0,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Staff 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 6,
                BaseAttackBonus = 1,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("Staff 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 7,
                BaseAttackBonus = 1,
                BaseArmorBonus = 5
             } );

             weaponProps.Add("Staff 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 2,
                BaseArmorBonus = 6
             } );

             weaponProps.Add("Staff 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 8
             } );

             weaponProps.Add("Staff 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 9
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupThrowingHammerProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("ThrowingHammer 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("ThrowingHammer 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 1,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("ThrowingHammer 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 10,
                BaseAttackBonus = 2,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("ThrowingHammer 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 3,
                BaseArmorBonus = 3
             } );

             weaponProps.Add("ThrowingHammer 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 12,
                BaseAttackBonus = 4,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("ThrowingHammer 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 13,
                BaseAttackBonus = 4,
                BaseArmorBonus = 4
             } );

             weaponProps.Add("ThrowingHammer 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 14,
                BaseAttackBonus = 5,
                BaseArmorBonus = 5
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupTwoHandFlailProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("TwoHandFlail 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("TwoHandFlail 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 5,
                MaximumDamage = 13,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("TwoHandFlail 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 6,
                MaximumDamage = 14,
                BaseAttackBonus = 4,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("TwoHandFlail 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 7,
                MaximumDamage = 15,
                BaseAttackBonus = 5,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("TwoHandFlail 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 8,
                MaximumDamage = 16,
                BaseAttackBonus = 6,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("TwoHandFlail 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 9,
                MaximumDamage = 17,
                BaseAttackBonus = 8,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("TwoHandFlail 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Heavy,
                MinimumDamage = 10,
                MaximumDamage = 18,
                BaseAttackBonus = 9,
                BaseArmorBonus = 0
             } );

             return weaponProps;
        }

        public Dictionary<string, BaseWeaponProperties> SetupWarhammerProperties(WeaponTypes weaponType, WeaponLevels weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Warhammer 1", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Warhammer 2", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Warhammer 3", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Warhammer 4", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Warhammer 5", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Warhammer 6", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Warhammer 7", new BaseWeaponProperties(){
                WeaponType = weaponType,
                WeaponLevel = (int)weaponLevel,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 5,
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
            Greatsword,
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

        public enum WeaponLevels
        {
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven
            
        }
        
    }
}
    
