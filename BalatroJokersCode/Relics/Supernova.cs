using BalatroJokers.BalatroJokersCode.Enchantments;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class Supernova() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;
    
    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("NovaAmount", 2M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
        [
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
            .. HoverTipFactory.FromEnchantment<Nova>(this.DynamicVars["NovaAmount"].IntValue)
        ];
    
    public override async Task AfterObtained()
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 0, this.DynamicVars.Cards.IntValue)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };
        Nova canonicalEnchantment = ModelDb.Enchantment<Nova>();
        foreach (CardModel card in await CardSelectCmd.FromDeckForEnchantment(this.Owner, (EnchantmentModel) canonicalEnchantment, this.DynamicVars["NovaAmount"].IntValue, prefs))
        {
            CardCmd.Enchant(canonicalEnchantment.ToMutable(), card, (Decimal) this.DynamicVars["NovaAmount"].IntValue);
            CardCmd.Preview(card);
        }

        canonicalEnchantment = null;
    }
}