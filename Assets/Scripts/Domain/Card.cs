namespace Speed.Domain
{
    public sealed class Card
    {
        public Card(int id, Suit suit, Rank rank)
        {
            Id = id;
            Suit = suit;
            Rank = rank;
        }

        public int Id { get; }
        public Suit Suit { get; }
        public Rank Rank { get; }

        public override string ToString()
        {
            return $"{Suit}-{Rank}";
        }
    }
}
