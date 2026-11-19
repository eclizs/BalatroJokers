using BalatroJokers.BalatroJokersCode.Interfaces;
using BaseLib.Cards.Variables;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Hallucination() : BalatroJokersRelic, IChance, ITarot
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DisplayVar<Hallucination>("Numerator", r =>
        {
            var canonical = r.IsCanonical ? null : r;
            if (canonical == null) return r.GetNumerator(null).ToString();
            var player = canonical.IsCanonical ? null : canonical.Owner;
            return r.GetNumerator(player).ToString();
        }),
        new DynamicVar("Denominator", 2M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(BalatroJokersHoverTips.Tarot),
        HoverTipFactory.Static(BalatroJokersHoverTips.Joker)
    ];

    public override Task AfterPotionUsed(PotionModel potion, Creature? target) => this.UpdateTarotHistory(potion, Owner);

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not TreasureRoom) return;
        
        if (this.RollChance(Owner, DynamicVars["Denominator"].IntValue))
        {
            Flash();
            PotionProcureResult procure = await this.CreateTarot(Owner);
        }
    }
}