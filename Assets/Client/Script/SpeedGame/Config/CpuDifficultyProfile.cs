using UnityEngine;

namespace SpeedGame.Config
{
    [CreateAssetMenu(menuName = "SpeedGame/CPU Difficulty", fileName = "CpuDifficultyProfile")]
    public sealed class CpuDifficultyProfile : ScriptableObject
    {
        [Range(1, 5)] public int Level = 1;
        public float ReactionMeanMs = 650f;
        public float ReactionJitterMs = 220f;
        [Range(0f, 1f)] public float MistakeRate = 0.18f;
    }
}
