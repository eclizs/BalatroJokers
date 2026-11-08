using BalatroJokers.BalatroJokersCode.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheHighPriestess() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.None;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PotionAmount", 2M)];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(BalatroJokersHoverTips.Planet)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        int potionsProcured = 0;
        var tarots = ModelDb.PotionPool<BalatroJokersPlanetPotionPool>().AllPotions.ToList();
        while (this.Owner.HasOpenPotionSlots && potionsProcured < DynamicVars["PotionAmount"].IntValue)
        {
            var randomPlanet = Owner.RunState.Rng.CombatPotionGeneration.NextItem(tarots);
            if (!(await PotionCmd.TryToProcure(randomPlanet?.ToMutable(), this.Owner)).success)
                break;
            potionsProcured++;
        }
    }
}