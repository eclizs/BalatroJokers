using BalatroJokers.BalatroJokersCode.Powers;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Steel() : BalatroJokersEnchantment
{
    public override bool ShowAmount => false;
    public override bool HasExtraCardText => false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ new DynamicVar("DamageMult", 1.5M) ];

    private Task ApplyPower()
    {
        var hand = PileType.Hand.GetPile(this.Card.Owner).Cards;

        if (!hand.Contains(this.Card) || Card.Owner.HasPower<SteelPower>()) return Task.CompletedTask;

        return PowerCmd.Apply<SteelPower>(new ThrowingPlayerChoiceContext(), this.Card.Owner.Creature, 1M, null, null);
    }

    public override Task AfterPotionUsed(PotionModel potion, Creature? target) => ApplyPower();

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (oldPileType == PileType.Hand && card.Pile?.Type == PileType.Play)
            return Task.CompletedTask;
        
        return ApplyPower();
    }
}