using System;
using System.Collections.Generic;
using Telerobot.Game.Core;

namespace Telerobot.Game.Simulation
{
    public sealed class SimulationSummary
    {
        public int Seed;
        public SimProfileId ProfileId;
        public float DurationSeconds;
        public GameResult Result;
        public DefeatReason DefeatReason;
        public int PhasesCleared;
        public int AutoChargeStarts;
        public int DisabledCount;
        public int DestroyedCount;
        public int RipperHits;
        public int ZombiesKilled;
        public int PeakAliveCount;
        public int PeakAliveCap;
        public bool SessionDurationWithinTarget;
    }

    public sealed class BalanceEvaluation
    {
        public SimProfileId ProfileId;
        public int RunCount;
        public float AverageDurationSeconds;
        public float PhaseOneClearRate;
        public float PhaseTwoClearRate;
        public float PhaseThreeClearRate;
        public bool SessionDurationTargetMet;
        public bool PhaseOneTargetMet;
        public bool PhaseTwoTargetMet;
        public bool PhaseThreeTargetMet;
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
            return Run(seed, SimProfileId.Baseline, sink);
        }

        public SimulationSummary Run(int seed, SimProfileId profileId, ITelemetrySink sink)
        {
            if (sink == null) throw new ArgumentNullException("sink");
            var profile = config.GetSimPlayerProfile(profileId);
            var rng = new XorShiftRng(seed);
            var bus = new DomainEventBus();
            var sessionId = "sim-" + seed + "-" + profileId + "-" + config.Game.DataVersion;
            var bridge = new TelemetryBridge(bus, sink, "simulation", config.Game.DataVersion, sessionId, seed,
                profileId.ToString());
            var spawnSystem = new SpawnSystem(config);
            var upgradeSystem = new UpgradeSystem(config);
            var batterySystem = new BatterySystem(config.Battery);
            var durabilitySystem = new RobotDurabilitySystem();
            var robotAttackSystem = new RobotAttackSystem(config.Robot);
            var movement = new WaypointMovement();
            var session = new SessionState(seed);
            var baseState = new BaseState(config.Base.MaxHealth);
            var player = new PlayerState(config.Game.PlayerMaxHealth, config.Weapon.MagazineSize,
                config.Ammo.StartReserveAmmo, config.Weapon.GrenadesPerPhase);
            var robots = new List<RobotState>
            {
                new RobotState("haetae-1", config.Robot.MaxHealth, config.Battery.Maximum),
                new RobotState("haetae-2", config.Robot.MaxHealth, config.Battery.Maximum)
            };
            var modifiers = new RuntimeModifiers();
            var summary = new SimulationSummary { Seed = seed, ProfileId = profileId, Result = GameResult.InProgress };
            var simTime = 0f;
            var zombieSerial = 0;

            Publish(bus, "session_started", simTime, 0, "dataVersion", config.Game.DataVersion,
                "simProfileId", profileId);
            for (var phaseNumber = 1; phaseNumber <= 3 && summary.Result == GameResult.InProgress; phaseNumber++)
            {
                session.CurrentPhase = phaseNumber;
                var phase = config.GetPhase(phaseNumber);
                foreach (var robot in robots)
                    durabilitySystem.RestoreAtPhaseStart(robot, config.Robot.MaxHealth, robot.MaximumBattery);
                AssignRobotRoutes(robots, phase);
                if (config.Ammo.GrenadeResupplyPolicy == GrenadeResupplyPolicy.PhaseResetOnly)
                    player.Grenades = config.Weapon.GrenadesPerPhase;

                var pending = spawnSystem.Compose(phase, rng);
                var scheduler = new ContinuousSpawnScheduler(phase, rng);
                var zombies = new List<SimZombie>();
                var playerRuntime = new SimPlayerRuntime(profile.ReactionDelaySeconds);
                var nextPending = 0;
                var phaseElapsed = 0f;
                var baseSampleTimer = 0f;
                var pressureSampleTimer = 0f;
                var batterySampleTimer = 0f;
                var step = config.Validation != null && config.Validation.FixedStepSeconds > 0f
                    ? config.Validation.FixedStepSeconds : config.Game.FixedStepSeconds;

                Publish(bus, "phase_started", simTime, phaseNumber, "spawnCount", pending.Count);
                while (summary.Result == GameResult.InProgress)
                {
                    simTime += step;
                    phaseElapsed += step;
                    session.ElapsedTime = simTime;

                    var aliveBeforeSpawn = AliveCount(zombies);
                    var toSpawn = scheduler.Advance(step, aliveBeforeSpawn, pending.Count - nextPending);
                    for (var index = 0; index < toSpawn; index++)
                    {
                        var entry = pending[nextPending++];
                        var zombie = new SimZombie("zombie-" + (++zombieSerial), entry,
                            config.GetZombie(entry.Type), RouteLength(config.GetRoute(entry.Route)));
                        zombies.Add(zombie);
                        Publish(bus, "zombie_spawned", simTime, phaseNumber, "type", entry.Type, "routeId", entry.Route);
                    }

                    UpdatePlayer(bus, rng, profile, playerRuntime, player, zombies, simTime, phaseNumber, summary);
                    UpdateRobots(bus, profile, robots, zombies, batterySystem, robotAttackSystem, modifiers,
                        simTime, phaseNumber, step, summary);
                    UpdateZombies(bus, zombies, robots, baseState, player, batterySystem, durabilitySystem,
                        movement, simTime, phaseNumber, step, summary);
                    zombies.RemoveAll(item => item.Health.IsDead);

                    var alive = AliveCount(zombies);
                    if (alive > summary.PeakAliveCount)
                    {
                        summary.PeakAliveCount = alive;
                        summary.PeakAliveCap = phase.MaxAliveConcurrent;
                    }
                    if (alive > phase.MaxAliveConcurrent)
                        throw new InvalidOperationException("Continuous spawn exceeded maxAliveConcurrent.");

                    baseSampleTimer += step;
                    pressureSampleTimer += step;
                    batterySampleTimer += step;
                    if (baseSampleTimer >= config.Telemetry.SampleIntervalSeconds)
                    {
                        baseSampleTimer -= config.Telemetry.SampleIntervalSeconds;
                        Publish(bus, "base_hp_sampled", simTime, phaseNumber, "hp", baseState.Health.Current);
                    }
                    if (pressureSampleTimer >= config.Telemetry.RoutePressureSampleIntervalSeconds)
                    {
                        pressureSampleTimer -= config.Telemetry.RoutePressureSampleIntervalSeconds;
                        foreach (var route in phase.OpenRoutes)
                            Publish(bus, "route_pressure_sampled", simTime, phaseNumber, "routeId", route,
                                "aliveCount", RouteAliveCount(zombies, route), "distanceToBase", NearestDistanceToBase(zombies, route));
                    }
                    if ((config.Telemetry.BatteryEmitPolicy & BatteryEmitPolicy.EveryNSeconds) != 0 &&
                        batterySampleTimer >= config.Telemetry.BatteryEmitIntervalSeconds)
                    {
                        batterySampleTimer -= config.Telemetry.BatteryEmitIntervalSeconds;
                        foreach (var robot in robots)
                            Publish(bus, "robot_battery_changed", simTime, phaseNumber, "robotId", robot.Id,
                                "value", robot.Battery, "state", robot.BatteryBand);
                    }

                    if (baseState.Health.IsDead || player.Health.IsDead)
                    {
                        summary.Result = GameResult.Defeat;
                        summary.DefeatReason = baseState.Health.IsDead ? DefeatReason.BaseDestroyed : DefeatReason.PlayerDeath;
                        session.Result = summary.Result;
                        session.DefeatReason = summary.DefeatReason;
                        Publish(bus, "phase_failed", simTime, phaseNumber, "defeatReason", summary.DefeatReason);
                        break;
                    }
                    if (nextPending >= pending.Count && alive == 0)
                    {
                        summary.PhasesCleared++;
                        Publish(bus, "base_hp_sampled", simTime, phaseNumber, "hp", baseState.Health.Current);
                        Publish(bus, "player_hp_at_phase_end", simTime, phaseNumber, "hp", player.Health.Current);
                        Publish(bus, "phase_cleared", simTime, phaseNumber, "durationSeconds", phaseElapsed);
                        CombatRules.RecoverBase(baseState, config.Base.PhaseRecoveryFraction);
                        break;
                    }
                    if (simTime >= config.Game.TargetSessionMaximumSeconds)
                    {
                        CombatRules.ApplyDamage(baseState.Health, baseState.Health.Current);
                        Publish(bus, "base_damaged", simTime, phaseNumber, "amount", baseState.Health.Maximum,
                            "hp", baseState.Health.Current);
                    }
                }

                if (summary.Result == GameResult.InProgress && phaseNumber < 3)
                {
                    var offer = upgradeSystem.Offer(rng, session.SelectedUpgrades);
                    var selected = SelectUpgrade(profile, offer, rng);
                    upgradeSystem.Apply(selected, session, baseState, robots, player, modifiers);
                    Publish(bus, "upgrade_selected", simTime, phaseNumber, "upgradeId", selected.Id,
                        "rewardStep", phaseNumber);
                }
            }

            if (summary.Result == GameResult.InProgress)
            {
                summary.Result = GameResult.Victory;
                summary.DefeatReason = DefeatReason.None;
                session.Result = GameResult.Victory;
            }
            summary.DurationSeconds = simTime;
            summary.SessionDurationWithinTarget = simTime >= config.Game.TargetSessionMinimumSeconds &&
                simTime <= config.Game.TargetSessionMaximumSeconds;
            Publish(bus, "simulation_run_completed", simTime, 0, "result", summary.Result,
                "defeatReason", summary.DefeatReason, "durationSeconds", summary.DurationSeconds,
                "phasesCleared", summary.PhasesCleared, "autoChargeStarts", summary.AutoChargeStarts,
                "disabledCount", summary.DisabledCount, "destroyedCount", summary.DestroyedCount,
                "peakAlive", summary.PeakAliveCount);
            Publish(bus, "session_ended", simTime, 0, "result", summary.Result,
                "defeatReason", summary.DefeatReason, "durationSeconds", summary.DurationSeconds);
            sink.Flush();
            return summary;
        }

