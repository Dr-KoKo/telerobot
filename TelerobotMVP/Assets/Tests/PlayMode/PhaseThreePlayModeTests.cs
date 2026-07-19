using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseThreePlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator SecondRewardDeploysMedicalAndRippersOnThreeRoutesThenVictory()
        {
            yield return ClearAndChooseFirstUpgrade();
            yield return ClearAndChooseFirstUpgrade();
            Assert.That(Game.CurrentPhase, Is.EqualTo(3));
            Assert.That(Game.OpenRoutes.Count, Is.EqualTo(3));
            Assert.That(Object.FindFirstObjectByType<MedicalRobotActor>(), Is.Not.Null);
            Game.SpawnAllNowForTests();
            Assert.That(Game.AliveZombies.Exists(item => item.Type == ZombieType.Ripper), Is.True);
            Game.ClearCurrentWaveForTests();
            yield return null;
            yield return null;
            Assert.That(Game.Session.Result, Is.EqualTo(GameResult.Victory));
        }
    }
}
