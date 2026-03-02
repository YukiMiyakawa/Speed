using System.Threading;
using R3;
using SpeedGame.Animation;
using SpeedGame.Config;
using SpeedGame.Domain;
using UnityEngine;

namespace SpeedGame.Core
{
    public sealed class SpeedGameContext
    {
        public readonly SpeedGameModel Model;
        public readonly SpeedTimingSettings Timing;
        public readonly CpuDifficultyProfile CpuDifficulty;
        public readonly ICardAnimationService Animation;
        public readonly Subject<PlayerCommand> CommandStream;
        public readonly ReactiveProperty<PlayerSide?> Winner;
        public readonly CancellationToken DestroyToken;

        public float StuckTimer;

        public SpeedGameContext(
            SpeedRuleSettings rules,
            SpeedTimingSettings timing,
            CpuDifficultyProfile cpuDifficulty,
            ICardAnimationService animation,
            CancellationToken destroyToken)
        {
            Model = new SpeedGameModel(rules);
            Timing = timing;
            CpuDifficulty = cpuDifficulty;
            Animation = animation;
            DestroyToken = destroyToken;
            CommandStream = new Subject<PlayerCommand>();
            Winner = new ReactiveProperty<PlayerSide?>(null);
        }
    }
}
