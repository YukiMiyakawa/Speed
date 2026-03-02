using Cysharp.Threading.Tasks;

namespace SpeedGame.Core.Player
{
    /// <summary>
    /// PlayerAgent 内部ステートの共通インターフェース。
    /// </summary>
    public interface IPlayerAgentState
    {
        /// <summary>ステート遷移で入場した際に1回呼ばれる。</summary>
        UniTask EnterAsync();
        /// <summary>ステート遷移で退場する際に1回呼ばれる。</summary>
        UniTask ExitAsync();
        /// <summary>毎フレーム呼ばれる更新処理。</summary>
        UniTask TickAsync(float deltaTime);
    }
}