        public BalanceEvaluation EvaluateBalance(IEnumerable<int> seeds, SimProfileId profileId)
        {
            if (seeds == null) throw new ArgumentNullException("seeds");
            var report = new BalanceEvaluation { ProfileId = profileId };
            var phaseOne = 0;
            var phaseTwo = 0;
            var phaseThree = 0;
            foreach (var seed in seeds)
            {
                var summary = Run(seed, profileId, new InMemoryTelemetrySink());
                report.RunCount++;
                report.AverageDurationSeconds += summary.DurationSeconds;
                if (summary.PhasesCleared >= 1) phaseOne++;
                if (summary.PhasesCleared >= 2) phaseTwo++;
                if (summary.PhasesCleared >= 3) phaseThree++;
            }
            if (report.RunCount == 0) return report;
            report.AverageDurationSeconds /= report.RunCount;
            report.PhaseOneClearRate = (float)phaseOne / report.RunCount;
            report.PhaseTwoClearRate = (float)phaseTwo / report.RunCount;
            report.PhaseThreeClearRate = (float)phaseThree / report.RunCount;
            report.SessionDurationTargetMet = report.AverageDurationSeconds >= config.Game.TargetSessionMinimumSeconds &&
                report.AverageDurationSeconds <= config.Game.TargetSessionMaximumSeconds;
            report.PhaseOneTargetMet = report.PhaseOneClearRate >= 0.90f;
            report.PhaseTwoTargetMet = report.PhaseTwoClearRate >= 0.60f && report.PhaseTwoClearRate <= 0.75f;
            report.PhaseThreeTargetMet = report.PhaseThreeClearRate >= 0.35f && report.PhaseThreeClearRate <= 0.55f;
            return report;
        }

