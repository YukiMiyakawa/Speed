using System;

namespace SpeedGame.Domain
{
    [Serializable]
    public sealed class SpeedTimingSettings
    {
        public float SimultaneousWindowMs = 120f;
        public float StuckResetSeconds = 1.0f;
        public bool RefillAfterReset = false;
    }
}
