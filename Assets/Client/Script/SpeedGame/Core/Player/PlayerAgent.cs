using System;
using Cysharp.Threading.Tasks;
using SpeedGame.Domain;

namespace SpeedGame.Core.Player
{
    // プレイヤー単位の小さな状態機械。1ゲーム中に Player/Opponent の2インスタンスを並列運用する。
    public sealed class PlayerAgent
    {
        private readonly SpeedGameModel _model;
        private readonly IPlayerCommandSource _source;
        private readonly Action<PlayerCommand> _submit;
        private readonly PlayerAgentStateMachine _stateMachine = new();

        private readonly PlayerWaitingState _waitingState;
        private readonly PlayerCooldownState _cooldownState;

        public PlayerSide Side { get; }

        public PlayerAgent(PlayerSide side, SpeedGameModel model, IPlayerCommandSource source, Action<PlayerCommand> submit)
        {
            Side = side;
            _model = model;
            _source = source;
            _submit = submit;
            _waitingState = new PlayerWaitingState(this);
            _cooldownState = new PlayerCooldownState(this);
        }

        public UniTask EnterAsync()
        {
            return _stateMachine.ChangeStateAsync(_waitingState);
        }

        public UniTask ExitAsync()
        {
            return _stateMachine.ChangeStateAsync(null);
        }

        public void ResetInput()
        {
            _source.Reset();
        }

        public UniTask TickAsync(float deltaTime)
        {
            return _stateMachine.TickAsync(deltaTime);
        }

        internal bool TryFetchCommand(float deltaTime, out PlayerCommand command)
        {
            return _source.TryGetNextCommand(_model, Side, deltaTime, out command);
        }

        internal void Submit(PlayerCommand command)
        {
            _submit(command);
        }

        internal UniTask ChangeToWaitingAsync()
        {
            return _stateMachine.ChangeStateAsync(_waitingState);
        }

        internal UniTask ChangeToCooldownAsync()
        {
            return _stateMachine.ChangeStateAsync(_cooldownState);
        }

        private sealed class PlayerWaitingState : IPlayerAgentState
        {
            private readonly PlayerAgent _agent;

            public PlayerWaitingState(PlayerAgent agent)
            {
                _agent = agent;
            }

            public UniTask EnterAsync() => UniTask.CompletedTask;
            public UniTask ExitAsync() => UniTask.CompletedTask;

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

        private sealed class PlayerCooldownState : IPlayerAgentState
        {
            private readonly PlayerAgent _agent;
            private bool _leaveNextTick;

            public PlayerCooldownState(PlayerAgent agent)
            {
                _agent = agent;
            }

            public UniTask EnterAsync()
            {
                _leaveNextTick = false;
                return UniTask.CompletedTask;
            }

            public UniTask ExitAsync() => UniTask.CompletedTask;

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
}
