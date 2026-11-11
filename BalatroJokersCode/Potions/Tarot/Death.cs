using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class Death() : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    public override IEnumerable<IHoverTip> ExtraHoverTips => 
        [ 
            HoverTipFactory.Static(StaticHoverTip.Transform),
            HoverTipFactory.FromKeyword(CardKeyword.Eternal) 
        ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (PileType.Hand.GetPile(Owner).Cards.Count < 2) return;
        CardSelectorPrefs prefs = new CardSelectorPrefs(this.SelectionScreenPrompt, 2);
        
        if (target?.Player == null) return;
        
        var choiceOfCards = await CardSelectCmd.FromHand(choiceContext, target.Player, prefs, (c => c is { IsTransformable: true }), this);

        var cardList = choiceOfCards.ToList();
        var leftCard = cardList[0];
        var rightCardId = cardList[1].CanonicalInstance.Id;
        var rightCardEnchantment = cardList[1].Enchantment;
        var EnchantmentId = rightCardEnchantment?.CanonicalInstance.Id;

        var canonical = rightCardId == ModelId.none ? null : ModelDb.GetByIdOrNull<CardModel>(rightCardId);
        
        if (canonical == null) return;
        
        var player = leftCard.Owner;
        var deckVersion = leftCard.DeckVersion;
        var combatState = leftCard.CombatState;

        if (combatState == null) return;

        var combatReplacement = combatState.CreateCard(canonical, player);

        if (rightCardEnchantment != null && EnchantmentId != null)
        {
            CardCmd.Enchant(ModelDb.GetByIdOrNull<EnchantmentModel>(EnchantmentId)?.ToMutable(), combatReplacement, rightCardEnchantment.Amount);
        }
        
        await CardCmd.Transform(leftCard, combatReplacement);
        await Cmd.Wait(0.5f);
        
        if (deckVersion != null && !leftCard.Keywords.Contains(CardKeyword.Eternal))
        {
            var deckReplacement = player.RunState.CreateCard(canonical, player);
            if (rightCardEnchantment != null && EnchantmentId != null)
            {
                CardCmd.Enchant(ModelDb.GetByIdOrNull<EnchantmentModel>(EnchantmentId)?.ToMutable(), deckReplacement, rightCardEnchantment.Amount);
            }
            var result = await CardCmd.Transform(deckVersion, deckReplacement, CardPreviewStyle.MessyLayout);

            if (result is { success: true } added)
                combatReplacement.DeckVersion = added.cardAdded;
        }
        
    }
}