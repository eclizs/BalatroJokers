using BalatroJokers.BalatroJokersCode.Powers;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(EventRelicPool))]
public class PolychromeJimbo() : BalatroJokersRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Mult", 1.25M)];
    
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (!(room is CombatRoom))
            return;
        Flash();

        if (Owner.HasPower<PolychromePower>()) return;
        PolychromePower? polychromePower =
            await PowerCmd.Apply<PolychromePower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1M, null, null);
    }

    public override async Task AfterObtained()
    {
        if (Owner.RunState.CurrentRoom is not CombatRoom) return;
        
        Flash();

        if (Owner.HasPower<PolychromePower>()) return;
        PolychromePower? polychromePower =
            await PowerCmd.Apply<PolychromePower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1M, null, null);
    }
}