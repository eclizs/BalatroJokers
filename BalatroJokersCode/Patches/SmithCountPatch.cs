using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch(typeof(SmithRestSiteOption), nameof(SmithRestSiteOption.SmithCount), MethodType.Getter)]
class SmithCountPatch
{
    static void Postfix(ref SmithRestSiteOption __instance, ref object __result)
    {
        Player owner = (Player) Traverse.Create(__instance).Property("Owner").GetValue();

        bool playerHasCampfire = owner.Relics.OfType<Campfire>().Any();
        if (playerHasCampfire)
            __result = 2;
    }
}
