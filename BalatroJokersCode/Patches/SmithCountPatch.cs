using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BalatroJokers.BalatroJokersCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch(typeof(SmithRestSiteOption), nameof(SmithRestSiteOption.SmithCount), MethodType.Getter)]
class SmithCountPatch
{
    static void Postfix(ref SmithRestSiteOption __instance, ref object __result)
    {
        Player owner = (Player) Traverse.Create(__instance).Property("Owner").GetValue();
        
        if (Campfire.IsActive(owner))
            __result = Campfire.UpgradeAmount;
    }
}

[HarmonyPatch]
static class OnSelectPatch
{
    private static FieldInfo? _optionField;

    static MethodBase TargetMethod()
    {
        MethodInfo onSelect = AccessTools.Method(
            typeof(SmithRestSiteOption),
            nameof(SmithRestSiteOption.OnSelect))!;

        var asyncStateMachine =
            onSelect.GetCustomAttribute<AsyncStateMachineAttribute>()
            ?? throw new InvalidOperationException("OnSelect is not an async method.");

        _optionField = asyncStateMachine.StateMachineType
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(field => field.FieldType == typeof(SmithRestSiteOption));

        return AccessTools.Method(asyncStateMachine.StateMachineType, "MoveNext")!;
    }

    private static bool ShouldRequireManualConfirmation(SmithRestSiteOption option)
    {
        Player player = (Player)Traverse.Create(option).Property("Owner").GetValue();
        return !Campfire.IsActive(player);
    }

    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var setter = AccessTools.PropertySetter(
            typeof(CardSelectorPrefs),
            nameof(CardSelectorPrefs.RequireManualConfirmation))!;
        var shouldRequireConfirmation = AccessTools.Method(
            typeof(OnSelectPatch),
            nameof(ShouldRequireManualConfirmation))!;

        for (int i = 1; i < codes.Count; i++)
        {
            if (codes[i].Calls(setter) &&
                codes[i - 1].opcode == OpCodes.Ldc_I4_1)
            {
                var loadStateMachine = new CodeInstruction(OpCodes.Ldarg_0);
                codes[i - 1].MoveLabelsTo(loadStateMachine);
                codes[i - 1].MoveBlocksTo(loadStateMachine);
                codes[i - 1] = loadStateMachine;
                codes.Insert(i, new CodeInstruction(OpCodes.Ldfld, _optionField!));
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, shouldRequireConfirmation));
                return codes;
            }
        }

        throw new InvalidOperationException(
            "Could not find the RequireManualConfirmation initializer.");
    }
}
