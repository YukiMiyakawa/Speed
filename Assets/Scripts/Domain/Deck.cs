using System.Collections.Generic;
using System.Linq;

namespace Speed.Domain
{
    public sealed class Deck
    {
        private readonly Queue<Card> cards;

        public Deck(IEnumerable<Card> cards)
        {
            this.cards = new Queue<Card>(cards ?? Enumerable.Empty<Card>());
        }

        public int Count => cards.Count;
        public bool HasCards => cards.Count > 0;

        public Card Draw()
        {
            return cards.Count > 0 ? cards.Dequeue() : null;
        }

        public IReadOnlyCollection<Card> Snapshot()
        {
            return cards.ToArray();
        }
    }
}
