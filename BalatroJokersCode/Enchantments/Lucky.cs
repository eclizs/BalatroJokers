using System.Buffers;
using BalatroJokers.BalatroJokersCode.Interfaces;
using BaseLib.Cards.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Lucky() : BalatroJokersEnchantment, IChance
{
    public override bool ShowAmount => false;
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card) && card.Type == CardType.Attack;
    }

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
        new DamageVar(8M, ValueProp.Unpowered)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (this.RollChance(Card.Owner, DynamicVars["ExtraGoldChance"].IntValue))
            await PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, this.Card.Owner);
    }
    
    public override Decimal EnchantDamageAdditive(Decimal originalDamage, ValueProp props)
    {
        return !props.IsPoweredAttack() || this.RollChance(this.Card.Owner, DynamicVars["ExtraDamageChance"].IntValue) == false ? 0M : DynamicVars.Damage.BaseValue;
    }
}