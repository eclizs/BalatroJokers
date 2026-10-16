using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Blueprint() : BalatroJokersRelic
{
    private List<IHoverTip> _extraHoverTips = [];
    
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override bool HasUponPickupEffect => true;
    
    private void SetupDynamicText(ModelId relicId)
    {
        RelicModel relicModel = SaveUtil.RelicOrDeprecated(relicId);
        _extraHoverTips.AddRange(relicModel.HoverTips);
        ((StringVar)base.DynamicVars["Relic"]).StringValue = relicModel.Title.GetFormattedText();
    }
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => this._extraHoverTips;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new ReadOnlyCollection<DynamicVar>(new List<DynamicVar> { new StringVar("Relic") });

    public override async Task AfterObtained()
    {
        if (Owner.Relics.Count < 2)
        {
            await RelicCmd.Remove(this);
            return;
        }

        var relicToCopy = Owner.Relics[^2];
        
        SetupDynamicText(relicToCopy.Id);
        
        ModelId sourceId = relicToCopy.CanonicalInstance.Id;      
        RelicModel? canonical;
        canonical = sourceId == ModelId.none ? null : ModelDb.GetByIdOrNull<RelicModel>(sourceId);

        if (canonical == null) return;
        
        var newRelic = canonical.ToMutable();
        await RewardsCmd.OfferCustom(this.Owner, [
            (Reward)new RelicReward(newRelic, this.Owner)
        ]);
    }
}