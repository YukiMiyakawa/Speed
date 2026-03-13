namespace Speed.Domain
{
    public sealed class GameState
    {
        public GameState(PlayerState player, PlayerState cpu, TablePile leftPile, TablePile rightPile)
        {
            Player = player;
            Cpu = cpu;
            LeftPile = leftPile;
            RightPile = rightPile;
            Result = BattleResult.None;
        }

        public PlayerState Player { get; }
        public PlayerState Cpu { get; }
        public TablePile LeftPile { get; }
        public TablePile RightPile { get; }
        public bool IsWaitingForPileRefresh { get; set; }
        public bool IsGameOver { get; private set; }
        public BattleResult Result { get; private set; }

        public TablePile GetPile(PileId pileId)
        {
            return pileId == PileId.Left ? LeftPile : RightPile;
        }

        public void Finish(BattleResult result)
        {
            IsGameOver = true;
            Result = result;
        }
    }
}
