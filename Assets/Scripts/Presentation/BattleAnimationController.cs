using System;
using System.Collections;
using Speed.Domain;
using UnityEngine;

namespace Speed.Presentation
{
    public sealed class BattleAnimationController : MonoBehaviour
    {
        private const float PutDuration = 0.18f;
        private const float InvalidDuration = 0.16f;
        private const float RefreshDuration = 0.35f;

        public void PlayPutAnimation(CardView cardView, TablePileView pileView, Card playedCard, Action onComplete)
        {
            StartCoroutine(PlayPutAnimationRoutine(cardView, pileView, playedCard, onComplete));
        }

        public void PlayInvalidReturn(CardView cardView)
        {
            if (cardView != null)
            {
                StartCoroutine(PlayReturnRoutine(cardView, false));
            }
        }

        public void PlayCancelAnimation(CardView cardView)
        {
            if (cardView != null)
            {
                StartCoroutine(PlayReturnRoutine(cardView, true));
            }
        }

        public IEnumerator PlayPileRefreshAnimation(TablePileView leftPileView, TablePileView rightPileView, Card leftCard, Card rightCard)
        {
            leftPileView.SetBusy(true);
            rightPileView.SetBusy(true);
            leftPileView.SetPreviewCard(leftCard);
            rightPileView.SetPreviewCard(rightCard);

            var elapsed = 0f;
            while (elapsed < RefreshDuration)
            {
                elapsed += Time.deltaTime;
                var scale = 1f + Mathf.Sin((elapsed / RefreshDuration) * Mathf.PI) * 0.08f;
                leftPileView.transform.localScale = Vector3.one * scale;
                rightPileView.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            leftPileView.transform.localScale = Vector3.one;
            rightPileView.transform.localScale = Vector3.one;
            leftPileView.SetBusy(false);
            rightPileView.SetBusy(false);
        }

        private IEnumerator PlayPutAnimationRoutine(CardView cardView, TablePileView pileView, Card playedCard, Action onComplete)
        {
            if (cardView == null)
            {
                pileView.SetCard(playedCard);
                onComplete?.Invoke();
                yield break;
            }

            var start = cardView.RectTransform.position;
            var end = pileView.transform.position;
            var elapsed = 0f;
            cardView.SetInteractable(false);

            while (elapsed < PutDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / PutDuration);
                cardView.RectTransform.position = Vector3.Lerp(start, end, t);
                cardView.RectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.85f, t);
                yield return null;
            }

            pileView.SetCard(playedCard);
            Destroy(cardView.gameObject);
            onComplete?.Invoke();
        }

        private IEnumerator PlayReturnRoutine(CardView cardView, bool cancel)
        {
            var start = cardView.RectTransform.position;
            var end = cardView.OriginalPosition;
            var elapsed = 0f;
            var strength = cancel ? 18f : 8f;

            while (elapsed < InvalidDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / InvalidDuration);
                var offset = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t) * strength;
                cardView.RectTransform.position = Vector3.Lerp(start, end, t) + new Vector3(offset, 0f, 0f);
                yield return null;
            }

            cardView.ResetVisualState();
        }
    }
}
