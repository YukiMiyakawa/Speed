using System;

namespace SpeedGame.Domain
{
    [Serializable]
    public sealed class CardData
    {
        public int CardId;
        public int Rank;
        public CardSuit Suit;
        public bool IsJoker;
        public int? LockedRank;

        public int EffectiveRank => LockedRank ?? Rank;

        public CardData Clone()
        {
            return new CardData
            {
                CardId = CardId,
                Rank = Rank,
                Suit = Suit,
                IsJoker = IsJoker,
                LockedRank = LockedRank
            };
        }
    }
}
