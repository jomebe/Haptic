using System.Collections;
using System.Collections.Generic;
using Haptic.Core;
using Haptic.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Haptic.UI
{
    public sealed class GameUI : MonoBehaviour
    {
        AppBootstrap app;
        Canvas canvas;
        RectTransform safeRoot;
        PulseVisual pulse;
        GameObject mainPanel;
        GameObject levelPanel;
        GameObject settingsPanel;
        GameObject creditsPanel;
        GameObject gamePanel;
        GameObject overlay;
        GameObject tutorialCue;
        Text timerText;
        Text levelTitle;
        Text keyStatus;
        Text hapticSetting;
        Text visualSetting;
        Text soundSetting;
        Text handedSetting;
        readonly List<Button> levelButtons = new();
        readonly List<Text> levelLabels = new();
        MazeVisual mazeVisual;
        GameplayController gameplay;
        RectTransform joystickBase;
        RectTransform joystickKnob;
        TouchInputController touchInput;

        public void Build(AppBootstrap bootstrap)
        {
            app = bootstrap;
            DontDestroyOnLoad(gameObject);
            BuildCanvas();
            BuildBackground();
            BuildMainMenu();
            BuildLevelSelect();
            BuildSettings();
            BuildCredits();
            BuildGameplay();
            BuildOverlay();
        }

        public void ShowMainMenu()
        {
            UnbindGameplay();
            ShowOnly(mainPanel);
        }

        public void ShowLevelSelect()
        {
            RefreshLevelButtons();
            ShowOnly(levelPanel);
        }

        public void ShowSettings()
        {
            RefreshSettings();
            ShowOnly(settingsPanel);
        }

        public void ShowCredits() => ShowOnly(creditsPanel);

        public void ShowGameplay(GameplayController controller)
        {
            UnbindGameplay();
            gameplay = controller;
            gameplay.StateChanged += OnGameplayChanged;
            mazeVisual.Bind(gameplay, app.Saves.Settings.visualAssist);
            pulse.Bind(gameplay);
            touchInput.Initialize(gameplay, joystickKnob);
            levelTitle.text = $"{gameplay.LevelIndex + 1:00}  {gameplay.Level.Name}";
            ApplySettings(app.Saves.Settings);
            ShowOnly(gamePanel);
            HideOverlay();
            OnGameplayChanged();
            StopCoroutine(nameof(ShowTutorialCue));
            if (gameplay.Level.Tutorial != TutorialPattern.None)
                StartCoroutine(nameof(ShowTutorialCue));
        }

        public void ShowPause()
        {
            ShowOverlay("PAUSED", new[]
            {
                ("RESUME", (UnityEngine.Events.UnityAction)app.Resume),
                ("RESTART", app.RestartLevel),
                ("LEVELS", app.ShowLevelSelect)
            });
        }

        public void ShowLevelComplete(float time, bool newBest, bool finalLevel)
        {
            string message = newBest ? $"NEW BEST  {FormatTime(time)}" : $"CLEAR  {FormatTime(time)}";
            var actions = new List<(string, UnityEngine.Events.UnityAction)>
            {
                (finalLevel ? "LEVELS" : "NEXT SIGNAL", finalLevel
                    ? (UnityEngine.Events.UnityAction)app.ShowLevelSelect
                    : app.NextLevel),
                ("REPLAY", app.RestartLevel),
                ("MENU", app.ShowMainMenu)
            };
            ShowOverlay(message, actions.ToArray());
        }

        public void HideOverlay() => overlay.SetActive(false);

        public void ShowFailureFlash()
        {
            StartCoroutine(FailureFlash());
        }

        public void ApplySettings(GameSettings settings)
        {
            mazeVisual?.SetAssist(settings.visualAssist);
            if (joystickBase != null)
            {
                float x = settings.leftHanded ? 0.22f : 0.78f;
                joystickBase.anchorMin = joystickBase.anchorMax = new Vector2(x, 0.115f);
            }
        }

        public void RefreshSettings()
        {
            if (hapticSetting == null)
                return;
            GameSettings settings = app.Saves.Settings;
            hapticSetting.text = $"HAPTICS\n{settings.hapticIntensity.ToString().ToUpperInvariant()}";
            visualSetting.text = $"VISUAL ASSIST\n{settings.visualAssist.ToString().ToUpperInvariant()}";
            soundSetting.text = $"SOUND\n{(settings.soundEnabled ? "ON" : "OFF")}";
            handedSetting.text = $"CONTROL SIDE\n{(settings.leftHanded ? "LEFT" : "RIGHT")}";
        }

        void BuildCanvas()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            var raycaster = gameObject.AddComponent<GraphicRaycaster>();
            gameObject.AddComponent<DirectTouchButtonRouter>().Initialize(raycaster);

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("Event System", typeof(EventSystem));
                DontDestroyOnLoad(eventSystem);
            }

            safeRoot = UiFactory.Rect("Safe Area", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            safeRoot.gameObject.AddComponent<SafeAreaFitter>();
        }

        void BuildBackground()
        {
            Image background = UiFactory.Image("Void", transform, UiFactory.Background);
            background.rectTransform.SetAsFirstSibling();
            background.raycastTarget = false;

            RectTransform pulseRect = UiFactory.Rect("Pulse Field", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            pulseRect.SetSiblingIndex(1);
            pulse = pulseRect.gameObject.AddComponent<PulseVisual>();
            pulse.Build();
        }

        void BuildMainMenu()
        {
            mainPanel = CreateScreen("Main Menu");
            Text title = UiFactory.Text("Title", mainPanel.transform, "H A P T I C", 78, UiFactory.White);
            Place(title.rectTransform, 0.1f, 0.67f, 0.9f, 0.82f);
            title.fontStyle = FontStyle.Bold;
            Text subtitle = UiFactory.Text("Subtitle", mainPanel.transform, "FIND THE SIGNAL", 22, UiFactory.Cyan);
            Place(subtitle.rectTransform, 0.2f, 0.61f, 0.8f, 0.67f);

            RectTransform buttons = UiFactory.Rect("Actions", mainPanel.transform, new Vector2(0.17f, 0.16f), new Vector2(0.83f, 0.55f), Vector2.zero, Vector2.zero);
            UiFactory.AddVerticalLayout(buttons.gameObject, 18f);
            AddMenuButton(buttons, "START GAME", app.StartGame, 84f, true);
            AddMenuButton(buttons, "LEVEL SELECT", app.ShowLevelSelect, 72f);
            AddMenuButton(buttons, "SETTINGS", app.ShowSettings, 72f);
            AddMenuButton(buttons, "CREDITS", app.ShowCredits, 72f);
            AddMenuButton(buttons, "QUIT", app.Quit, 64f);
        }

        void BuildLevelSelect()
        {
            levelPanel = CreateScreen("Level Select");
            AddScreenTitle(levelPanel, "SIGNALS");
            for (int i = 0; i < LevelCatalog.Levels.Count; i++)
            {
                int captured = i;
                int column = i % 2;
                int row = i / 2;
                Button button = UiFactory.Button($"Level {i + 1}", levelPanel.transform, string.Empty, () => app.StartLevel(captured));
                Place(button.GetComponent<RectTransform>(), 0.09f + column * 0.43f, 0.70f - row * 0.125f,
                    0.48f + column * 0.43f, 0.80f - row * 0.125f);
                Text label = button.GetComponentInChildren<Text>();
                label.alignment = TextAnchor.MiddleLeft;
                levelButtons.Add(button);
                levelLabels.Add(label);
            }
            Button back = UiFactory.Button("Back", levelPanel.transform, "BACK", app.ShowMainMenu);
            Place(back.GetComponent<RectTransform>(), 0.28f, 0.05f, 0.72f, 0.115f);
        }

        void BuildSettings()
        {
            settingsPanel = CreateScreen("Settings");
            AddScreenTitle(settingsPanel, "CALIBRATION");
            hapticSetting = AddSettingButton("Haptic Intensity", 0.68f, app.CycleHapticIntensity);
            visualSetting = AddSettingButton("Visual Assist", 0.55f, app.CycleVisualAssist);
            soundSetting = AddSettingButton("Sound", 0.42f, app.ToggleSound);
            handedSetting = AddSettingButton("Handedness", 0.29f, app.ToggleLeftHanded);

            Button reset = UiFactory.Button("Reset", settingsPanel.transform, "RESET PROGRESS", ConfirmReset, new Color(0.28f, 0.06f, 0.12f, 0.95f));
            Place(reset.GetComponent<RectTransform>(), 0.18f, 0.16f, 0.82f, 0.225f);
            Button back = UiFactory.Button("Back", settingsPanel.transform, "BACK", app.ShowMainMenu);
            Place(back.GetComponent<RectTransform>(), 0.28f, 0.05f, 0.72f, 0.115f);
        }

        void BuildCredits()
        {
            creditsPanel = CreateScreen("Credits");
            AddScreenTitle(creditsPanel, "CREDITS");
            Text body = UiFactory.Text("Body", creditsPanel.transform,
                "DESIGN + CODE\nJOMEBE\n\nPROCEDURAL AUDIO\nGENERATED IN UNITY\n\nBUILT WITHOUT PAID ASSETS\n\nHAPTICS ARE THE MAP", 27, UiFactory.White);
            body.lineSpacing = 1.35f;
            Place(body.rectTransform, 0.12f, 0.22f, 0.88f, 0.70f);
            Button back = UiFactory.Button("Back", creditsPanel.transform, "BACK", app.ShowMainMenu);
            Place(back.GetComponent<RectTransform>(), 0.28f, 0.05f, 0.72f, 0.115f);
        }

        void BuildGameplay()
        {
            gamePanel = CreateScreen("Gameplay");
            Image touchLayer = UiFactory.Image("Swipe Surface", gamePanel.transform, Color.clear);
            touchLayer.raycastTarget = false;

            levelTitle = UiFactory.Text("Level", gamePanel.transform, string.Empty, 25, UiFactory.White, TextAnchor.MiddleLeft);
            Place(levelTitle.rectTransform, 0.06f, 0.89f, 0.72f, 0.96f);
            timerText = UiFactory.Text("Timer", gamePanel.transform, "00:00.00", 28, UiFactory.Cyan, TextAnchor.MiddleLeft);
            Place(timerText.rectTransform, 0.06f, 0.83f, 0.42f, 0.89f);
            keyStatus = UiFactory.Text("Key", gamePanel.transform, string.Empty, 24, new Color(1f, 0.82f, 0.22f, 1f), TextAnchor.MiddleRight);
            Place(keyStatus.rectTransform, 0.58f, 0.83f, 0.93f, 0.89f);

            RectTransform mazeRect = UiFactory.Rect("Signal Space", gamePanel.transform, new Vector2(0.06f, 0.23f), new Vector2(0.94f, 0.81f), Vector2.zero, Vector2.zero);
            mazeVisual = mazeRect.gameObject.AddComponent<MazeVisual>();

            Image baseImage = UiFactory.Image("Control Field", gamePanel.transform, new Color(0.05f, 0.27f, 0.32f, 0.42f));
            baseImage.raycastTarget = false;
            joystickBase = baseImage.rectTransform;
            joystickBase.anchorMin = joystickBase.anchorMax = new Vector2(0.78f, 0.115f);
            joystickBase.sizeDelta = Vector2.one * 170f;
            Image knobImage = UiFactory.Image("Control", joystickBase, new Color(0.16f, 0.93f, 1f, 0.72f));
            knobImage.raycastTarget = false;
            joystickKnob = knobImage.rectTransform;
            joystickKnob.anchorMin = joystickKnob.anchorMax = Vector2.one * 0.5f;
            joystickKnob.sizeDelta = Vector2.one * 62f;
            joystickKnob.anchoredPosition = Vector2.zero;
            touchInput = touchLayer.gameObject.AddComponent<TouchInputController>();

            Button restart = UiFactory.Button("Restart", gamePanel.transform, "RESTART", app.RestartLevel);
            Place(restart.GetComponent<RectTransform>(), 0.06f, 0.055f, 0.31f, 0.105f);
            Button pauseButton = UiFactory.Button("Pause", gamePanel.transform, "II", app.Pause);
            Place(pauseButton.GetComponent<RectTransform>(), 0.82f, 0.89f, 0.94f, 0.96f);

            Image cueImage = UiFactory.Image("Tutorial Cue", gamePanel.transform, new Color(0.02f, 0.08f, 0.11f, 0.95f));
            cueImage.raycastTarget = false;
            tutorialCue = cueImage.gameObject;
            Place((RectTransform)tutorialCue.transform, 0.24f, 0.42f, 0.76f, 0.59f);
            tutorialCue.AddComponent<CanvasGroup>();
            tutorialCue.SetActive(false);
        }

        void BuildOverlay()
        {
            overlay = UiFactory.Image("Overlay", safeRoot, new Color(0.005f, 0.012f, 0.025f, 0.94f)).gameObject;
            overlay.SetActive(false);
        }

        void ShowOverlay(string heading, (string label, UnityEngine.Events.UnityAction action)[] actions)
        {
            foreach (Transform child in overlay.transform)
                Destroy(child.gameObject);
            overlay.SetActive(true);
            overlay.transform.SetAsLastSibling();

            Text title = UiFactory.Text("Heading", overlay.transform, heading, 42, UiFactory.Cyan);
            Place(title.rectTransform, 0.12f, 0.61f, 0.88f, 0.73f);
            for (int i = 0; i < actions.Length; i++)
            {
                Button button = UiFactory.Button($"Action {i}", overlay.transform, actions[i].label, actions[i].action);
                float top = 0.55f - i * 0.095f;
                Place(button.GetComponent<RectTransform>(), 0.22f, top - 0.07f, 0.78f, top);
            }
        }

        void ConfirmReset()
        {
            ShowOverlay("ERASE ALL TIMES?", new[]
            {
                ("RESET", (UnityEngine.Events.UnityAction)app.ResetProgress),
                ("CANCEL", (UnityEngine.Events.UnityAction)HideOverlay)
            });
        }

        void ShowOnly(GameObject target)
        {
            mainPanel.SetActive(target == mainPanel);
            levelPanel.SetActive(target == levelPanel);
            settingsPanel.SetActive(target == settingsPanel);
            creditsPanel.SetActive(target == creditsPanel);
            gamePanel.SetActive(target == gamePanel);
            HideOverlay();
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            StartCoroutine(FadeIn(group));
        }

        IEnumerator FadeIn(CanvasGroup group)
        {
            float started = Time.unscaledTime;
            while (group != null && Time.unscaledTime - started < 0.22f)
            {
                group.alpha = Mathf.SmoothStep(0f, 1f, (Time.unscaledTime - started) / 0.22f);
                yield return null;
            }
            if (group != null) group.alpha = 1f;
        }

        IEnumerator ShowTutorialCue()
        {
            string cue = gameplay.Level.Tutorial switch
            {
                TutorialPattern.Wall => "WALL\n. . .",
                TutorialPattern.Goal => "SIGNAL\n~ ~ ~",
                TutorialPattern.Trap => "DANGER\n! . !!",
                TutorialPattern.Key => "KEY\n: :",
                _ => string.Empty
            };
            foreach (Transform child in tutorialCue.transform)
                Destroy(child.gameObject);
            UiFactory.Text("Pattern", tutorialCue.transform, cue, 34, UiFactory.Cyan);
            tutorialCue.SetActive(true);
            CanvasGroup group = tutorialCue.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            float started = Time.unscaledTime;
            while (Time.unscaledTime - started < 0.25f)
            {
                group.alpha = (Time.unscaledTime - started) / 0.25f;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(1.65f);
            tutorialCue.SetActive(false);
        }

        IEnumerator FailureFlash()
        {
            Image flash = UiFactory.Image("Failure", safeRoot, new Color(1f, 0.05f, 0.2f, 0f));
            flash.transform.SetAsLastSibling();
            flash.raycastTarget = false;
            float started = Time.unscaledTime;
            while (Time.unscaledTime - started < 0.55f)
            {
                float t = (Time.unscaledTime - started) / 0.55f;
                Color color = flash.color;
                color.a = Mathf.Sin(t * Mathf.PI) * 0.38f;
                flash.color = color;
                yield return null;
            }
            Destroy(flash.gameObject);
        }

        void OnGameplayChanged()
        {
            if (gameplay == null)
                return;
            timerText.text = FormatTime(gameplay.Elapsed);
            keyStatus.text = gameplay.Level.RequiresKey ? (gameplay.HasKey ? "KEY  READY" : "KEY  --") : string.Empty;
            mazeVisual.Refresh();
        }

        void UnbindGameplay()
        {
            if (gameplay != null)
                gameplay.StateChanged -= OnGameplayChanged;
            gameplay = null;
            pulse?.Bind(null);
        }

        void RefreshLevelButtons()
        {
            for (int i = 0; i < levelButtons.Count; i++)
            {
                bool unlocked = i + 1 <= app.Saves.Data.unlockedLevel;
                levelButtons[i].interactable = unlocked;
                float best = app.Saves.Data.bestTimes[i];
                string time = best > 0f ? FormatTime(best) : "--:--.--";
                levelLabels[i].text = unlocked
                    ? $"{i + 1:00}  {LevelCatalog.Levels[i].Name}\n      {time}"
                    : $"{i + 1:00}  LOCKED";
            }
        }

        Text AddSettingButton(string name, float bottom, UnityEngine.Events.UnityAction action)
        {
            Button button = UiFactory.Button(name, settingsPanel.transform, string.Empty, action);
            Place(button.GetComponent<RectTransform>(), 0.13f, bottom, 0.87f, bottom + 0.095f);
            return button.GetComponentInChildren<Text>();
        }

        static GameObject CreateScreenShell(string name)
        {
            var screen = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            return screen;
        }

        GameObject CreateScreenInstance(string name)
        {
            GameObject screen = CreateScreenShell(name);
            screen.transform.SetParent(safeRoot, false);
            var rect = (RectTransform)screen.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return screen;
        }

        void AddScreenTitle(GameObject screen, string value)
        {
            Text title = UiFactory.Text("Title", screen.transform, value, 52, UiFactory.White);
            title.fontStyle = FontStyle.Bold;
            Place(title.rectTransform, 0.1f, 0.84f, 0.9f, 0.94f);
        }

        static void AddMenuButton(RectTransform parent, string label, UnityEngine.Events.UnityAction action,
            float height, bool accent = false)
        {
            Button button = UiFactory.Button(label, parent, label, action, accent ? UiFactory.CyanDim : null);
            UiFactory.PreferredHeight(button.gameObject, height);
        }

        static void Place(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remaining = seconds - minutes * 60f;
            return $"{minutes:00}:{remaining:00.00}";
        }

        GameObject CreateScreen(string name)
        {
            return CreateScreenInstance(name);
        }
    }
}
