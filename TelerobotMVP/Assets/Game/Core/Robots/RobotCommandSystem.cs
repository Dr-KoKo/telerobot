using System;

namespace Telerobot.Game.Core
{
    public sealed class RobotCommandSystem : ICommandInput
    {
        public static readonly RobotCommand[] AllowedCommands =
        {
            RobotCommand.DefendPosition,
            RobotCommand.PatrolRoute,
            RobotCommand.ReturnToBase,
            RobotCommand.Charge
        };
        private readonly RobotCommand[] commands;

        public RobotCommandSystem()
            : this(new CommandConfig { Commands = AllowedCommands })
        {
        }

        public RobotCommandSystem(CommandConfig config)
        {
            commands = config == null || config.Commands == null ? AllowedCommands : config.Commands;
            if (commands.Length != 4) throw new ArgumentException("Exactly four robot commands are required.");
            foreach (var required in AllowedCommands)
                if (Array.IndexOf(commands, required) < 0) throw new ArgumentException("Command contract is incomplete: " + required);
        }

        public bool IssueCommand(RobotState robot, RobotCommand command, RouteId route)
        {
            if (robot == null || Array.IndexOf(commands, command) < 0 || robot.Health.IsDead) return false;
            if (robot.Mode == RobotMode.Disabled || robot.Mode == RobotMode.Recovery) return false;
            robot.Command = command;
            robot.AssignedRoute = route;
            robot.Mode = command == RobotCommand.PatrolRoute ? RobotMode.Patrol
                : command == RobotCommand.ReturnToBase || command == RobotCommand.Charge ? RobotMode.ReturnToCharge
                : RobotMode.Standby;
            return true;
        }
    }
}
