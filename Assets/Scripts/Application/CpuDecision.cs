using Speed.Domain;

namespace Speed.Application
{
    public enum CpuDecisionType { PlayCard, LookAheadMiss, FalseMiss }

    public class CpuDecision
    {
        public CpuDecisionType Type      { get; }
        public Card             Card      { get; }
        public PileId           TargetPile { get; }
        public int              HandIndex { get; }

        public CpuDecision(CpuDecisionType type, Card card, PileId targetPile, int handIndex)
        {
            Type       = type;
            Card       = card;
            TargetPile = targetPile;
            HandIndex  = handIndex;
        }

        public static CpuDecision Miss() =>
            new CpuDecision(CpuDecisionType.LookAheadMiss, null, PileId.Left, -1);
    }
}
