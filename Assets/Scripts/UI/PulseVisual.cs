using Haptic.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Haptic.UI
{
    public sealed class PulseVisual : MonoBehaviour
    {
        readonly Image[] rings = new Image[4];
        GameplayController gameplay;
        Sprite ringSprite;

        public void Build()
        {
            ringSprite = CreateRingSprite();
            for (int i = 0; i < rings.Length; i++)
            {
                Image image = UiFactory.Image($"Pulse {i + 1}", transform, UiFactory.Cyan);
                image.sprite = ringSprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                RectTransform rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = Vector2.one * (160f + i * 118f);
                rings[i] = image;
            }
        }

        public void Bind(GameplayController controller) => gameplay = controller;

        void Update()
        {
            float speed = 0.45f;
            Color baseColor = UiFactory.Cyan;
            if (gameplay?.Maze != null)
            {
                int distance = gameplay.Maze.DistanceToExit(gameplay.PlayerPosition);
                speed = Mathf.Lerp(1.8f, 0.55f, Mathf.Clamp01(distance / 18f));
                if (gameplay.Maze.DistanceToNearest(gameplay.PlayerPosition, MazeRuntime.Cell.Trap) <= 2)
                    baseColor = UiFactory.Magenta;
            }

            for (int i = 0; i < rings.Length; i++)
            {
                float phase = Mathf.Repeat(Time.unscaledTime * speed + i * 0.22f, 1f);
                rings[i].rectTransform.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.24f, phase);
                Color color = baseColor;
                color.a = Mathf.Sin(phase * Mathf.PI) * (0.23f - i * 0.025f);
                rings[i].color = color;
            }
        }

        static Sprite CreateRingSprite()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Procedural Pulse Ring",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[size * size];
            Vector2 center = Vector2.one * (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.88f) * 45f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f);
        }
    }
}

