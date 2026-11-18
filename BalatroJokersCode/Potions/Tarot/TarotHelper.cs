using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
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
    public override TargetType TargetType => TargetType.AnyPlayer;
    
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
        
        if (target?.Player == null) return;
        
        foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, target.Player, prefs, (c => canonicalEnchantment.CanEnchant(c)), this))
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

public abstract class TransformingTarot : BalatroJokersTarotPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;
    
    protected abstract CardType TransformType { get; }
    protected abstract int CardAmount { get; }
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(CardAmount)
    ];

    private CardModel GetCanonicalForTransform(CardModel original)
    {
        var card = CardFactory.CreateRandomCardForTransform(original,
            Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                .Where(c => c.Type == TransformType), true, Owner.RunState.Rng.CombatCardGeneration);

        return card.CanonicalInstance ?? original.CanonicalInstance;
    }

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(this.SelectionScreenPrompt, 0, DynamicVars.Cards.IntValue);
        
        if (target?.Player == null) return;
        
        var player = target.Player;

        foreach (CardModel card in await CardSelectCmd.FromHand(choiceContext, player, prefs,
                     (c => c.IsTransformable), this))
        {
            var canonical = GetCanonicalForTransform(card);
            
            var combatReplacement = card.CombatState?.CreateCard(canonical, target.Player);
            
            var result1 = await CardCmd.Transform(card, combatReplacement);
            await Cmd.Wait(0.5f);
            
            if (card.DeckVersion != null && !card.Keywords.Contains(CardKeyword.Eternal))
            {
                var deckReplacement = player.RunState.CreateCard(canonical, player);
                var result2 = await CardCmd.Transform(card.DeckVersion, deckReplacement);
                
                if (result2 is { success: true } added)
                    combatReplacement.DeckVersion = added.cardAdded;
            }
        }
    }
}