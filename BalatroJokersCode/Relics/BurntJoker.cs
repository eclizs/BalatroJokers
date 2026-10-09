using System.Threading.Tasks;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SilentRelicPool))]
public class BurntJoker() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Owner != this.Owner || this.Owner.Creature.Side != this.Owner.Creature.CombatState.CurrentSide)
            return Task.CompletedTask;
        this.Flash();
        CardCmd.Upgrade(card);
        return Task.CompletedTask;
    }
}