using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilityLibraries;

namespace Kesmai.Server.Items
{
    [TestClass]
    public class WeaponManagerTests
    {
        private WeaponManager _weaponManager;

        [Setup]
        public void Setup()
        {
            _weaponManager = new WeaponManager();
        }

        [TestMethod]
        public void VerifyWeaponsByWeaponType()
        {
            var axeValues = _weaponManager.WeaponSetup(WeaponDTO.WeaponTypes.Axe, 1);

        }
    }
}