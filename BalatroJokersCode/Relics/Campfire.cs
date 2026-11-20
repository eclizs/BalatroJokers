using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Campfire() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public static int GetAmount(Player player)
    {
        return 1 + player.Relics.OfType<Campfire>().Count(r => !r.IsMelted);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

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
