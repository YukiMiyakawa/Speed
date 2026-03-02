using System;

namespace SpeedGame.Domain
{
    [Serializable]
    /// <summary>
    /// プレイヤー入力を表すコマンド。オンライン時は送受信の基本単位として扱う。
    /// </summary>
    public readonly struct PlayerCommand
    {
        /// <summary>コマンド発行者のサイド。</summary>
        public readonly PlayerSide Side;
        /// <summary>手札インデックス。ドロー要求時は -1。</summary>
        public readonly int HandIndex;
        /// <summary>対象レーン。ドロー要求時は未使用。</summary>
        public readonly PileLane Lane;
        /// <summary>山札ドロー要求かどうか。</summary>
        public readonly bool IsDrawRequest;
        /// <summary>コマンド生成時刻（UTC）。</summary>
        public readonly DateTimeOffset Timestamp;

        private PlayerCommand(PlayerSide side, int handIndex, PileLane lane, bool isDrawRequest, DateTimeOffset timestamp)
        {
            Side = side;
            HandIndex = handIndex;
            Lane = lane;
            IsDrawRequest = isDrawRequest;
            Timestamp = timestamp;
        }

        /// <summary>カードを台札へ出すコマンドを生成する。</summary>
        public static PlayerCommand Play(PlayerSide side, int handIndex, PileLane lane)
        {
            return new PlayerCommand(side, handIndex, lane, false, DateTimeOffset.UtcNow);
        }

        /// <summary>山札から補充するコマンドを生成する。</summary>
        public static PlayerCommand Draw(PlayerSide side)
        {
            return new PlayerCommand(side, -1, PileLane.Left, true, DateTimeOffset.UtcNow);
        }
    }
}
