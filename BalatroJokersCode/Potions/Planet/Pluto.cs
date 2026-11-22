using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace BalatroJokers.BalatroJokersCode.Potions;

public class Pluto() : BalatroJokersPlanetPotion
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override IEnumerable<CardModel> GetCardsForUpgrade()
    {
        return PileType.Deck.GetPile(this.Owner).Cards.ToList().StableShuffle(Owner.PlayerRng.NonCombatGeneration).Take(DynamicVars.Cards.IntValue);
    }
}