using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace BalatroJokers.BalatroJokersCode.Potions.Tarot;

public class TheWorld() : TransformingTarot
{
    protected override CardType TransformType => CardType.None;
    protected override int CardAmount => 1;

    protected override bool CardIsValid(CardModel card)
    {
        return card.Pool == ModelDb.CardPool<ColorlessCardPool>();
    }

    protected override CardModel GetCanonicalForTransform(CardModel original)
    {
        var card = CardFactory.CreateRandomCardForTransform(original,
            ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                .Where(CardIsValid), true, Owner.RunState.Rng.CombatCardGeneration);

        return card.CanonicalInstance ?? original.CanonicalInstance;
    }
}