using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheHermit() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(100)];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target?.Player == null) return;
        
        var gold = Math.Min(target.Player.Gold, this.DynamicVars.Gold.IntValue);
        
        await PlayerCmd.GainGold(gold, target.Player);
    }
}