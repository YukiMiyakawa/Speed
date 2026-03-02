using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using SpeedGame.Animation;
using SpeedGame.Config;
using SpeedGame.Core.Player;
using SpeedGame.Domain;
using SpeedGame.State;
using UnityEngine;

namespace SpeedGame.Core
{
    public sealed class SpeedGameController : MonoBehaviour
    {
        // 相手入力の供給元を切り替える。Photon移行時は RemoteQueue を常用し、CPUを外す。
        private enum OpponentControlMode
        {
            Cpu,
            RemoteQueue
        }

        [Header("Rule / Timing")]
        [SerializeField] private SpeedRuleSettings ruleSettings = new();
        [SerializeField] private SpeedTimingSettings timingSettings = new();
        [SerializeField] private CpuDifficultyProfile cpuDifficulty;

        [Header("Animation")]
        [SerializeField] private SpeedAnimationProfile animationProfile;

        [Header("Input Abstraction")]
        [SerializeField] private OpponentControlMode opponentControlMode = OpponentControlMode.Cpu;

        private SpeedGameContext _context;
        private SpeedStateMachine _stateMachine;
        private readonly Queue<PlayerCommand> _queuedCommands = new();
        private CancellationTokenSource _agentLoopCts;

        // ローカル入力ソース（オフラインの自分操作）
        private LocalPlayerCommandSource _localPlayerSource;
        // 相手入力ソース（CPUまたはネットワーク）
        private IPlayerCommandSource _opponentSource;
        // ネットワーク入力用の実体。Photon受信をここに流し込む。
        private RemoteQueueCommandSource _remoteOpponentSource;

        public PlayerAgent PlayerAgent { get; private set; }
        public PlayerAgent OpponentAgent { get; private set; }

        public SetupState SetupState { get; private set; }
        public PlayerInputState PlayerInputState { get; private set; }
        public ResolveCommandState ResolveCommandState { get; private set; }
        public GameOverState GameOverState { get; private set; }

        public ReadOnlyReactiveProperty<PlayerSide?> Winner => _context.Winner;
        public bool HasQueuedCommand => _queuedCommands.Count > 0;

        private async UniTaskVoid Start()
        {
            if (cpuDifficulty == null)
            {
                cpuDifficulty = ScriptableObject.CreateInstance<CpuDifficultyProfile>();
            }

            ICardAnimationService animation = animationProfile != null
                ? new DotweenCardAnimationService(animationProfile)
                : null;

            _context = new SpeedGameContext(ruleSettings, timingSettings, cpuDifficulty, animation, destroyCancellationToken);
            _stateMachine = new SpeedStateMachine();

            BuildPlayerAgents();

            SetupState = new SetupState(this, _context);
            PlayerInputState = new PlayerInputState(this, _context);
            ResolveCommandState = new ResolveCommandState(this, _context);
            GameOverState = new GameOverState(this);

            await ChangeStateAsync(SetupState);

            while (!destroyCancellationToken.IsCancellationRequested)
            {
                await _stateMachine.TickAsync();
                await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);
            }
        }

        public UniTask ChangeStateAsync(ISpeedGameState next)
        {
            return _stateMachine.ChangeStateAsync(next);
        }

        public void ResetRound()
        {
            _queuedCommands.Clear();
            PlayerAgent?.ResetInput();
            OpponentAgent?.ResetInput();
        }

        public async UniTask StartAgentLoopsAsync()
        {
            await StopAgentLoopsAsync();

            if (PlayerAgent == null || OpponentAgent == null)
            {
                return;
            }

            _agentLoopCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            await PlayerAgent.EnterAsync();
            await OpponentAgent.EnterAsync();

            RunAgentLoop(PlayerAgent, _agentLoopCts.Token).Forget();
            RunAgentLoop(OpponentAgent, _agentLoopCts.Token).Forget();
        }

        public async UniTask StopAgentLoopsAsync()
        {
            if (_agentLoopCts != null)
            {
                _agentLoopCts.Cancel();
                _agentLoopCts.Dispose();
                _agentLoopCts = null;
            }

            if (PlayerAgent != null)
            {
                await PlayerAgent.ExitAsync();
            }

            if (OpponentAgent != null)
            {
                await OpponentAgent.ExitAsync();
            }
        }

        public void RequestPlayFromPlayer(int handIndex, PileLane lane)
        {
            _localPlayerSource?.Enqueue(PlayerCommand.Play(PlayerSide.Player, handIndex, lane));
        }

        public void RequestDrawFromPlayerStock()
        {
            _localPlayerSource?.Enqueue(PlayerCommand.Draw(PlayerSide.Player));
        }

        public void ReceiveRemoteCommand(PlayerCommand command)
        {
            // Photonなどの受信イベントから呼ぶ接続点。
            _remoteOpponentSource?.PushFromNetwork(command);
        }

        public void EnqueueCommand(PlayerCommand command)
        {
            _queuedCommands.Enqueue(command);
        }

        public bool TryDequeueCommand(out PlayerCommand command)
        {
            if (_queuedCommands.Count > 0)
            {
                command = _queuedCommands.Dequeue();
                return true;
            }

            command = default;
            return false;
        }

        public UniTask AnimateMoveToLaneAsync(RectTransform card, RectTransform lane)
        {
            return _context.Animation == null
                ? UniTask.CompletedTask
                : _context.Animation.MoveToFoundationAsync(card, lane, destroyCancellationToken);
        }

        public UniTask AnimateDrawFromStockAsync(RectTransform card, RectTransform stock, RectTransform handSlot)
        {
            return _context.Animation == null
                ? UniTask.CompletedTask
                : _context.Animation.DrawFromStockAsync(card, stock, handSlot, destroyCancellationToken);
        }

        public UniTask AnimateBounceBackAsync(RectTransform card, Vector2 origin)
        {
            return _context.Animation == null
                ? UniTask.CompletedTask
                : _context.Animation.BounceBackAsync(card, origin, destroyCancellationToken);
        }

        public string GetStateSummaryForDebug()
        {
            if (_context == null)
            {
                return "not-initialized";
            }

            var left = _context.Model.LeftTop?.EffectiveRank.ToString() ?? "-";
            var right = _context.Model.RightTop?.EffectiveRank.ToString() ?? "-";
            return $"PHand:{_context.Model.PlayerHand.Count} OHand:{_context.Model.OpponentHand.Count} " +
                   $"PStock:{_context.Model.PlayerStockCount} OStock:{_context.Model.OpponentStockCount} " +
                   $"L:{left} R:{right}";
        }

        private void BuildPlayerAgents()
        {
            _localPlayerSource = new LocalPlayerCommandSource();

            // 相手入力源の差し替え点。オンラインでは RemoteQueue/Photon 実装に置換する。
            _opponentSource = opponentControlMode switch
            {
                OpponentControlMode.RemoteQueue => _remoteOpponentSource = new RemoteQueueCommandSource(),
                _ => new CpuPlayerCommandSource(cpuDifficulty)
            };

            // 各プレイヤーで独立FSMを保持し、入力取得だけを差し替え可能にしている。
            PlayerAgent = new PlayerAgent(PlayerSide.Player, _context.Model, _localPlayerSource, EnqueueCommand);
            OpponentAgent = new PlayerAgent(PlayerSide.Opponent, _context.Model, _opponentSource, EnqueueCommand);
        }

        private async UniTaskVoid RunAgentLoop(PlayerAgent agent, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await agent.TickAsync(Time.deltaTime);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
