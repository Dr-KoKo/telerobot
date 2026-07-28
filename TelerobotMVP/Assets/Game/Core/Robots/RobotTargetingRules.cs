namespace Telerobot.Game.Core
{
    public static class RobotTargetingRules
    {
        public static bool AllowsRoute(
            RobotCommand command,
            RouteId assignedRoute,
            RouteId candidateRoute,
            bool spawnScheduleComplete)
        {
            if (candidateRoute == assignedRoute) return true;
            if (command == RobotCommand.DefendPosition) return spawnScheduleComplete;
            return command != RobotCommand.PatrolRoute;
        }
    }
}
