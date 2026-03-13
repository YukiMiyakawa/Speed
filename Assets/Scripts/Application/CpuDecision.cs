using Speed.Domain;

namespace Speed.Application
{
    public readonly struct CpuDecision
    {
        public CpuDecision(Card card, PileId pileId, bool shouldPlay)
        {
            Card = card;
            PileId = pileId;
            ShouldPlay = shouldPlay;
        }

        public Card Card { get; }
        public PileId PileId { get; }
        public bool ShouldPlay { get; }

        public static CpuDecision Pass => new CpuDecision(null, PileId.Left, false);
    }
}
