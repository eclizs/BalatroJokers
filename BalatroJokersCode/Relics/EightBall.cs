using BalatroJokers.BalatroJokersCode.Interfaces;
using BalatroJokers.BalatroJokersCode.Pools;
using BalatroJokers.BalatroJokersCode.Potions.Tarot;
using BaseLib.Cards.Variables;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace BalatroJokers.BalatroJokersCode.Relics;

[Pool(typeof(SharedRelicPool))]
public class EightBall() : BalatroJokersRelic, IChance
{
    private int _cardsPlayedThisTurn;
    private int _triggerCount;
    
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => this._cardsPlayedThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(8),
        new DisplayVar<EightBall>("Numerator", r =>
        {
            var canonical = r.IsCanonical ? null : r;
            if (canonical == null) return r.GetNumerator(null).ToString();
            var player = canonical.IsCanonical ? null : canonical.Owner;
            return r.GetNumerator(player).ToString();
        }),
        new DynamicVar("Denominator", 4M),
        new RepeatVar(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(BalatroJokersHoverTips.Tarot)];

    public PotionModel? LastTarotOrPlanetUsed = null;

    public override Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (potion.Owner != Owner || potion is TheFool)
            return Task.CompletedTask;

        bool valid =
            ModelDb.PotionPool<BalatroJokersTarotPotionPool>()
                .AllPotions.Contains(potion.CanonicalInstance)
            || ModelDb.PotionPool<BalatroJokersPlanetPotionPool>()
                .AllPotions.Contains(potion.CanonicalInstance);

        if (valid)
            TarotHistory.LastUsed[Owner] = potion.CanonicalInstance.Id;

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !CombatManager.Instance.IsInProgress)
            return;
        ++_cardsPlayedThisTurn;
        InvokeDisplayAmountChanged();

        if (_cardsPlayedThisTurn == DynamicVars.Cards.BaseValue && _triggerCount < DynamicVars.Repeat.BaseValue)
        {
            if (this.RollChance(Owner, DynamicVars["Denominator"].IntValue))
            {
                var tarots = ModelDb.PotionPool<BalatroJokersTarotPotionPool>().AllPotions.ToList();
                var randomTarot = Owner.RunState.Rng.CombatPotionGeneration.NextItem(tarots);
                PotionProcureResult procure = await PotionCmd.TryToProcure(randomTarot?.ToMutable(), Owner);
                _triggerCount++;
            }
        }
    }
    
    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom) return Task.CompletedTask;
        
        _cardsPlayedThisTurn = 0;
        InvokeDisplayAmountChanged();
        
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        _cardsPlayedThisTurn = 0;
        InvokeDisplayAmountChanged();

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        this._cardsPlayedThisTurn = 0;
        InvokeDisplayAmountChanged();

        return Task.CompletedTask;
    }
}