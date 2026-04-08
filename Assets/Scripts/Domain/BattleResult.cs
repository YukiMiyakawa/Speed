namespace Speed.Domain
{
    public enum BattleResultType { PlayerWin, CpuWin, Draw }

    public class BattleResult
    {
        public BattleResultType Type { get; }

        public BattleResult(BattleResultType type)
        {
            Type = type;
        }
    }
}
