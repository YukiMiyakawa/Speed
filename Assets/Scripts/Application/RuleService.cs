using System;
using Speed.Domain;

namespace Speed.Application
{
    public static class RuleService
    {
        /// <summary>
        /// Returns true if card can be placed on topCard (±1, A-K wrap).
        /// </summary>
        public static bool CanPlace(Card card, Card topCard)
        {
            if (card == null || topCard == null) return false;
            int diff = Math.Abs(card.Rank - topCard.Rank);
            return diff == 1 || diff == 12; // 12 handles K(13)-A(1) wrap
        }
    }
}
