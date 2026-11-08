using BalatroJokers.BalatroJokersCode.Enchantments;
using BalatroJokers.BalatroJokersCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Powers;

public class SteelPower() : BalatroJokersPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("CurrentMult", 1M)];

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Owner != Owner.Player) return Task.CompletedTask;
        
        var hand = PileType.Hand.GetPile(this.Owner.Player).Cards;

        var steelCardsAmount = hand.Count(c => c.Enchantment?.CanonicalInstance == ModelDb.Enchantment<Steel>());

        if (steelCardsAmount == 0)
        {
            PowerCmd.Remove(this);
            return Task.CompletedTask;
        }
        
        DynamicVars["CurrentMult"].BaseValue = (decimal)Math.Pow(1.5, steelCardsAmount);
        
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return dealer != this.Owner && !this.Owner.Pets.Contains<Creature>(dealer) ||
               !props.IsPoweredAttack() || cardSource == null ? 1M : DynamicVars["CurrentMult"].BaseValue;
    }
}