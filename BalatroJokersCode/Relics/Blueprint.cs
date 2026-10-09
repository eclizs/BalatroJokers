using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Blueprint() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        if (Owner.Relics.Count < 2) await RelicCmd.Remove(this);

        var relicToCopy = Owner.Relics[^2];
        ModelId sourceId = relicToCopy.CanonicalInstance.Id;      
        RelicModel? canonical;
        canonical = sourceId == ModelId.none ? null : ModelDb.GetByIdOrNull<RelicModel>(sourceId);

        if (canonical == null) await Task.CompletedTask;
        
        var newRelic = canonical.ToMutable();
        await RewardsCmd.OfferCustom(this.Owner, [
            (Reward)new RelicReward(newRelic, this.Owner)
        ]);
    }
}