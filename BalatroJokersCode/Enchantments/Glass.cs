using BalatroJokers.BalatroJokersCode.Interfaces;
using BaseLib.Cards.Variables;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Glass() : BalatroJokersEnchantment, IChance
{
    public override bool ShowAmount => false;
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card) => 
        base.CanEnchant(card) && card.Type == CardType.Attack;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DisplayVar<Glass>("BaseChance", e =>
        {
            var card = e.IsCanonical ? null : e.Card;
            if (card == null) return e.GetNumerator(null).ToString();
            var player = card.IsCanonical ? null : card.Owner;
            return e.GetNumerator(player).ToString();
        }),
        new DynamicVar("ExhaustChance", 4M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromKeyword(CardKeyword.Exhaust) ];

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay,
        ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card.Owner.Creature != this.Card.Owner.Creature)
            return (pileType, position);
        
        return this.RollChance(this.Card.Owner, DynamicVars["ExhaustChance"].IntValue) ? (PileType.Exhaust, position) : (pileType, position);
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        return !props.IsPoweredAttack() ? 1M : 2M;
    }
}