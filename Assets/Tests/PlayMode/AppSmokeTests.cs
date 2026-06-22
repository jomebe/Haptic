using System.Collections;
using Haptic.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
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

            DirectTouchButtonRouter router = Object.FindFirstObjectByType<DirectTouchButtonRouter>();
            Assert.IsNotNull(router);
            UnityEngine.UI.Button startButton = GameObject.Find("START GAME").GetComponent<UnityEngine.UI.Button>();
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            Vector2 startScreenPoint = RectTransformUtility.WorldToScreenPoint(null, startRect.TransformPoint(startRect.rect.center));
            router.TapAt(startScreenPoint);
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
