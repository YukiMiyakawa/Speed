using Cysharp.Threading.Tasks;

namespace SpeedGame.Core.Player
{
    /// <summary>
    /// 行動直後に1 tick 待機し、連続入力を抑制してから待機ステートへ戻すステート。
    /// </summary>
    public sealed class PlayerCooldownState : IPlayerAgentState
    {
        /// <summary>遷移を委譲する親エージェント。</summary>
        private readonly PlayerAgent _agent;
        /// <summary>次 tick で待機へ戻るためのフラグ。</summary>
        private bool _leaveNextTick;

        /// <summary>クールダウンステートを構築する。</summary>
        public PlayerCooldownState(PlayerAgent agent)
        {
            _agent = agent;
        }

        /// <summary>入場時に待機フラグを初期化する。</summary>
        public UniTask EnterAsync()
        {
            _leaveNextTick = false;
            return UniTask.CompletedTask;
        }

        /// <summary>ステート退場時に後処理は行わない。</summary>
        public UniTask ExitAsync() => UniTask.CompletedTask;

        /// <summary>1 tick 経過後に待機ステートへ戻る。</summary>
        public UniTask TickAsync(float deltaTime)
        {
            if (!_leaveNextTick)
            {
                _leaveNextTick = true;
                return UniTask.CompletedTask;
            }

            return _agent.ChangeToWaitingAsync();
        }
    }
}
