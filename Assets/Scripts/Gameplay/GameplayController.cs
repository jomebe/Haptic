using System;
using System.Collections;
using Haptic.Audio;
using Haptic.Haptics;
using Haptic.Save;
using UnityEngine;

namespace Haptic.Gameplay
{
    public sealed class GameplayController : MonoBehaviour
    {
        HapticManager haptics;
        AudioManager audioManager;
        SaveSystem saveSystem;
        float feedbackAt;
        bool acceptingInput;
        bool isPaused;

        public event Action StateChanged;
        public event Action<float, bool> LevelCompleted;
        public event Action PlayerFailed;

        public MazeRuntime Maze { get; private set; }
        public LevelDefinition Level { get; private set; }
        public Vector2Int PlayerPosition { get; private set; }
        public bool HasKey { get; private set; }
        public float Elapsed { get; private set; }
        public int LevelIndex { get; private set; }
        public bool IsRunning => acceptingInput && !isPaused;

        public void Initialize(int levelIndex, HapticManager hapticManager, AudioManager audio, SaveSystem saves)
        {
            LevelIndex = levelIndex;
            Level = LevelCatalog.Levels[levelIndex];
            haptics = hapticManager;
            audioManager = audio;
            saveSystem = saves;
            Restart();
        }

        public void Restart()
        {
            StopAllCoroutines();
            Maze = new MazeRuntime(Level);
            PlayerPosition = Maze.Start;
            HasKey = false;
            Elapsed = 0f;
            isPaused = false;
            acceptingInput = true;
            feedbackAt = Time.unscaledTime + 0.55f;
            StateChanged?.Invoke();
            StartCoroutine(PlayTutorialCue());
        }

        public void SetPaused(bool value)
        {
            isPaused = value;
            if (value) haptics.Stop();
        }

        void Update()
        {
            if (!IsRunning)
                return;

            Elapsed += Time.deltaTime;
            if (Time.unscaledTime >= feedbackAt)
            {
                PlayEnvironmentalFeedback();
                int distance = Maze.DistanceToExit(PlayerPosition);
                feedbackAt = Time.unscaledTime + Mathf.Lerp(0.34f, 0.92f, Mathf.Clamp01(distance / 14f));
            }
            StateChanged?.Invoke();
        }

        public void TryMove(Vector2Int direction)
        {
            if (!IsRunning || (direction != Vector2Int.up && direction != Vector2Int.down &&
                               direction != Vector2Int.left && direction != Vector2Int.right))
                return;

            int previousDistance = Maze.DistanceToExit(PlayerPosition);
            Vector2Int target = PlayerPosition + direction;
            MazeRuntime.Cell cell = Maze.GetCell(target);

            if (cell == MazeRuntime.Cell.Wall)
            {
                haptics.HitWall();
                audioManager.Error();
                StateChanged?.Invoke();
                return;
            }

            PlayerPosition = target;
            audioManager.Move();

            if (cell == MazeRuntime.Cell.Trap)
            {
                StartCoroutine(FailAndRestart());
                return;
            }

            if (cell == MazeRuntime.Cell.Key)
            {
                HasKey = true;
                Maze.CollectKey(target);
                haptics.KeyCollected();
                audioManager.Key();
            }
            else if (cell == MazeRuntime.Cell.Exit)
            {
                if (Maze.RequiresKey && !HasKey)
                {
                    haptics.LockedExit();
                    audioManager.Error();
                }
                else
                {
                    CompleteLevel();
                    return;
                }
            }
            else
            {
                int newDistance = Maze.DistanceToExit(PlayerPosition);
                if (newDistance < previousDistance)
                    haptics.CorrectDirection(newDistance);
                else
                    haptics.WrongDirection();
            }

            feedbackAt = Mathf.Max(feedbackAt, Time.unscaledTime + 0.18f);
            StateChanged?.Invoke();
        }

        void CompleteLevel()
        {
            acceptingInput = false;
            haptics.GoalReached();
            audioManager.Success();
            float previousBest = saveSystem.Data.bestTimes[LevelIndex];
            bool newBest = previousBest <= 0f || Elapsed < previousBest;
            saveSystem.CompleteLevel(LevelIndex, Elapsed);
            StateChanged?.Invoke();
            LevelCompleted?.Invoke(Elapsed, newBest);
        }

        IEnumerator FailAndRestart()
        {
            acceptingInput = false;
            haptics.Failure();
            audioManager.Error();
            PlayerFailed?.Invoke();
            StateChanged?.Invoke();
            yield return new WaitForSecondsRealtime(1.15f);
            Restart();
        }

        void PlayEnvironmentalFeedback()
        {
            int trapDistance = Maze.DistanceToNearest(PlayerPosition, MazeRuntime.Cell.Trap);
            int keyDistance = HasKey ? int.MaxValue : Maze.DistanceToNearest(PlayerPosition, MazeRuntime.Cell.Key);
            int goalDistance = Maze.DistanceToExit(PlayerPosition);

            if (trapDistance <= 2)
                haptics.TrapNearby();
            else if (keyDistance <= 3)
                haptics.KeyNearby();
            else if (goalDistance <= 7)
                haptics.GoalNearby(goalDistance);
            else if (Maze.AdjacentWallCount(PlayerPosition) > 0)
                haptics.WallNearby();
            else
                haptics.GoalNearby(goalDistance);
        }

        IEnumerator PlayTutorialCue()
        {
            if (Level.Tutorial == TutorialPattern.None)
                yield break;
            yield return new WaitForSecondsRealtime(0.25f);
            switch (Level.Tutorial)
            {
                case TutorialPattern.Wall: haptics.WallNearby(); break;
                case TutorialPattern.Goal: haptics.GoalNearby(4); break;
                case TutorialPattern.Trap: haptics.TrapNearby(); break;
                case TutorialPattern.Key: haptics.KeyNearby(); break;
            }
        }
    }
}

