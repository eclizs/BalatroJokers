using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class InvisibleJoker() : BalatroJokersRelic
{
    private const int RemainingCombats = 2;
    private int _combatsFinished;
    private bool _gaveRelic;
    
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool ShowCounter => !IsUsedUp;

    public override int DisplayAmount => CombatsFinished;
    
    public override bool IsUsedUp => this.GaveRelic;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("RemainingCombats", RemainingCombats)];

    private int CombatsFinished
    {
        get => _combatsFinished;
        set
        {
            this.AssertMutable();
            _combatsFinished = value;
            this.InvokeDisplayAmountChanged();
        }
    }

    private bool GaveRelic
    {
        get => _gaveRelic;
        set
        {
            this.AssertMutable();
            _gaveRelic = value;
            this.InvokeDisplayAmountChanged();
            if (!this._gaveRelic)
                return;
            this.Status = RelicStatus.Disabled;
        }
    }

    private RelicModel GetRandomRelic()
    {
        if (Owner.Relics.Count < 2)
        {
            RelicCmd.Remove(this);
            return null;
        }

        var relicToCopy = Owner.RunState.Rng.Niche.NextItem(Owner.Relics);
        
        ModelId sourceId = relicToCopy.CanonicalInstance.Id;
        var canonical = sourceId == ModelId.none ? null : ModelDb.GetByIdOrNull<RelicModel>(sourceId);

        var newRelic = canonical?.ToMutable();
        
        return newRelic;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        ++this.CombatsFinished;
        this.DynamicVars["RemainingCombats"].BaseValue = Decimal.Max((Decimal) (RemainingCombats - this.CombatsFinished), 0M);
        return Task.CompletedTask;
    }
    
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != this.Owner || room is not CombatRoom || this.GaveRelic || RemainingCombats - this.CombatsFinished > 0)
            return false;
        RelicModel relicToCopy = GetRandomRelic();
        if (relicToCopy == null) return false;
        
        rewards.Add((Reward) new RelicReward(relicToCopy, player));
            
        return true;
    }
    
    public override Task AfterModifyingRewards()
    {
        this.Flash();
        this.GaveRelic = true;
        return Task.CompletedTask;
    }
}