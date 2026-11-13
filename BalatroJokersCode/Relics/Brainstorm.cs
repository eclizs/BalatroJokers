using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Brainstorm() : BalatroJokersRelic
{
    private ModelId? _copiedRelic;
    private List<IHoverTip> _extraHoverTips = [];
    
    [SavedProperty]
    private ModelId? CopiedRelic
    {
        get => _copiedRelic;
        set
        {
            AssertMutable();
            _copiedRelic = !(this._copiedRelic != null) ? value : throw new InvalidOperationException("Recursive Core setup called twice!");
            if (!(this._copiedRelic != null))
                return;
            RelicModel relicModel = SaveUtil.RelicOrDeprecated(this._copiedRelic);
            _extraHoverTips.AddRange(relicModel.HoverTips);
            ((StringVar)base.DynamicVars["StarterRelic"]).StringValue = relicModel.Title.GetFormattedText();
        }
    }
    
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => this._extraHoverTips;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StringVar("StarterRelic")];

    protected override bool IsAllowedInternal(IRunState runState)
    {
        return runState.CurrentActIndex >= 1;
    }
    
    public override async Task AfterObtained()
    {
        if (Owner.Relics.Count < 2)
        {
            await RelicCmd.Remove(this);
            return;
        }

        var relicToCopy = Owner.Relics.First();

        CopiedRelic = relicToCopy.Id;
        
        ModelId sourceId = relicToCopy.CanonicalInstance.Id;
        RelicModel? canonical = sourceId == ModelId.none ? null : ModelDb.GetByIdOrNull<RelicModel>(sourceId);
        
        if (canonical == null) return;
        
        var newRelic = canonical.ToMutable();
        await RewardsCmd.OfferCustom(this.Owner, [
            (Reward)new RelicReward(newRelic, this.Owner)
        ]);
    }
}