        private void UpdatePlayer(DomainEventBus bus, IDeterministicRng rng, SimPlayerProfileConfig profile,
            SimPlayerRuntime runtime, PlayerState player, List<SimZombie> zombies, float simTime, int phase,
            SimulationSummary summary)
        {
            var step = config.Validation.FixedStepSeconds;
            if (player.Ammo.IsReloading)
            {
                CombatRules.TickReload(player.Ammo, step);
                return;
            }
            if (runtime.ResupplyRemaining > 0f)
            {
                runtime.ResupplyRemaining = Math.Max(0f, runtime.ResupplyRemaining - step);
                if (runtime.ResupplyRemaining <= 0f)
                {
                    CombatRules.Resupply(player.Ammo, config.Ammo);
                    Publish(bus, "ammo_resupplied", simTime, phase, "supplyKind", runtime.ResupplyKind);
                }
                return;
            }

            var alive = AliveCount(zombies);
            if (!runtime.GrenadeUsedThisPhase && player.Grenades > 0 && alive >= profile.GrenadeClusterThreshold)
            {
                runtime.GrenadeUsedThisPhase = true;
                player.Grenades--;
                var affected = 0;
                var ordered = AliveZombiesByPressure(zombies);
                for (var index = 0; index < ordered.Count && index < config.Grenade.MaxTargets; index++)
                {
                    ApplyZombieDamage(bus, ordered[index], config.Grenade.CenterDamage, "player_grenade", simTime, phase, summary);
                    affected++;
                }
                Publish(bus, "grenade_used", simTime, phase, "affectedCount", affected);
            }

            runtime.ActionTimer = Math.Max(0f, runtime.ActionTimer - step);
            if (runtime.ActionTimer > 0f) return;
            runtime.ActionTimer = profile.FireIntervalSeconds;
            if (player.Ammo.Loaded <= 0)
            {
                if (!CombatRules.BeginReload(player.Ammo, config.Weapon.ReloadSeconds))
                {
                    runtime.ResupplyRemaining = config.Ammo.ResupplyUseSeconds;
                    runtime.ResupplyKind = profile.RoutePriorityPolicy == SimRoutePriorityPolicy.LateReactive
                        ? SupplyKind.Safe : SupplyKind.Risky;
                }
                return;
            }

            var target = SelectPlayerTarget(zombies, profile, rng);
            if (target == null) return;
            if (!CombatRules.TryFire(player.Ammo)) return;
            if (rng.NextFloat() > profile.AimAccuracy) return;
            var region = rng.NextFloat() < profile.HeadshotRate ? HitRegion.Head : HitRegion.Body;
            ApplyZombieDamage(bus, target, CombatRules.CalculateBulletDamage(config.Weapon, region),
                "player", simTime, phase, summary);
        }

