using BalatroJokers.BalatroJokersCode.Pools;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

[Pool(typeof(SharedPotionPool))]
public class TheFool() : BalatroJokersTarotPotion
{
    private List<IHoverTip> _extraHoverTips = 
    [
        HoverTipFactory.Static(BalatroJokersHoverTips.Tarot),
        HoverTipFactory.Static(BalatroJokersHoverTips.Planet)
    ];
    
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("LastTarotOrPlanetUsed")];

    public override IEnumerable<IHoverTip> ExtraHoverTips => _extraHoverTips;
    

    private PotionPoolModel GetPotionPool(PotionModel potion)
    {
        if (ModelDb.PotionPool<BalatroJokersTarotPotionPool>().AllPotions.Contains(potion.CanonicalInstance))
            return ModelDb.PotionPool<BalatroJokersTarotPotionPool>();
        else if (ModelDb.PotionPool<BalatroJokersPlanetPotionPool>().AllPotions.Contains(potion.CanonicalInstance))
            return ModelDb.PotionPool<BalatroJokersPlanetPotionPool>();
        else return null;
    }

    private Task SetupDynamicVar(ModelId? potionId)
    {
        var potion = ModelDb.GetByIdOrNull<PotionModel>(potionId);
        if (potion == null) return Task.CompletedTask;
        PotionPoolModel potionPool = GetPotionPool(potion);

        switch (potionPool)
        {
            case BalatroJokersTarotPotionPool:
                ((StringVar)DynamicVars["LastTarotOrPlanetUsed"]).StringValue = $"[color=#C093F8]{potion.Title.GetFormattedText()}[/color]" ?? "";
                _extraHoverTips.AddRange(potion.HoverTips);
                break;
            case  BalatroJokersPlanetPotionPool:
                ((StringVar)DynamicVars["LastTarotOrPlanetUsed"]).StringValue = $"[color=#96E3FC]{potion.Title.GetFormattedText()}[/color]" ?? "";
                _extraHoverTips.AddRange(potion.HoverTips);
                break;
        }
        return Task.CompletedTask;
    }

    public override Task AfterPotionUsed(PotionModel potion, Creature? target) => SetupDynamicVar(TarotHistory.LastUsed[Owner]);

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var id = TarotHistory.LastUsed[Owner];
        if (id == ModelId.none) return;

        var canonical = ModelDb.GetByIdOrNull<PotionModel>(id);
        if (canonical == null) return;

        await PotionCmd.TryToProcure(canonical.ToMutable(), Owner);
    }
}