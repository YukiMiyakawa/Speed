using System;
using System.Collections.Generic;
using Speed.Domain;

namespace Speed.Application
{
    public sealed class DealService
    {
        private const int InitialHandCount = 5;
        private const int CardsPerPlayer = 25;

        public GameState CreateInitialState(int seed)
        {
            var random = new Random(seed);
            var cards = CreateDeck();
            Shuffle(cards, random);

            var playerHand = new Hand();
            var cpuHand = new Hand();
            var index = 0;

            for (var i = 0; i < InitialHandCount; i++)
            {
                playerHand.Add(cards[index++]);
                cpuHand.Add(cards[index++]);
            }

            var leftPile = new TablePile(PileId.Left, cards[index++]);
            var rightPile = new TablePile(PileId.Right, cards[index++]);

            var playerDeckCards = new List<Card>();
            var cpuDeckCards = new List<Card>();

            while (playerHand.Count + playerDeckCards.Count < CardsPerPlayer)
            {
                playerDeckCards.Add(cards[index++]);
            }

            while (cpuHand.Count + cpuDeckCards.Count < CardsPerPlayer)
            {
                cpuDeckCards.Add(cards[index++]);
            }

            var player = new PlayerState(PlayerId.Player, false, playerHand, new Deck(playerDeckCards));
            var cpu = new PlayerState(PlayerId.Cpu, true, cpuHand, new Deck(cpuDeckCards));
            return new GameState(player, cpu, leftPile, rightPile);
        }

        private static List<Card> CreateDeck()
        {
            var cards = new List<Card>(52);
            var id = 0;

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(id++, suit, rank));
                }
            }

            return cards;
        }

        private static void Shuffle(IList<Card> cards, Random random)
        {
            for (var i = cards.Count - 1; i > 0; i--)
            {
                var swapIndex = random.Next(i + 1);
                (cards[i], cards[swapIndex]) = (cards[swapIndex], cards[i]);
            }
        }
    }
}
