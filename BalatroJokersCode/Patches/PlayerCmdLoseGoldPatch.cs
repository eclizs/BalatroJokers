using BalatroJokers.BalatroJokersCode.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace BalatroJokers.BalatroJokersCode.Patches;

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
