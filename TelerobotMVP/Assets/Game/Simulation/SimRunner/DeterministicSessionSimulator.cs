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
        public HaetaeSpecialization Haetae1Specialization;
        public HaetaeSpecialization Haetae2Specialization;
        public float Haetae1DamageDealt;
        public float Haetae2DamageDealt;
        public float Haetae1CombatBatterySpent;
        public float Haetae2CombatBatterySpent;
        public int Haetae1Level2Phase;
        public int Haetae2Level2Phase;
        public float Haetae1Level2SimTime;
        public float Haetae2Level2SimTime;
        public bool FirstLevel2WithinPhase2SixtySeconds;
        public bool BothLevel2BeforePhase3;
        public int Haetae1KillsContributed;
        public int Haetae2KillsContributed;
        public int Haetae1DisabledCount;
        public int Haetae2DisabledCount;
        public int Haetae1DestroyedCount;
        public int Haetae2DestroyedCount;
        public float BaseHealthRemaining;
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
            return Run(seed, SimProfileId.Baseline, sink, null);
        }

        public SimulationSummary Run(int seed, SimProfileId profileId, ITelemetrySink sink)
        {
            return Run(seed, profileId, sink, null);
        }

        public SimulationSummary Run(
            int seed,
            SimProfileId profileId,
            ITelemetrySink sink,
            SimulationRunOptions options)
        {
            if (sink == null) throw new ArgumentNullException("sink");
            var profile = config.GetSimPlayerProfile(profileId);
            var loadout = options != null && options.SpecializationLoadout != null
                ? options.SpecializationLoadout
                : profile.DefaultSpecializationLoadout;
            ValidateLoadout(loadout);
            var spawnRng = new XorShiftRng(seed);
            var playerRng = new XorShiftRng(seed ^ unchecked((int)0x6D2B79F5));
            var bus = new DomainEventBus();
            var sessionId = "sim-" + seed + "-" + profileId + "-" + config.Game.DataVersion;
            var bridge = new TelemetryBridge(bus, sink, "simulation", config.Game.DataVersion, sessionId, seed,
                profileId.ToString());
            var spawnSystem = new SpawnSystem(config);
            var batterySystem = new BatterySystem(config.Battery);
            var durabilitySystem = new RobotDurabilitySystem();
            var robotAttackSystem = new RobotAttackSystem(config.Robot);
            var robotCombatPolicy = new RobotCombatPolicy(config);
            var progressionSystem = new HaetaeProgressionSystem();
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
            var robotRuntimes = new List<SimRobotRuntime>
            {
                new SimRobotRuntime(),
                new SimRobotRuntime()
            };
            var modifiers = new RuntimeModifiers();
            var summary = new SimulationSummary
            {
                Seed = seed,
                ProfileId = profileId,
                Result = GameResult.InProgress,
                Haetae1Level2SimTime = -1f,
                Haetae2Level2SimTime = -1f
            };
            var simTime = 0f;
            var phaseTwoStartedAt = -1f;
            var phaseThreeStartedAt = -1f;
            var zombieSerial = 0;

            Publish(bus, "session_started", simTime, 0, "dataVersion", config.Game.DataVersion,
                "simProfileId", profileId);
            for (var phaseNumber = 1; phaseNumber <= config.Phases.Count &&
                summary.Result == GameResult.InProgress; phaseNumber++)
            {
                session.CurrentPhase = phaseNumber;
                var phase = config.GetPhase(phaseNumber);
                if (phaseNumber == 2) phaseTwoStartedAt = simTime;
                else if (phaseNumber == 3) phaseThreeStartedAt = simTime;
                foreach (var robot in robots)
                    durabilitySystem.RestoreAtPhaseStart(robot, config.Robot.MaxHealth, robot.MaximumBattery);
                AssignRobotRoutes(robots, phase);
                foreach (var runtime in robotRuntimes) runtime.RoutePosition = 0f;
                if (config.Ammo.GrenadeResupplyPolicy == GrenadeResupplyPolicy.PhaseResetOnly)
                    player.Grenades = config.Weapon.GrenadesPerPhase;

                var pending = spawnSystem.Compose(phase, spawnRng);
                var scheduler = new ContinuousSpawnScheduler(phase, spawnRng);
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

                    UpdatePlayer(bus, playerRng, profile, playerRuntime, player, robots, zombies,
                        progressionSystem, simTime, phaseNumber, summary);
                    UpdateRobots(bus, profile, robots, robotRuntimes, zombies, batterySystem, robotAttackSystem,
                        robotCombatPolicy, modifiers, progressionSystem, simTime, phaseNumber, step, summary);
                    ApplyReadySpecializations(bus, robots, loadout, progressionSystem, summary, simTime, phaseNumber);
                    UpdateZombies(bus, zombies, robots, baseState, player, batterySystem, durabilitySystem,
                        robotCombatPolicy, movement, simTime, phaseNumber, step, summary);
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
            summary.BaseHealthRemaining = baseState.Health.Current;
            var firstLevel2At = EarliestNonNegative(summary.Haetae1Level2SimTime, summary.Haetae2Level2SimTime);
            summary.FirstLevel2WithinPhase2SixtySeconds = firstLevel2At >= 0f && phaseTwoStartedAt >= 0f &&
                firstLevel2At <= phaseTwoStartedAt + 60f;
            summary.BothLevel2BeforePhase3 = phaseThreeStartedAt >= 0f &&
                summary.Haetae1Level2SimTime >= 0f && summary.Haetae1Level2SimTime < phaseThreeStartedAt &&
                summary.Haetae2Level2SimTime >= 0f && summary.Haetae2Level2SimTime < phaseThreeStartedAt;
            Publish(bus, "simulation_run_completed", simTime, 0, "result", summary.Result,
                "defeatReason", summary.DefeatReason, "durationSeconds", summary.DurationSeconds,
                "phasesCleared", summary.PhasesCleared, "autoChargeStarts", summary.AutoChargeStarts,
                "disabledCount", summary.DisabledCount, "destroyedCount", summary.DestroyedCount,
                "peakAlive", summary.PeakAliveCount,
                "haetae1Specialization", summary.Haetae1Specialization,
                "haetae2Specialization", summary.Haetae2Specialization,
                "haetae1DamageDealt", summary.Haetae1DamageDealt,
                "haetae2DamageDealt", summary.Haetae2DamageDealt,
                "haetae1CombatBatterySpent", summary.Haetae1CombatBatterySpent,
                "haetae2CombatBatterySpent", summary.Haetae2CombatBatterySpent,
                "haetae1Level2Phase", summary.Haetae1Level2Phase,
                "haetae1Level2SimTime", summary.Haetae1Level2SimTime,
                "haetae2Level2Phase", summary.Haetae2Level2Phase,
                "haetae2Level2SimTime", summary.Haetae2Level2SimTime,
                "firstLevel2WithinPhase2SixtySeconds", summary.FirstLevel2WithinPhase2SixtySeconds,
                "bothLevel2BeforePhase3", summary.BothLevel2BeforePhase3,
                "haetae1KillsContributed", summary.Haetae1KillsContributed,
                "haetae2KillsContributed", summary.Haetae2KillsContributed,
                "haetae1DisabledCount", summary.Haetae1DisabledCount,
                "haetae2DisabledCount", summary.Haetae2DisabledCount,
                "haetae1DestroyedCount", summary.Haetae1DestroyedCount,
                "haetae2DestroyedCount", summary.Haetae2DestroyedCount,
                "baseHealthRemaining", summary.BaseHealthRemaining);
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
            SimPlayerRuntime runtime, PlayerState player, List<RobotState> robots, List<SimZombie> zombies,
            HaetaeProgressionSystem progression, float simTime, int phase, SimulationSummary summary)
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
                    ApplyZombieDamage(bus, ordered[index], config.Grenade.CenterDamage,
                        DamageSource.Player("player"), robots, progression, simTime, phase, summary);
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
                DamageSource.Player("player"), robots, progression, simTime, phase, summary);
        }

        private void UpdateRobots(DomainEventBus bus, SimPlayerProfileConfig profile, List<RobotState> robots,
            List<SimRobotRuntime> runtimes, List<SimZombie> zombies, BatterySystem battery,
            RobotAttackSystem attacks, RobotCombatPolicy combatPolicy, RuntimeModifiers modifiers,
            HaetaeProgressionSystem progression, float simTime, int phase, float step,
            SimulationSummary summary)
        {
            for (var robotIndex = 0; robotIndex < robots.Count; robotIndex++)
            {
                var robot = robots[robotIndex];
                var runtime = runtimes[robotIndex];
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
                    var zombiePosition = Math.Max(0f, (1f - target.Progress) * target.PathLength);
                    runtime.TargetDistance = Math.Abs(zombiePosition - runtime.RoutePosition);
                    attacks.Tick(robot, step);
                    var decision = combatPolicy.Decide(robot, runtime.TargetDistance);
                    var profileConfig = combatPolicy.ActiveProfile(robot);
                    if (decision.Movement == RobotMovementIntent.Approach)
                        runtime.RoutePosition = Math.Min(zombiePosition,
                            runtime.RoutePosition + config.Robot.MoveSpeed * step);
                    else if (decision.Movement == RobotMovementIntent.Retreat)
                        runtime.RoutePosition = Math.Max(0f, runtime.RoutePosition - config.Robot.MoveSpeed * step);

                    var batteryBefore = robot.Battery;
                    battery.Drain(robot, RobotActivity.Combat, step,
                        modifiers.CombatDrainMultiplier * profileConfig.CombatBatteryMultiplier *
                        config.HaetaeProgression.CombatBatteryMultiplier(robot.Progression));
                    AddRobotBatteryMetric(summary, robotIndex, Math.Max(0f, batteryBefore - robot.Battery));
                    var attack = attacks.Advance(robot, target.Id, decision, profileConfig,
                        modifiers.FirstDashDamageMultiplier,
                        config.HaetaeProgression.AttackCooldownMultiplier(robot.Progression));
                    attack.Damage *= config.HaetaeProgression.DamageMultiplier(robot.Progression);
                    if (attack.Damage > 0f)
                    {
                        var targets = SelectSimAttackTargets(zombies, target, attack);
                        foreach (var affected in targets)
                        {
                            var before = affected.Health.Current;
                            ApplyZombieDamage(bus, affected, attack.Damage, DamageSource.Haetae(robot.Id),
                                robots, progression, simTime, phase, summary);
                            AddRobotDamageMetric(summary, robotIndex, before - affected.Health.Current);
                        }
                    }
                }
                if (previousMode != RobotMode.Disabled && robot.Mode == RobotMode.Disabled)
                {
                    summary.DisabledCount++;
                    AddRobotDisabledMetric(summary, robotIndex);
                    Publish(bus, "robot_disabled", simTime, phase, "robotId", robot.Id);
                }
            }
        }

        private void UpdateZombies(DomainEventBus bus, List<SimZombie> zombies, List<RobotState> robots,
            BaseState baseState, PlayerState player, BatterySystem battery, RobotDurabilitySystem durability,
            RobotCombatPolicy combatPolicy, IMovementModel movement, float simTime, int phase, float step,
            SimulationSummary summary)
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
                    var incomingMultiplier = combatPolicy.ActiveProfile(robot).IncomingDamageMultiplier *
                        config.HaetaeProgression.IncomingDamageMultiplier(robot.Progression);
                    var destroyed = durability.ApplyDamage(robot, zombie.Config.RobotDamage * incomingMultiplier);
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
                        AddRobotDestroyedMetric(summary, robots.IndexOf(robot));
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

        private static void AssignRobotRoutes(List<RobotState> robots, PhaseConfig phase)
        {
            for (var index = 0; index < robots.Count; index++)
                robots[index].AssignedRoute = phase.OpenRoutes[Math.Min(index, phase.OpenRoutes.Length - 1)];
        }

        private static void ApplyReadySpecializations(
            DomainEventBus bus,
            List<RobotState> robots,
            HaetaeSpecializationPair loadout,
            HaetaeProgressionSystem progression,
            SimulationSummary summary,
            float simTime,
            int phase)
        {
            for (var index = 0; index < robots.Count && index < 2; index++)
            {
                var robot = robots[index];
                if (robot.Progression.SpecializationReady)
                {
                    var specialization = loadout.ForIndex(index);
                    if (progression.SelectSpecialization(robot, specialization) ==
                        SpecializationSelectionResult.Selected)
                    {
                        if (index == 0) summary.Haetae1Specialization = specialization;
                        else summary.Haetae2Specialization = specialization;
                        Publish(bus, "haetae_specialization_selected", simTime, phase,
                            "robotId", robot.Id, "specialization", specialization, "level", robot.Progression.Level,
                            "readyDurationSeconds", 0f);
                    }
                }

                while (robot.Progression.Specialization != HaetaeSpecialization.Unselected &&
                       robot.Progression.UnspentMasteryPoints > 0)
                {
                    var rankOffset = robot.Progression.PowerRank + robot.Progression.ArmorRank +
                        robot.Progression.EfficiencyRank + robot.Progression.AttackSpeedRank + index;
                    var upgrade = (HaetaeMasteryUpgrade)(rankOffset % 4);
                    if (progression.SelectMasteryUpgrade(robot, upgrade) != MasterySelectionResult.Selected)
                        break;
                    Publish(bus, "haetae_mastery_selected", simTime, phase,
                        "robotId", robot.Id, "upgrade", upgrade, "level", robot.Progression.Level,
                        "remainingPoints", robot.Progression.UnspentMasteryPoints,
                        "powerRank", robot.Progression.PowerRank, "armorRank", robot.Progression.ArmorRank,
                        "efficiencyRank", robot.Progression.EfficiencyRank,
                        "attackSpeedRank", robot.Progression.AttackSpeedRank);
                }
            }
        }

        private static void ValidateLoadout(HaetaeSpecializationPair loadout)
        {
            if (loadout == null || !IsSelectable(loadout.Haetae1) || !IsSelectable(loadout.Haetae2))
                throw new ArgumentException("Simulation specialization loadout must contain two selectable roles.");
        }

        private static bool IsSelectable(HaetaeSpecialization role)
        {
            return role == HaetaeSpecialization.Melee ||
                   role == HaetaeSpecialization.Ranged ||
                   role == HaetaeSpecialization.Balanced;
        }

        private static List<SimZombie> SelectSimAttackTargets(
            List<SimZombie> zombies,
            SimZombie primary,
            RobotAttackResult attack)
        {
            var result = new List<SimZombie> { primary };
            if (attack.AreaRadius <= 0f || attack.MaximumTargets <= 1) return result;
            var candidates = zombies.FindAll(item =>
                item != primary && !item.Health.IsDead && item.Entry.Route == primary.Entry.Route &&
                Math.Abs(item.Progress - primary.Progress) * primary.PathLength <= attack.AreaRadius);
            candidates.Sort((left, right) =>
            {
                var progress = right.Progress.CompareTo(left.Progress);
                return progress != 0 ? progress : string.CompareOrdinal(left.Id, right.Id);
            });
            for (var index = 0; index < candidates.Count && result.Count < attack.MaximumTargets; index++)
                result.Add(candidates[index]);
            return result;
        }

        private static void AddRobotDamageMetric(SimulationSummary summary, int robotIndex, float amount)
        {
            if (robotIndex == 0) summary.Haetae1DamageDealt += Math.Max(0f, amount);
            else summary.Haetae2DamageDealt += Math.Max(0f, amount);
        }

        private static void AddRobotBatteryMetric(SimulationSummary summary, int robotIndex, float amount)
        {
            if (robotIndex == 0) summary.Haetae1CombatBatterySpent += Math.Max(0f, amount);
            else summary.Haetae2CombatBatterySpent += Math.Max(0f, amount);
        }

        private void ApplyZombieDamage(DomainEventBus bus, SimZombie zombie, float damage, DamageSource source,
            List<RobotState> robots, HaetaeProgressionSystem progression, float simTime, int phase,
            SimulationSummary summary)
        {
            if (zombie == null || zombie.Health.IsDead) return;
            var applied = CombatRules.ApplyDamage(zombie.Health, damage);
            progression.RecordContribution(zombie.State, source, applied, robots);
            if (!zombie.Health.IsDead) return;
            var awards = progression.AwardForDeath(
                zombie.State,
                zombie.Config.HaetaeExperienceReward,
                robots,
                config.HaetaeProgression);
            foreach (var award in awards)
            {
                if (award.AppliedAmount <= 0) continue;
                Publish(bus, "haetae_xp_gained", simTime, phase,
                    "robotId", award.RobotId, "zombieId", award.ZombieId, "zombieType", award.ZombieType,
                    "rewardAmount", award.RewardAmount, "appliedAmount", award.AppliedAmount,
                    "xpBefore", award.ExperienceBefore, "xpAfter", award.ExperienceAfter,
                    "levelBefore", award.LevelBefore, "levelAfter", award.LevelAfter);
                if (!award.LevelReached) continue;
                Publish(bus, "haetae_level_reached", simTime, phase,
                    "robotId", award.RobotId, "fromLevel", award.LevelBefore, "toLevel", award.LevelAfter,
                    "experience", award.ExperienceAfter,
                    "specializationReady", robots.Find(item => item.Id == award.RobotId).Progression.SpecializationReady);
                if (award.MasteryPointsGained > 0)
                {
                    var robot = robots.Find(item => item.Id == award.RobotId);
                    Publish(bus, "haetae_mastery_point_gained", simTime, phase,
                        "robotId", award.RobotId, "pointsGained", award.MasteryPointsGained,
                        "unspentPoints", robot.Progression.UnspentMasteryPoints, "level", award.LevelAfter);
                }
                if (!award.SpecializationUnlocked) continue;
                RecordRobotLevel2Metric(summary, robots, award.RobotId, phase, simTime);
                Publish(bus, "haetae_specialization_ready", simTime, phase,
                    "robotId", award.RobotId, "level", award.LevelAfter);
            }
            summary.ZombiesKilled++;
            var contributors = new List<string>(zombie.State.Contribution.HaetaeIds);
            contributors.Sort(StringComparer.Ordinal);
            foreach (var contributor in contributors)
                AddRobotKillMetric(summary, robots.FindIndex(item => item.Id == contributor));
            Publish(bus, "zombie_killed", simTime, phase, "type", zombie.Entry.Type,
                "zombieId", zombie.Id, "routeId", zombie.Entry.Route, "by", source.SourceId,
                "sourceKind", source.Kind, "contributingHaetaeCount", contributors.Count,
                "contributingHaetaeIds", string.Join("|", contributors));
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

        private static float EarliestNonNegative(float first, float second)
        {
            if (first < 0f) return second;
            if (second < 0f) return first;
            return Math.Min(first, second);
        }

        private static void RecordRobotLevel2Metric(
            SimulationSummary summary,
            List<RobotState> robots,
            string robotId,
            int phase,
            float simTime)
        {
            var index = robots.FindIndex(item => item.Id == robotId);
            if (index == 0 && summary.Haetae1Level2SimTime < 0f)
            {
                summary.Haetae1Level2Phase = phase;
                summary.Haetae1Level2SimTime = simTime;
            }
            else if (index == 1 && summary.Haetae2Level2SimTime < 0f)
            {
                summary.Haetae2Level2Phase = phase;
                summary.Haetae2Level2SimTime = simTime;
            }
        }

        private static void AddRobotKillMetric(SimulationSummary summary, int robotIndex)
        {
            if (robotIndex == 0) summary.Haetae1KillsContributed++;
            else if (robotIndex == 1) summary.Haetae2KillsContributed++;
        }

        private static void AddRobotDisabledMetric(SimulationSummary summary, int robotIndex)
        {
            if (robotIndex == 0) summary.Haetae1DisabledCount++;
            else if (robotIndex == 1) summary.Haetae2DisabledCount++;
        }

        private static void AddRobotDestroyedMetric(SimulationSummary summary, int robotIndex)
        {
            if (robotIndex == 0) summary.Haetae1DestroyedCount++;
            else if (robotIndex == 1) summary.Haetae2DestroyedCount++;
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
            public readonly ZombieState State;
            public readonly float PathLength;
            public float Progress;
            public float AttackCooldown;
            public HealthState Health { get { return State.Health; } }

            public SimZombie(string id, SpawnEntry entry, ZombieConfig config, float pathLength)
            {
                Id = id;
                Entry = entry;
                Config = config;
                State = new ZombieState(id, entry.Type, entry.Route, config.MaxHealth);
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

        private sealed class SimRobotRuntime
        {
            public float RoutePosition;
            public float TargetDistance;
        }
    }
}
