using DG.Tweening;
using UnityEngine;

namespace SpeedGame.Config
{
    [CreateAssetMenu(menuName = "SpeedGame/Animation Profile", fileName = "SpeedAnimationProfile")]
    public sealed class SpeedAnimationProfile : ScriptableObject
    {
        [Header("Move To Foundation")]
        public float MoveDuration = 0.2f;
        public Ease MoveEase = Ease.OutCubic;

        [Header("Draw From Stock")]
        public float DrawDuration = 0.16f;
        public Ease DrawEase = Ease.OutSine;

        [Header("Bounce Back")]
        public float BounceDuration = 0.14f;
        public Ease BounceEase = Ease.OutBack;
        public float BounceScale = 1.07f;
    }
}
