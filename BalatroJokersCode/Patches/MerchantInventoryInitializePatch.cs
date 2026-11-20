using BalatroJokers.BalatroJokersCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch(typeof(NMerchantInventory), nameof(NMerchantInventory.Initialize))]
static class MerchantInventoryInitializePatch
{
    static void Prefix(
        MerchantInventory inventory,
        ref MerchantDialogueSet dialogue)
    {
        foreach (var astronomer in inventory.Player.Relics.OfType<Astronomer>())
            astronomer.PopulateFreeEntries(inventory);
        
        if (CreditCard.GetGoldAmount(inventory.Player) == 0)
            return;
        
        dialogue = MerchantDialogueSet.CreateFromLocStrings(
            (IEnumerable<LocString>) LocManager.Instance
                .GetTable("merchant_room")
                .GetLocStringsWithPrefix("CREDIT-CARD.MERCHANT.talk."));
    }
}
