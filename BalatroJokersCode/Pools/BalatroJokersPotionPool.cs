using BaseLib.Abstracts;

namespace BalatroJokers.BalatroJokersCode.Pools;

public class BalatroJokersPotionPool : CustomPotionPoolModel
{
    public override bool IsShared => false;
}