        private void UpdateRobots(DomainEventBus bus, SimPlayerProfileConfig profile, List<RobotState> robots,
            List<SimZombie> zombies, BatterySystem battery, RobotAttackSystem attacks, RuntimeModifiers modifiers,
            float simTime, int phase, float step, SimulationSummary summary)
        {
            foreach (var robot in robots)
            {
                if (robot.IsDestroyed) continue;
                if (robot.Mode == RobotMode.Disabled || robot.Mode == RobotMode.Recovery)
                {
                    var before = robot.Mode;
                    battery.TickDisabledRecovery(robot, step);
                    if (before != RobotMode.Disabled && robot.Mode == RobotMode.Disabled)
                    {
                        summary.DisabledCount++;
                        Publish(bus, "robot_disabled", simTime, phase, "robotId", robot.Id);
                    }
                    continue;
                }
                if (robot.Mode == RobotMode.Charging ||
                    robot.Battery <= robot.MaximumBattery * profile.RobotChargeThresholdFraction)
                {
                    if (robot.Mode != RobotMode.Charging)
                    {
                        summary.AutoChargeStarts++;
                        Publish(bus, "robot_auto_charge_started", simTime, phase, "robotId", robot.Id);
                    }
                    battery.Charge(robot, step, modifiers.ChargeRateMultiplier);
                    attacks.EndEngagement(robot);
                    continue;
                }

                var target = SelectRobotTarget(zombies, robot.AssignedRoute);
                var previousMode = robot.Mode;
                if (target == null)
                {
                    battery.Drain(robot, RobotActivity.Patrol, step, modifiers.CombatDrainMultiplier);
                    attacks.EndEngagement(robot);
                }
                else
                {
                    battery.Drain(robot, RobotActivity.Combat, step, modifiers.CombatDrainMultiplier);
                    var damage = attacks.Advance(robot, target.Id, step, true, modifiers.FirstDashDamageMultiplier);
                    if (damage > 0f) ApplyZombieDamage(bus, target, damage, robot.Id, simTime, phase, summary);
                }
                if (previousMode != RobotMode.Disabled && robot.Mode == RobotMode.Disabled)
                {
                    summary.DisabledCount++;
                    Publish(bus, "robot_disabled", simTime, phase, "robotId", robot.Id);
                }
            }
        }

