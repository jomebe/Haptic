using System.Collections.Generic;
using Haptic.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace Haptic.Tests.Editor
{
    public sealed class LevelValidationTests
    {
        [Test]
        public void CatalogContainsTenPlayableLevels()
        {
            Assert.AreEqual(10, LevelCatalog.Levels.Count);
            foreach (LevelDefinition definition in LevelCatalog.Levels)
            {
                var maze = new MazeRuntime(definition);
                Assert.Less(maze.DistanceToExit(maze.Start), int.MaxValue, $"{definition.Name} exit is unreachable");
                if (definition.RequiresKey)
                    Assert.Less(maze.DistanceToNearest(maze.Start, MazeRuntime.Cell.Key), int.MaxValue, $"{definition.Name} has no key");
            }
        }

        [Test]
        public void RequiredKeysAreReachableWithoutEnteringExit()
        {
            foreach (LevelDefinition definition in LevelCatalog.Levels)
            {
                if (!definition.RequiresKey)
                    continue;
                var maze = new MazeRuntime(definition);
                Assert.IsTrue(CanReachKey(maze), $"{definition.Name} key is unreachable");
            }
        }

        static bool CanReachKey(MazeRuntime maze)
        {
            var visited = new HashSet<Vector2Int> { maze.Start };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(maze.Start);
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (maze.GetCell(current) == MazeRuntime.Cell.Key)
                    return true;
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int next = current + direction;
                    if (visited.Contains(next) || maze.GetCell(next) == MazeRuntime.Cell.Wall || maze.GetCell(next) == MazeRuntime.Cell.Exit)
                        continue;
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
            return false;
        }
    }
}

