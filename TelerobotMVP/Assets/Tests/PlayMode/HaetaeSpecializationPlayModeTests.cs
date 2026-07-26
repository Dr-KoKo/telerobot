using System.Collections;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeSpecializationPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator SelectionTargetsOneRobotAndPublishesOneEvent()
        {
            var first = Game.Robots[0];
            var second = Game.Robots[1];
            MakeReady(first.State);
            MakeReady(second.State);

            var result = Game.SelectHaetaeSpecialization(first.State.Id, HaetaeSpecialization.Melee);
            yield return null;

            Assert.That(result, Is.EqualTo(SpecializationSelectionResult.Selected));
            Assert.That(first.State.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Melee));
            Assert.That(second.State.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Unselected));
            var events = Game.EventHistory.Where(item => item.Name == "haetae_specialization_selected").ToArray();
            Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events[0].Payload["robotId"], Is.EqualTo(first.State.Id));
            Assert.That(events[0].Payload["specialization"], Is.EqualTo(HaetaeSpecialization.Melee.ToString()));
        }

        [UnityTest]
        public IEnumerator DestroyedRobotCanChooseAndKeepsRoleAfterPhaseRestore()
        {
            var robot = Game.Robots[0];
            MakeReady(robot.State);
            robot.ReceiveZombieHit(Game.Config.Robot.MaxHealth, false);
            Assert.That(robot.State.IsDestroyed, Is.True);

            Assert.That(Game.SelectHaetaeSpecialization(robot.State.Id, HaetaeSpecialization.Ranged),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            robot.RestoreForPhaseStart();
            yield return null;

            Assert.That(robot.State.IsDestroyed, Is.False);
            Assert.That(robot.State.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Ranged));
        }

        private void MakeReady(RobotState robot)
        {
            robot.Progression.Level = 2;
            robot.Progression.Experience = Game.Config.HaetaeProgression.ExperiencePerLevel;
        }
    }
}
