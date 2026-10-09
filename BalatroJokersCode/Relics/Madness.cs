using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BalatroJokers.BalatroJokersCode.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Madness() : BalatroJokersRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Shop;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new ReadOnlyCollection<DynamicVar>(new List<DynamicVar>
        {
            new PowerVar<MadnessPower>(10M),
            new DynamicVar("AdditionalPercentage", 10M)
        });
    
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (room.RoomType != RoomType.Elite)
            return;
        
        this.DynamicVars["MadnessPower"].BaseValue += this.DynamicVars["AdditionalPercentage"].BaseValue;
        
        RelicModel? relicToRemove;
        do
        {
            relicToRemove = Owner.RunState.Rng.Niche.NextItem(Owner.Relics);
            if (relicToRemove == null || Owner.Relics.Count <= 2) return;
        } while (relicToRemove == this || relicToRemove.Rarity == RelicRarity.Starter);
        
        await RelicCmd.Remove(relicToRemove);
        this.Flash();
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom)
            return;
        this.Flash();
        MadnessPower? madnessPower = await PowerCmd.Apply<MadnessPower>(new ThrowingPlayerChoiceContext(),
            this.Owner.Creature, (Decimal)this.DynamicVars["MadnessPower"].IntValue, this.Owner.Creature, null);
    }
}