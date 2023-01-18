using System;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
    public partial class CreatureEntity : MobileEntity
    {
         private readonly int minHP = 150;

        private const double rate = .075;

        private const double blockRate = .9;

        private const double attackRate = .4;

        private const double resistRate = 1.6;

        private const int minAttackBlock = 10;

        /* Set All Summon Stats dynamically based on Player Level. Magic skill seems a bit too strong to scale off of by itself
         * And technically not exposed yet. Can come back and redo if Magic/Player level are exposed */
        
        public CommonSummonStats SetSummonStats(MobileEntity playerEntity)
        {
            if (playerEntity == null)
            {
                return new CommonSummonStats();
            }

            if (playerEntity is PlayerEntity player)
            {
                var attackBlock = SetBlockAndAttack(playerEntity.Level);
                var health = SetHealth(playerEntity.Level);
                var resist = SetMagicalResistance(playerEntity.Level);

                return new CommonSummonStats()
                {
                    Health = health,
                    Block = attackBlock.Item1,
                    Attack = attackBlock.Item2,
                    MagicResistance = resist
                };
            }
        }

       /* Health Scales with Player Health. */
       private int? SetHealth(int playerLevel)
        {
            var hpFactor = playerLevel * healthRate;

            var nonRoundedHP = minHP * hpFactor;

            return (int)Math.Round(nonRoundedHP) + minHP;
        }

        /* Total Block can only go up to 30 for now. (using BG top dodge on crits to balance for AG)
           Total Weapon Skill can only go up 19 */
        private (int?, int?) SetBlockAndAttack(int playerLevel)
        {
            var totalBlock = minAttackBlock + (int)(playerLevel * blockRate);

            var totalAttack = minAttackBlock + (int)(playerLevel * attackRate);

            if (totalBlock > 40)
            {
                totalBlock = 40;
            }

            if (totalAttack > 21)
            {
                totalAttack = 21;
            }

            return (totalBlock, totalAttack);
        }

        private int? SetMagicalResistance(int playerLevel)
        {
            var totalResist = (int)(playerLevel * resistRate);

            if (totalResist > 40)
            {
                totalResist = 40;
            }

            return totalResist;
        }

        public class CommonSummonStats
        {
            public int? Attack { get; set; }
            public int? Block { get; set; }
            public int? Health { get; set; }
            public int? MagicResistance { get; set; }
        }

    }

    

}

