using BalatroJokers.BalatroJokersCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Interfaces;

public interface IChance
{
    
}

public static class ChanceMethods
{
    public static int GetNumerator(this IChance _, Player? player)
    {
        int? jokerCount = player?.Relics.Count(r => r.CanonicalInstance == ModelDb.Relic<OopsAllSixes>());
        if (jokerCount is null or 0) return 1;
        return (int)Math.Pow(2, jokerCount.Value);
    }

    public static bool RollChance(this IChance IChance, Player player, int chance)
    {
        chance = chance < 1 ? 1 : chance;
        return (float)IChance.GetNumerator(player) / chance >= player.RunState.Rng.Niche.NextFloat();
    }
}