using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using HarmonyLib;
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

[HarmonyPatch(typeof(SmithRestSiteOption), nameof(SmithRestSiteOption.SmithCount), MethodType.Getter)]
class SmithCountPatch
{
    static void Postfix(ref SmithRestSiteOption __instance,ref object __result)
    {
        Player owner = (Player) Traverse.Create(__instance).Property("Owner").GetValue();

        bool playerHasCampfire = owner.Relics.OfType<Campfire>().Any();
        if (playerHasCampfire) __result = 2;
    }
}