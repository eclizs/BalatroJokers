using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Potions.Planet;

public class Mercury() : BalatroJokersPlanetPotion
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override IEnumerable<CardModel>? GetCardsForUpgrade()
    {
        IEnumerable<CardModel> cards;
        int i = 0;
        List<CardModel> cardsForUpgrade;
        do
        {
            bool chooseRarity = Owner.PlayerRng.NonCombatGeneration.NextBool();

            List<CardRarity> cardRarities = [.. PileType.Deck.GetPile(Owner).Cards.Select(c => c.Rarity).Distinct()];
            List<CardType> cardTypes = [.. PileType.Deck.GetPile(Owner).Cards.Select(c => c.Type).Distinct()];

            if (chooseRarity)
            {
                CardRarity rarity = Owner.RunState.Rng.Niche.NextItem(cardRarities);
                cards = PileType.Deck.GetPile(Owner).Cards.Where(c => c.Rarity == rarity && c.MaxUpgradeLevel > 0)
                    .ToList()
                    .StableShuffle(Owner.PlayerRng.NonCombatGeneration)
                    .Take(DynamicVars.Cards.IntValue);
            }

            CardType type = Owner.RunState.Rng.Niche.NextItem(cardTypes);
            cards = PileType.Deck.GetPile(Owner).Cards.Where(c => c.Type == type && c.MaxUpgradeLevel > 0).ToList()
                .StableShuffle(Owner.PlayerRng.NonCombatGeneration)
                .Take(DynamicVars.Cards.IntValue);
            i++;
            cardsForUpgrade = [.. cards];
        } while (cardsForUpgrade.Count < 2 && i <= 10);

        return cardsForUpgrade.Count != 2 ? null : cardsForUpgrade;
    }
}