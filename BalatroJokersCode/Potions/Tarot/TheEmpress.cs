using BalatroJokers.BalatroJokersCode.Potions.Tarot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace BalatroJokers.BalatroJokersCode.Potions;

public class TheEmpress() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("SharpAmount", 2M)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [.. HoverTipFactory.FromEnchantment<Sharp>(DynamicVars["SharpAmount"].IntValue) ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(this.SelectionScreenPrompt, 0, DynamicVars.Cards.IntValue);
        Sharp canonicalEnchantment = ModelDb.Enchantment<Sharp>();
        foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, this.Owner, prefs, (c => c is { Type: CardType.Attack, Enchantment: null }), this))
        {
            CardCmd.Enchant(canonicalEnchantment.ToMutable(), card, (Decimal) this.DynamicVars["SharpAmount"].IntValue);
        }
        canonicalEnchantment = null;
    }
}