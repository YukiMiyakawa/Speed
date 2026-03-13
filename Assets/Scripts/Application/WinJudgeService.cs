using Speed.Domain;

namespace Speed.Application
{
    public sealed class WinJudgeService
    {
        public BattleResult EvaluateImmediate(GameState gameState)
        {
            if (gameState.Player.Hand.Count == 0)
            {
                return BattleResult.PlayerWin;
            }

            if (gameState.Cpu.Hand.Count == 0)
            {
                return BattleResult.CpuWin;
            }

            return BattleResult.None;
        }

        public BattleResult EvaluateDeckEmptyResolution(GameState gameState)
        {
            if (gameState.Player.Hand.Count < gameState.Cpu.Hand.Count)
            {
                return BattleResult.PlayerWin;
            }

            if (gameState.Player.Hand.Count > gameState.Cpu.Hand.Count)
            {
                return BattleResult.CpuWin;
            }

            return BattleResult.Draw;
        }
    }
}
