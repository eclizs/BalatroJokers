using BalatroJokers.BalatroJokersCode.Enchantments;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.HasTurnEndInHandEffect), MethodType.Getter)]
class TurnEndGetterPatch
{
    static void Postfix(CardModel __instance, ref object __result)
    {
        if (Gold.IsActive(__instance))
            __result = true;
    }
}

[HarmonyPatch(typeof(CardModel), "OnTurnEndInHand")]
class OnTurnEndInHandPatch
{
    static void Postfix(CardModel __instance, ref object __result)
    {
        if (Gold.IsActive(__instance))
        {
            var amount = __instance.Enchantment.Amount;
            PlayerCmd.GainGold(amount, __instance.Owner);
        }
    }
}