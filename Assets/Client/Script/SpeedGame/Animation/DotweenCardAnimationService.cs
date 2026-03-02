using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SpeedGame.Config;
using UnityEngine;

namespace SpeedGame.Animation
{
    public sealed class DotweenCardAnimationService : ICardAnimationService
    {
        private readonly SpeedAnimationProfile _profile;

        public DotweenCardAnimationService(SpeedAnimationProfile profile)
        {
            _profile = profile;
        }

        public UniTask MoveToFoundationAsync(RectTransform card, RectTransform lane, CancellationToken ct)
        {
            if (card == null || lane == null)
            {
                return UniTask.CompletedTask;
            }

            card.DOKill();
            return card.DOMove(lane.position, _profile.MoveDuration)
                .SetEase(_profile.MoveEase)
                .ToUniTask(cancellationToken: ct);
        }

        public UniTask DrawFromStockAsync(RectTransform card, RectTransform stock, RectTransform handSlot, CancellationToken ct)
        {
            if (card == null || stock == null || handSlot == null)
            {
                return UniTask.CompletedTask;
            }

            card.position = stock.position;
            card.DOKill();
            return card.DOMove(handSlot.position, _profile.DrawDuration)
                .SetEase(_profile.DrawEase)
                .ToUniTask(cancellationToken: ct);
        }

        public async UniTask BounceBackAsync(RectTransform card, Vector2 origin, CancellationToken ct)
        {
            if (card == null)
            {
                return;
            }

            card.DOKill();
            var sequence = DOTween.Sequence();
            sequence.Join(card.DOAnchorPos(origin, _profile.BounceDuration).SetEase(_profile.BounceEase));
            sequence.Join(card.DOScale(_profile.BounceScale, _profile.BounceDuration * 0.5f));
            sequence.Append(card.DOScale(1f, _profile.BounceDuration * 0.5f));
            await sequence.ToUniTask(cancellationToken: ct);
        }
    }
}
