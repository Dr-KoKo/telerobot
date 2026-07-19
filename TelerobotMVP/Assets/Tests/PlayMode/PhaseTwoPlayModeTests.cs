using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseTwoPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator FirstRewardOpensEastAlleyAndSpawnsBruiser()
        {
            yield return ClearAndChooseFirstUpgrade();
            Assert.That(Game.CurrentPhase, Is.EqualTo(2));
            Assert.That(Game.OpenRoutes, Does.Contain(RouteId.EastAlley));
            Game.SpawnAllNowForTests();
            Assert.That(Game.AliveZombies.Exists(item => item.Type == ZombieType.Bruiser), Is.True);
        }
    }
}
