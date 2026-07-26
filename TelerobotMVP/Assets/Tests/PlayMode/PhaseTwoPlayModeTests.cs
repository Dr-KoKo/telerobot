using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class PhaseTwoPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator PhaseOneClearOpensEastAlleyAndSpawnsBruiserWithoutRewardGate()
        {
            yield return ClearAndAdvancePhase();
            Assert.That(Game.CurrentPhase, Is.EqualTo(2));
            Assert.That(Game.OpenRoutes, Does.Contain(RouteId.EastAlley));
            Game.SpawnAllNowForTests();
            Assert.That(Game.AliveZombies.Exists(item => item.Type == ZombieType.Bruiser), Is.True);
        }

        [UnityTest]
        public IEnumerator PhaseTwoClearImmediatelyStartsPhaseThreeAndOpensSouthTunnel()
        {
            yield return ClearAndAdvancePhase();
            yield return ClearAndAdvancePhase();
            Assert.That(Game.CurrentPhase, Is.EqualTo(3));
            Assert.That(Game.OpenRoutes, Does.Contain(RouteId.SouthTunnel));
            Assert.That(Game.EventHistory, Has.None.Matches<Telerobot.Game.Core.DomainEvent>(
                item => item.Name == "upgrade_selected"));
        }
    }
}
