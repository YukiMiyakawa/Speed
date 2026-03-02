using System;

namespace SpeedGame.Domain
{
    [Serializable]
    public readonly struct PlayerCommand
    {
        public readonly PlayerSide Side;
        public readonly int HandIndex;
        public readonly PileLane Lane;
        public readonly bool IsDrawRequest;
        public readonly DateTimeOffset Timestamp;

        private PlayerCommand(PlayerSide side, int handIndex, PileLane lane, bool isDrawRequest, DateTimeOffset timestamp)
        {
            Side = side;
            HandIndex = handIndex;
            Lane = lane;
            IsDrawRequest = isDrawRequest;
            Timestamp = timestamp;
        }

        public static PlayerCommand Play(PlayerSide side, int handIndex, PileLane lane)
        {
            return new PlayerCommand(side, handIndex, lane, false, DateTimeOffset.UtcNow);
        }

        public static PlayerCommand Draw(PlayerSide side)
        {
            return new PlayerCommand(side, -1, PileLane.Left, true, DateTimeOffset.UtcNow);
        }
    }
}
