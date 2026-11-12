using System.Reflection;
using BalatroJokers.BalatroJokersCode.Pools;
using BalatroJokers.BalatroJokersCode.Potions;
using BalatroJokers.BalatroJokersCode.Potions.Tarot;
using BalatroJokers.BalatroJokersCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace BalatroJokers.BalatroJokersCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "BalatroJokers"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        TarotHistory.Initialize();
        Type[] modTypes = Assembly.GetExecutingAssembly().GetTypes();

        foreach (Type relicType in modTypes
                     .Where(type => !type.IsAbstract && typeof(BalatroJokersRelic).IsAssignableFrom(type))
                     .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            MainFile.Logger.Info("Relic: " + relicType.Name ?? "None");
            ModHelper.AddModelToPool(typeof(BalatroJokersRelicPool), relicType);
        }

        foreach (Type potionType in modTypes
                     .Where(type => !type.IsAbstract && typeof(BalatroJokersTarotPotion).IsAssignableFrom(type))
                     .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            MainFile.Logger.Info("Tarot Potion: " + potionType.Name ?? "None");
            ModHelper.AddModelToPool(typeof(BalatroJokersTarotPotionPool), potionType);
        }

        foreach (Type potionType in modTypes
                     .Where(type => !type.IsAbstract && typeof(BalatroJokersPlanetPotion).IsAssignableFrom(type))
                     .OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            MainFile.Logger.Info("Planet potion: " + potionType.Name);
            ModHelper.AddModelToPool(typeof(BalatroJokersPlanetPotionPool), potionType);
        }

        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        Harmony harmony = new(ModId);

        harmony.PatchAll();
    }
}
