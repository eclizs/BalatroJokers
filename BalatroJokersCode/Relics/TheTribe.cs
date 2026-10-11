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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class TheTribe() : BalatroJokersRelic
{
    private int _cardsPlayed = 0;
    private CardType _cardTypeToPlay;
    private CardRarity _cardRarityToPlay;
    private bool _powerApplied;
    
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    
    public override int DisplayAmount => this._cardsPlayed;

    private int CardsPlayed
    {
        get => _cardsPlayed;
        set
        {
            this.AssertMutable();
            _cardsPlayed = value;
        }
    }
    
    private CardType CardTypeToPlay
    {
        get => _cardTypeToPlay;
        set
        {
            this.AssertMutable();
            _cardTypeToPlay = value;
        }
    }
    
    private CardRarity CardRarityToPlay
    {
        get => _cardRarityToPlay;
        set
        {
            this.AssertMutable();
            _cardRarityToPlay = value;
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

    protected override IEnumerable<DynamicVar> CanonicalVars => 
        new ReadOnlyCollection<DynamicVar>(new List<DynamicVar> { new CardsVar(3) });

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature))
            return Task.CompletedTask;
        MainFile.Logger.Info("ShowCounter is " + (ShowCounter ? "active" : "inactive") + " with the counter being: " + this.DisplayAmount);
        this.CardsPlayed = 0;
        this.CardTypeToPlay = CardType.None;
        this.CardRarityToPlay = CardRarity.None;
        this.PowerApplied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        MainFile.Logger.Info("ShowCounter is " + (ShowCounter ? "active" : "inactive") + " with the counter being: " + this.DisplayAmount);

        if (cardPlay.Card.Owner != this.Owner || !CombatManager.Instance.IsInProgress)
            return;

        if (cardPlay.Card.Rarity == this.CardRarityToPlay || cardPlay.Card.Type == this.CardTypeToPlay)
        {
            this.CardsPlayed++;
            if (cardPlay.Card.Rarity == this.CardRarityToPlay) MainFile.Logger.Info("card of the same rarity was played!");
            else if (cardPlay.Card.Type == this.CardTypeToPlay) MainFile.Logger.Info("card of the same type was played!");
        }
        else
        {
            this.CardsPlayed = 1;
            this.CardTypeToPlay = cardPlay.Card.Type;
            if (cardPlay.Card.Type != this.CardTypeToPlay)
            {
                MainFile.Logger.Info("a different card type was played!");
            }
            
            this.CardRarityToPlay = cardPlay.Card.Rarity;
            if (cardPlay.Card.Rarity != this.CardRarityToPlay)
            {
                MainFile.Logger.Info("a different card rarity was played!");
            }
        }

        if (CardsPlayed == 3)
        {
            if (PowerApplied) return;
            
            this.PowerApplied = true;
            DoubleDamagePower? doubleDamagePower = await PowerCmd.Apply<DoubleDamagePower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                1M,
                Owner.Creature,
                null);
        }
    }
    
    public override Task AfterCombatEnd(CombatRoom _)
    {
        this.CardsPlayed = 0;
        this.CardTypeToPlay = CardType.None;
        this.CardRarityToPlay = CardRarity.None;
        this.PowerApplied = false;
        return Task.CompletedTask;
    }
}