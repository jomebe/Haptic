using System.Collections;
using Haptic.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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

            app.StartLevel(0);
            yield return null;
            Assert.AreEqual(ScreenState.Playing, app.State);
            Assert.IsNotNull(app.Gameplay);
            Assert.AreEqual(app.Gameplay.Maze.Start, app.Gameplay.PlayerPosition);
        }
    }
}
