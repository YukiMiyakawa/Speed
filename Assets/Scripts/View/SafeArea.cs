using UnityEngine;

namespace Speed.View
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _rt;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        private void Apply()
        {
            var area = Screen.safeArea;
            var min  = new Vector2(area.xMin / Screen.width,  area.yMin / Screen.height);
            var max  = new Vector2(area.xMax / Screen.width,  area.yMax / Screen.height);
            _rt.anchorMin = min;
            _rt.anchorMax = max;
        }
    }
}
