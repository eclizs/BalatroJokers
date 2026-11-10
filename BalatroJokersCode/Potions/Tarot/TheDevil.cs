using BalatroJokers.BalatroJokersCode.Enchantments;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheDevil() : EnchantingTarot<Gold>
{
    protected override decimal EnchantAmount => 5;

    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [ new CardsVar(1), new DynamicVar("GoldAmount", EnchantAmount) ];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [..HoverTipFactory.FromEnchantment<Gold>(DynamicVars["GoldAmount"].IntValue)];
}