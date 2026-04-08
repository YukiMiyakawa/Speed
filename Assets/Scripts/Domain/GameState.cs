using System.Collections.Generic;

namespace Speed.Domain
{
    public class GameState
    {
        public List<Card> PlayerHand { get; } = new List<Card>();
        public List<Card> CpuHand    { get; } = new List<Card>();
        public List<Card> PlayerDeck { get; } = new List<Card>(); // [0] = top
        public List<Card> CpuDeck    { get; } = new List<Card>(); // [0] = top
        public List<Card> LeftPile   { get; } = new List<Card>(); // [0] = top
        public List<Card> RightPile  { get; } = new List<Card>(); // [0] = top

        public Card LeftPileTop  => LeftPile.Count  > 0 ? LeftPile[0]  : null;
        public Card RightPileTop => RightPile.Count > 0 ? RightPile[0] : null;
    }
}
