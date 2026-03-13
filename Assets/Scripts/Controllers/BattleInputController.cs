using Speed.Application;
using Speed.Domain;
using Speed.Presentation;
using UnityEngine;

namespace Speed.Controllers
{
    public sealed class BattleInputController : MonoBehaviour
    {
        private const float MinDragDistance = 90f;
        private const float MaxDragAngle = 38f;

        private GameController gameController;

        public void Initialize(GameController controller)
        {
            gameController = controller;
        }

        public void HandleCardReleased(CardView cardView, Vector2 startScreenPosition, Vector2 endScreenPosition)
        {
            if (gameController == null || !gameController.CanAcceptInput())
            {
                gameController?.AnimationController.PlayInvalidReturn(cardView);
                return;
            }

            var dragVector = endScreenPosition - startScreenPosition;
            if (dragVector.magnitude < MinDragDistance)
            {
                gameController.AnimationController.PlayInvalidReturn(cardView);
                return;
            }

            if (!TryResolvePile(startScreenPosition, dragVector, out var pileId))
            {
                gameController.AnimationController.PlayInvalidReturn(cardView);
                return;
            }

            var result = gameController.TryPlayerPut(cardView.Card.Id, pileId, cardView);
            if (result == PutCardResult.BlockedByAnimation)
            {
                gameController.AnimationController.PlayCancelAnimation(cardView);
            }
        }

        private bool TryResolvePile(Vector2 startScreenPosition, Vector2 dragVector, out PileId pileId)
        {
            var leftPileView = gameController.GetPileView(PileId.Left);
            var rightPileView = gameController.GetPileView(PileId.Right);

            var leftCenter = RectTransformUtility.WorldToScreenPoint(null, leftPileView.transform.position);
            var rightCenter = RectTransformUtility.WorldToScreenPoint(null, rightPileView.transform.position);
            var leftAngle = Vector2.Angle(dragVector, leftCenter - startScreenPosition);
            var rightAngle = Vector2.Angle(dragVector, rightCenter - startScreenPosition);

            if (leftAngle > MaxDragAngle && rightAngle > MaxDragAngle)
            {
                pileId = PileId.Left;
                return false;
            }

            pileId = leftAngle <= rightAngle ? PileId.Left : PileId.Right;
            return true;
        }
    }
}
