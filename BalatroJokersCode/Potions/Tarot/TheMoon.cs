using MegaCrit.Sts2.Core.Entities.Cards;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheMoon() : TransformingTarot
{
    protected override CardType TransformType => CardType.Skill;
    protected override int CardAmount => 1;
}