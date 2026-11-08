using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheEmpress() : TarotHelper<Sharp>
{
    protected override decimal EnchantAmount => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar("SharpAmount", EnchantAmount)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [.. HoverTipFactory.FromEnchantment<Sharp>(DynamicVars["SharpAmount"].IntValue) ];
}