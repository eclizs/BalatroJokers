using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
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
        Block,
        Both
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
            (decimal)(this.Card.GainsBlock
                ? this.Card.Type == CardType.Attack ? EnchantedCardType.Both : EnchantedCardType.Block
                : EnchantedCardType.Damage);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this.Card)
            return Task.CompletedTask;
        
        // These cards store Nova's growth in their native counters instead.
        if (this.Card is not (TheScythe or GeneticAlgorithm))
            this.ExtraAmount += this.Amount;

        if (this.Card is TheScythe theScytheCardVersion)
        {
            int previousCurrentDamage = theScytheCardVersion.CurrentDamage;
            theScytheCardVersion.IncreasedDamage += this.Amount;
            theScytheCardVersion.CurrentDamage =
                ModelDb.Card<TheScythe>().CurrentDamage + theScytheCardVersion.IncreasedDamage;
            MainFile.Logger.Info(
                $"Nova: {theScytheCardVersion} (Card Version) increased CurrentDamage from {previousCurrentDamage} to {theScytheCardVersion.CurrentDamage}.");
        }

        if (this.Card is GeneticAlgorithm geneticAlgorithmCardVersion)
        {
            int previousCurrentBlock = geneticAlgorithmCardVersion.CurrentBlock;
            geneticAlgorithmCardVersion.IncreasedBlock += this.Amount;
            geneticAlgorithmCardVersion.CurrentBlock =
                ModelDb.Card<GeneticAlgorithm>().CurrentBlock + geneticAlgorithmCardVersion.IncreasedBlock;
            MainFile.Logger.Info(
                $"Nova: {geneticAlgorithmCardVersion} (Card Version) increased CurrentBlock from {previousCurrentBlock} to {geneticAlgorithmCardVersion.CurrentBlock}.");
        }
        
        CardModel? deckVersion = this.Card.DeckVersion;
        if (deckVersion != null)
        {
            // Special case for The Scythe and Genetic Algorithm
            switch (deckVersion)
            {
                case TheScythe theScytheDeckVersion:
                {
                    int previousCurrentDamage = theScytheDeckVersion.CurrentDamage;
                    theScytheDeckVersion.IncreasedDamage += this.Amount;
                    theScytheDeckVersion.CurrentDamage =
                        ModelDb.Card<TheScythe>().CurrentDamage + theScytheDeckVersion.IncreasedDamage;
                    MainFile.Logger.Info(
                        $"Nova: {theScytheDeckVersion} (Deck Version) increased CurrentDamage from {previousCurrentDamage} to {theScytheDeckVersion.CurrentDamage}.");
                    return Task.CompletedTask;
                }
                case GeneticAlgorithm geneticAlgorithmDeckVersion:
                {
                    int previousCurrentBlock = geneticAlgorithmDeckVersion.CurrentBlock;
                    geneticAlgorithmDeckVersion.IncreasedBlock += this.Amount;
                    geneticAlgorithmDeckVersion.CurrentBlock =
                        ModelDb.Card<GeneticAlgorithm>().CurrentBlock + geneticAlgorithmDeckVersion.IncreasedBlock;
                    MainFile.Logger.Info(
                        $"Nova: {geneticAlgorithmDeckVersion} (Deck Version) increased CurrentBlock from {previousCurrentBlock} to {geneticAlgorithmDeckVersion.CurrentBlock}.");
                    return Task.CompletedTask;
                }
            }

            switch (this.DynamicVars["CardType"].IntValue)
            {
                case (int)EnchantedCardType.Block:
                    deckVersion.DynamicVars.Block.UpgradeValueBy(this.Amount);
                    deckVersion.DynamicVars.Block.FinalizeUpgrade();
                    break;
                case (int)EnchantedCardType.Damage:
                    deckVersion.DynamicVars.Damage.UpgradeValueBy(this.Amount);
                    deckVersion.DynamicVars.Damage.FinalizeUpgrade();
                    break;
                case (int)EnchantedCardType.Both:
                    deckVersion.DynamicVars.Block.UpgradeValueBy(this.Amount);
                    deckVersion.DynamicVars.Block.FinalizeUpgrade();
                    deckVersion.DynamicVars.Damage.UpgradeValueBy(this.Amount);
                    deckVersion.DynamicVars.Damage.FinalizeUpgrade();
                    break;
            }
        }
        return Task.CompletedTask;
    }
    
    public override decimal EnchantBlockAdditive(decimal originalBlock)
    {
        return this.Card is TheScythe or GeneticAlgorithm ? 0M : this.ExtraAmount;
    }

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        return this.Card is TheScythe or GeneticAlgorithm || !props.IsPoweredAttack() ? 0M : this.ExtraAmount;
    }
}
