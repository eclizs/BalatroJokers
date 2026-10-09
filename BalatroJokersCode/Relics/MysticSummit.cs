using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class MysticSummit() : BalatroJokersRelic
{
    
    private bool _powerApplied;
    
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    
    private bool PowerApplied
    {
        get => this._powerApplied;
        set
        {
            this.AssertMutable();
            this._powerApplied = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new ReadOnlyCollection<DynamicVar>(new List<DynamicVar>
            {
                new PowerVar<StrengthPower>(2M), 
                new PowerVar<DexterityPower>(2M)
            });
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new ReadOnlyCollection<IHoverTip>(new List<IHoverTip>
            {
                HoverTipFactory.FromPower<StrengthPower>(),
                HoverTipFactory.FromPower<DexterityPower>()
            });
        }
    }

    private async Task ApplyPowers()
    {
        if (this.PowerApplied)
            return;
        this.PowerApplied = true;
        this.Flash();
        StrengthPower? strengthPower = await PowerCmd.Apply<StrengthPower>((PlayerChoiceContext) new ThrowingPlayerChoiceContext(), this.Owner.Creature, this.DynamicVars.Dexterity.BaseValue, (Creature) null, (CardModel) null);
        DexterityPower? dexterityPower = await PowerCmd.Apply<DexterityPower>((PlayerChoiceContext) new ThrowingPlayerChoiceContext(), this.Owner.Creature, this.DynamicVars.Dexterity.BaseValue, (Creature) null, (CardModel) null);
    }
    
    private async Task RemovePowers()
    {
        if (!this.PowerApplied)
            return;
        this.PowerApplied = false;
        this.Flash();
        StrengthPower strengthPower = await PowerCmd.Apply<StrengthPower>((PlayerChoiceContext) new ThrowingPlayerChoiceContext(), this.Owner.Creature, -this.DynamicVars.Dexterity.BaseValue, (Creature) null, (CardModel) null);
        DexterityPower dexterityPower = await PowerCmd.Apply<DexterityPower>((PlayerChoiceContext) new ThrowingPlayerChoiceContext(), this.Owner.Creature, -this.DynamicVars.Dexterity.BaseValue, (Creature) null, (CardModel) null);
    }

    private void RefreshStatus()
    {
        if (CombatManager.Instance.IsInProgress && IsDrawPileEmpty)
            this.Status = RelicStatus.Active;
        else
            this.Status = RelicStatus.Normal;
    }

    private async Task ChangePowerStatus()
    {
        this.RefreshStatus();
        if (!CombatManager.Instance.IsInProgress)
            return;
        if (IsDrawPileEmpty) await this.ApplyPowers();
        else await this.RemovePowers();
    }

    private bool IsDrawPileEmpty => PileType.Draw.GetPile(this.Owner).Cards.Count == 0;
    
    public override async Task AfterObtained()
    {
        await this.ChangePowerStatus();
    }
        
    public override Task AfterCombatEnd(CombatRoom room)
    {
        this.RefreshStatus();
        return Task.CompletedTask;
    }
    
    public override Task AfterCombatVictory(CombatRoom room)
    {
        this.PowerApplied = false;
        this.RefreshStatus();
        return Task.CompletedTask;
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await this.ChangePowerStatus();
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        await this.ChangePowerStatus();
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if(side == CombatSide.Player) await this.ChangePowerStatus();
    }
}