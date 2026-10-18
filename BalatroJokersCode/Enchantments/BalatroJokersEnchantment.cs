using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class BalatroJokersEnchantment : CustomEnchantmentModel
{
    protected override string? CustomIconPath
    {
        get
        {
            var path = Path.Join(MainFile.ResPath, "images", "enchantments", $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png");
            return ResourceLoader.Exists(path) ? path : null;
        }
    }
}