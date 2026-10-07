using System.Collections.ObjectModel;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class GoldenTicket() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new ReadOnlyCollection<DynamicVar>(new List<DynamicVar> { new GoldVar(4) });
        }
    }
    
    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != this.Owner || cardPlay.Card.Rarity != CardRarity.Rare)
            return;
        this.Flash();
        await PlayerCmd.GainGold(this.DynamicVars.Gold.IntValue, this.Owner);
    }
}