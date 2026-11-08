using BalatroJokers.BalatroJokersCode.Powers;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Steel() : BalatroJokersEnchantment
{
    public override bool ShowAmount => false;
    public override bool HasExtraCardText => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new DynamicVar("DamageMult", 1.5M) ];

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        var hand = PileType.Hand.GetPile(this.Card.Owner).Cards;

        if (hand.Contains(this.Card))
        {
            if (Card.Owner.HasPower<SteelPower>()) return Task.CompletedTask;
            PowerCmd.Apply<SteelPower>(new ThrowingPlayerChoiceContext(), this.Card.Owner.Creature, 1M, null, null);
        }
        
        return Task.CompletedTask;
    }
}