using BalatroJokers.BalatroJokersCode.Cards;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace BalatroJokers.BalatroJokersCode.Cards;

[Pool(typeof(TokenCardPool))]
public class StoneWall() : BalatroJokersCard(-1,
    CardType.Skill, CardRarity.Ancient,
    TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [ new BlockVar(10M, ValueProp.Unpowered)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return Task.CompletedTask;
        
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3M);
    }
}