using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Gold() : BalatroJokersEnchantment
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => true;

    public static bool IsActive(CardModel? card)
    {
        if (card is null) return false;

        return card.Enchantment?.CanonicalInstance == ModelDb.Enchantment<Gold>();
    }
}