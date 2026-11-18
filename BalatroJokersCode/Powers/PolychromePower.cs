using BalatroJokers.BalatroJokersCode.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Powers;

public class PolychromePower() : BalatroJokersPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("CurrentMult", 1M)];

    private Task ModifyPowerAmount()
    {
        var player = Owner.Player;
        
        if (player?.RunState.CurrentRoom is not CombatRoom room) return Task.CompletedTask;

        var amount = player.Relics.Count(r => r.CanonicalInstance == ModelDb.Relic<PolychromeJimbo>());
        
        DynamicVars["CurrentMult"].BaseValue = (decimal)Math.Pow((double)1.25M, amount);
        return Task.CompletedTask;
    }

    public override Task AfterPotionUsed(PotionModel potion, Creature? target) => ModifyPowerAmount();

    public override Task AfterApplied(Creature? applier, CardModel? cardSource) => ModifyPowerAmount();

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return dealer != this.Owner && !this.Owner.Pets.Contains<Creature>(dealer) ||
               !props.IsPoweredAttack() || cardSource == null ? 1M : DynamicVars["CurrentMult"].BaseValue;
    }
}