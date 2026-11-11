using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class Temperance() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CommonAmount", 10M),
        new DynamicVar("UncommonAmount", 20M),
        new DynamicVar("RareAmount", 30M),
        new GoldVar(250)
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target?.Player == null) return;

        var commonCount = target.Player.Relics.Count(r => r.Rarity == RelicRarity.Common);
        var uncommonCount = target.Player.Relics.Count(r => r.Rarity == RelicRarity.Uncommon);
        var rareCount = target.Player.Relics.Count(r => r.Rarity == RelicRarity.Rare);
        
        var total = commonCount*DynamicVars["CommonAmount"].IntValue + uncommonCount*DynamicVars["UncommonAmount"].IntValue + rareCount*DynamicVars["RareAmount"].IntValue;
        
        var gold = Math.Min(total, this.DynamicVars.Gold.IntValue);
        
        await PlayerCmd.GainGold(gold, target.Player);
    }
}