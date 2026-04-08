using System.Collections.Generic;
using Speed.Domain;

namespace Speed.Application
{
    public static class StalemateService
    {
        public static bool IsStalemate(GameState state) =>
            !CanPlay(state.PlayerHand, state) && !CanPlay(state.CpuHand, state);

        public static bool CanPlayerPlay(GameState state) => CanPlay(state.PlayerHand, state);
        public static bool CanCpuPlay(GameState state)    => CanPlay(state.CpuHand,    state);

        private static bool CanPlay(List<Card> hand, GameState state)
        {
            foreach (var card in hand)
            {
                if (RuleService.CanPlace(card, state.LeftPileTop))  return true;
                if (RuleService.CanPlace(card, state.RightPileTop)) return true;
            }
            return false;
        }
    }
}
