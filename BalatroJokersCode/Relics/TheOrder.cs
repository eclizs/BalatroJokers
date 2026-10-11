using BalatroJokers.BalatroJokersCode.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class TheOrder() : BalatroJokersRelic
{
    private int _cardsPlayedInOrder = 0;
    private bool _powerApplied;
    
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override int DisplayAmount => this._cardsPlayedInOrder;
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    private int CardsPlayedInOrder
    {
        get => _cardsPlayedInOrder;
        set
        {
            this.AssertMutable();
            _cardsPlayedInOrder = value;
        }
    }
    
    private bool PowerApplied
    {
        get => _powerApplied;
        set
        {
            this.AssertMutable();
            _powerApplied = value;
        }
    }
    
    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature))
            return Task.CompletedTask;
        MainFile.Logger.Info("ShowCounter is " + (ShowCounter ? "active" : "inactive") + " with the counter being: " + this.DisplayAmount);
        this.CardsPlayedInOrder = 0;
        this.PowerApplied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        MainFile.Logger.Info("ShowCounter is " + (ShowCounter ? "active" : "inactive") + " with the counter being: " + this.DisplayAmount);

        if (cardPlay.Card.Owner != this.Owner || !CombatManager.Instance.IsInProgress)
            return;
        switch (CardsPlayedInOrder)
        {
            case 0:
                if (cardPlay.Card.Type != CardType.Attack)
                {
                    CardsPlayedInOrder = 0;
                    return;
                }
                break;
            case 1:
                if (cardPlay.Card.Type != CardType.Skill)
                {
                    CardsPlayedInOrder = 0;
                    return;
                }
                break;
            case 2:
                if (cardPlay.Card.Type != CardType.Power)
                {
                    CardsPlayedInOrder = 0;
                    return;
                }
                break;
            default: return;
        }
        CardsPlayedInOrder++;
        if (CardsPlayedInOrder == 3)
        {
            if (PowerApplied) return;
            
            PowerApplied = true;
            TripleDamagePower? tripleDamagePower = await PowerCmd.Apply<TripleDamagePower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                1M,
                Owner.Creature,
                null);
        }
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        this.CardsPlayedInOrder = 0;
        this.PowerApplied = false;
        return Task.CompletedTask;
    }
}