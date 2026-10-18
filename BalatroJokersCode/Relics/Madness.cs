using BalatroJokers.BalatroJokersCode.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Madness() : BalatroJokersRelic
{
    private int _powerAmount = 10;
    
    private string _lastDestroyedRelicName;
    
    private string _destroyedRelicList;
    
    private List<IHoverTip> _extraHoverTips = [];

    
    public override RelicRarity Rarity => RelicRarity.Shop;
    
    [SavedProperty]
    private int PowerAmount
    {
        get => this._powerAmount;
        set
        {
            this.AssertMutable();
            _powerAmount = value;
            DynamicVars["MadnessPower"].BaseValue = value;
        }
    }
    
    [SavedProperty]
    private string LastDestroyedRelicName
    {
        get => this._lastDestroyedRelicName;
        set
        {
            AssertMutable();
            this._lastDestroyedRelicName = value;
        }
    }

    [SavedProperty]
    private string DestroyedRelicList
    {
        get => this._destroyedRelicList;
        set
        {
            this.AssertMutable();
            this._destroyedRelicList = value;
            UpdateDestroyedRelicHoverTip();
        }
    }
    
    private void UpdateDestroyedRelicHoverTip()
    {
        LocString description = new(
            "relics",
            "BALATROJOKERS-MADNESS.destroyedRelic");

        description.Add("RelicList", _destroyedRelicList ?? "");

        var updatedTip = new HoverTip(description);

        if (_extraHoverTips.Count == 0)
        {
            _extraHoverTips.Add(updatedTip);
        }
        else
        {
            _extraHoverTips[0] = updatedTip;
        }
    }
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => this._extraHoverTips;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<MadnessPower>(this.PowerAmount),
        new DynamicVar("AdditionalPercentage", 10M)
    ];
    
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (room.RoomType != RoomType.Elite)
            return;
        
        this.PowerAmount += this.DynamicVars["AdditionalPercentage"].IntValue;
        
        RelicModel? relicToRemove;
        int i = 0;
        do
        {
            relicToRemove = Owner.RunState.Rng.Niche.NextItem(Owner.Relics);
            if (relicToRemove == null || Owner.Relics.Count <= 2) return;
            if (i++ >= 20) return;
        } while (relicToRemove == this || relicToRemove.Rarity == RelicRarity.Starter);

        this.LastDestroyedRelicName = relicToRemove.Title.GetFormattedText();
        this.DestroyedRelicList += "- [gold]" + this.LastDestroyedRelicName + "[/gold]\n";
        
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
