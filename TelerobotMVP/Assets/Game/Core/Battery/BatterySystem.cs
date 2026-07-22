using System;

namespace Telerobot.Game.Core
{
    public sealed class BatterySystem
    {
        private readonly BatteryConfig config;

        public BatterySystem(BatteryConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public void Drain(RobotState robot, RobotActivity activity, float deltaTime, float combatMultiplier)
        {
            if (robot == null || robot.IsDestroyed || robot.Mode == RobotMode.Disabled || robot.Mode == RobotMode.Recovery || robot.Mode == RobotMode.Charging) return;
            var rate = activity == RobotActivity.Combat
                ? config.CombatDrainPerSecond * combatMultiplier
                : activity == RobotActivity.Patrol ? config.PatrolDrainPerSecond : config.IdleDrainPerSecond;
            robot.Battery = Math.Max(0f, robot.Battery - rate * Math.Max(0f, deltaTime));
            RefreshBandAndMode(robot);
        }

        public void ApplyRipperHit(RobotState robot)
        {
            if (robot == null || robot.IsDestroyed) return;
            robot.Battery = Math.Max(0f, robot.Battery - config.RipperHitDrain);
            RefreshBandAndMode(robot);
        }

        public void TickDisabledRecovery(RobotState robot, float deltaTime)
        {
            if (robot == null || robot.IsDestroyed || robot.Battery > 0f && robot.Mode != RobotMode.Recovery) return;
            if (robot.Mode == RobotMode.Disabled)
            {
                robot.DisabledElapsed += Math.Max(0f, deltaTime);
                if (robot.DisabledElapsed >= config.DisabledHoldSeconds) robot.Mode = RobotMode.Recovery;
                return;
            }
            if (robot.Mode != RobotMode.Recovery) return;
            robot.Battery = Math.Min(robot.MaximumBattery, robot.Battery + config.RecoveryPerSecond * Math.Max(0f, deltaTime));
            robot.BatteryBand = BandFor(robot);
            if (robot.Battery >= config.MoveEnableThreshold)
            {
                robot.Mode = RobotMode.ReturnToCharge;
                robot.Command = RobotCommand.ReturnToBase;
            }
        }

        public void Charge(RobotState robot, float deltaTime, float chargeMultiplier)
        {
            if (robot == null || !robot.CanCharge) return;
            robot.Mode = RobotMode.Charging;
            robot.Battery = Math.Min(robot.MaximumBattery,
                robot.Battery + config.ChargePerSecond * chargeMultiplier * Math.Max(0f, deltaTime));
            robot.BatteryBand = BatteryBand.Charging;
            if (robot.Battery >= robot.MaximumBattery)
            {
                robot.Mode = RobotMode.Standby;
                robot.BatteryBand = BatteryBand.Normal;
            }
        }

        public BatteryBand BandFor(RobotState robot)
        {
            if (robot.Battery <= 0f) return BatteryBand.Depleted;
            if (robot.Battery <= config.CriticalMaximum) return BatteryBand.Critical;
            if (robot.Battery <= config.LowPowerMaximum) return BatteryBand.LowPower;
            return BatteryBand.Normal;
        }

        public float MoveMultiplier(RobotState robot)
        {
            return BandFor(robot) == BatteryBand.LowPower || BandFor(robot) == BatteryBand.Critical
                ? config.LowPowerMoveMultiplier : 1f;
        }

        public float AttackMultiplier(RobotState robot)
        {
            return BandFor(robot) == BatteryBand.LowPower || BandFor(robot) == BatteryBand.Critical
                ? config.LowPowerAttackMultiplier : 1f;
        }

        private void RefreshBandAndMode(RobotState robot)
        {
            robot.BatteryBand = BandFor(robot);
            if (robot.BatteryBand == BatteryBand.Depleted)
            {
                robot.Mode = RobotMode.Disabled;
                robot.DisabledElapsed = 0f;
            }
            else if ((robot.BatteryBand == BatteryBand.LowPower || robot.BatteryBand == BatteryBand.Critical) &&
                     robot.Mode != RobotMode.ReturnToCharge)
            {
                robot.Mode = RobotMode.LowBattery;
            }
        }
    }
}
