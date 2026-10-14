using System.Collections.ObjectModel;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(IroncladRelicPool))]
public class MarbleJoker() : BalatroJokersRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new ReadOnlyCollection<DynamicVar>(new List<DynamicVar> { new CardsVar(1) });

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new ReadOnlyCollection<IHoverTip>(new List<IHoverTip> { HoverTipFactory.FromCard<GiantRock>() });

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != this.Owner || this.Owner.PlayerCombatState.TurnNumber > 1)
            return;
        AbstractRoom currentRoom = combatState.RunState.CurrentRoom;
        if (currentRoom == null) return;
        if (currentRoom.RoomType != RoomType.Elite) return;
        this.Flash();
        List<CardModel> list = [];
        for (int i = 0; i < this.DynamicVars.Cards.IntValue; ++i)
        {
            list.Add(combatState.CreateCard((CardModel) ModelDb.Card<GiantRock>(), this.Owner));
        }
        foreach (CardModel cardModel in list)
            cardModel.SetToFreeThisTurn();
        
        IReadOnlyList<CardPileAddResult> combat = await CardPileCmd.AddGeneratedCardsToCombat((IEnumerable<CardModel>) list, PileType.Hand, this.Owner);
    }
}
    