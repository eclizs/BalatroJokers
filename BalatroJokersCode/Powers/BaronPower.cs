using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Powers;

public class BaronPower() : BalatroJokersPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override Decimal ModifyDamageMultiplicative(
        Creature? target,
        Decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        IReadOnlyList<CardModel> cards = PileType.Hand.GetPile(this.Owner.Player).Cards;
        decimal dmgReductionAmt = 0;
        foreach (var card in cards)
        {
            if (card.Rarity == CardRarity.Rare) dmgReductionAmt += 0.05M;
        }

        MainFile.Logger.Info("ModifyDamageMultiplicative:percentage amount:" + dmgReductionAmt);
        MainFile.Logger.Info("ModifyDamageMultiplicative:final dmg percentage:" + (1M - dmgReductionAmt));
        return target != this.Owner || !props.IsPoweredAttack() ? 1M : (1M - dmgReductionAmt);
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy)
            return;
        await PowerCmd.Remove((PowerModel) this);
    }
}