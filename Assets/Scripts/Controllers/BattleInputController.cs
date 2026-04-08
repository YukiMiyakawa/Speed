using UnityEngine;
using Speed.Domain;
using Speed.View;

namespace Speed.Controllers
{
    public class BattleInputController : MonoBehaviour
    {
        [Header("References")]
        public BattleView BattleView;
        public Camera     GameCamera;

        [Header("Drag Thresholds")]
        [Tooltip("Minimum drag distance (world units) to attempt a card play")]
        public float MinDragDistance = 0.8f;
        [Tooltip("Maximum angle (degrees) between drag direction and pile direction")]
        public float MaxAngleDegrees = 55f;

        private bool     _isActive;
        private CardView _draggedCard;
        private int      _draggedHandIndex = -1;
        private Vector3  _dragStartWorld;

        private GameController GameController => _gc != null ? _gc : (_gc = GetComponent<GameController>());
        private GameController _gc;

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active) CancelDrag();
        }

        private void Update()
        {
            if (!_isActive) return;

#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouse();
#else
            HandleTouch();
#endif
        }

        // ---- Mouse (Editor / Desktop) ----
        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
                TryBeginDrag(ScreenToWorld(Input.mousePosition));
            else if (Input.GetMouseButton(0) && _draggedCard != null)
                UpdateDrag(ScreenToWorld(Input.mousePosition));
            else if (Input.GetMouseButtonUp(0) && _draggedCard != null)
                EndDrag(ScreenToWorld(Input.mousePosition));
        }

        // ---- Touch ----
        private void HandleTouch()
        {
            if (Input.touchCount == 0) return;
            var t = Input.GetTouch(0);
            var w = ScreenToWorld(t.position);
            switch (t.phase)
            {
                case TouchPhase.Began:     TryBeginDrag(w);                              break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary: if (_draggedCard != null) UpdateDrag(w);    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:   if (_draggedCard != null) EndDrag(w);       break;
            }
        }

        // ---- Drag lifecycle ----
        private void TryBeginDrag(Vector3 worldPos)
        {
            if (GameController.IsPlayerFouled) return;

            var hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var cv = hit.GetComponentInParent<CardView>();
            if (cv == null) return;

            int idx = BattleView.GetPlayerHandIndex(cv);
            if (idx < 0) return;

            // お手付き: touching own hand cards during stalemate flip
            if (GameController.Phase == BattlePhase.StalemateFlipping)
            {
                GameController.TriggerPlayerFoul();
                return;
            }

            _draggedCard      = cv;
            _draggedHandIndex = idx;
            _dragStartWorld   = worldPos;
            _draggedCard.SetDragging(true);
            _draggedCard.SetSortingOrder(50);
            GameController.NotifyPlayerDragging();
        }

        private void UpdateDrag(Vector3 worldPos)
        {
            if (GameController.IsPlayerFouled) { CancelDrag(); return; }
            _draggedCard.SetPosition(worldPos);
            GameController.NotifyPlayerDragging();
        }

        private void EndDrag(Vector3 worldPos)
        {
            _draggedCard.SetDragging(false);
            _draggedCard.SetSortingOrder(0);

            float dragDist = (worldPos - _dragStartWorld).magnitude;
            if (dragDist >= MinDragDistance)
            {
                var  dragVec  = (worldPos - _dragStartWorld).normalized;
                var  leftPos  = BattleView.GetPileWorldPosition(PileId.Left);
                var  rightPos = BattleView.GetPileWorldPosition(PileId.Right);
                var  toLeft   = (leftPos  - _dragStartWorld).normalized;
                var  toRight  = (rightPos - _dragStartWorld).normalized;
                float leftAngle  = Vector3.Angle(dragVec, toLeft);
                float rightAngle = Vector3.Angle(dragVec, toRight);

                PileId? target = null;
                float   best   = MaxAngleDegrees;
                if (leftAngle  < best) { best = leftAngle;  target = PileId.Left;  }
                if (rightAngle < best) {                     target = PileId.Right; }

                if (target.HasValue)
                {
                    var result = GameController.TryPlayerPutCard(_draggedHandIndex, target.Value);
                    if (!result.IsSuccess)
                        BattleView.AnimateCardBounceBack(_draggedCard);

                    _draggedCard      = null;
                    _draggedHandIndex = -1;
                    return;
                }
            }

            BattleView.AnimateCardBounceBack(_draggedCard);
            _draggedCard      = null;
            _draggedHandIndex = -1;
        }

        private void CancelDrag()
        {
            if (_draggedCard == null) return;
            _draggedCard.SetDragging(false);
            BattleView.AnimateCardBounceBack(_draggedCard);
            _draggedCard      = null;
            _draggedHandIndex = -1;
        }

        private Vector3 ScreenToWorld(Vector2 screen)
        {
            var cam = GameCamera != null ? GameCamera : Camera.main;
            var wp  = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, cam.nearClipPlane));
            return new Vector3(wp.x, wp.y, 0f);
        }
    }
}
