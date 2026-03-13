using Speed.Controllers;
using Speed.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Speed.Presentation
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Text label;

        private BattleInputController inputController;
        private Canvas rootCanvas;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private Vector2 beginScreenPosition;

        public Card Card { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public Vector3 OriginalPosition { get; private set; }

        private void Awake()
        {
            RectTransform = (RectTransform)transform;
            canvasGroup = GetComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
        }

        public void Bind(Card card, BattleInputController controller)
        {
            Card = card;
            inputController = controller;
            label.text = GetRankLabel(card.Rank) + "\n" + card.Suit.ToString()[0];
            ResetVisualState();
        }

        public void SetInteractable(bool interactable)
        {
            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.alpha = interactable ? 1f : 0.92f;
        }

        public void ResetVisualState()
        {
            if (originalParent != null)
            {
                transform.SetParent(originalParent, false);
            }

            RectTransform.localScale = Vector3.one;
            RectTransform.anchoredPosition = Vector2.zero;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            OriginalPosition = RectTransform.position;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (inputController == null)
            {
                return;
            }

            originalParent = transform.parent;
            beginScreenPosition = eventData.position;
            OriginalPosition = RectTransform.position;
            transform.SetParent(rootCanvas.transform, true);
            SetInteractable(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SetInteractable(true);
            inputController.HandleCardReleased(this, beginScreenPosition, eventData.position);
        }

        private static string GetRankLabel(Rank rank)
        {
            return rank switch
            {
                Rank.A => "A",
                Rank.J => "J",
                Rank.Q => "Q",
                Rank.K => "K",
                _ => ((int)rank).ToString()
            };
        }
    }
}
