using BalatroJokers.BalatroJokersCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Powers;

public class MadnessPower() : BalatroJokersPower
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
        if (!props.IsPoweredAttack() || cardSource == null || cardSource.Owner.Creature != Owner)
            return 1M;
        int num1 = CombatManager.Instance.History.CardPlaysStarted.Count(
            e =>
                e.HappenedThisTurn(CombatState) && e.CardPlay.Card.Type == CardType.Attack &&
                e.CardPlay.Card.Owner.Creature == Owner);
        CardPile pile1 = cardSource.Pile;
        int num2 = pile1 != null && pile1.Type == PileType.Play ? 1 : 0;
        return num1 > num2 ? 1M : 1M + Amount / 100M;
    }
}