using System.Collections.ObjectModel;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using BalatroJokers.BalatroJokersCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Baron() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new ReadOnlyCollection<IHoverTip>(
                new List<IHoverTip> { HoverTipFactory.FromKeyword(CardKeyword.Retain) });
        }
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (!Baron.CanAffect(card))
            return Task.CompletedTask;
        CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (!(room is CombatRoom))
            return Task.CompletedTask;
        foreach (CardModel allCard in this.Owner.PlayerCombatState.AllCards)
        {
            if (Baron.CanAffect(allCard))
                CardCmd.ApplyKeyword(allCard, CardKeyword.Retain);
        }
        return Task.CompletedTask;
    }

    private static bool CanAffect(CardModel card)
    {
        return card.Rarity == CardRarity.Rare && !card.GetKeywordsWithSources(KeywordSources.Local).Contains(CardKeyword.Retain);
    }

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature))
            return;
        var cards = PileType.Hand.GetPile(this.Owner).Cards;
        decimal dmgReductionAmt = 0;
        foreach (var card in cards)
        {
            if (card.Rarity == CardRarity.Rare) dmgReductionAmt += 5M;
        }

        if (dmgReductionAmt > 0)
        {
            this.Flash();
            BaronPower? baronPower = await PowerCmd.Apply<BaronPower>(choiceContext, this.Owner.Creature, dmgReductionAmt, this.Owner.Creature, null);
        }
    }
}