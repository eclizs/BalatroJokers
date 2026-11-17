using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Wild() : BalatroJokersEnchantment
{
    public override bool ShowAmount => false;
    public override bool HasExtraCardText => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Transform)];

    private CardModel? GetCardForTransform(Player? player)
    {
        if (player == null) return null;
        var pool = player.UnlockState.CardPools.ToList();

        var canonicalEnchantment = ModelDb.Enchantment<Wild>();

        var candidates = CardFactory.FilterForCombat(
                pool.SelectMany(p => p.GetUnlockedCards(
                    player.UnlockState,
                    player.RunState.CardMultiplayerConstraint)))
            .Concat(ModelDb.AllCards.Where(c => c.Rarity == CardRarity.Ancient))
            .Distinct()
            .Where(c =>
                c.Type != CardType.Power &&
                canonicalEnchantment.CanEnchant(c) &&
                !c.Keywords.Contains(CardKeyword.Exhaust))
            .Where(c => c.MultiplayerConstraint !=
                        (player.RunState.Players.Count > 1
                            ? CardMultiplayerConstraint.SingleplayerOnly
                            : CardMultiplayerConstraint.MultiplayerOnly))
            .ToList();

        if (candidates.Count == 0) return null;
        
        var canonical = player.RunState.Rng.CombatCardGeneration
            .NextItem(candidates);

        var card = player.Creature.CombatState.CreateCard(canonical, player);
        
        var enchant = CardCmd.Enchant<Wild>(card, 1);
        return enchant == null ? null : card;
    }

    public override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (cardPlay?.Card != this.Card) return Task.CompletedTask;

        var replacement = GetCardForTransform(this.Card.Owner);
        if (replacement == null) return Task.CompletedTask;

        Traverse.Create(this.Card)
            .Property("TransformIntoAfterPlay")
            .SetValue(replacement);
        
        return Task.CompletedTask;
    }
}