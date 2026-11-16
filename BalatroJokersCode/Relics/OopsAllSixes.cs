using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

// Relic doesn't do anything by itself; IChance checks if a player has this relic
[Pool(typeof(SharedRelicPool))]
public class OopsAllSixes() : BalatroJokersRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Shop;
}