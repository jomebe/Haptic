using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Haptic.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Haptic.UI;

namespace Haptic.Tests.PlayMode
{
    public sealed class AppSmokeTests
    {
        [UnityTest]
        public IEnumerator AppLoadsMenuAndStartsFirstLevel()
        {
            SceneManager.LoadScene("Haptic");
            yield return null;
            yield return null;

            AppBootstrap app = Object.FindFirstObjectByType<AppBootstrap>();
            Assert.IsNotNull(app);
            Assert.AreEqual(ScreenState.MainMenu, app.State);
            Assert.IsNotNull(Object.FindFirstObjectByType<Canvas>());

            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            Assert.IsNotNull(eventSystem);
            Assert.IsNotNull(eventSystem.GetComponent<StandaloneInputModule>());
            GraphicRaycaster raycaster = Object.FindFirstObjectByType<GraphicRaycaster>();
            Assert.IsNotNull(raycaster);

            string[] menuNames = { "START GAME", "LEVEL SELECT", "SETTINGS", "CREDITS", "QUIT" };
            foreach (string menuName in menuNames)
            {
                Button menuButton = GameObject.Find(menuName).GetComponent<Button>();
                Assert.IsTrue(menuButton.gameObject.activeInHierarchy, $"{menuName} is hidden");
                RectTransform rect = menuButton.GetComponent<RectTransform>();
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                Assert.Greater(corners[2].x - corners[0].x, Screen.width * 0.5f, $"{menuName} width is invalid");
                Assert.Greater(corners[2].y - corners[0].y, Screen.height * 0.06f, $"{menuName} height is invalid");
                Assert.GreaterOrEqual(corners[0].x, 0f, $"{menuName} is left of screen");
                Assert.GreaterOrEqual(corners[0].y, 0f, $"{menuName} is below screen");
                Assert.LessOrEqual(corners[2].x, Screen.width, $"{menuName} is right of screen");
                Assert.LessOrEqual(corners[2].y, Screen.height, $"{menuName} is above screen");
            }

            Button startButton = GameObject.Find("START GAME").GetComponent<Button>();
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            Vector2 startScreenPoint = RectTransformUtility.WorldToScreenPoint(null, startRect.TransformPoint(startRect.rect.center));
            var pointer = new PointerEventData(eventSystem)
            {
                position = startScreenPoint,
                button = PointerEventData.InputButton.Left
            };
            var hits = new List<RaycastResult>();
            raycaster.Raycast(pointer, hits);
            Assert.IsTrue(hits.Any(hit => hit.gameObject.GetComponentInParent<Button>() == startButton));
            ExecuteEvents.Execute(startButton.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            yield return null;
            Assert.AreEqual(ScreenState.Playing, app.State);
            Assert.IsNotNull(app.Gameplay);
            Assert.AreEqual(app.Gameplay.Maze.Start, app.Gameplay.PlayerPosition);

            TouchInputController input = Object.FindFirstObjectByType<TouchInputController>();
            Assert.IsNotNull(input);
            Vector2Int start = app.Gameplay.PlayerPosition;
            input.BeginGesture(new Vector2(200f, 300f));
            input.UpdateGesture(new Vector2(320f, 300f));
            input.EndGesture();
            yield return null;
            Assert.AreEqual(start + Vector2Int.right, app.Gameplay.PlayerPosition);
        }
    }
}
