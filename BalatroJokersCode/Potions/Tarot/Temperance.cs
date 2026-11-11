using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class Temperance() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("CurrentAmount"),
        new DynamicVar("CommonAmount", 10M),
        new DynamicVar("UncommonAmount", 20M),
        new DynamicVar("RareAmount", 30M),
        new GoldVar(250)
    ];

    private int CalculateGold(Player player)
    {
        var commonCount = player.Relics.Count(r => r.Rarity == RelicRarity.Common);
        var uncommonCount = player.Relics.Count(r => r.Rarity == RelicRarity.Uncommon);
        var rareCount = player.Relics.Count(r => r.Rarity == RelicRarity.Rare);
        
        var total = commonCount*DynamicVars["CommonAmount"].IntValue + uncommonCount*DynamicVars["UncommonAmount"].IntValue + rareCount*DynamicVars["RareAmount"].IntValue;
        return Math.Min(total, this.DynamicVars.Gold.IntValue);
    }
    
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        ((StringVar)DynamicVars["CurrentAmount"]).StringValue = CalculateGold(Owner).ToString();
        return Task.CompletedTask;
    }

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target?.Player == null) return;
        
        var gold = CalculateGold(target.Player);
        
        await PlayerCmd.GainGold(gold, target.Player);
    }
}