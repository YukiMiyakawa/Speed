using Cysharp.Threading.Tasks;

namespace SpeedGame.Core.Player
{
    /// <summary>
    /// 送出待ちコマンドを上位へ渡し、クールダウンへ遷移するステート。
    /// </summary>
    public sealed class PrepareCommandState : IPlayerAgentState
    {
        /// <summary>遷移・送信を委譲する親エージェント。</summary>
        private readonly PlayerAgent _agent;

        /// <summary>コマンド送出準備ステートを構築する。</summary>
        public PrepareCommandState(PlayerAgent agent)
        {
            _agent = agent;
        }

        /// <summary>ステート入場時に追加初期化は行わない。</summary>
        public UniTask EnterAsync() => UniTask.CompletedTask;

        /// <summary>ステート退場時に後処理は行わない。</summary>
        public UniTask ExitAsync() => UniTask.CompletedTask;

        /// <summary>保持中コマンドをキューへ送出し、クールダウンへ遷移する。</summary>
        public UniTask TickAsync(float deltaTime)
        {
            if (_agent.TryTakePendingCommand(out var command))
            {
                _agent.Submit(command);
            }

            return _agent.ChangeToCooldownAsync();
        }
    }
}
