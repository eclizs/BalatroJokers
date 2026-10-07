using System.Collections.ObjectModel;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class CreditCard() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;
    
    public static bool IsActive(Player player)
    {
        return player.Relics.OfType<CreditCard>().Any();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return (IEnumerable<DynamicVar>) new ReadOnlyCollection<DynamicVar>(new List<DynamicVar>{new GoldVar(100)});
        }
    }
}

[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.LoseGold))]
class PlayerCmdLoseGoldPatch
{
    private static void LoseGold(Decimal amount, Player player, GoldLossType goldLossType = GoldLossType.Lost)
    {
        SfxCmd.Play("event:/sfx/ui/gold/gold_1");
        PlayerMapPointHistoryEntry? entry = player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
        if (entry != null)
        {
            switch (goldLossType)
            {
                case GoldLossType.Spent:
                    entry.GoldSpent += (int) amount;
                    break;
                case GoldLossType.Lost:
                    entry.GoldLost += (int) amount;
                    break;
                case GoldLossType.Stolen:
                    entry.GoldStolen += (int) amount;
                    entry.MarkLootStolen((int) amount);
                    break;
            }
        }
        player.Gold = int.Max(-100, player.Gold - (int) amount);
    }
    
    static bool Prefix(ref object __result, Decimal amount, Player player, GoldLossType goldLossType = GoldLossType.Lost)
    {
        if (!CreditCard.IsActive(player))
            return true;

        LoseGold(amount, player, goldLossType);
        __result = Task.CompletedTask;
        return false;
    }
}

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

[HarmonyPatch(typeof(NMerchantInventory), nameof(NMerchantInventory.Initialize))]
static class MerchantInventoryInitializePatch
{
    static void Prefix(
        MerchantInventory inventory,
        ref MerchantDialogueSet dialogue)
    {
        if (!CreditCard.IsActive(inventory.Player))
            return;
        
        dialogue = MerchantDialogueSet.CreateFromLocStrings((IEnumerable<LocString>) LocManager.Instance.GetTable("merchant_room").GetLocStringsWithPrefix("CREDIT-CARD.MERCHANT.talk."));
    }
}
