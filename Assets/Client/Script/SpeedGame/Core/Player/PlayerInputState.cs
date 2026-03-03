using Cysharp.Threading.Tasks;

namespace SpeedGame.Core.Player
{
    /// <summary>
    /// 入力ソースからコマンドを待機し、取得できたら送出準備ステートへ遷移する。
    /// </summary>
    public sealed class PlayerInputState : IPlayerAgentState
    {
        /// <summary>遷移・送信を委譲する親エージェント。</summary>
        private readonly PlayerAgent _agent;

        /// <summary>入力受付ステートを構築する。</summary>
        public PlayerInputState(PlayerAgent agent)
        {
            _agent = agent;
        }

        /// <summary>ステート入場時に追加初期化は行わない。</summary>
        public UniTask EnterAsync() => UniTask.CompletedTask;

        /// <summary>ステート退場時に後処理は行わない。</summary>
        public UniTask ExitAsync() => UniTask.CompletedTask;

        /// <summary>入力を1回ポーリングし、取得できたら送出準備へ遷移する。</summary>
        public UniTask TickAsync(float deltaTime)
        {
            if (_agent.TryFetchCommand(deltaTime, out var command))
            {
                _agent.SetPendingCommand(command);
                return _agent.ChangeToPrepareAsync();
            }

            return UniTask.CompletedTask;
        }
    }
}
