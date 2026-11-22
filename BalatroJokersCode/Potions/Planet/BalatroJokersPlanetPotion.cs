using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace BalatroJokers.BalatroJokersCode.Potions;

public abstract class BalatroJokersPlanetPotion : BalatroJokersPotion
{
    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.Self;

    protected abstract IEnumerable<CardModel>? GetCardsForUpgrade();

    protected override Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var cardModels = GetCardsForUpgrade();
        if (cardModels == null) return Task.CompletedTask;
        foreach (CardModel card in cardModels)
            CardCmd.Upgrade(card, CardPreviewStyle.GridLayout);
        return Task.CompletedTask;
    }
}
