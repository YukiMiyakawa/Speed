using UnityEngine;

namespace Speed.View
{
    public class BattleLayout : MonoBehaviour
    {
        [Header("Center Piles")]
        public Transform LeftPileAnchor;
        public Transform RightPileAnchor;

        [Header("Player (Bottom)")]
        public Transform PlayerDeckAnchor;
        public Transform PlayerHandRoot;

        [Header("CPU (Top)")]
        public Transform CpuDeckAnchor;
        public Transform CpuHandRoot;

        [Header("Card Spacing")]
        public float HandCardSpacing = 1.1f;

        public Vector3 GetPlayerHandPosition(int index, int total) =>
            GetHandPosition(PlayerHandRoot.position, index, total);

        public Vector3 GetCpuHandPosition(int index, int total) =>
            GetHandPosition(CpuHandRoot.position, index, total);

        private Vector3 GetHandPosition(Vector3 center, int index, int total)
        {
            if (total <= 1) return center;
            float span = HandCardSpacing * (total - 1);
            float x    = center.x - span * 0.5f + index * HandCardSpacing;
            return new Vector3(x, center.y, center.z - index * 0.01f);
        }
    }
}
