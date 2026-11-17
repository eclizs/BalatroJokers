using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(EventRelicPool))]
public class PolychromeJimbo() : BalatroJokersRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    
}