        private void UpdateZombies(DomainEventBus bus, List<SimZombie> zombies, List<RobotState> robots,
            BaseState baseState, PlayerState player, BatterySystem battery, RobotDurabilitySystem durability,
            IMovementModel movement, float simTime, int phase, float step, SimulationSummary summary)
        {
            foreach (var zombie in zombies)
            {
                if (zombie.Health.IsDead) continue;
                if (zombie.Progress < 1f)
                {
                    zombie.Progress = movement.Advance(zombie.Progress, zombie.Config.MoveSpeed, step, zombie.PathLength);
                    if (zombie.Progress < 1f) continue;
                }
                zombie.AttackCooldown -= step;
                if (zombie.AttackCooldown > 0f) continue;
                zombie.AttackCooldown = zombie.Config.AttackInterval;

                var targetKind = SelectZombieTargetKind(zombie.Config, robots, baseState, player);
                if (targetKind == TargetKind.Robot)
                {
                    var robot = robots.Find(item => !item.IsDestroyed);
                    if (robot == null) continue;
                    var before = robot.Health.Current;
                    var destroyed = durability.ApplyDamage(robot, zombie.Config.RobotDamage);
                    Publish(bus, "robot_damaged", simTime, phase, "robotId", robot.Id,
                        "amount", before - robot.Health.Current, "hp", robot.Health.Current);
                    if (zombie.Config.Type == ZombieType.Ripper)
                    {
                        battery.ApplyRipperHit(robot);
                        summary.RipperHits++;
                        Publish(bus, "ripper_attacked_robot", simTime, phase, "robotId", robot.Id,
                            "batteryDrained", config.Battery.RipperHitDrain);
                    }
                    if (destroyed)
                    {
                        summary.DestroyedCount++;
                        Publish(bus, "robot_destroyed", simTime, phase, "robotId", robot.Id);
                    }
                }
                else if (targetKind == TargetKind.Player)
                {
                    var applied = CombatRules.ApplyDamage(player.Health, zombie.Config.PlayerDamage);
                    Publish(bus, "player_damaged", simTime, phase, "amount", applied, "hp", player.Health.Current);
                    if (player.Health.IsDead) Publish(bus, "player_died", simTime, phase);
                }
                else
                {
                    var applied = CombatRules.ApplyDamage(baseState.Health, zombie.Config.BaseDamage);
                    Publish(bus, "base_damaged", simTime, phase, "amount", applied, "hp", baseState.Health.Current);
                }
            }
        }

        private static TargetKind SelectZombieTargetKind(ZombieConfig zombie, List<RobotState> robots,
            BaseState baseState, PlayerState player)
        {
            foreach (var target in zombie.TargetPriority)
            {
                if (target == TargetKind.Robot && robots.Exists(item => !item.IsDestroyed)) return target;
                if (target == TargetKind.Player && !player.Health.IsDead) return target;
                if (target == TargetKind.Base && !baseState.Health.IsDead) return target;
            }
            return TargetKind.Base;
        }

        private static SimZombie SelectPlayerTarget(List<SimZombie> zombies, SimPlayerProfileConfig profile,
            IDeterministicRng rng)
        {
            var alive = AliveZombiesByPressure(zombies);
            if (alive.Count == 0) return null;
            if (rng.NextFloat() < profile.RipperFocus)
            {
                var ripper = alive.Find(item => item.Entry.Type == ZombieType.Ripper);
                if (ripper != null) return ripper;
            }
            if (profile.RoutePriorityPolicy == SimRoutePriorityPolicy.LateReactive) return alive[alive.Count - 1];
            return alive[0];
        }

        private static SimZombie SelectRobotTarget(List<SimZombie> zombies, RouteId route)
        {
            SimZombie best = null;
            foreach (var zombie in zombies)
            {
                if (zombie.Health.IsDead || zombie.Entry.Route != route) continue;
                if (best == null || zombie.Progress > best.Progress ||
                    Math.Abs(zombie.Progress - best.Progress) < 0.0001f && string.CompareOrdinal(zombie.Id, best.Id) < 0)
                    best = zombie;
            }
            return best;
        }

