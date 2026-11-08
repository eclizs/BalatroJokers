using MegaCrit.Sts2.Core.Entities.Players;

namespace BalatroJokers.BalatroJokersCode.Interfaces;

public interface IChance
{
    
}

public static class ChanceMethods
{
    // Placeholder before implementing the oops all 6s joker
    public static int GetNumerator(this IChance _, Player? player)
    {
        // bool hasJoker = player?.Relics.Contains(OopsAll6s);
        // if (!hasJoker) return 1;
        // return (int)Math.Pow(2, power.Amount);
        return 1;
    }

    public static bool RollChance(this IChance IChance, Player player, int chance)
    {
        chance = chance < 1 ? 1 : chance;
        return (float)IChance.GetNumerator(player) / chance >= player.RunState.Rng.Niche.NextFloat();
    }
}