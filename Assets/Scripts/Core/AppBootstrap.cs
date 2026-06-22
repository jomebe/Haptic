using Haptic.Audio;
using Haptic.Gameplay;
using Haptic.Haptics;
using Haptic.Save;
using Haptic.UI;
using UnityEngine;

namespace Haptic.Core
{
    public sealed class AppBootstrap : MonoBehaviour
    {
        HapticManager haptics;
        AudioManager audioManager;
        SaveSystem saves;
        GameUI ui;
        GameplayController gameplay;

        public SaveSystem Saves => saves;
        public GameplayController Gameplay => gameplay;
        public ScreenState State { get; private set; }

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            saves = new SaveSystem(LevelCatalog.Levels.Count);

            haptics = new GameObject("Haptic Manager").AddComponent<HapticManager>();
            audioManager = new GameObject("Audio Manager").AddComponent<AudioManager>();
            ui = new GameObject("Game UI").AddComponent<GameUI>();
            ui.Build(this);
            ApplySettings();
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            DestroyGameplay();
            State = ScreenState.MainMenu;
            ui.ShowMainMenu();
        }

        public void ShowLevelSelect()
        {
            DestroyGameplay();
            State = ScreenState.LevelSelect;
            ui.ShowLevelSelect();
        }

        public void ShowSettings()
        {
            State = ScreenState.Settings;
            ui.ShowSettings();
        }

        public void ShowCredits()
        {
            State = ScreenState.Credits;
            ui.ShowCredits();
        }

        public void StartGame()
        {
            StartLevel(Mathf.Clamp(saves.Data.unlockedLevel - 1, 0, LevelCatalog.Levels.Count - 1));
        }

        public void StartLevel(int index)
        {
            if (index < 0 || index >= LevelCatalog.Levels.Count || index + 1 > saves.Data.unlockedLevel)
                return;

            DestroyGameplay();
            gameplay = new GameObject("Gameplay").AddComponent<GameplayController>();
            gameplay.LevelCompleted += OnLevelCompleted;
            gameplay.PlayerFailed += ui.ShowFailureFlash;
            gameplay.Initialize(index, haptics, audioManager, saves);
            State = ScreenState.Playing;
            ui.ShowGameplay(gameplay);
        }

        public void Pause()
        {
            if (State != ScreenState.Playing)
                return;
            State = ScreenState.Paused;
            gameplay.SetPaused(true);
            ui.ShowPause();
        }

        public void Resume()
        {
            if (State != ScreenState.Paused)
                return;
            State = ScreenState.Playing;
            gameplay.SetPaused(false);
            ui.HideOverlay();
        }

        public void RestartLevel()
        {
            if (gameplay == null)
                return;
            State = ScreenState.Playing;
            gameplay.Restart();
            ui.ShowGameplay(gameplay);
        }

        public void NextLevel()
        {
            if (gameplay == null)
                return;
            int next = gameplay.LevelIndex + 1;
            if (next < LevelCatalog.Levels.Count)
                StartLevel(next);
            else
                ShowLevelSelect();
        }

        public void CycleHapticIntensity()
        {
            saves.Settings.hapticIntensity = (HapticIntensity)(((int)saves.Settings.hapticIntensity + 1) % 3);
            SaveAndRefreshSettings();
            haptics.HitWall();
        }

        public void CycleVisualAssist()
        {
            saves.Settings.visualAssist = (VisualAssist)(((int)saves.Settings.visualAssist + 1) % 3);
            SaveAndRefreshSettings();
        }

        public void ToggleSound()
        {
            saves.Settings.soundEnabled = !saves.Settings.soundEnabled;
            SaveAndRefreshSettings();
        }

        public void ToggleLeftHanded()
        {
            saves.Settings.leftHanded = !saves.Settings.leftHanded;
            SaveAndRefreshSettings();
        }

        public void ResetProgress()
        {
            saves.ResetProgress();
            ui.ShowSettings();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("Quit requested.");
#else
            Application.Quit();
#endif
        }

        void OnLevelCompleted(float time, bool newBest)
        {
            State = ScreenState.LevelComplete;
            ui.ShowLevelComplete(time, newBest, gameplay.LevelIndex == LevelCatalog.Levels.Count - 1);
        }

        void SaveAndRefreshSettings()
        {
            saves.Save();
            ApplySettings();
            ui.RefreshSettings();
        }

        void ApplySettings()
        {
            haptics.SetIntensity(saves.Settings.hapticIntensity);
            audioManager.SetEnabled(saves.Settings.soundEnabled);
            ui.ApplySettings(saves.Settings);
        }

        void DestroyGameplay()
        {
            if (gameplay == null)
                return;
            gameplay.LevelCompleted -= OnLevelCompleted;
            gameplay.PlayerFailed -= ui.ShowFailureFlash;
            haptics.Stop();
            Destroy(gameplay.gameObject);
            gameplay = null;
        }
    }
}

