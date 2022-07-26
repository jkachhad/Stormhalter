using System;
using Kesmai.Server.Game;
using System.Collections.Generic;

namespace Kesmai.Server.Items
{
    public class WeaponManager : Weapon, IWeapon
    {   
        private WeaponDTO _weaponDTO;
        public WeaponManager()
        {
            _weaponDTO = new WeaponDTO();
        }

       public WeaponDTO.BaseWeaponProperties WeaponSetup(WeaponDTO.WeaponTypes weaponType, WeaponDTO.WeaponLevels weaponLevel)
        {
            var weaponSetupAgain = new Dictionary<string, WeaponDTO.BaseWeaponProperties>();

            switch (weaponType)
            {
                case WeaponDTO.WeaponTypes.Axe:
                    weaponSetupAgain = _weaponDTO.SetupAxeProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Crossbow:
                    weaponSetupAgain = _weaponDTO.SetupCrossbowProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Dagger:
                    weaponSetupAgain = _weaponDTO.SetupDaggerProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Flail:
                    weaponSetupAgain = _weaponDTO.SetupFlailProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Greataxe:
                    weaponSetupAgain = _weaponDTO.SetupGreatAxeProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Greatsword:
                    weaponSetupAgain = _weaponDTO.SetupGreatSwordProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Halberd:
                    weaponSetupAgain = _weaponDTO.SetupHalberdProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Longbow:
                    weaponSetupAgain = _weaponDTO.SetupLongbowProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Longsword:
                    weaponSetupAgain = _weaponDTO.SetupLongswordProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Pike:
                    weaponSetupAgain = _weaponDTO.SetupPikeProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Rapier:
                    weaponSetupAgain = _weaponDTO.SetupRapierProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Shortsword:
                    weaponSetupAgain = _weaponDTO.SetupShortswordProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Spear:
                    weaponSetupAgain = _weaponDTO.SetupSpearProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Staff:
                    weaponSetupAgain = _weaponDTO.SetupStaffProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.ThrowingHammer:
                    weaponSetupAgain = _weaponDTO.SetupThrowingHammerProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.TwoHandFlail:
                    weaponSetupAgain = _weaponDTO.SetupTwoHandFlailProperties(weaponType, weaponLevel);
                    break;
                case WeaponDTO.WeaponTypes.Warhammer:
                    weaponSetupAgain = _weaponDTO.SetupWarhammerProperties(weaponType, weaponLevel);
                    break;
            }


            return weaponSetupAgain[$"{weaponType.ToString()} {(int)weaponLevel}"];
        }
    }
}

