using BalatroJokers.BalatroJokersCode.Cards;
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

public class TheTower() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    
    public override IEnumerable<IHoverTip> ExtraHoverTips => 
    [ 
        HoverTipFactory.FromCard<StoneWall>(),
        HoverTipFactory.Static(StaticHoverTip.Transform),
        HoverTipFactory.FromKeyword(CardKeyword.Eternal) 
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(this.SelectionScreenPrompt, DynamicVars.Cards.IntValue);
        
        if (target?.Player == null) return;
        
        foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, target.Player, prefs, null, this))
        {
            CardModel? combatReplacement = card.CombatState.CreateCard<StoneWall>(target.Player);
            CardPileAddResult? nullable = await CardCmd.Transform(card, combatReplacement);
            await Cmd.Wait(0.5f);
            
            if (card.DeckVersion != null && !card.Keywords.Contains(CardKeyword.Eternal))
            {
                var deckReplacement = target.Player.RunState.CreateCard<StoneWall>(target.Player);
                var result = await CardCmd.Transform(card.DeckVersion, deckReplacement);

                if (result is { success: true } added)
                    combatReplacement.DeckVersion = added.cardAdded;
            }
        }
    }
}