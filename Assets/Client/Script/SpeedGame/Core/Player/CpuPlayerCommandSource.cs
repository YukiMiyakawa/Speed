using System;
using SpeedGame.Config;
using SpeedGame.Domain;
using UnityEngine;

namespace SpeedGame.Core.Player
{
    public sealed class CpuPlayerCommandSource : IPlayerCommandSource
    {
        private readonly CpuDifficultyProfile _difficulty;
        private readonly System.Random _random = new();
        private float _cooldownSeconds;

        public CpuPlayerCommandSource(CpuDifficultyProfile difficulty)
        {
            _difficulty = difficulty;
            _cooldownSeconds = 0f;
        }

        public void Reset()
        {
            _cooldownSeconds = 0f;
        }

        public bool TryGetNextCommand(SpeedGameModel model, PlayerSide side, float deltaTime, out PlayerCommand command)
        {
            _cooldownSeconds -= deltaTime;
            if (_cooldownSeconds > 0f)
            {
                command = default;
                return false;
            }

            _cooldownSeconds = NextDelaySeconds();

            if (_random.NextDouble() < _difficulty.MistakeRate)
            {
                command = default;
                return false;
            }

            var hand = side == PlayerSide.Player ? model.PlayerHand : model.OpponentHand;
            for (var i = 0; i < hand.Count; i++)
            {
                var left = PlayerCommand.Play(side, i, PileLane.Left);
                if (model.CanApplyCommand(left))
                {
                    command = left;
                    return true;
                }

                var right = PlayerCommand.Play(side, i, PileLane.Right);
                if (model.CanApplyCommand(right))
                {
                    command = right;
                    return true;
                }
            }

            command = default;
            return false;
        }

        private float NextDelaySeconds()
        {
            var jitter = (float)(_random.NextDouble() * 2.0 - 1.0) * _difficulty.ReactionJitterMs;
            var ms = Mathf.Max(50f, _difficulty.ReactionMeanMs + jitter);
            return ms / 1000f;
        }
    }
}
