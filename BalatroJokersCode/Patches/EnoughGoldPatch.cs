using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch(typeof(MerchantEntry), nameof(MerchantEntry.EnoughGold), MethodType.Getter)]
class EnoughGoldPatch
{
    static void Postfix(MerchantEntry __instance, ref object __result)
    {
        Player? player = Traverse.Create(__instance).Field("_player").GetValue<Player>();

        if (player is not null && CreditCard.IsActive(player))
            __result = __instance.Cost <= player.Gold + 100;
    }
}
