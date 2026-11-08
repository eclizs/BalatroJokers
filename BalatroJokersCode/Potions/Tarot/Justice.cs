using BalatroJokers.BalatroJokersCode.Enchantments;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class Justice() : TarotHelper<Glass>
{
    protected override decimal EnchantAmount => 1;
}