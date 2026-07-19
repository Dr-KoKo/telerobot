using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class RobotCommandPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator CommandSurfaceContainsExactlyFourCommandsAndChargeLocksCombat()
        {
            Assert.That(RobotCommandSystem.AllowedCommands, Is.EqualTo(new[]
            {
                RobotCommand.DefendPosition, RobotCommand.PatrolRoute, RobotCommand.ReturnToBase, RobotCommand.Charge
            }));
            var robot = Game.Robots[0];
            Assert.That(robot.Issue(RobotCommand.Charge, RouteId.NorthRoad), Is.True);
            yield return null;
            Assert.That(robot.State.Command, Is.EqualTo(RobotCommand.Charge));
            Assert.That(robot.State.CanAttack, Is.False);
        }

        [UnityTest]
        public IEnumerator ReturnToBaseMovesToRallyInsteadOfChargingStation()
        {
            var robot = Game.Robots[0];
            var rally = Game.ToVector(Game.Config.World.BaseRally);
            robot.transform.position = rally;

            Assert.That(robot.Issue(RobotCommand.ReturnToBase, RouteId.NorthRoad), Is.True);
            yield return null;
            yield return null;

            Assert.That(robot.State.Command, Is.EqualTo(RobotCommand.ReturnToBase));
            Assert.That(Vector3.Distance(robot.transform.position, rally), Is.LessThan(0.01f));
            Assert.That(Vector3.Distance(robot.transform.position, Game.ChargingPosition), Is.GreaterThan(1f));
        }
    }
}
