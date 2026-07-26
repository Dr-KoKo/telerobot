using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeSpecializationTests
    {
        private HaetaeProgressionSystem system;

        [SetUp]
        public void SetUp()
        {
            system = new HaetaeProgressionSystem();
        }

        [Test]
        public void LevelOneAndUnselectedChoiceAreRejectedWithoutMutation()
        {
            var robot = Robot("haetae-1");
            Assert.That(system.SelectSpecialization(robot, HaetaeSpecialization.Melee),
                Is.EqualTo(SpecializationSelectionResult.NotLevelTwo));
            robot.Progression.Level = 2;
            Assert.That(system.SelectSpecialization(robot, HaetaeSpecialization.Unselected),
                Is.EqualTo(SpecializationSelectionResult.InvalidChoice));
            Assert.That(robot.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Unselected));
        }

        [TestCase(HaetaeSpecialization.Melee)]
        [TestCase(HaetaeSpecialization.Ranged)]
        [TestCase(HaetaeSpecialization.Balanced)]
        public void EverySelectableRoleCanBeChosenExactlyOnce(HaetaeSpecialization role)
        {
            var robot = ReadyRobot("haetae-1");
            var health = robot.Health.Current;
            var battery = robot.Battery;
            var mode = robot.Mode;
            var command = robot.Command;
            var route = robot.AssignedRoute;

            Assert.That(system.SelectSpecialization(robot, role),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            Assert.That(robot.Progression.Specialization, Is.EqualTo(role));
            Assert.That(robot.Progression.SpecializationReady, Is.False);
            Assert.That(system.SelectSpecialization(robot, DifferentRole(role)),
                Is.EqualTo(SpecializationSelectionResult.AlreadySelected));
            Assert.That(robot.Progression.Specialization, Is.EqualTo(role));
            Assert.That(robot.Health.Current, Is.EqualTo(health));
            Assert.That(robot.Battery, Is.EqualTo(battery));
            Assert.That(robot.Mode, Is.EqualTo(mode));
            Assert.That(robot.Command, Is.EqualTo(command));
            Assert.That(robot.AssignedRoute, Is.EqualTo(route));
        }

        [Test]
        public void SameAndMixedRolesRemainIndependent()
        {
            var first = ReadyRobot("haetae-1");
            var second = ReadyRobot("haetae-2");
            Assert.That(system.SelectSpecialization(first, HaetaeSpecialization.Melee),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            Assert.That(system.SelectSpecialization(second, HaetaeSpecialization.Melee),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            Assert.That(first.Progression.Specialization, Is.EqualTo(second.Progression.Specialization));

            var newFirst = ReadyRobot("haetae-1");
            var newSecond = ReadyRobot("haetae-2");
            system.SelectSpecialization(newFirst, HaetaeSpecialization.Melee);
            system.SelectSpecialization(newSecond, HaetaeSpecialization.Ranged);
            Assert.That(newFirst.Progression.Specialization, Is.Not.EqualTo(newSecond.Progression.Specialization));
        }

        [Test]
        public void UnselectedRobotCanChooseSpecializationAfterReachingLevelThree()
        {
            var robot = Robot("haetae-1");
            robot.Progression.Level = 3;
            robot.Progression.Experience = 150;

            Assert.That(robot.Progression.SpecializationReady, Is.True);
            Assert.That(system.SelectSpecialization(robot, HaetaeSpecialization.Ranged),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            Assert.That(robot.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Ranged));
        }

        [Test]
        public void NewSessionRobotResetsPreviousChoice()
        {
            var previous = ReadyRobot("haetae-1");
            system.SelectSpecialization(previous, HaetaeSpecialization.Balanced);

            var fresh = Robot("haetae-1");

            Assert.That(fresh.Progression.Level, Is.EqualTo(1));
            Assert.That(fresh.Progression.Experience, Is.Zero);
            Assert.That(fresh.Progression.Specialization, Is.EqualTo(HaetaeSpecialization.Unselected));
        }

        private static RobotState Robot(string id)
        {
            return new RobotState(id, 300f, 100f);
        }

        private static RobotState ReadyRobot(string id)
        {
            var robot = Robot(id);
            robot.Progression.Level = 2;
            robot.Progression.Experience = 75;
            return robot;
        }

        private static HaetaeSpecialization DifferentRole(HaetaeSpecialization role)
        {
            return role == HaetaeSpecialization.Melee
                ? HaetaeSpecialization.Ranged
                : HaetaeSpecialization.Melee;
        }
    }
}
