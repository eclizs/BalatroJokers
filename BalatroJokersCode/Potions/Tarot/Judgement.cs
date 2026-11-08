using BalatroJokers.BalatroJokersCode.Potions.Tarot;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace BalatroJokers.BalatroJokersCode.Potions;

[Pool(typeof(SharedPotionPool))]
public class Judgement() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("RelicAmount", 1)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(BalatroJokersHoverTips.Joker)];

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var relic = RelicFactory.PullNextRelicFromFront(Owner).ToMutable();
        RelicCmd.Obtain(relic, Owner);
        return Task.CompletedTask;
    }
}