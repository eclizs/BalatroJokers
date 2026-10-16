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
    
    private enum MatchKind
    {
        None,
        Type,
        Rarity
    }

    private MatchKind _matchKind;
    
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
            this.InvokeDisplayAmountChanged();
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

        this.CardsPlayed = 0;
        this.CardTypeToPlay = CardType.None;
        this.CardRarityToPlay = CardRarity.None;
        this.PowerApplied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != this.Owner || !CombatManager.Instance.IsInProgress)
            return;

        CardType type = cardPlay.Card.Type;
        CardRarity rarity = cardPlay.Card.Rarity;

        switch (CardsPlayed)
        {
            case 0:
                CardsPlayed = 1;
                CardTypeToPlay = type;
                CardRarityToPlay = rarity;
                _matchKind = MatchKind.None;
                return;
            case 1:
                bool sameType = type == CardTypeToPlay;
                bool sameRarity = rarity == CardRarityToPlay;

                if (!sameType && !sameRarity)
                {
                    CardsPlayed = 1;
                    CardTypeToPlay = type;
                    CardRarityToPlay = rarity;
                    return;
                }

                _matchKind = sameType ? MatchKind.Type : MatchKind.Rarity;
                CardsPlayed = 2;
                break;
            default:
                if (_matchKind == MatchKind.Type && type == CardTypeToPlay ||
                    _matchKind == MatchKind.Rarity && rarity == CardRarityToPlay)
                {
                    CardsPlayed = 3;
                }
                else
                {
                    CardsPlayed = 1;
                    CardTypeToPlay = type;
                    CardRarityToPlay = rarity;
                    _matchKind = MatchKind.None;
                    return;
                }
                break;
        }

        if (CardsPlayed == 3 && !PowerApplied)
        {
            PowerApplied = true;
            await PowerCmd.Apply<DoubleDamagePower>(
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