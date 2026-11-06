using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using BalatroJokers.BalatroJokersCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace BalatroJokers.BalatroJokersCode.Potions;


[Pool(typeof(SharedPotionPool))]
public abstract class BalatroJokersPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath
    {
        get
        {
            var path = Path.Join(MainFile.ResPath, "images", "potions", $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png");
            return ResourceLoader.Exists(path) ? path : null;
        }
    }
    
}
