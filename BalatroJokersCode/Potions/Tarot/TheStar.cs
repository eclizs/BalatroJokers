using MegaCrit.Sts2.Core.Entities.Cards;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheStar() : TransformingTarot
{
    protected override CardType TransformType => CardType.Attack;
    protected override int CardAmount => 1;
}