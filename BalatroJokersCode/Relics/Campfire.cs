using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Campfire() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override Task AfterRestSiteSmith(Player player)
    {
        if (player != this.Owner)
            return Task.CompletedTask;
        this.Flash();
        this.Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }
    
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        this.Status = room is RestSiteRoom ? RelicStatus.Active : RelicStatus.Normal;
        return Task.CompletedTask;
    }
}
