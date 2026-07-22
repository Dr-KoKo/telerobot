using System.Collections;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class RobotCommandPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator CommandSurfaceContainsExactlyThreeCommandsAndReturnToBaseAutoCharges()
        {
            Assert.That(RobotCommandSystem.AllowedCommands, Is.EqualTo(new[]
            {
                RobotCommand.DefendPosition, RobotCommand.PatrolRoute, RobotCommand.ReturnToBase
            }));
            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            robot.State.Battery = 50f;
            robot.transform.position = Game.GetRobotFormationPosition(robot, Game.ToVector(Game.Config.World.BaseRally));
            var batteryBefore = robot.State.Battery;

            Assert.That(robot.Issue(RobotCommand.ReturnToBase, RouteId.NorthRoad), Is.True);
            yield return null;

            Assert.That(robot.State.Command, Is.EqualTo(RobotCommand.DefendPosition));
            Assert.That(robot.State.Mode, Is.EqualTo(RobotMode.Charging));
            Assert.That(robot.State.Battery, Is.GreaterThan(batteryBefore));
            Assert.That(robot.State.CanAttack, Is.False);
            Assert.That(Game.EventHistory.Any(item => item.Name == "robot_auto_charge_started" &&
                item.Payload["robotId"].ToString() == robot.State.Id), Is.True);
        }

        [UnityTest]
        public IEnumerator ReturnToBaseCompletesIntoDefendInsteadOfGettingStuck()
        {
            var robot = Game.Robots[0];
            var rally = Game.GetRobotFormationPosition(robot, Game.ToVector(Game.Config.World.BaseRally));
            robot.transform.position = rally;

            Assert.That(robot.Issue(RobotCommand.ReturnToBase, RouteId.NorthRoad), Is.True);
            yield return null;
            yield return null;

            Assert.That(robot.State.Command, Is.EqualTo(RobotCommand.DefendPosition));
            Assert.That(robot.State.Mode, Is.Not.EqualTo(RobotMode.ReturnToCharge));
            Assert.That(Vector3.Distance(robot.transform.position, rally), Is.LessThan(0.01f));
            Assert.That(Vector3.Distance(robot.transform.position, Game.ChargingPosition), Is.GreaterThan(1f));
        }

        [UnityTest]
        public IEnumerator BaseThreatInterruptsAutomaticChargingAndRestoresCombat()
        {
            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            robot.State.Battery = 50f;
            robot.transform.position = Game.GetRobotFormationPosition(robot, Game.ToVector(Game.Config.World.BaseRally));
            yield return null;
            Assert.That(robot.State.Mode, Is.EqualTo(RobotMode.Charging));

            Game.SpawnAllNowForTests();
            var target = Game.AliveZombies[0];
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != target) other.ReceiveDamage(99999f, "test");
            target.enabled = false;
            target.State.Route = RouteId.EastAlley;
            target.State.Health.Maximum = 500f;
            target.State.Health.Current = 500f;
            target.transform.position = robot.transform.position + Vector3.forward;
            var healthBefore = target.State.Health.Current;
            yield return null;
            yield return null;

            Assert.That(robot.State.Mode, Is.EqualTo(RobotMode.Engage));
            Assert.That(robot.State.CanAttack, Is.True);
            Assert.That(robot.State.CurrentTargetId, Is.EqualTo(target.State.Id));
            Assert.That(target.State.Health.Current, Is.LessThan(healthBefore));
        }

        [UnityTest]
        public IEnumerator RobotsSeparateWhileEngagingTheSameZombie()
        {
            Game.SpawnAllNowForTests();
            var target = Game.AliveZombies[0];
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != target) other.ReceiveDamage(99999f, "test");
            target.enabled = false;
            target.State.Health.Maximum = 500f;
            target.State.Health.Current = 500f;
            target.transform.position = Vector3.forward * 1.5f;

            foreach (var robot in Game.Robots)
            {
                robot.transform.position = Vector3.zero;
                Assert.That(robot.Issue(RobotCommand.DefendPosition, RouteId.NorthRoad), Is.True);
            }
            var healthBefore = target.State.Health.Current;
            for (var frame = 0; frame < 12; frame++) yield return null;

            Assert.That(Vector3.Distance(Game.Robots[0].transform.position, Game.Robots[1].transform.position),
                Is.GreaterThan(0.8f));
            Assert.That(target.State.Health.Current, Is.LessThan(healthBefore));
        }

        [UnityTest]
        public IEnumerator DefendTargetsZombieClosestToBaseInsteadOfRobot()
        {
            Game.SpawnAllNowForTests();
            var nearBase = Game.AliveZombies[0];
            var nearRobot = Game.AliveZombies[1];
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != nearBase && other != nearRobot) other.ReceiveDamage(99999f, "test");
            nearBase.enabled = false;
            nearRobot.enabled = false;
            nearBase.transform.position = new Vector3(0f, 0.8f, 2f);
            nearRobot.transform.position = new Vector3(0f, 0.8f, 10f);

            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            robot.transform.position = new Vector3(0f, 0.8f, 9f);
            Assert.That(robot.Issue(RobotCommand.DefendPosition, RouteId.NorthRoad), Is.True);
            yield return null;

            Assert.That(robot.State.CurrentTargetId, Is.EqualTo(nearBase.State.Id));
        }

        [UnityTest]
        public IEnumerator HaetaeChainsToNearbyCrossRouteZombieAfterKill()
        {
            Game.SpawnAllNowForTests();
            var firstTarget = Game.AliveZombies[0];
            var followUpTarget = Game.AliveZombies[1];
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != firstTarget && other != followUpTarget) other.ReceiveDamage(99999f, "test");
            firstTarget.enabled = false;
            followUpTarget.enabled = false;
            firstTarget.State.Route = RouteId.NorthRoad;
            followUpTarget.State.Route = RouteId.EastAlley;
            firstTarget.State.Health.Current = 1f;

            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            Game.Robots[1].transform.position = new Vector3(-20f, 0.8f, -20f);
            var combatCenter = Game.BaseTransform.position;
            combatCenter.y = 0.8f;
            robot.transform.position = combatCenter;
            firstTarget.transform.position = combatCenter + Vector3.forward * 1.2f;
            followUpTarget.transform.position = combatCenter + Vector3.forward * 1.3f + Vector3.right * 1.4f;
            Assert.That(robot.Issue(RobotCommand.DefendPosition, RouteId.NorthRoad), Is.True);

            for (var frame = 0; frame < 5 && !firstTarget.State.Health.IsDead; frame++) yield return null;

            Assert.That(firstTarget.State.Health.IsDead, Is.True);
            Assert.That(robot.State.CurrentTargetId, Is.EqualTo(followUpTarget.State.Id));
            Assert.That(robot.State.Mode, Is.EqualTo(RobotMode.Engage));
        }

        [UnityTest]
        public IEnumerator DefendOutsideLeashReturnsHomeAndIgnoresNearbyThreat()
        {
            Game.SpawnAllNowForTests();
            var target = Game.AliveZombies[0];
            foreach (var other in Game.AliveZombies.ToArray())
                if (other != target) other.ReceiveDamage(99999f, "test");
            target.enabled = false;

            var robot = Game.Robots[0];
            Game.Robots[1].enabled = false;
            var outside = Game.BaseTransform.position + Vector3.forward * (Game.Config.Robot.DefendLeashRadius + 2f);
            outside.y = 0.8f;
            robot.transform.position = outside;
            target.transform.position = outside + Vector3.forward;
            Assert.That(robot.Issue(RobotCommand.DefendPosition, RouteId.NorthRoad), Is.True);
            var distanceBefore = Vector3.Distance(robot.transform.position, Game.BaseTransform.position);
            yield return null;
            yield return null;

            Assert.That(Vector3.Distance(robot.transform.position, Game.BaseTransform.position), Is.LessThan(distanceBefore));
            Assert.That(robot.State.CurrentTargetId, Is.Null);
        }

        [UnityTest]
        public IEnumerator SelectAllFansOutCommandsThenIndividualSelectionCanDiverge()
        {
            Game.ToggleSelectAllRobots(true);
            Assert.That(Game.SelectedRobots.Count, Is.EqualTo(2));
            Assert.That(Game.IssueCommandToSelected(RobotCommand.PatrolRoute, RouteId.NorthRoad), Is.EqualTo(2));
            Assert.That(Game.Robots[0].State.Command, Is.EqualTo(RobotCommand.PatrolRoute));
            Assert.That(Game.Robots[1].State.Command, Is.EqualTo(RobotCommand.PatrolRoute));

            Game.SelectOnlyRobot(Game.Robots[0]);
            Assert.That(Game.IssueCommandToSelected(RobotCommand.DefendPosition, RouteId.NorthRoad), Is.EqualTo(1));
            yield return null;

            Assert.That(Game.Robots[0].State.Command, Is.EqualTo(RobotCommand.DefendPosition));
            Assert.That(Game.Robots[1].State.Command, Is.EqualTo(RobotCommand.PatrolRoute));
        }

        [UnityTest]
        public IEnumerator DestroyedRobotEmitsOnceRejectsCommandsAndRestoresAtNextPhase()
        {
            var robot = Game.Robots[0];
            robot.ReceiveZombieHit(Game.Config.Robot.MaxHealth, false);
            robot.ReceiveZombieHit(10f, false);
            Assert.That(robot.State.Mode, Is.EqualTo(RobotMode.Destroyed));
            Assert.That(robot.Issue(RobotCommand.ReturnToBase, RouteId.NorthRoad), Is.False);
            var destructionEvents = 0;
            foreach (var gameEvent in Game.EventHistory)
                if (gameEvent.Name == "robot_destroyed" && gameEvent.Payload["robotId"].ToString() == robot.State.Id) destructionEvents++;
            Assert.That(destructionEvents, Is.EqualTo(1));

            yield return ClearAndChooseFirstUpgrade();

            Assert.That(Game.CurrentPhase, Is.EqualTo(2));
            Assert.That(robot.State.Mode, Is.EqualTo(RobotMode.Standby));
            Assert.That(robot.State.Health.Current, Is.EqualTo(Game.Config.Robot.MaxHealth));
            Assert.That(robot.State.Battery, Is.EqualTo(robot.State.MaximumBattery).Within(0.01f));
            Assert.That(robot.Issue(RobotCommand.DefendPosition, RouteId.NorthRoad), Is.True);
        }
    }
}
