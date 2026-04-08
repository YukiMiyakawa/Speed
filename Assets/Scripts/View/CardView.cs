using UnityEngine;
using DG.Tweening;
using Speed.Domain;

namespace Speed.View
{
    public class CardView : MonoBehaviour
    {
        private Transform _frontTransform;
        private Transform _backTransform;
        private Vector3   _homePosition;

        public Card CardData   { get; private set; }
        public bool IsDragging { get; private set; }

        /// <summary>
        /// Called after instantiating the card prefab as a child.
        /// Finds "Front" and "Back" children automatically.
        /// </summary>
        public void Setup(Card card, GameObject prefabInstance)
        {
            CardData = card;
            if (prefabInstance == null) return;
            foreach (Transform child in prefabInstance.transform)
            {
                if (child.name.StartsWith("Front")) _frontTransform = child;
                else if (child.name.StartsWith("Back")) _backTransform = child;
            }
        }

        public void SetFaceUp(bool faceUp)
        {
            if (_frontTransform != null) _frontTransform.gameObject.SetActive(faceUp);
            if (_backTransform  != null) _backTransform.gameObject.SetActive(!faceUp);
        }

        public void SetHomePosition(Vector3 pos)
        {
            _homePosition      = pos;
            transform.position = pos;
        }

        public Vector3 HomePosition => _homePosition;

        public void SetDragging(bool dragging) => IsDragging = dragging;

        public void SetPosition(Vector3 pos) => transform.position = pos;

        public void SetSortingOrder(int order)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
                sr.sortingOrder = order;
        }

        public Tween MoveTo(Vector3 target, float duration) =>
            transform.DOMove(target, duration).SetEase(Ease.OutCubic);

        public Tween MoveToHome(float duration) =>
            transform.DOMove(_homePosition, duration).SetEase(Ease.OutBack);

        public Tween BounceBack(float duration) =>
            transform.DOMove(_homePosition, duration).SetEase(Ease.OutBack);

        /// <summary>Short false-play animation: move toward pile then snap back.</summary>
        public Tween FalsePlayAnimation(Vector3 pilePos, float duration)
        {
            var seq = DOTween.Sequence();
            var mid = Vector3.Lerp(transform.position, pilePos, 0.25f);
            seq.Append(transform.DOMove(mid, duration * 0.35f).SetEase(Ease.OutCubic));
            seq.Append(transform.DOMove(_homePosition, duration * 0.65f).SetEase(Ease.OutBack));
            return seq;
        }
    }
}
