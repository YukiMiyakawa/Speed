using System;
using System.Collections.Generic;
using Speed.Domain;

namespace Speed.Application
{
    public static class DealService
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        /// Deals a full 52-card shuffled deck into the given GameState.
        /// Player: 5 hand + 20 deck. CPU: 5 hand + 20 deck. Center: 1 left + 1 right.
        /// </summary>
        public static void Deal(GameState state)
        {
            var all = CreateFullDeck();
            Shuffle(all);

            int i = 0;
            for (int j = 0; j < 5; j++)  state.PlayerHand.Add(all[i++]);
            for (int j = 0; j < 5; j++)  state.CpuHand.Add(all[i++]);
            state.LeftPile.Add(all[i++]);
            state.RightPile.Add(all[i++]);
            for (int j = 0; j < 20; j++) state.PlayerDeck.Add(all[i++]);
            for (int j = 0; j < 20; j++) state.CpuDeck.Add(all[i++]);
        }

        private static List<Card> CreateFullDeck()
        {
            var deck = new List<Card>(52);
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                for (int rank = 1; rank <= 13; rank++)
                    deck.Add(new Card(suit, rank));
            return deck;
        }

        private static void Shuffle(List<Card> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                var tmp = cards[i];
                cards[i] = cards[j];
                cards[j] = tmp;
            }
        }
    }
}
