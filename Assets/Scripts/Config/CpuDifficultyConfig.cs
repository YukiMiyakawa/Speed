using System;
using System.Collections.Generic;
using Speed.Application;
using UnityEngine;

namespace Speed.Config
{
    [CreateAssetMenu(menuName = "Speed/Cpu Difficulty Config", fileName = "CpuDifficultyConfig")]
    public sealed class CpuDifficultyConfig : ScriptableObject
    {
        [SerializeField] private List<Entry> entries = new List<Entry>
        {
            new Entry(1, 0.65f, 0.18f),
            new Entry(2, 0.50f, 0.12f),
            new Entry(3, 0.38f, 0.07f),
            new Entry(4, 0.26f, 0.03f),
            new Entry(5, 0.18f, 0.01f)
        };

        public CpuDifficultySettings GetSettings(int level)
        {
            foreach (var entry in entries)
            {
                if (entry.Level == level)
                {
                    return new CpuDifficultySettings(entry.ReactionSeconds, entry.MistakeRate);
                }
            }

            return CreateDefaultSettings(level);
        }

        public static CpuDifficultySettings CreateDefaultSettings(int level)
        {
            return level switch
            {
                1 => new CpuDifficultySettings(0.65f, 0.18f),
                2 => new CpuDifficultySettings(0.50f, 0.12f),
                4 => new CpuDifficultySettings(0.26f, 0.03f),
                5 => new CpuDifficultySettings(0.18f, 0.01f),
                _ => new CpuDifficultySettings(0.38f, 0.07f)
            };
        }

        [Serializable]
        private sealed class Entry
        {
            public Entry(int level, float reactionSeconds, float mistakeRate)
            {
                Level = level;
                ReactionSeconds = reactionSeconds;
                MistakeRate = mistakeRate;
            }

            [field: SerializeField] public int Level { get; private set; }
            [field: SerializeField] public float ReactionSeconds { get; private set; }
            [field: SerializeField] public float MistakeRate { get; private set; }
        }
    }
}
