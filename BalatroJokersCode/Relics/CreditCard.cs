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
    
    public static int GetGoldAmount(Player player)
    {
        return 100 * player.Relics.OfType<CreditCard>().Count(r => !r.IsMelted);
    }

    public static int GetAvailableGold(Player player)
    {
        return player.Gold + GetGoldAmount(player);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(100)];
}
