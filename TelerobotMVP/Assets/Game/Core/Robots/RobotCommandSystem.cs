using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public sealed class RobotCommandSystem : ICommandInput
    {
        public static readonly RobotCommand[] AllowedCommands =
        {
            RobotCommand.DefendPosition,
            RobotCommand.PatrolRoute,
            RobotCommand.ReturnToBase
        };
        private readonly RobotCommand[] commands;

        public RobotCommandSystem()
            : this(new CommandConfig { Commands = AllowedCommands })
        {
        }

        public RobotCommandSystem(CommandConfig config)
        {
            commands = config == null || config.Commands == null ? AllowedCommands : config.Commands;
            if (commands.Length != 3) throw new ArgumentException("Exactly three robot commands are required.");
            foreach (var required in AllowedCommands)
                if (Array.IndexOf(commands, required) < 0) throw new ArgumentException("Command contract is incomplete: " + required);
        }

        public bool IssueCommand(RobotState robot, RobotCommand command, RouteId route)
        {
            if (robot == null || Array.IndexOf(commands, command) < 0 || robot.IsDestroyed) return false;
            if (robot.Mode == RobotMode.Disabled || robot.Mode == RobotMode.Recovery) return false;
            robot.Command = command;
            robot.AssignedRoute = route;
            robot.Mode = command == RobotCommand.PatrolRoute ? RobotMode.Patrol
                : command == RobotCommand.ReturnToBase ? RobotMode.ReturnToCharge
                : RobotMode.Standby;
            return true;
        }
    }

    public sealed class RobotDurabilitySystem
    {
        public bool ApplyDamage(RobotState robot, float damage)
        {
            if (robot == null) throw new ArgumentNullException("robot");
            if (robot.IsDestroyed) return false;
            CombatRules.ApplyDamage(robot.Health, damage);
            if (!robot.Health.IsDead) return false;
            robot.Mode = RobotMode.Destroyed;
            robot.FirstDashUsed = false;
            return true;
        }

        public void RestoreAtPhaseStart(RobotState robot, float maximumHealth, float usableBattery)
        {
            if (robot == null) throw new ArgumentNullException("robot");
            if (!robot.IsDestroyed) return;
            robot.Health.Maximum = Math.Max(1f, maximumHealth);
            robot.Health.Current = robot.Health.Maximum;
            robot.MaximumBattery = Math.Max(1f, robot.MaximumBattery);
            robot.Battery = Math.Max(1f, Math.Min(robot.MaximumBattery, usableBattery));
            robot.BatteryBand = BatteryBand.Normal;
            robot.Mode = RobotMode.Standby;
            robot.Command = RobotCommand.DefendPosition;
            robot.DisabledElapsed = 0f;
            robot.FirstDashUsed = false;
        }
    }

    public sealed class RobotSelectionModel
    {
        private readonly List<string> availableIds = new List<string>();
        private readonly List<string> selectedIds = new List<string>();

        public IReadOnlyList<string> SelectedIds { get { return selectedIds; } }
        public bool IsAllSelected { get { return availableIds.Count > 0 && selectedIds.Count == availableIds.Count; } }

        public RobotSelectionModel(IEnumerable<string> robotIds)
        {
            if (robotIds == null) throw new ArgumentNullException("robotIds");
            foreach (var id in robotIds)
            {
                if (string.IsNullOrWhiteSpace(id) || availableIds.Contains(id))
                    throw new ArgumentException("Robot selection ids must be non-empty and unique.");
                availableIds.Add(id);
            }
        }

        public void SelectOnly(string robotId)
        {
            if (!availableIds.Contains(robotId)) throw new ArgumentException("Unknown robot id: " + robotId);
            selectedIds.Clear();
            selectedIds.Add(robotId);
        }

        public void ToggleAll(bool selected)
        {
            selectedIds.Clear();
            if (selected) selectedIds.AddRange(availableIds);
            else if (availableIds.Count > 0) selectedIds.Add(availableIds[0]);
        }

        public bool IsSelected(string robotId)
        {
            return selectedIds.Contains(robotId);
        }
    }
}
