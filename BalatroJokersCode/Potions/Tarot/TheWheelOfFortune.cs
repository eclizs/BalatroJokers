using BalatroJokers.BalatroJokersCode.Interfaces;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Cards.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheWheelOfFortune() : BalatroJokersTarotPotion, IChance
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DisplayVar<TheWheelOfFortune>("Numerator", p =>
        {
            var canonical = p.IsCanonical ? null : p;
            if (canonical == null) return p.GetNumerator(null).ToString();
            var player = canonical.IsCanonical ? null : canonical.Owner;
            return p.GetNumerator(player).ToString();
        }),
        new DynamicVar("Denominator", 4M)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..HoverTipFactory.FromRelic<FoilJimbo>(),
        ..HoverTipFactory.FromRelic<HolographicJimbo>(),
        ..HoverTipFactory.FromRelic<PolychromeJimbo>()
    ];

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var player = target.Player;
        if (player == null) return Task.CompletedTask;
        if (this.RollChance(player, DynamicVars["Denominator"].IntValue))
        {
            var randomInt = player.RunState.Rng.Niche.NextInt(0,100);
            switch (randomInt)
            {
                case < 15:
                    RelicCmd.Obtain<FoilJimbo>(player);
                    break;
                case < 50:
                    RelicCmd.Obtain<HolographicJimbo>(player);
                    break;
                default:
                    RelicCmd.Obtain<PolychromeJimbo>(player);
                    break;
            }
        }
        
        return Task.CompletedTask;
    }
}