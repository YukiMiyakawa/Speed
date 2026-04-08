using Speed.Domain;

namespace Speed.Application
{
    public static class BattleCommandProcessor
    {
        public static PutCardResult TryPutCard(GameState state, PlayerId player, int handIndex, PileId pileId)
        {
            var hand = player == PlayerId.Player ? state.PlayerHand : state.CpuHand;
            if (handIndex < 0 || handIndex >= hand.Count)
                return PutCardResult.InvalidRule();

            var card    = hand[handIndex];
            var pileTop = pileId == PileId.Left ? state.LeftPileTop  : state.RightPileTop;
            var pile    = pileId == PileId.Left ? state.LeftPile     : state.RightPile;

            if (!RuleService.CanPlace(card, pileTop))
                return PutCardResult.InvalidRule();

            hand.RemoveAt(handIndex);
            pile.Insert(0, card);
            return PutCardResult.Success();
        }

        /// <summary>
        /// Flips one card from each player's deck onto the center piles (stalemate relief).
        /// Returns true if at least one card was flipped.
        /// </summary>
        public static bool FlipCenterPiles(GameState state)
        {
            bool flipped = false;
            if (state.PlayerDeck.Count > 0)
            {
                var card = state.PlayerDeck[0];
                state.PlayerDeck.RemoveAt(0);
                state.LeftPile.Insert(0, card);
                flipped = true;
            }
            if (state.CpuDeck.Count > 0)
            {
                var card = state.CpuDeck[0];
                state.CpuDeck.RemoveAt(0);
                state.RightPile.Insert(0, card);
                flipped = true;
            }
            return flipped;
        }
    }
}
