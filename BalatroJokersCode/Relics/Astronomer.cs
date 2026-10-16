using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Astronomer() : BalatroJokersRelic
{
    private MerchantInventory? _populatedInventory;
    private HashSet<MerchantEntry> _freeEntries = [];
    
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _freeEntries = [];
        _populatedInventory = null;
    }

    public void PopulateFreeEntries(MerchantInventory inventory)
    {
        if (ReferenceEquals(_populatedInventory, inventory))
            return;

        _populatedInventory = inventory;
        _freeEntries.Clear();

        var alreadyFree = inventory.Player.Relics.OfType<Astronomer>()
            .Where(relic => ReferenceEquals(relic._populatedInventory, inventory))
            .SelectMany(relic => relic._freeEntries)
            .ToHashSet();

        AddRandom(inventory.CharacterCardEntries.Concat(inventory.ColorlessCardEntries), alreadyFree);
        AddRandom(inventory.RelicEntries, alreadyFree);
        AddRandom(inventory.PotionEntries, alreadyFree);
    }

    private void AddRandom<T>(IEnumerable<T> entries, HashSet<MerchantEntry> alreadyFree)
        where T : MerchantEntry
    {
        var availableEntries = entries.Where(entry => entry.IsStocked && !alreadyFree.Contains(entry)).ToList();
        if (availableEntries.Count == 0)
            return;

        int index = Owner.PlayerRng.Shops.NextInt(availableEntries.Count);
        _freeEntries.Add(availableEntries[index]);
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is MerchantRoom) this.Flash();
        return Task.CompletedTask;
    }

    public override Decimal ModifyMerchantPrice(
        Player player,
        MerchantEntry entry,
        Decimal originalPrice)
    {
        if (player != Owner)
            return originalPrice;

        return _freeEntries.Contains(entry) ? 0M : originalPrice;
    }
    
    public override Task AfterItemPurchased(
        Player player,
        MerchantEntry itemPurchased,
        int goldSpent)
    {
        if (player == Owner)
            _freeEntries.Remove(itemPurchased);

        return Task.CompletedTask;
    }
}
