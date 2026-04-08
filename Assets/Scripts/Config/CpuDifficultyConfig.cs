using UnityEngine;
using Speed.Application;

namespace Speed.Config
{
    [CreateAssetMenu(fileName = "CpuDifficultyConfig", menuName = "Speed/CpuDifficultyConfig")]
    public class CpuDifficultyConfig : ScriptableObject
    {
        [Header("Level 1 (Easiest)")]
        public float Level1ReactionMs = 650f;
        public float Level1MissRate   = 0.18f;

        [Header("Level 2")]
        public float Level2ReactionMs = 500f;
        public float Level2MissRate   = 0.12f;

        [Header("Level 3")]
        public float Level3ReactionMs = 380f;
        public float Level3MissRate   = 0.07f;

        [Header("Level 4")]
        public float Level4ReactionMs = 260f;
        public float Level4MissRate   = 0.03f;

        [Header("Level 5 (Hardest)")]
        public float Level5ReactionMs = 180f;
        public float Level5MissRate   = 0.01f;

        [Header("Miss Split (0=all false play, 1=all skip)")]
        [Range(0f, 1f)]
        public float LookAheadMissRatio = 0.5f;

        public CpuDifficultySettings GetSettings(int level)
        {
            float rt, mr;
            switch (level)
            {
                case 1:  rt = Level1ReactionMs; mr = Level1MissRate; break;
                case 2:  rt = Level2ReactionMs; mr = Level2MissRate; break;
                case 3:  rt = Level3ReactionMs; mr = Level3MissRate; break;
                case 4:  rt = Level4ReactionMs; mr = Level4MissRate; break;
                default: rt = Level5ReactionMs; mr = Level5MissRate; break;
            }
            return new CpuDifficultySettings(rt, mr, LookAheadMissRatio);
        }
    }
}
