using Speed.Domain;

namespace Speed.Application
{
    public static class WinJudgeService
    {
        /// <summary>Returns result if either hand is empty, otherwise null.</summary>
        public static BattleResult CheckHandEmpty(GameState state)
        {
            bool pEmpty = state.PlayerHand.Count == 0;
            bool cEmpty = state.CpuHand.Count == 0;
            if (pEmpty && cEmpty) return new BattleResult(BattleResultType.Draw);
            if (pEmpty) return new BattleResult(BattleResultType.PlayerWin);
            if (cEmpty) return new BattleResult(BattleResultType.CpuWin);
            return null;
        }

        /// <summary>Returns result based on hand counts when stalemate with no decks.</summary>
        public static BattleResult CheckStalemateResult(GameState state)
        {
            int p = state.PlayerHand.Count;
            int c = state.CpuHand.Count;
            if (p < c) return new BattleResult(BattleResultType.PlayerWin);
            if (c < p) return new BattleResult(BattleResultType.CpuWin);
            return new BattleResult(BattleResultType.Draw);
        }
    }
}
