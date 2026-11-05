using BalatroJokers.BalatroJokersCode.Powers;
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

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class JokerStencil() : BalatroJokersRelic
{
    private bool _powerApplied;

    private int _lastHandCount;
    
    private bool PowerApplied
    {
        get => _powerApplied;
        set
        {
            this.AssertMutable();
            _powerApplied = value;
        }
    }

    private int LastHandCount
    {
        get => _lastHandCount;
        set
        {
            this.AssertMutable();
            _lastHandCount = value;
        }
    }

    private async Task ChangePowerAmount()
    {
        var cards = PileType.Hand.GetPile(this.Owner).Cards;
        decimal additionalAmount = (LastHandCount - cards.Count) * DynamicVars["PercentageDecrease"].BaseValue;
        LastHandCount = cards.Count;

        if (!PowerApplied)
        {
            PowerApplied = true;
            JokerStencilPower? jokerStencilPower1 = await PowerCmd.Apply<JokerStencilPower>(
                new ThrowingPlayerChoiceContext(),
                this.Owner.Creature,
                DynamicVars["InitialPercentage"].BaseValue,
                this.Owner.Creature,
                null
            );
        }
        
        JokerStencilPower? jokerStencilPower2 = await PowerCmd.Apply<JokerStencilPower>(
            new ThrowingPlayerChoiceContext(),
            this.Owner.Creature,
            additionalAmount,
            this.Owner.Creature,
            null
        );
    }
    
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new("InitialPercentage", 30M),
        new("PercentageDecrease", 3m)
    ];

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom)
            return Task.CompletedTask;
        
        LastHandCount = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || this.Owner.PlayerCombatState.TurnNumber > 1)
            return;
        
        await ChangePowerAmount();
    }
    
    public override async Task AfterCardEnteredCombat(CardModel card) => await ChangePowerAmount();

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (oldPileType == PileType.Hand && card.Pile?.Type == PileType.Play)
            return;
        
        await ChangePowerAmount();
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        PowerApplied = false;
        LastHandCount = 0;
        return Task.CompletedTask;
    }
}