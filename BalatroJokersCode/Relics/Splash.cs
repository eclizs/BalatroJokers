using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers.Mocks;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Splash() : BalatroJokersRelic
{
    private int _cardsPlayedThisTurn;
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => _cardsPlayedThisTurn;

    public int CardsPlayedThisTurn
    {
        get => _cardsPlayedThisTurn;
        set
        {
            this.AssertMutable();
            _cardsPlayedThisTurn = value;
            this.InvokeDisplayAmountChanged();
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
        
        this.CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != this.Owner || !CombatManager.Instance.IsInProgress)
            return;
        this.CardsPlayedThisTurn++;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        this.CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }
    
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature) || this.CardsPlayedThisTurn == 0)
            return;
        Creature target = this.Owner.RunState.Rng.CombatTargets.NextItem<Creature>((IEnumerable<Creature>) this.Owner.Creature.CombatState.HittableEnemies);
        if (target == null)
            return;
        VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_blunt");
        
        DamageVar damage = new DamageVar((Decimal) this.CardsPlayedThisTurn, ValueProp.Unpowered);
        IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, target, damage, this.Owner.Creature);
    }
}