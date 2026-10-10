using System.Collections.ObjectModel;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class TheDuo() : BalatroJokersRelic
{
    private bool _powerApplied = false;
    public override RelicRarity Rarity => RelicRarity.Rare;

    private bool PowerApplied
    {
        get => this._powerApplied;
        set
        {
            this.AssertMutable();
            this._powerApplied = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new ReadOnlyCollection<DynamicVar>(new List<DynamicVar> { new CardsVar(2) });

    private bool HasTwoCopies()
    {
        List<CardModel> uniqueCards = [];
        foreach (CardModel card in PileType.Hand.GetPile(this.Owner).Cards)
        {
            if (uniqueCards.Contains(card.CanonicalInstance)) return true;
            uniqueCards.Add(card.CanonicalInstance);
        }

        return false;
    }

    private async Task ChangePowerStatus()
    {
        if (PowerApplied)
        {
            if (HasTwoCopies()) return;
            PowerApplied = false;
            await PowerCmd.Remove(this.Owner.Creature.GetPower<DoubleDamagePower>());
        }
        else
        {
            if (!HasTwoCopies()) return;
            PowerApplied = true;
            DoubleDamagePower doubleDamagePower = await PowerCmd.Apply<DoubleDamagePower>(
                (PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner.Creature, 1M, this.Owner.Creature,
                (CardModel)null);
        }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature) || !HasTwoCopies())
            return;
        PowerApplied = true;
        DoubleDamagePower doubleDamagePower = await PowerCmd.Apply<DoubleDamagePower>(
            (PlayerChoiceContext)new ThrowingPlayerChoiceContext(), this.Owner.Creature, 1M, this.Owner.Creature,
            (CardModel)null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ChangePowerStatus();
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        await ChangePowerStatus();
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        await ChangePowerStatus();
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        PowerApplied = false;
        return Task.CompletedTask;
    }
}