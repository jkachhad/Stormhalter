using System.IO;

using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
    public class CreatureEntityBossProps
    {
        private CreatureEntity _creatureEntity;

        public CreatureEntityBossProps(CreatureEntity creatureEntity)
        {
            _creatureEntity = creatureEntity;
        }

        /* This needs a look from Lux. 
                I don't know if the implementation is right due to access of source code is limited. 
                Overall this is to clean up the Bosses and to establish Default Props using a c# file. 

                To what extent this could be used would be determined by the person implementing the bosses. 
            */
        public CreatureEntity SetBossProperties(BossType bossType, CreatureImmunity immunities = null, CreatureWeakness weakness = null)
        {
            if (immunities != null)
            {
                _creatureEntity.CreatureImmunity = immunities;
            }

            if (weakness != null)
            {
                _creatureEntity.CreatureWeakness = weakness;
            }

            switch(bossType)
            {
                case BossType.Minor:
                    _creatureEntity.CreatureImmunity = setMinorBossProps().Immunity;
                    _creatureEntity.CreatureWeaknesss = setMinorBossProps().Weakness;
                case BossType.Major:
                    _creatureEntity.CreatureImmunity = setMajorBossProps().Immunity;
                    _creatureEntity.CreatureWeaknesss = setMajorBossProps().Weakness;
                case BossType.Notable:
                    _creatureEntity.CreatureImmunity = setNotableBossProps().Immunity;
                    _creatureEntity.CreatureWeaknesss = setNotableBossProps().Weakness;
            }

            return _creatureEntity;
        }

        private BossProperties setMajorBossProps()
        {
            var majorBossProps = new BossProperties(){
                // Modeled off of Axe Drake
                Immunity = CreatureImmunity.Piercing | CreatureImmunity.Slashing | CreatureImmunity.Bashing |
                   CreatureImmunity.Projectile | CreatureImmunity.Magic | CreatureImmunity.Poison,
                Weakness = CreatureWeakness.Silver | CreatureWeakness.BlueGlowing | CreatureWeakness.DeathSpell | CreatureWeakness.IceSpearSpell
            };

            return majorBossProps;
        }

        private BossProperties setMinorBossProps()
        {
            var minorBossProps = new BossProperties(){

              // To be implemented. 

            };
            return minorBossProps;
        }

        private BossProperties setNotableBossProps()
        {
            var notableBossProps = new BossProperties()
            {
                // To be implemented. 
            };
        }

        internal class BossProperties 
        {
            public CreatureImmunity Immunity { get; set; }
            public CreatureWeakness Weakness {get;set;}
            
        }

        public enum BossType
        {
            Minor,
            Major,
            Notable
        }
    }
}