using System;

namespace Speed.Application
{
    [Serializable]
    public class CpuDifficultySettings
    {
        public float ReactionTimeMs;     // ms to wait before deciding
        public float MissRate;           // 0..1 combined miss probability
        public float LookAheadMissRatio; // 0..1 fraction of misses that are look-ahead (skip)

        public CpuDifficultySettings(float reactionTimeMs, float missRate, float lookAheadMissRatio)
        {
            ReactionTimeMs     = reactionTimeMs;
            MissRate           = missRate;
            LookAheadMissRatio = lookAheadMissRatio;
        }
    }
}
