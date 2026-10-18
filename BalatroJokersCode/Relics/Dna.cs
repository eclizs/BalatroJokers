using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Dna() : BalatroJokersRelic
{
    private int _cardsPlayedThisTurn;
    private const int CombatThreshold = 2;
    private int _combatsLeft = 2;
    private CardModel? _lastCardPlayed;
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override bool IsUsedUp => this.CombatsLeft <= 0;
    
    public override bool ShowCounter => true;

    public override int DisplayAmount => this._combatsLeft;

    private int CardsPlayedThisTurn
    {
        get => this._cardsPlayedThisTurn;
        set
        {
            this.AssertMutable();
            this._cardsPlayedThisTurn = value;
        }
    }
    
    [SavedProperty]
    private int CombatsLeft
    {
        get => this._combatsLeft;
        set
        {
            this.AssertMutable();
            this._combatsLeft = value;
            this.DynamicVars["CombatsLeft"].BaseValue = this._combatsLeft;
            this.InvokeDisplayAmountChanged();
            if (!this.IsUsedUp)
                return;
            this.Status = RelicStatus.Disabled;
        }
    }

    private CardModel? LastCardPlayed
    {
        get => this._lastCardPlayed;
        set
        {
            this.AssertMutable();
            this._lastCardPlayed = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CombatThreshold", CombatThreshold),
        new DynamicVar("CardThreshold", 1M),
        new DynamicVar("CombatsLeft", this.CombatsLeft)
    ];

    private void RefreshCounter()
    {
        if (this.IsUsedUp) this.Status = RelicStatus.Disabled;
        else this.Status = (this.CardsPlayedThisTurn <= this.DynamicVars["CardThreshold"].BaseValue && CombatManager.Instance.IsInProgress) ? RelicStatus.Active : RelicStatus.Normal;
    }
    
    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != this.Owner || !CombatManager.Instance.IsInProgress || this.IsUsedUp)
            return Task.CompletedTask;
        ++this.CardsPlayedThisTurn;
        this.RefreshCounter();
        this.LastCardPlayed = cardPlay.Card;
        return Task.CompletedTask;
    }
    
    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature) || this.IsUsedUp)
            return;
        if (this.CardsPlayedThisTurn == this.DynamicVars["CardThreshold"].BaseValue)
        {
            this.Flash();
            
            CardModel? lastCardPlayed = this.LastCardPlayed;
            if (lastCardPlayed == null) return;
            
            CardModel clonedCard = this.Owner.RunState.CloneCard(lastCardPlayed);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(clonedCard, PileType.Deck, clonedBy: (AbstractModel) this));

            CardModel? addCard = this.Owner.Creature.CombatState?.CreateCard(lastCardPlayed.CanonicalInstance, this.Owner);
            if(addCard == null) return;
            if (lastCardPlayed.IsUpgraded) CardCmd.Upgrade(addCard);
            await CardPileCmd.AddGeneratedCardToCombat(addCard, PileType.Hand, this.Owner);
            
            this.CombatsLeft--;
        }
        this.CardsPlayedThisTurn = 0;
        this.RefreshCounter();
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        this.CardsPlayedThisTurn = 0;
        this.RefreshCounter();
        return Task.CompletedTask;
    }

    public override Task AfterActEntered()
    {
        this.CombatsLeft = CombatThreshold;
        return Task.CompletedTask;
    }
}
