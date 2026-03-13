namespace Speed.Domain
{
    public sealed class PlayerState
    {
        public PlayerState(PlayerId playerId, bool isCpu, Hand hand, Deck deck)
        {
            PlayerId = playerId;
            IsCpu = isCpu;
            Hand = hand;
            Deck = deck;
        }

        public PlayerId PlayerId { get; }
        public bool IsCpu { get; }
        public Hand Hand { get; }
        public Deck Deck { get; }
    }
}
