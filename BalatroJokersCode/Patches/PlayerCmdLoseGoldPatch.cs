using System.Reflection.Emit;
using BalatroJokers.BalatroJokersCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.LoseGold))]
static class PlayerCmdLoseGoldPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var max = AccessTools.Method(typeof(int), nameof(int.Max), [typeof(int), typeof(int)]);
        var goldSetter = AccessTools.PropertySetter(typeof(Player), nameof(Player.Gold));
        var clampGold = AccessTools.Method(typeof(PlayerCmdLoseGoldPatch), nameof(ClampGold));

        for (int i = 0; i < codes.Count - 1; i++)
        {
            if (!codes[i].Calls(max) || !codes[i + 1].Calls(goldSetter))
                continue;

            codes[i].opcode = OpCodes.Ldarg_1;
            codes[i].operand = null;
            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, clampGold));
            return codes;
        }

        throw new InvalidOperationException("Could not find the gold clamp in PlayerCmd.LoseGold.");
    }

    private static int ClampGold(int minimum, int gold, Player player)
    {
        var goldAmount = CreditCard.GetGoldAmount(player);
        return int.Max(goldAmount > 0 ? -goldAmount : minimum, gold);
    }
}
