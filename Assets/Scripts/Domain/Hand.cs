using System.Collections.Generic;
using System.Linq;

namespace Speed.Domain
{
    public sealed class Hand
    {
        private readonly List<Card> cards = new List<Card>();

        public IReadOnlyList<Card> Cards => cards;
        public int Count => cards.Count;

        public void Add(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
            }
        }

        public bool TryGet(int cardId, out Card card)
        {
            card = cards.FirstOrDefault(item => item.Id == cardId);
            return card != null;
        }

        public bool TryRemove(int cardId, out Card removedCard)
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (cards[i].Id != cardId)
                {
                    continue;
                }

                removedCard = cards[i];
                cards.RemoveAt(i);
                return true;
            }

            removedCard = null;
            return false;
        }
    }
}
