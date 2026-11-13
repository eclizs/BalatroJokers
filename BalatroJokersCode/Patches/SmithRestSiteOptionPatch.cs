using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BalatroJokers.BalatroJokersCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch]
static class SmithRestSiteOptionOnSelectPatch
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

    private static int GetUpgradeCount(SmithRestSiteOption option)
    {
        Player player = (Player)Traverse.Create(option).Property("Owner").GetValue();
        return Campfire.IsActive(player) ? Campfire.UpgradeAmount : 1;
    }

    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var promptGetter = AccessTools.PropertyGetter(
            typeof(CardSelectorPrefs),
            nameof(CardSelectorPrefs.UpgradeSelectionPrompt))!;
        var constructor = AccessTools.Constructor(
            typeof(CardSelectorPrefs), [typeof(LocString), typeof(int)])!;
        var getUpgradeCount = AccessTools.Method(
            typeof(SmithRestSiteOptionOnSelectPatch), nameof(GetUpgradeCount))!;

        int countIndex = codes.FindIndex(1, instruction =>
            instruction.opcode == OpCodes.Newobj && Equals(instruction.operand, constructor));

        if (countIndex < 2 || !codes[countIndex - 1].LoadsConstant(1) ||
            !codes[countIndex - 2].Calls(promptGetter))
        {
            throw new InvalidOperationException(
                "Could not find the smith CardSelectorPrefs count initializer.");
        }

        var loadOption = new CodeInstruction(OpCodes.Ldarg_0);
        codes[countIndex - 1].MoveLabelsTo(loadOption);
        codes[countIndex - 1].MoveBlocksTo(loadOption);
        codes[countIndex - 1] = loadOption;
        codes.Insert(countIndex, new CodeInstruction(OpCodes.Ldfld, _optionField!));
        codes.Insert(countIndex + 1, new CodeInstruction(OpCodes.Call, getUpgradeCount));

        var setter = AccessTools.PropertySetter(
            typeof(CardSelectorPrefs),
            nameof(CardSelectorPrefs.RequireManualConfirmation))!;
        var shouldRequireConfirmation = AccessTools.Method(
            typeof(SmithRestSiteOptionOnSelectPatch),
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

[HarmonyPatch(typeof(RestSiteOption), nameof(RestSiteOption.Description), MethodType.Getter)]
static class RestSiteLocalizationPatch
{
    static void Postfix(RestSiteOption __instance, ref object __result)
    {
        Player? owner = Traverse.Create(__instance).Property("Owner").GetValue<Player>();
        MainFile.Logger.Info("Owner is " + owner ?? "null");
        if (owner is not null && Campfire.IsActive(owner) && __instance is SmithRestSiteOption)
        {
            __result = new LocString("rest_site_ui",
                __instance.IsEnabled
                    ? "OPTION_CAMPFIRE_SMITH.description"
                    : $"OPTION_{__instance.OptionId}.descriptionDisabled");
        }
    }
}
