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
        
        /* Stormhalter charts https://docs.google.com/spreadsheets/d/16CxfCJLUUMPGuWnDws4UgOnKPOrr-2vzlaG_5F5y0uc/edit#gid=0 */
        #region 
        public Dictionary<string, BaseWeaponProperties> SetupAxeProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupCrossbowProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Crossbow 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Crossbow,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Crossbow,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Crossbow,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 4,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Crossbow,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 5,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Crossbow,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 6,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Crossbow,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 8,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Crossbow 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Crossbow,
                WeaponLevel = 7,
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

        public Dictionary<string, BaseWeaponProperties> SetupDaggerProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Dagger 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Dagger,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 4,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Dagger 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Dagger,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 5,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Dagger 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Dagger,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 6,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Dagger 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Dagger,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 7,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Dagger 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Dagger,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Dagger 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Dagger,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Dagger 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Dagger,
                WeaponLevel = 7,
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
        
        public Dictionary<string, BaseWeaponProperties> SetupFlailProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Flail 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Flail,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Flail 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Flail,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Flail 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Flail,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Flail 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Flail,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Flail 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Flail,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Flail 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Flail,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Flail 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Flail,
                WeaponLevel = 7,
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
        
        public Dictionary<string, BaseWeaponProperties> SetupGreatAxeProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Greataxe 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Greataxe,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greataxe 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greataxe,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greataxe 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greataxe,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greataxe 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greataxe,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greataxe 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greataxe,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greataxe 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greataxe,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greataxe 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greataxe,
                WeaponLevel = 7,
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
        
        public Dictionary<string, BaseWeaponProperties> SetupGreatSwordProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Greatsword 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Greatsword,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greatsword 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greatsword,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Greatsword 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greatsword,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greatsword 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greatsword,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 13,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Greatsword 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greatsword,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 14,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greatsword 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greatsword,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 15,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Greatsword 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Greatsword,
                WeaponLevel = 7,
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
        
        public Dictionary<string, BaseWeaponProperties> SetupHalberdProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Halberd 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Halberd,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Halberd 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Halberd,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Halberd 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Halberd,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 3,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Halberd 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Halberd,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 1
             } );

             weaponProps.Add("Halberd 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Halberd,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 4,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Axe 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Halberd,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 4,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 2
             } );

             weaponProps.Add("Axe 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Halberd,
                WeaponLevel = 7,
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

        public Dictionary<string, BaseWeaponProperties> SetupLongbowProperties(WeaponTypes weaponType, int weaponLevel)
        {
             Dictionary<string, BaseWeaponProperties> weaponProps = new Dictionary<string, BaseWeaponProperties>();
             
             weaponProps.Add("Longbow 1", new BaseWeaponProperties(){
                _WeaponTypes = WeaponTypes.Longbow,
                WeaponLevel = 1,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 8,
                BaseAttackBonus = 1,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 2", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Longbow,
                WeaponLevel = 2,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 1,
                MaximumDamage = 9,
                BaseAttackBonus = 2,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 3", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Longbow,
                WeaponLevel = 3,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.VeryLight,
                MinimumDamage = 2,
                MaximumDamage = 10,
                BaseAttackBonus = 3,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 4", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Longbow,
                WeaponLevel = 4,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 2,
                MaximumDamage = 11,
                BaseAttackBonus = 4,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 5", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Longbow,
                WeaponLevel = 5,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Light,
                MinimumDamage = 3,
                MaximumDamage = 12,
                BaseAttackBonus = 5,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 6", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Longbow,
                WeaponLevel = 6,
                BasePrice = 2400,
                Weight = 1000,
                Penetration = ShieldPenetration.Medium,
                MinimumDamage = 3,
                MaximumDamage = 13,
                BaseAttackBonus = 6,
                BaseArmorBonus = 0
             } );

             weaponProps.Add("Longbow 7", new BaseWeaponProperties(){
                weaponTypes = WeaponTypes.Longbow,
                WeaponLevel = 7,
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

        public Dictionary<string, BaseWeaponProperties> SetupLongswordProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupPikeProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupRapierProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupShortswordProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupSpearProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupStaffProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupThrowingHammerProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupTwoHandFlailProperties(WeaponTypes weaponType, int weaponLevel)
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

        public Dictionary<string, BaseWeaponProperties> SetupWarhammerProperties(WeaponTypes weaponType, int weaponLevel)
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
        
    }