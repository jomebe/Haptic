using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Haptic.UI
{
    public static class UiFactory
    {
        static Font font;
        public static readonly Color Background = new(0.015f, 0.025f, 0.045f, 1f);
        public static readonly Color Panel = new(0.035f, 0.065f, 0.095f, 0.94f);
        public static readonly Color Cyan = new(0.15f, 0.95f, 1f, 1f);
        public static readonly Color CyanDim = new(0.08f, 0.32f, 0.38f, 1f);
        public static readonly Color Magenta = new(1f, 0.16f, 0.5f, 1f);
        public static readonly Color White = new(0.88f, 0.96f, 1f, 1f);

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static Image Image(string name, Transform parent, Color color)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Text(string name, Transform parent, string value, int size, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        public static Button Button(string name, Transform parent, string label, UnityAction clicked,
            Color? accent = null)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = accent ?? Panel;
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.72f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.45f, 0.85f, 0.9f, 1f);
            colors.disabledColor = new Color(0.25f, 0.3f, 0.34f, 0.65f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.onClick.AddListener(clicked);
            Text text = Text("Label", rect, label, 28, White);
            text.rectTransform.offsetMin = new Vector2(12f, 6f);
            text.rectTransform.offsetMax = new Vector2(-12f, -6f);
            return button;
        }

        public static void AddVerticalLayout(GameObject target, float spacing, TextAnchor alignment = TextAnchor.UpperCenter)
        {
            var layout = target.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        public static void PreferredHeight(GameObject target, float height)
        {
            var element = target.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
        }

        static Font Font => font ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}

