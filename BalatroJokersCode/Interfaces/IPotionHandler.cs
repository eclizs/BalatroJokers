using BalatroJokers.BalatroJokersCode.Pools;
using BalatroJokers.BalatroJokersCode.Potions.Tarot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Interfaces;

public interface IPotionHandler
{
    
}

public static class TarotMethods
{
    public static Task UpdateTarotHistory(this IPotionHandler _, PotionModel potion, Player player)
    {
        if (player is null || potion.Owner != player || potion is TheFool) return Task.CompletedTask;

        bool valid =
            ModelDb.PotionPool<BalatroJokersTarotPotionPool>()
                .AllPotions.Contains(potion.CanonicalInstance)
            || ModelDb.PotionPool<BalatroJokersPlanetPotionPool>()
                .AllPotions.Contains(potion.CanonicalInstance);

        if (valid)
            TarotHistory.LastUsed[player] = potion.CanonicalInstance.Id;

        return Task.CompletedTask;
    }

    public static async Task<PotionProcureResult> CreateTarot(this IPotionHandler _, Player player)
    {
        var tarots = ModelDb.PotionPool<BalatroJokersTarotPotionPool>().AllPotions.ToList();
        var randomTarot = player.RunState.Rng.CombatPotionGeneration.NextItem(tarots);
        PotionProcureResult procure = await PotionCmd.TryToProcure(randomTarot?.ToMutable(), player);
        return procure;
    }
}