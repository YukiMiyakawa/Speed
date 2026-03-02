using Cysharp.Threading.Tasks;

namespace SpeedGame.Core.Player
{
    /// <summary>
    /// 入力ソースからコマンドを待機し、取得できたら上位へ提出するステート。
    /// </summary>
    public sealed class PlayerWaitingState : IPlayerAgentState
    {
        /// <summary>遷移・送信を委譲する親エージェント。</summary>
        private readonly PlayerAgent _agent;

        /// <summary>入力待ちステートを構築する。</summary>
        public PlayerWaitingState(PlayerAgent agent)
        {
            _agent = agent;
        }

        /// <summary>ステート入場時に追加初期化は行わない。</summary>
        public UniTask EnterAsync() => UniTask.CompletedTask;

        /// <summary>ステート退場時に後処理は行わない。</summary>
        public UniTask ExitAsync() => UniTask.CompletedTask;

        /// <summary>入力を1回ポーリングし、取得できたら提出してクールダウンへ遷移する。</summary>
        public UniTask TickAsync(float deltaTime)
        {
            if (_agent.TryFetchCommand(deltaTime, out var command))
            {
                _agent.Submit(command);
                return _agent.ChangeToCooldownAsync();
            }

            return UniTask.CompletedTask;
        }
    }
}
