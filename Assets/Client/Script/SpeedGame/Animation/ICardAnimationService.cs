using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SpeedGame.Animation
{
    public interface ICardAnimationService
    {
        UniTask MoveToFoundationAsync(RectTransform card, RectTransform lane, CancellationToken ct);
        UniTask DrawFromStockAsync(RectTransform card, RectTransform stock, RectTransform handSlot, CancellationToken ct);
        UniTask BounceBackAsync(RectTransform card, Vector2 origin, CancellationToken ct);
    }
}
