using System.Buffers;
using BalatroJokers.BalatroJokersCode.Interfaces;
using BaseLib.Cards.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Lucky() : BalatroJokersEnchantment, IChance
{
    public override bool ShowAmount => false;
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card) => 
        base.CanEnchant(card) && card.Type == CardType.Attack;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DisplayVar<Lucky>("BaseChance", e =>
        {
            var card = e.IsCanonical ? null : e.Card;
            if (card == null) return e.GetNumerator(null).ToString();
            var player = card.IsCanonical ? null : card.Owner;
            return e.GetNumerator(player).ToString();
        }),
        new DynamicVar("ExtraDamageChance", 3),
        new DynamicVar("ExtraGoldChance", 5),
        new GoldVar(30),
        new DamageVar(5M, ValueProp.Unpowered)
    ];

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay?.Card.Owner is not { } player || cardPlay?.Card.CanonicalInstance != this.Card.CanonicalInstance) return;
        if (this.RollChance(Card.Owner, DynamicVars["ExtraDamageChance"].IntValue))
            await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), player.Creature, DynamicVars.Damage.BaseValue, player.Creature, null);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        if (cardPlay?.Card.Owner is not { } player) return;
        
        if (this.RollChance(Card.Owner, DynamicVars["ExtraGoldChance"].IntValue))
            await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, player);
    }
}