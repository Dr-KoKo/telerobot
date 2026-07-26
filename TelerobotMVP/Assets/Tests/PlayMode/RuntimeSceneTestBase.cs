using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public abstract class RuntimeSceneTestBase
    {
        protected MvpGameController Game;

        [UnitySetUp]
        public IEnumerator LoadMvp()
        {
            PlayerPreferences.ClearSavedValuesForTests();
            var operation = SceneManager.LoadSceneAsync("MVP", LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;
            Game = Object.FindFirstObjectByType<MvpGameController>();
            Assert.That(Game, Is.Not.Null, "MVP scene must contain MvpGameController.");
            Game.SetAcceleratedSpawningForTests(true);
        }

        protected IEnumerator ClearAndAdvancePhase()
        {
            var previousPhase = Game.CurrentPhase;
            Game.ClearCurrentWaveForTests();
            yield return null;
            yield return null;
            Assert.That(Game.CurrentPhase, Is.EqualTo(previousPhase + 1));
        }
    }
}
