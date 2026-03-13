namespace Speed.Domain
{
    public sealed class TablePile
    {
        public TablePile(PileId pileId, Card topCard)
        {
            PileId = pileId;
            TopCard = topCard;
        }

        public PileId PileId { get; }
        public Card TopCard { get; private set; }
        public bool IsPlayingPutCardAnimation { get; set; }

        public void SetTopCard(Card card)
        {
            TopCard = card;
        }
    }
}
