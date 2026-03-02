using System;
using Cysharp.Threading.Tasks;
using SpeedGame.Domain;

namespace SpeedGame.Core.Player
{
    /// <summary>
    /// プレイヤー単位の状態機械。1ゲーム中に Player/Opponent の2インスタンスを並列運用する。
    /// </summary>
    public sealed class PlayerAgent
    {
        /// <summary>判定済みの現在ゲーム状態。</summary>
        private readonly SpeedGameModel _model;
        /// <summary>入力ソース抽象（ローカル/CPU/ネットワーク）。</summary>
        private readonly IPlayerCommandSource _source;
        /// <summary>取得したコマンドを上位へ渡す送信口。</summary>
        private readonly Action<PlayerCommand> _submit;
        /// <summary>プレイヤー専用の内部ステートマシン。</summary>
        private readonly PlayerAgentStateMachine _stateMachine = new();

        /// <summary>入力待ちステート。</summary>
        private readonly PlayerWaitingState _waitingState;
        /// <summary>行動直後のクールダウンステート。</summary>
        private readonly PlayerCooldownState _cooldownState;

        /// <summary>このエージェントが担当するサイド。</summary>
        public PlayerSide Side { get; }

        /// <summary>プレイヤーエージェントを構築する。</summary>
        public PlayerAgent(PlayerSide side, SpeedGameModel model, IPlayerCommandSource source, Action<PlayerCommand> submit)
        {
            Side = side;
            _model = model;
            _source = source;
            _submit = submit;
            _waitingState = new PlayerWaitingState(this);
            _cooldownState = new PlayerCooldownState(this);
        }

        /// <summary>内部ステートを入力待ちに遷移して開始する。</summary>
        public UniTask EnterAsync()
        {
            return _stateMachine.ChangeStateAsync(_waitingState);
        }

        /// <summary>内部ステートを停止する。</summary>
        public UniTask ExitAsync()
        {
            return _stateMachine.ChangeStateAsync(null);
        }

        /// <summary>入力ソースのバッファを初期化する。</summary>
        public void ResetInput()
        {
            _source.Reset();
        }

        /// <summary>プレイヤー内部FSMを1フレーム進める。</summary>
        public UniTask TickAsync(float deltaTime)
        {
            return _stateMachine.TickAsync(deltaTime);
        }

        /// <summary>入力ソースから次コマンドを取り出す。</summary>
        internal bool TryFetchCommand(float deltaTime, out PlayerCommand command)
        {
            return _source.TryGetNextCommand(_model, Side, deltaTime, out command);
        }

        /// <summary>上位の確定処理キューへコマンドを渡す。</summary>
        internal void Submit(PlayerCommand command)
        {
            _submit(command);
        }

        /// <summary>入力待ちステートへ遷移する。</summary>
        internal UniTask ChangeToWaitingAsync()
        {
            return _stateMachine.ChangeStateAsync(_waitingState);
        }

        /// <summary>クールダウンステートへ遷移する。</summary>
        internal UniTask ChangeToCooldownAsync()
        {
            return _stateMachine.ChangeStateAsync(_cooldownState);
        }
    }
}
