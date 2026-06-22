using UnityEngine;

namespace Haptic.UI
{
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        Rect lastSafeArea;
        Vector2Int lastScreen;

        void Update()
        {
            if (lastSafeArea == Screen.safeArea && lastScreen.x == Screen.width && lastScreen.y == Screen.height)
                return;
            Apply();
        }

        void Apply()
        {
            lastSafeArea = Screen.safeArea;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            rect.anchorMin = lastSafeArea.position / lastScreen;
            rect.anchorMax = (lastSafeArea.position + lastSafeArea.size) / lastScreen;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
