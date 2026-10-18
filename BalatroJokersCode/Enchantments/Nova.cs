using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Enchantments;

public class Nova : BalatroJokersEnchantment
{
    [SavedProperty]
    private int _extraAmount = 0;
    
    private enum EnchantedCardType
    {
        None,
        Damage,
        Block
    }
    
    public override bool ShowAmount => true;

    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card) => 
        base.CanEnchant(card) && card.GetKeywordsWithSources(KeywordSources.Local).Contains(CardKeyword.Exhaust) &&
        (card.GainsBlock || card.Type == CardType.Attack);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CardType", (decimal) EnchantedCardType.None)
    ];

    private int ExtraAmount
    {
        get => this._extraAmount;
        set
        {
            this.AssertMutable();
            this._extraAmount = value;
        }
    }

    protected override void OnEnchant()
    {
        this.DynamicVars["CardType"].BaseValue =
            (Decimal) ((this.Card.GainsBlock) ? EnchantedCardType.Block : EnchantedCardType.Damage);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this.Card)
            return Task.CompletedTask;
        this.ExtraAmount += this.Amount;
        CardModel? deckVersion = this.Card.DeckVersion;
        if (deckVersion != null)
        {
            switch (this.DynamicVars["CardType"].IntValue)
            {
                case (int)EnchantedCardType.Block:
                    deckVersion.DynamicVars.Block.UpgradeValueBy(this.Amount - 1);
                    deckVersion.DynamicVars.Block.FinalizeUpgrade();
                    break;
                case (int)EnchantedCardType.Damage:
                    deckVersion.DynamicVars.Damage.UpgradeValueBy(this.Amount - 1);
                    deckVersion.DynamicVars.Damage.FinalizeUpgrade();
                    break;
            }
        }
        return Task.CompletedTask;
    }
    
    public override decimal EnchantBlockAdditive(decimal originalBlock)
    {
        return this.ExtraAmount - 1;
    }

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        return !props.IsPoweredAttack() ? 0M : this.ExtraAmount - 1;
    }
}