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
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class HalfJoker() : BalatroJokersRelic
{
    private int _cardsPlayedThisTurn;
    
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => this._cardsPlayedThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new ReadOnlyCollection<DynamicVar>(new List<DynamicVar>
        {
            new DynamicVar("CardThreshold", 3),
            new DamageVar(5M, ValueProp.Unpowered)
        });
    
    private bool IsLessThanThreshold => this._cardsPlayedThisTurn <= this.DynamicVars["CardThreshold"].BaseValue;

    private void RefreshCounter()
    {
        this.Status = IsLessThanThreshold ? RelicStatus.Active : RelicStatus.Normal;
        this.InvokeDisplayAmountChanged();
    }


    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != this.Owner || !CombatManager.Instance.IsInProgress)
            return Task.CompletedTask;
        ++this._cardsPlayedThisTurn;
        this.RefreshCounter();
        return Task.CompletedTask;
    }
    
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains<Creature>(this.Owner.Creature))
            return;
        ICombatState? combatState = Owner.Creature.CombatState;
        var ownerPlayerCombatState = this.Owner.PlayerCombatState;
        if (ownerPlayerCombatState != null && !IsLessThanThreshold)
            return;
        this.Flash();
        VfxCmd.PlayOnCreatureCenters((IEnumerable<Creature>) combatState.HittableEnemies, "vfx/vfx_attack_slash");
        IEnumerable<DamageResult> damageResults = await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>) combatState.HittableEnemies, this.DynamicVars.Damage, this.Owner.Creature);
    }
    
    public override Task AfterCombatEnd(CombatRoom _)
    {
        this._cardsPlayedThisTurn = 0;
        this.Status = RelicStatus.Normal;
        this.InvokeDisplayAmountChanged();

        return Task.CompletedTask;
    }
}