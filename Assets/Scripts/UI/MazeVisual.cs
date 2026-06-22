using System.Collections.Generic;
using Haptic.Core;
using Haptic.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Haptic.UI
{
    public sealed class MazeVisual : MonoBehaviour
    {
        readonly Dictionary<Vector2Int, Image> cellImages = new();
        GameplayController gameplay;
        VisualAssist assist;
        RectTransform playerMarker;
        float cellSize;

        public void Bind(GameplayController controller, VisualAssist visualAssist)
        {
            gameplay = controller;
            assist = visualAssist;
            Rebuild();
        }

        public void SetAssist(VisualAssist value)
        {
            assist = value;
            Refresh();
        }

        public void Refresh()
        {
            if (gameplay?.Maze == null)
                return;

            Vector2Int player = gameplay.PlayerPosition;
            foreach (var pair in cellImages)
            {
                int distance = Mathf.Abs(pair.Key.x - player.x) + Mathf.Abs(pair.Key.y - player.y);
                MazeRuntime.Cell cell = gameplay.Maze.GetCell(pair.Key);
                bool visible = assist == VisualAssist.Full || (assist == VisualAssist.Minimal && distance <= 2);
                Color color = CellColor(cell);
                color.a = visible ? (cell == MazeRuntime.Cell.Floor ? 0.10f : 0.62f) : 0f;
                pair.Value.color = color;
            }
            playerMarker.gameObject.SetActive(assist != VisualAssist.Off);
            playerMarker.anchoredPosition = PositionFor(player);
        }

        void Rebuild()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            cellImages.Clear();

            MazeRuntime maze = gameplay.Maze;
            var rect = (RectTransform)transform;
            float available = Mathf.Min(rect.rect.width > 0 ? rect.rect.width : 760f, rect.rect.height > 0 ? rect.rect.height : 760f);
            cellSize = Mathf.Floor(Mathf.Min(available / maze.Width, available / maze.Height) * 0.9f);

            for (int y = 0; y < maze.Height; y++)
            for (int x = 0; x < maze.Width; x++)
            {
                Vector2Int position = new(x, y);
                var image = UiFactory.Image($"Cell {x},{y}", transform, CellColor(maze.GetCell(position)));
                image.raycastTarget = false;
                image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
                image.rectTransform.sizeDelta = Vector2.one * Mathf.Max(2f, cellSize - 3f);
                image.rectTransform.anchoredPosition = PositionFor(position);
                cellImages[position] = image;
            }

            Image marker = UiFactory.Image("Position", transform, UiFactory.Cyan);
            marker.raycastTarget = false;
            playerMarker = marker.rectTransform;
            playerMarker.anchorMin = playerMarker.anchorMax = Vector2.one * 0.5f;
            playerMarker.sizeDelta = Vector2.one * Mathf.Max(8f, cellSize * 0.42f);
            playerMarker.SetAsLastSibling();
            Refresh();
        }

        Vector2 PositionFor(Vector2Int position)
        {
            float width = gameplay.Maze.Width * cellSize;
            float height = gameplay.Maze.Height * cellSize;
            return new Vector2(-width * 0.5f + cellSize * (position.x + 0.5f),
                -height * 0.5f + cellSize * (position.y + 0.5f));
        }

        static Color CellColor(MazeRuntime.Cell cell) => cell switch
        {
            MazeRuntime.Cell.Wall => new Color(0.08f, 0.36f, 0.42f, 0.7f),
            MazeRuntime.Cell.Trap => UiFactory.Magenta,
            MazeRuntime.Cell.Key => new Color(1f, 0.82f, 0.22f, 1f),
            MazeRuntime.Cell.Exit => new Color(0.25f, 1f, 0.6f, 1f),
            _ => new Color(0.22f, 0.5f, 0.56f, 0.12f)
        };
    }
}