        private static UpgradeConfig SelectUpgrade(SimPlayerProfileConfig profile, List<UpgradeConfig> offer,
            IDeterministicRng rng)
        {
            if (profile.UpgradeSelectionPolicy == SimUpgradeSelectionPolicy.RandomOfThree)
                return offer[rng.NextInt(offer.Count)];
            var priorities = profile.UpgradeSelectionPolicy == SimUpgradeSelectionPolicy.IntendedMeta
                ? new[] { "high_efficiency_battery", "combat_power_save", "base_armor", "haetae_charge_boost" }
                : new[] { "base_armor", "combat_power_save", "high_efficiency_battery", "emergency_barrier" };
            foreach (var id in priorities)
            {
                var match = offer.Find(item => item.Id == id);
                if (match != null) return match;
            }
            return offer[0];
        }

        private static void AssignRobotRoutes(List<RobotState> robots, PhaseConfig phase)
        {
            for (var index = 0; index < robots.Count; index++)
                robots[index].AssignedRoute = phase.OpenRoutes[Math.Min(index, phase.OpenRoutes.Length - 1)];
        }

        private static void ApplyZombieDamage(DomainEventBus bus, SimZombie zombie, float damage, string source,
            float simTime, int phase, SimulationSummary summary)
        {
            if (zombie == null || zombie.Health.IsDead) return;
            CombatRules.ApplyDamage(zombie.Health, damage);
            if (!zombie.Health.IsDead) return;
            summary.ZombiesKilled++;
            Publish(bus, "zombie_killed", simTime, phase, "type", zombie.Entry.Type,
                "routeId", zombie.Entry.Route, "by", source);
        }

        private static List<SimZombie> AliveZombiesByPressure(List<SimZombie> zombies)
        {
            var alive = zombies.FindAll(item => !item.Health.IsDead);
            alive.Sort((left, right) =>
            {
                var progress = right.Progress.CompareTo(left.Progress);
                return progress != 0 ? progress : string.CompareOrdinal(left.Id, right.Id);
            });
            return alive;
        }

        private static int AliveCount(List<SimZombie> zombies)
        {
            var count = 0;
            foreach (var zombie in zombies) if (!zombie.Health.IsDead) count++;
            return count;
        }

        private static int RouteAliveCount(List<SimZombie> zombies, RouteId route)
        {
            var count = 0;
            foreach (var zombie in zombies)
                if (!zombie.Health.IsDead && zombie.Entry.Route == route) count++;
            return count;
        }

        private static float NearestDistanceToBase(List<SimZombie> zombies, RouteId route)
        {
            var nearest = 1f;
            foreach (var zombie in zombies)
                if (!zombie.Health.IsDead && zombie.Entry.Route == route) nearest = Math.Min(nearest, 1f - zombie.Progress);
            return nearest;
        }

        private static float RouteLength(RouteConfig route)
        {
            var length = 0f;
            for (var index = 1; index < route.Waypoints.Length; index++)
                length += Float3.Distance(route.Waypoints[index - 1], route.Waypoints[index]);
            return Math.Max(1f, length);
        }

        private static void Publish(DomainEventBus bus, string name, float time, int phase, params object[] payload)
        {
            var gameEvent = new DomainEvent(name, time, phase);
            if (payload.Length % 2 != 0) throw new ArgumentException("Telemetry payload must contain key/value pairs.");
            for (var index = 0; index < payload.Length; index += 2)
                gameEvent.With(Convert.ToString(payload[index]), payload[index + 1]);
            bus.Publish(gameEvent);
        }

        private sealed class SimZombie
        {
            public readonly string Id;
            public readonly SpawnEntry Entry;
            public readonly ZombieConfig Config;
            public readonly HealthState Health;
            public readonly float PathLength;
            public float Progress;
            public float AttackCooldown;

            public SimZombie(string id, SpawnEntry entry, ZombieConfig config, float pathLength)
            {
                Id = id;
                Entry = entry;
                Config = config;
                Health = new HealthState(config.MaxHealth);
                PathLength = pathLength;
            }
        }

        private sealed class SimPlayerRuntime
        {
            public float ActionTimer;
            public float ResupplyRemaining;
            public SupplyKind ResupplyKind;
            public bool GrenadeUsedThisPhase;

            public SimPlayerRuntime(float reactionDelay)
            {
                ActionTimer = Math.Max(0f, reactionDelay);
            }
        }
    }
}
