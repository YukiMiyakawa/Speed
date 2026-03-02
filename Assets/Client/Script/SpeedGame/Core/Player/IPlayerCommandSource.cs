using SpeedGame.Domain;

namespace SpeedGame.Core.Player
{
    // PlayerAgent が参照する入力境界。CPU/ローカル/Photon受信を差し替えるための抽象。
    public interface IPlayerCommandSource
    {
        void Reset();
        // 実装側で任意の入力ソースからコマンドを取り出す。
        bool TryGetNextCommand(SpeedGameModel model, PlayerSide side, float deltaTime, out PlayerCommand command);
    }
}
