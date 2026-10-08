using System.Collections.ObjectModel;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class MysticSummit() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new ReadOnlyCollection<DynamicVar>(new List<DynamicVar> { new PowerVar<VigorPower>(5M) });
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new ReadOnlyCollection<IHoverTip>(new List<IHoverTip>{HoverTipFactory.FromPower<VigorPower>()});
        }
    }

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler != this.Owner)
            return;
        this.Flash();
        VigorPower? vigorPower = await PowerCmd.Apply<VigorPower>((PlayerChoiceContext) new ThrowingPlayerChoiceContext(), this.Owner.Creature, (Decimal) this.DynamicVars["VigorPower"].IntValue, this.Owner.Creature, null);
    }
}