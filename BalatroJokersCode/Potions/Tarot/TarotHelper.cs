using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public abstract class EnchantingTarot<TEnchant> : BalatroJokersTarotPotion
    where TEnchant : EnchantmentModel
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;
    
    protected abstract decimal EnchantAmount { get; }
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];
    
    public override IEnumerable<IHoverTip> ExtraHoverTips => [.. HoverTipFactory.FromEnchantment<TEnchant>() ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(this.SelectionScreenPrompt, DynamicVars.Cards.IntValue);
        TEnchant canonicalEnchantment = ModelDb.Enchantment<TEnchant>();
        foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, this.Owner, prefs, (c => c is {Enchantment: null } && canonicalEnchantment.CanEnchant(c)), this))
        {
            CardCmd.Enchant(canonicalEnchantment.ToMutable(), card, EnchantAmount);
            if (card.DeckVersion != null)
            {
                CardCmd.Enchant(canonicalEnchantment.ToMutable(), card.DeckVersion, EnchantAmount);
                CardCmd.Preview(card.DeckVersion);
            }
        }
        canonicalEnchantment = null;
    }
}