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

public class TheHangedMan() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];
    
    public override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromKeyword(CardKeyword.Exhaust) ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(this.SelectionScreenPrompt, DynamicVars.Cards.IntValue);
        
        if (target?.Player == null) return;
        
        foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, target.Player, prefs, null, this))
        {
            await CardCmd.Exhaust(choiceContext, card);
            if (card.DeckVersion != null && !card.Keywords.Contains(CardKeyword.Eternal))
            {
                await CardPileCmd.RemoveFromDeck(card.DeckVersion);
            }
        }
    }
}