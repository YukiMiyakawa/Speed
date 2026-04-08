using System;
using System.Collections.Generic;
using Speed.Domain;

namespace Speed.Application
{
    public static class CpuDecisionService
    {
        private static readonly Random Rng = new Random();

        public static CpuDecision Decide(GameState state, CpuDifficultySettings settings)
        {
            float roll = (float)Rng.NextDouble();
            if (roll < settings.MissRate)
            {
                if ((float)Rng.NextDouble() < settings.LookAheadMissRatio)
                    return CpuDecision.Miss();

                var falseMove = GetFalseMove(state);
                return falseMove ?? CpuDecision.Miss();
            }

            var validMoves = GetValidMoves(state);
            if (validMoves.Count == 0) return CpuDecision.Miss();

            var chosen = validMoves[Rng.Next(validMoves.Count)];
            return new CpuDecision(CpuDecisionType.PlayCard, chosen.card, chosen.pile, chosen.index);
        }

        private static List<(Card card, PileId pile, int index)> GetValidMoves(GameState state)
        {
            var moves = new List<(Card, PileId, int)>();
            for (int i = 0; i < state.CpuHand.Count; i++)
            {
                var c = state.CpuHand[i];
                if (RuleService.CanPlace(c, state.LeftPileTop))  moves.Add((c, PileId.Left,  i));
                if (RuleService.CanPlace(c, state.RightPileTop)) moves.Add((c, PileId.Right, i));
            }
            return moves;
        }

        private static CpuDecision GetFalseMove(GameState state)
        {
            var invalid = new List<(Card card, PileId pile, int index)>();
            for (int i = 0; i < state.CpuHand.Count; i++)
            {
                var c = state.CpuHand[i];
                if (!RuleService.CanPlace(c, state.LeftPileTop))
                    invalid.Add((c, PileId.Left, i));
                else if (!RuleService.CanPlace(c, state.RightPileTop))
                    invalid.Add((c, PileId.Right, i));
            }
            if (invalid.Count == 0) return null;
            var chosen = invalid[Rng.Next(invalid.Count)];
            return new CpuDecision(CpuDecisionType.FalseMiss, chosen.card, chosen.pile, chosen.index);
        }
    }
}
