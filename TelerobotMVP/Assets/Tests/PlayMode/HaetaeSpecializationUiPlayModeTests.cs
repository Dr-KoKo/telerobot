using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeSpecializationUiPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator PanelIsNonBlockingTargetsOneReadyRobotAndCanReopen()
        {
            Ready(Game.Robots[0].State);
            Ready(Game.Robots[1].State);
            var view = Object.FindFirstObjectByType<HaetaeSpecializationView>();
            var elapsed = Game.Session.ElapsedTime;

            Assert.That(view.Open(), Is.True);
            Assert.That(view.IsOpen, Is.True);
            Assert.That(view.ChoiceCount, Is.EqualTo(3));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(Game.InputBlocked, Is.True);
            yield return null;

            Assert.That(Game.Session.ElapsedTime, Is.GreaterThan(elapsed));
            var firstTarget = view.TargetRobotId;
            Assert.That(view.CycleTarget(), Is.True);
            var secondTarget = view.TargetRobotId;
            Assert.That(secondTarget, Is.Not.EqualTo(firstTarget));
            Assert.That(view.Select(HaetaeSpecialization.Ranged),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            Assert.That(Game.Robots.Find(item => item.State.Id == secondTarget)
                .State.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Ranged));
            Assert.That(Game.Robots.Find(item => item.State.Id == firstTarget)
                .State.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Unselected));

            view.Close();
            Assert.That(view.Open(), Is.True);
            Assert.That(view.TargetRobotId, Is.EqualTo(firstTarget));
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator SameAndMixedSelectionsRemainPerRobot()
        {
            Ready(Game.Robots[0].State);
            Ready(Game.Robots[1].State);
            var view = Object.FindFirstObjectByType<HaetaeSpecializationView>();

            view.Open();
            var first = view.TargetRobotId;
            Assert.That(view.Select(HaetaeSpecialization.Balanced),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            var second = view.TargetRobotId;
            Assert.That(second, Is.Not.EqualTo(first));
            Assert.That(view.Select(HaetaeSpecialization.Balanced),
                Is.EqualTo(SpecializationSelectionResult.Selected));

            Assert.That(Game.Robots[0].State.Progression.Specialization,
                Is.EqualTo(HaetaeSpecialization.Balanced));
            Assert.That(Game.Robots[1].State.Progression.Specialization,
                Is.EqualTo(HaetaeSpecialization.Balanced));
            Assert.That(view.IsOpen, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpecializedLevelThreeRobotSpendsMasteryPointWithoutPausing()
        {
            var view = Object.FindFirstObjectByType<HaetaeSpecializationView>();
            var robot = Game.Robots[0].State;
            robot.Progression.Level = 3;
            robot.Progression.Experience = 150;
            robot.Progression.Specialization = HaetaeSpecialization.Ranged;
            robot.Progression.UnspentMasteryPoints = 1;

            Assert.That(view.Open(), Is.True);
            Assert.That(view.IsChoosingMastery, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(view.SelectMastery(HaetaeMasteryUpgrade.Efficiency),
                Is.EqualTo(MasterySelectionResult.Selected));
            Assert.That(robot.Progression.EfficiencyRank, Is.EqualTo(1));
            Assert.That(robot.Progression.UnspentMasteryPoints, Is.Zero);
            Assert.That(view.IsOpen, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AttackSpeedIsTheFourthMasteryChoiceAndLastPointClosesSafely()
        {
            var view = Object.FindFirstObjectByType<HaetaeSpecializationView>();
            var robot = Game.Robots[0].State;
            robot.Progression.Level = 3;
            robot.Progression.Experience = 150;
            robot.Progression.Specialization = HaetaeSpecialization.Balanced;
            robot.Progression.UnspentMasteryPoints = 1;

            Assert.That(view.Open(), Is.True);
            Assert.That(view.ChoiceCount, Is.EqualTo(4));
            Assert.That(view.SelectMastery(HaetaeMasteryUpgrade.AttackSpeed),
                Is.EqualTo(MasterySelectionResult.Selected));
            Assert.That(robot.Progression.AttackSpeedRank, Is.EqualTo(1));
            Assert.That(robot.Progression.UnspentMasteryPoints, Is.Zero);
            Assert.That(view.IsOpen, Is.False);
            yield return null;
        }

        private void Ready(RobotState robot)
        {
            robot.Progression.Level = 2;
            robot.Progression.Experience = Game.Config.HaetaeProgression.ExperiencePerLevel;
        }
    }
}
