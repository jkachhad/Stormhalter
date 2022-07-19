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

        public WeaponDTO.BaseWeaponProperties WeaponSetup(WeaponDTO.WeaponTypes weaponTypes, int weaponLevel)
        {
            var setupWeaponData = _weaponDTO.SetupAxeWeaponDTOProperties();

            return new WeaponDTO.BaseWeaponProperties() {setupWeaponData[$"{weaponTypes.ToString()} + {weaponLevel}"]};
        }
    }
}

