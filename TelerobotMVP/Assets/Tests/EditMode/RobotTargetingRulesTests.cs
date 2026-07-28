using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class RobotTargetingRulesTests
    {
        [TestCase(RobotCommand.DefendPosition, RouteId.NorthRoad, RouteId.NorthRoad, false, true)]
        [TestCase(RobotCommand.DefendPosition, RouteId.NorthRoad, RouteId.EastAlley, false, false)]
        [TestCase(RobotCommand.DefendPosition, RouteId.NorthRoad, RouteId.EastAlley, true, true)]
        [TestCase(RobotCommand.PatrolRoute, RouteId.NorthRoad, RouteId.NorthRoad, false, true)]
        [TestCase(RobotCommand.PatrolRoute, RouteId.NorthRoad, RouteId.EastAlley, false, false)]
        [TestCase(RobotCommand.PatrolRoute, RouteId.NorthRoad, RouteId.EastAlley, true, false)]
        [TestCase(RobotCommand.ReturnToBase, RouteId.NorthRoad, RouteId.EastAlley, false, true)]
        [TestCase(RobotCommand.ReturnToBase, RouteId.NorthRoad, RouteId.EastAlley, true, true)]
        public void RouteEligibilityChangesOnlyForDefendCleanup(
            RobotCommand command,
            RouteId assignedRoute,
            RouteId candidateRoute,
            bool spawnScheduleComplete,
            bool expected)
        {
            Assert.That(
                RobotTargetingRules.AllowsRoute(
                    command,
                    assignedRoute,
                    candidateRoute,
                    spawnScheduleComplete),
                Is.EqualTo(expected));
        }
    }
}
