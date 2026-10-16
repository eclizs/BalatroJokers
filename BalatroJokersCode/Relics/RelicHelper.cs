using System.Collections.ObjectModel;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Relics;

public abstract class MatchingCardPowerRelic<TPower> : BalatroJokersRelic
    where TPower : PowerModel
{
    private bool _powerApplied;

    protected abstract int RequiredCopies { get; }

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new ReadOnlyCollection<DynamicVar>(
            new List<DynamicVar> { new CardsVar(RequiredCopies) });

    private bool PowerApplied
    {
        get => _powerApplied;
        set
        {
            this.AssertMutable();
            _powerApplied = value;
        }
    }

    private bool HasRequiredCopies()
    {
        return PileType.Hand
            .GetPile(Owner)
            .Cards
            .GroupBy(card => card.CanonicalInstance)
            .Any(group => group.Count() == RequiredCopies);
    }

    private async Task ApplyPower()
    {
        if (this.PowerApplied)
            return;
        PowerApplied = true;
        this.Flash();
        await PowerCmd.Apply<TPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            1M,
            Owner.Creature,
            null);
    }

    private async Task RemovePower()
    {
        if (!this.PowerApplied)
            return;
        PowerApplied = false;
        PowerModel power = Owner.Creature.GetPower<TPower>();
        if (power is null) return;
        await PowerCmd.Decrement(power);
    }

    private async Task ChangePowerStatus()
    {
        if (PowerApplied)
        {
            if (HasRequiredCopies())
                return;

            await RemovePower();
        }
        else if (HasRequiredCopies())
        {
            await ApplyPower();
        }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature)) await ChangePowerStatus();
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        ChangePowerStatus();

    public override Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal) =>
        ChangePowerStatus();

    public override Task AfterCardDiscarded(
        PlayerChoiceContext choiceContext,
        CardModel card) =>
        ChangePowerStatus();

    public override Task AfterCardEnteredCombat(CardModel card) => ChangePowerStatus();

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy) => ChangePowerStatus();

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        PowerApplied = false;
        return Task.CompletedTask;
    }

}