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
    private readonly HashSet<MerchantEntry> _freeEntries = [];
    
    public override RelicRarity Rarity => RelicRarity.Rare;

    public void PopulateFreeEntries(MerchantInventory inventory)
    {
        if (ReferenceEquals(_populatedInventory, inventory))
            return;

        _populatedInventory = inventory;
        _freeEntries.Clear();

        AddRandom(inventory.CharacterCardEntries);
        AddRandom(inventory.ColorlessCardEntries);
        AddRandom(inventory.RelicEntries);
        AddRandom(inventory.PotionEntries);
    }

    private void AddRandom<T>(IReadOnlyList<T> entries) where T : MerchantEntry
    {
        if (entries.Count == 0)
            return;

        int index = Owner.PlayerRng.Shops.NextInt(entries.Count);
        _freeEntries.Add(entries[index]);
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is MerchantRoom)
        {
            this.Flash();
        }
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
}