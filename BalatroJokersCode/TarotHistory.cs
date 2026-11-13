using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

public static class TarotHistory
{
    public static readonly SavedSpireField<Player, ModelId> LastUsed =
        new(() => ModelId.none, "BalatroJokers_LastTarotOrPlanetUsed");
    
    public static void Initialize() => _ = LastUsed;
}