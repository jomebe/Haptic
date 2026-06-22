using System;
using System.Collections.Generic;
using UnityEngine;

namespace Haptic.Gameplay
{
    public sealed class MazeRuntime
    {
        public enum Cell { Floor, Wall, Trap, Key, Exit }

        readonly Cell[,] cells;
        readonly Dictionary<Vector2Int, int> distanceToExit = new();

        public int Width { get; }
        public int Height { get; }
        public Vector2Int Start { get; }
        public Vector2Int Exit { get; }
        public bool RequiresKey { get; }

        public MazeRuntime(LevelDefinition definition)
        {
            if (definition.Rows == null || definition.Rows.Length == 0)
                throw new ArgumentException("Level has no maze rows.");

            Height = definition.Rows.Length;
            Width = definition.Rows[0].Length;
            RequiresKey = definition.RequiresKey;
            cells = new Cell[Width, Height];
            bool foundStart = false;
            bool foundExit = false;

            for (int row = 0; row < Height; row++)
            {
                if (definition.Rows[row].Length != Width)
                    throw new ArgumentException($"Level row {row} has inconsistent width.");

                for (int x = 0; x < Width; x++)
                {
                    int y = Height - row - 1;
                    char symbol = definition.Rows[row][x];
                    cells[x, y] = symbol switch
                    {
                        '#' => Cell.Wall,
                        'T' => Cell.Trap,
                        'K' => Cell.Key,
                        'E' => Cell.Exit,
                        _ => Cell.Floor
                    };

                    if (symbol == 'S') { Start = new Vector2Int(x, y); foundStart = true; }
                    if (symbol == 'E') { Exit = new Vector2Int(x, y); foundExit = true; }
                }
            }

            if (!foundStart || !foundExit)
                throw new ArgumentException("Level requires one start and one exit.");

            BuildDistanceField();
        }

        public Cell GetCell(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= Width || position.y >= Height)
                return Cell.Wall;
            return cells[position.x, position.y];
        }

        public void CollectKey(Vector2Int position)
        {
            if (GetCell(position) == Cell.Key)
                cells[position.x, position.y] = Cell.Floor;
        }

        public int DistanceToExit(Vector2Int position) =>
            distanceToExit.TryGetValue(position, out int distance) ? distance : int.MaxValue;

        public int DistanceToNearest(Vector2Int position, Cell target)
        {
            int best = int.MaxValue;
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (cells[x, y] == target)
                    best = Mathf.Min(best, Mathf.Abs(position.x - x) + Mathf.Abs(position.y - y));
            return best;
        }

        public int AdjacentWallCount(Vector2Int position)
        {
            int count = 0;
            foreach (Vector2Int direction in Directions)
                if (GetCell(position + direction) == Cell.Wall)
                    count++;
            return count;
        }

        void BuildDistanceField()
        {
            var queue = new Queue<Vector2Int>();
            distanceToExit[Exit] = 0;
            queue.Enqueue(Exit);
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int next = current + direction;
                    if (GetCell(next) == Cell.Wall || distanceToExit.ContainsKey(next))
                        continue;
                    distanceToExit[next] = distanceToExit[current] + 1;
                    queue.Enqueue(next);
                }
            }
        }

        static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };
    }
}
