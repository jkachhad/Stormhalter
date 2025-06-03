using System;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public interface ILevelingItem
{
    int ItemExperience {get; set;}

    bool HasAlreadyLeveled {get; set;}

    int GrantExp (int Exp);
    bool HasAmuletLeveled();
}