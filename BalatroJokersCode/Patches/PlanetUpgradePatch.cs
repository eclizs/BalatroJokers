using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch]
internal static class PlanetUpgradePatch
{
    [ThreadStatic]
    private static CardModel? _planetUpgradeTarget;

    internal static void UpgradeWithPlanet(CardModel card)
    {
        CardModel? previous = _planetUpgradeTarget;
        _planetUpgradeTarget = card;
        try
        {
            CardCmd.Upgrade(card, CardPreviewStyle.GridLayout);
        }
        finally
        {
            _planetUpgradeTarget = previous;
        }
    }

    private static int GetEligibilityLimit(CardModel card)
    {
        int normalLimit = card.MaxUpgradeLevel;
        return normalLimit > 0 && ReferenceEquals(card, _planetUpgradeTarget)
            ? int.MaxValue
            : normalLimit;
    }

    private static int GetAssignmentLimit(CardModel card)
    {
        return ReferenceEquals(card, PlanetUpgradeReplayPatch.RestoringCard)
            ? int.MaxValue
            : GetEligibilityLimit(card);
    }

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.IsUpgradable));
        yield return AccessTools.PropertySetter(typeof(CardModel), nameof(CardModel.CurrentUpgradeLevel));
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var getter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel));
        var helper = AccessTools.Method(typeof(PlanetUpgradePatch),
            original.Name == "get_IsUpgradable" ? nameof(GetEligibilityLimit) : nameof(GetAssignmentLimit));
        return ReplaceCall(instructions, getter, helper, original);
    }

    internal static IEnumerable<CodeInstruction> ReplaceCall(
        IEnumerable<CodeInstruction> instructions, MethodInfo calledMethod, MethodInfo replacement, MethodBase original)
    {
        var codes = instructions.ToList();
        var call = codes.SingleOrDefault(instruction => instruction.Calls(calledMethod))
            ?? throw new InvalidOperationException($"Could not find {calledMethod.Name} in {original.Name}.");

        call.opcode = OpCodes.Call;
        call.operand = replacement;
        return codes;
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Title), MethodType.Getter)]
internal static class PlanetUpgradeTitlePatch
{
    private static int GetTitleUpgradeLimit(CardModel card)
    {
        return Math.Max(card.MaxUpgradeLevel, card.CurrentUpgradeLevel);
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var getter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel));
        var helper = AccessTools.Method(typeof(PlanetUpgradeTitlePatch), nameof(GetTitleUpgradeLimit));
        return PlanetUpgradePatch.ReplaceCall(instructions, getter, helper, original);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
internal static class PlanetUpgradeReplayPatch
{
    [ThreadStatic]
    private static CardModel? _restoringCard;

    internal static CardModel? RestoringCard => _restoringCard;

    private static void ReplayUpgrade(CardModel card)
    {
        CardModel? previous = _restoringCard;
        _restoringCard = card;
        try
        {
            card.UpgradeInternal();
        }
        finally
        {
            _restoringCard = previous;
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var upgrade = AccessTools.Method(typeof(CardModel), nameof(CardModel.UpgradeInternal));
        var replay = AccessTools.Method(typeof(PlanetUpgradeReplayPatch), nameof(ReplayUpgrade));
        return PlanetUpgradePatch.ReplaceCall(instructions, upgrade, replay, original);
    }
}
