using System;

namespace SpeedGame.Domain
{
    [Serializable]
    public sealed class SpeedRuleSettings
    {
        public bool AllowAdjacent = true;
        public bool AllowSameRank = true;
        public bool AllowAceKingWrap = true;
        public bool WinByHandOnly = true;
    }
}
