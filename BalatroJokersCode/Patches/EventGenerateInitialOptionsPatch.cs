using System.Reflection;
using System.Reflection.Emit;
using BalatroJokers.BalatroJokersCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch]
static class EventGenerateInitialOptionsPatch
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(EventModel).Assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                typeof(EventModel).IsAssignableFrom(type))
            .Select(type => AccessTools.Method(type, "GenerateInitialOptions"))
            .Where(method => method is not null)
            .Cast<MethodBase>();
    }

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo goldGetter =
            AccessTools.PropertyGetter(typeof(Player), nameof(Player.Gold));
        MethodInfo availableGold =
            AccessTools.Method(typeof(CreditCard), nameof(CreditCard.GetAvailableGold))!;

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(goldGetter))
                yield return new CodeInstruction(OpCodes.Call, availableGold);
            else
                yield return instruction;
        }
    }
}

// Special patch for endless conveyor because the logic for checking gold is inside GenerateGrabSomethingOffTheBeltOption
[HarmonyPatch(typeof(EndlessConveyor), "GenerateGrabSomethingOffTheBeltOption")]
static class GenerateGrabSomethingOffTheBeltOptionPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo goldGetter =
            AccessTools.PropertyGetter(typeof(Player), nameof(Player.Gold));
        MethodInfo availableGold =
            AccessTools.Method(typeof(CreditCard), nameof(CreditCard.GetAvailableGold))!;

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(goldGetter))
                yield return new CodeInstruction(OpCodes.Call, availableGold);
            else
                yield return instruction;
        }
    }
}
