using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class CreditCard() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;
    
    public static bool IsActive(Player player)
    {
        return player.Relics.OfType<CreditCard>().Any();
    }

    public static int GetAvailableGold(Player player)
    {
        return player.Gold + (IsActive(player) ? 100 : 0);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return (IEnumerable<DynamicVar>) new ReadOnlyCollection<DynamicVar>(new List<DynamicVar>{new GoldVar(100)});
        }
    }
}
