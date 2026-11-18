using MegaCrit.Sts2.Core.Entities.Cards;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheSun() : TransformingTarot
{
    protected override CardType TransformType => CardType.Power;
    protected override int CardAmount => 1;
}