using System.Collections.ObjectModel;
using BalatroJokers.BalatroJokersCode.Relics;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class DriversLicense() : BalatroJokersRelic
{
    private bool _powerApplied;
    
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    private bool PowerApplied
    {
        get => this._powerApplied;
        set
        {
            this.AssertMutable();
            this._powerApplied = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            return new ReadOnlyCollection<DynamicVar>(new List<DynamicVar>
            {
                new PowerVar<StrengthPower>(2M), 
                new PowerVar<DexterityPower>(2M)
            });
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            return new ReadOnlyCollection<IHoverTip>(new List<IHoverTip>
            {
                HoverTipFactory.FromPower<StrengthPower>(),
                HoverTipFactory.FromPower<DexterityPower>()
            });
        }
    }
}