using BalatroJokers.BalatroJokersCode.Pools;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace BalatroJokers.BalatroJokersCode.Potions;

[Pool(typeof(SharedPotionPool))]
public class TheFool() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("LastTarotOrPlanetUsed")];

    private bool PotionIsValid(PotionModel potion)
    {
        return (ModelDb.PotionPool<BalatroJokersTarotPotionPool>().AllPotions.Contains(potion.CanonicalInstance) ||
                ModelDb.PotionPool<BalatroJokersPlanetPotionPool>().AllPotions.Contains(potion.CanonicalInstance))
               && potion.CanonicalInstance != ModelDb.Potion<TheFool>();
    }

    private PotionPoolModel GetPotionPool(PotionModel potion)
    {
        if (ModelDb.PotionPool<BalatroJokersTarotPotionPool>().AllPotions.Contains(potion.CanonicalInstance))
            return ModelDb.PotionPool<BalatroJokersTarotPotionPool>();
        else if (ModelDb.PotionPool<BalatroJokersPlanetPotionPool>().AllPotions.Contains(potion.CanonicalInstance))
            return ModelDb.PotionPool<BalatroJokersPlanetPotionPool>();
        else return null;
    }

    private Task SetupDynamicVar(PotionModel potion)
    {
        if (!PotionIsValid(potion)) return Task.CompletedTask;
        
        PotionPoolModel potionPool = GetPotionPool(potion);

        switch (potionPool)
        {
            case BalatroJokersTarotPotionPool:
                ((StringVar)DynamicVars["LastTarotOrPlanetUsed"]).StringValue = $"[purple]{potion.Title.GetFormattedText()}[/purple]" ?? "";
                break;
            case  BalatroJokersPlanetPotionPool:
                ((StringVar)DynamicVars["LastTarotOrPlanetUsed"]).StringValue = $"[aqua]{potion.Title.GetFormattedText()}[/aqua]" ?? "";
                break;
        }
        return Task.CompletedTask;
    }

    public override Task AfterPotionUsed(PotionModel potion, Creature? target) => SetupDynamicVar(potion);

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var lastPotionUsed = CombatManager.Instance.History.Entries.OfType<PotionUsedEntry>().
            LastOrDefault(p => PotionIsValid(p.Potion))
            ?.Potion;

        var sourceId = lastPotionUsed?.CanonicalInstance.Id;

        if (sourceId == null) return;
        
        var canonical = sourceId == ModelId.none ? null : ModelDb.GetByIdOrNull<PotionModel>(sourceId);

        if (canonical == null)
            return;
    
        PotionProcureResult procure = await PotionCmd.TryToProcure(canonical.ToMutable(), this.Owner);
    }
}