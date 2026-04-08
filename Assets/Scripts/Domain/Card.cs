using System;

namespace Speed.Domain
{
    public enum Suit { Spade, Heart, Diamond, Club }

    [Serializable]
    public class Card
    {
        public Suit Suit { get; }
        public int Rank { get; } // 1=A, 2-10, 11=J, 12=Q, 13=K

        public Card(Suit suit, int rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public string RankName
        {
            get
            {
                switch (Rank)
                {
                    case 1: return "A";
                    case 11: return "J";
                    case 12: return "Q";
                    case 13: return "K";
                    default: return Rank.ToString();
                }
            }
        }

        public override string ToString() => $"{Suit}_{RankName}";
    }
}
