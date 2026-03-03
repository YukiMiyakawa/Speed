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

        /// <summary>入力受付ステート。</summary>
        private readonly PlayerInputState _inputState;
        /// <summary>コマンド送出準備ステート。</summary>
        private readonly PrepareCommandState _prepareState;
        /// <summary>行動直後のクールダウンステート。</summary>
        private readonly PlayerCooldownState _cooldownState;

        private bool _hasPendingCommand;
        private PlayerCommand _pendingCommand;

        /// <summary>このエージェントが担当するサイド。</summary>
        public PlayerSide Side { get; }

        /// <summary>プレイヤーエージェントを構築する。</summary>
        public PlayerAgent(PlayerSide side, SpeedGameModel model, IPlayerCommandSource source, Action<PlayerCommand> submit)
        {
            Side = side;
            _model = model;
            _source = source;
            _submit = submit;
            _inputState = new PlayerInputState(this);
            _prepareState = new PrepareCommandState(this);
            _cooldownState = new PlayerCooldownState(this);
        }

        /// <summary>内部ステートを入力受付に遷移して開始する。</summary>
        public UniTask EnterAsync()
        {
            return _stateMachine.ChangeStateAsync(_inputState);
        }

        /// <summary>内部ステートを停止する。</summary>
        public UniTask ExitAsync()
        {
            return _stateMachine.ChangeStateAsync(null);
        }

        /// <summary>入力ソースと未送出コマンドを初期化する。</summary>
        public void ResetInput()
        {
            _source.Reset();
            _hasPendingCommand = false;
            _pendingCommand = default;
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

        /// <summary>コマンドを送出待ちとして保持する。</summary>
        internal void SetPendingCommand(PlayerCommand command)
        {
            _pendingCommand = command;
            _hasPendingCommand = true;
        }

        /// <summary>保持中の送出待ちコマンドを取り出す。</summary>
        internal bool TryTakePendingCommand(out PlayerCommand command)
        {
            if (_hasPendingCommand)
            {
                command = _pendingCommand;
                _pendingCommand = default;
                _hasPendingCommand = false;
                return true;
            }

            command = default;
            return false;
        }

        /// <summary>上位の確定処理キューへコマンドを渡す。</summary>
        internal void Submit(PlayerCommand command)
        {
            _submit(command);
        }

        /// <summary>入力受付ステートへ遷移する。</summary>
        internal UniTask ChangeToInputAsync()
        {
            return _stateMachine.ChangeStateAsync(_inputState);
        }

        /// <summary>コマンド送出準備ステートへ遷移する。</summary>
        internal UniTask ChangeToPrepareAsync()
        {
            return _stateMachine.ChangeStateAsync(_prepareState);
        }

        /// <summary>クールダウンステートへ遷移する。</summary>
        internal UniTask ChangeToCooldownAsync()
        {
            return _stateMachine.ChangeStateAsync(_cooldownState);
        }
    }
}
