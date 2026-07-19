using System;
using System.Collections.Generic;
using Telerobot.Game.Core;

namespace Telerobot.Game.Simulation
{
    public sealed class SimulationSummary
    {
        public int Seed;
        public float DurationSeconds;
        public GameResult Result;
        public int ChargeCommands;
        public int DisabledCount;
        public int RipperHits;
        public int ZombiesKilled;
    }

    public sealed class DeterministicSessionSimulator
    {
        private readonly GameplayConfig config;

        public DeterministicSessionSimulator(GameplayConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public SimulationSummary Run(int seed, ITelemetrySink sink)
        {
            var rng = new XorShiftRng(seed);
            var bus = new DomainEventBus();
            var sessionId = "sim-" + seed + "-" + config.Game.DataVersion;
            var bridge = new TelemetryBridge(bus, sink, "simulation", config.Game.DataVersion, sessionId, seed);
            var spawnSystem = new SpawnSystem(config);
            var upgradeSystem = new UpgradeSystem(config);
            var session = new SessionState(seed);
            var baseState = new BaseState(config.Game.BaseMaxHealth);
            var player = new PlayerState(config.Game.PlayerMaxHealth, config.Weapon.MagazineSize, config.Weapon.ReserveAmmo, config.Weapon.GrenadesPerPhase);
            var robots = new List<RobotState>
            {
                new RobotState("haetae-1", config.Robot.MaxHealth, config.Battery.Maximum),
                new RobotState("haetae-2", config.Robot.MaxHealth, config.Battery.Maximum)
            };
            var modifiers = new RuntimeModifiers();
            var battery = new BatterySystem(config.Battery);
            var summary = new SimulationSummary { Seed = seed, Result = GameResult.InProgress };
            var simTime = 0f;

            Publish(bus, "session_started", simTime, 0, "dataVersion", config.Game.DataVersion);
            for (var phaseNumber = 1; phaseNumber <= 3; phaseNumber++)
            {
                session.CurrentPhase = phaseNumber;
                var phase = config.GetPhase(phaseNumber);
                var spawns = spawnSystem.Compose(phase, rng);
                Publish(bus, "phase_started", simTime, phaseNumber, "spawnCount", spawns.Count);
                for (var index = 0; index < spawns.Count; index++)
                {
                    Publish(bus, "zombie_spawned", simTime, phaseNumber,
                        "type", spawns[index].Type, "routeId", spawns[index].Route);
                }

                var duration = phase.TargetDurationSeconds * (0.96f + rng.NextFloat() * 0.08f);
                var elapsed = 0f;
                var killed = 0;
                var nextRipperHit = 15f;
                var batterySampleTimer = 0f;
                var pressureSampleTimer = 0f;
                var step = config.Validation != null && config.Validation.FixedStepSeconds > 0f
                    ? config.Validation.FixedStepSeconds : config.Game.FixedStepSeconds;
                while (elapsed < duration)
                {
                    elapsed += step;
                    simTime += step;
                    session.ElapsedTime = simTime;
                    batterySampleTimer += step;
                    pressureSampleTimer += step;
                    foreach (var robot in robots)
                    {
                        if (robot.Mode == RobotMode.Charging)
                        {
                            battery.Charge(robot, step, modifiers.ChargeRateMultiplier);
                        }
                        else
                        {
                            battery.Drain(robot, RobotActivity.Combat, step, modifiers.CombatDrainMultiplier);
                            if (robot.Mode == RobotMode.Disabled)
                            {
                                summary.DisabledCount++;
                                Publish(bus, "robot_disabled", simTime, phaseNumber, "robotId", robot.Id);
                                battery.TickDisabledRecovery(robot, config.Battery.DisabledHoldSeconds + config.Battery.MoveEnableThreshold / config.Battery.RecoveryPerSecond);
                            }
                            if (robot.Battery < robot.MaximumBattery * 0.20f && robot.Mode != RobotMode.Charging)
                            {
                                summary.ChargeCommands++;
                                Publish(bus, "robot_charge_commanded", simTime, phaseNumber, "robotId", robot.Id);
                                robot.Mode = RobotMode.Charging;
                            }
                        }
                        if (batterySampleTimer >= 1f)
                            Publish(bus, "robot_battery_changed", simTime, phaseNumber,
                                "robotId", robot.Id, "value", robot.Battery, "state", robot.BatteryBand);
                    }
                    if (batterySampleTimer >= 1f) batterySampleTimer -= 1f;

                    if (phaseNumber == 3 && elapsed >= nextRipperHit)
                    {
                        var target = robots[rng.NextInt(robots.Count)];
                        battery.ApplyRipperHit(target);
                        summary.RipperHits++;
                        Publish(bus, "ripper_attacked_robot", simTime, phaseNumber,
                            "robotId", target.Id, "batteryDrained", config.Battery.RipperHitDrain);
                        nextRipperHit += 30f;
                    }

                    var targetKilled = Math.Min(spawns.Count, (int)(elapsed / duration * spawns.Count));
                    while (killed < targetKilled)
                    {
                        var spawn = spawns[killed];
                        Publish(bus, "zombie_killed", simTime, phaseNumber,
                            "type", spawn.Type, "routeId", spawn.Route, "by", killed % 3 == 0 ? "player" : "robot");
                        killed++;
                        summary.ZombiesKilled++;
                    }
                    if (pressureSampleTimer >= 10f)
                    {
                        foreach (var route in phase.OpenRoutes)
                            Publish(bus, "route_pressure_sampled", simTime, phaseNumber,
                                "routeId", route, "aliveCount", spawns.Count - killed);
                        pressureSampleTimer -= 10f;
                    }
                }
                Publish(bus, "base_hp_sampled", simTime, phaseNumber, "hp", baseState.Health.Current);
                Publish(bus, "player_hp_at_phase_end", simTime, phaseNumber, "hp", player.Health.Current);
                Publish(bus, "phase_cleared", simTime, phaseNumber);
                CombatRules.RecoverBase(baseState, config.Game.BasePhaseRecoveryFraction);

                if (phaseNumber < 3)
                {
                    var offer = upgradeSystem.Offer(rng);
                    var selected = offer[rng.NextInt(offer.Count)];
                    upgradeSystem.Apply(selected, session, baseState, robots, player, modifiers);
                    Publish(bus, "upgrade_selected", simTime, phaseNumber,
                        "upgradeId", selected.Id, "rewardStep", phaseNumber);
                }
            }

            summary.DurationSeconds = simTime;
            summary.Result = GameResult.Victory;
            Publish(bus, "simulation_run_completed", simTime, 0,
                "result", summary.Result, "durationSeconds", summary.DurationSeconds,
                "chargeCommands", summary.ChargeCommands, "disabledCount", summary.DisabledCount);
            Publish(bus, "session_ended", simTime, 0,
                "result", summary.Result, "defeatReason", DefeatReason.None);
            sink.Flush();
            return summary;
        }

        private static void Publish(DomainEventBus bus, string name, float time, int phase, params object[] payload)
        {
            var gameEvent = new DomainEvent(name, time, phase);
            if (payload.Length % 2 != 0) throw new ArgumentException("Telemetry payload must contain key/value pairs.");
            for (var index = 0; index < payload.Length; index += 2)
                gameEvent.With(Convert.ToString(payload[index]), payload[index + 1]);
            bus.Publish(gameEvent);
        }
    }
}
