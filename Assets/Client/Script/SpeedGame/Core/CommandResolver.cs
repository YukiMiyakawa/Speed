using SpeedGame.Domain;

namespace SpeedGame.Core
{
    /// <summary>
    /// コマンド確定の中央責務。オンライン移行時はこの責務を権威側へ移しやすい。
    /// </summary>
    public sealed class CommandResolver
    {
        private readonly SpeedGameContext _context;

        public CommandResolver(SpeedGameContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 1コマンドを確定し、勝者が決まった場合は返す。
        /// </summary>
        public bool TryResolve(PlayerCommand command, out PlayerSide? winner)
        {
            winner = null;

            if (!_context.Model.TryApplyCommand(command, out _))
            {
                return false;
            }

            if (_context.Model.IsWin(command.Side))
            {
                winner = command.Side;
            }

            return true;
        }
    }
}
