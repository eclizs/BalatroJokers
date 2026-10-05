using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Campfire() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
}

[HarmonyPatch(typeof(SmithRestSiteOption), nameof(SmithRestSiteOption.SmithCount), MethodType.Getter)]
class SmithCountPatch
{
    static void Postfix(ref object __result)
    {
        __result = 2;
    }
}