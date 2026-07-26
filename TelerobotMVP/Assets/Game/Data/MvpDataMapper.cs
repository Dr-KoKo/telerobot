using System;
using System.Collections.Generic;
using Telerobot.Game.Core;

namespace Telerobot.Game.Data
{
    public static class MvpDataMapper
    {
        public static GameplayConfig Map(MvpContentCatalog source)
        {
            Validate(source);
            var result = new GameplayConfig
            {
                Game = new GameRulesConfig
                {
                    PlayerMaxHealth = source.game.playerMaxHealth,
                    TargetSessionMinimumSeconds = source.game.targetSessionMinimumSeconds,
                    TargetSessionMaximumSeconds = source.game.targetSessionMaximumSeconds,
                    FixedStepSeconds = source.game.fixedStepSeconds,
                    PlayerMoveSpeed = source.game.playerMoveSpeed,
                    SprintMultiplier = source.game.sprintMultiplier,
                    Gravity = source.game.gravity,
                    MouseSensitivity = source.game.mouseSensitivity,
                    CameraDistance = source.game.cameraDistance,
                    ThirdPersonFieldOfView = source.game.thirdPersonFieldOfView,
                    FirstPersonFieldOfView = source.game.firstPersonFieldOfView,
                    FirstPersonEyeHeight = source.game.firstPersonEyeHeight,
                    CameraCollisionRadius = source.game.cameraCollisionRadius,
                    CameraCollisionPadding = source.game.cameraCollisionPadding,
                    JumpHeight = source.game.jumpHeight,
                    GroundedVelocity = source.game.groundedVelocity,
                    DataVersion = source.dataVersion
                },
                Base = new BaseConfig
                {
                    MaxHealth = source.baseConfig.maxHealth,
                    PhaseRecoveryFraction = source.baseConfig.phaseRecoveryFraction,
                    WarningFraction = source.baseConfig.warningFraction,
                    AllowPlayerRepair = source.baseConfig.allowPlayerRepair
                },
                Ammo = new AmmoConfig
                {
                    StartReserveAmmo = source.ammo.startReserveAmmo,
                    ReserveAmmoMax = source.ammo.reserveAmmoMax,
                    ResupplyPolicy = source.ammo.resupplyPolicy,
                    ResupplyAmount = source.ammo.resupplyAmount,
                    ResupplyUseSeconds = source.ammo.resupplyUseSeconds,
                    ResupplyCooldownSeconds = source.ammo.resupplyCooldownSeconds,
                    GrenadeResupplyPolicy = source.ammo.grenadeResupplyPolicy
                },
                Weapon = new WeaponConfig
                {
                    BaseDamage = source.weapon.baseDamage,
                    HeadshotMultiplier = source.weapon.headshotMultiplier,
                    MagazineSize = source.weapon.magazineSize,
                    ReloadSeconds = source.weapon.reloadSeconds,
                    FireIntervalSeconds = source.weapon.fireIntervalSeconds,
                    GrenadesPerPhase = source.weapon.grenadesPerPhase,
                    Range = source.weapon.range
                },
                Grenade = new GrenadeConfig
                {
                    Radius = source.grenade.radius,
                    InnerRadius = source.grenade.innerRadius,
                    CenterDamage = source.grenade.centerDamage,
                    EdgeDamage = source.grenade.edgeDamage,
                    MaxTargets = source.grenade.maxTargets,
                    ThrowDistance = source.grenade.throwDistance
                },
                Battery = new BatteryConfig
                {
                    Maximum = source.battery.maximum,
                    LowPowerMaximum = source.battery.lowPowerMaximum,
                    CriticalMaximum = source.battery.criticalMaximum,
                    IdleDrainPerSecond = source.battery.idleDrainPerSecond,
                    PatrolDrainPerSecond = source.battery.patrolDrainPerSecond,
                    CombatDrainPerSecond = source.battery.combatDrainPerSecond,
                    RipperHitDrain = source.battery.ripperHitDrain,
                    ChargePerSecond = source.battery.chargePerSecond,
                    LowPowerMoveMultiplier = source.battery.lowPowerMoveMultiplier,
                    LowPowerAttackMultiplier = source.battery.lowPowerAttackMultiplier,
                    DisabledHoldSeconds = source.battery.disabledHoldSeconds,
                    RecoveryPerSecond = source.battery.recoveryPerSecond,
                    MoveEnableThreshold = source.battery.moveEnableThreshold,
                    YellowWarningFraction = source.battery.yellowWarningFraction,
                    RedWarningFraction = source.battery.redWarningFraction
                },
                Robot = new RobotConfig
                {
                    MaxHealth = source.robot.maxHealth,
                    MoveSpeed = source.robot.moveSpeed,
                    DashDamage = source.robot.dashDamage,
                    BiteDamage = source.robot.biteDamage,
                    BiteCooldownSeconds = source.robot.biteCooldownSeconds,
                    DashCooldownSeconds = source.robot.dashCooldownSeconds,
                    DetectionRadius = source.robot.detectionRadius,
                    EngageRange = source.robot.engageRange,
                    SeparationRadius = source.robot.separationRadius,
                    SeparationStrength = source.robot.separationStrength,
                    FormationSpacing = source.robot.formationSpacing,
                    DefendLeashRadius = source.robot.defendLeashRadius,
                    RunnerKillTargetMinimumSeconds = source.robot.runnerKillTargetMinimumSeconds,
                    RunnerKillTargetMaximumSeconds = source.robot.runnerKillTargetMaximumSeconds,
                    BruiserKillTargetMinimumSeconds = source.robot.bruiserKillTargetMinimumSeconds,
                    BruiserKillTargetMaximumSeconds = source.robot.bruiserKillTargetMaximumSeconds
                },
                Medical = new MedicalConfig
                {
                    MaxHealth = source.medical.maxHealth,
                    HealPerSecond = source.medical.healPerSecond,
                    Radius = source.medical.radius
                },
                Barrier = new BarrierConfig { MaxHealth = source.barrier.maxHealth },
                Warnings = new WarningConfig
                {
                    BatteryYellowFraction = source.warnings.batteryYellowFraction,
                    BatteryRedFraction = source.warnings.batteryRedFraction
                },
                World = new WorldLayoutConfig
                {
                    BasePosition = Point(source.world.basePosition),
                    PlayerStart = Point(source.world.playerStart),
                    RobotStarts = Points(source.world.robotStarts),
                    BaseRally = Point(source.world.baseRally),
                    ChargingStation = Point(source.world.chargingStation),
                    SafeSupply = Point(source.world.safeSupply),
                    RiskySupply = Point(source.world.riskySupply),
                    MedicalAnchor = Point(source.world.medicalAnchor),
                    SupplyInteractionRadius = source.world.supplyInteractionRadius,
                    SupplyExitTolerance = source.world.supplyExitTolerance,
                    BaseChargingRadius = source.world.baseChargingRadius,
                    ChargingArrivalRadius = source.world.chargingArrivalRadius
                },
                Commands = new CommandConfig { Commands = (RobotCommand[])source.commands.commands.Clone() },
                Telemetry = new TelemetryConfig
                {
                    EnabledEvents = (string[])source.telemetry.enabledEvents.Clone(),
                    SinkFolder = source.telemetry.sinkFolder,
                    RequiredFields = (string[])source.telemetry.requiredFields.Clone(),
                    SampleIntervalSeconds = source.telemetry.sampleIntervalSeconds,
                    RoutePressureSampleIntervalSeconds = source.telemetry.routePressureSampleIntervalSeconds,
                    BatteryEmitPolicy = source.telemetry.batteryEmitPolicy,
                    BatteryEmitIntervalSeconds = source.telemetry.batteryEmitIntervalSeconds
                },
                Validation = new ValidationConfig
                {
                    Seeds = (int[])source.validation.seeds.Clone(),
                    FixedStepSeconds = source.validation.fixedStepSeconds
                },
                HaetaeProgression = new HaetaeProgressionConfig
                {
                    ExperiencePerLevel = source.haetaeProgression.experiencePerLevel,
                    ReadyAlertSeconds = source.haetaeProgression.readyAlertSeconds,
                    PowerDamageBonusPerRank = source.haetaeProgression.powerDamageBonusPerRank,
                    ArmorDamageReductionPerRank = source.haetaeProgression.armorDamageReductionPerRank,
                    EfficiencyBatteryReductionPerRank = source.haetaeProgression.efficiencyBatteryReductionPerRank,
                    AttackSpeedBonusPerRank = source.haetaeProgression.attackSpeedBonusPerRank,
                    MinimumReductionMultiplier = source.haetaeProgression.minimumReductionMultiplier
                }
            };

            foreach (var specialization in source.haetaeSpecializations)
            {
                result.HaetaeSpecializations.Add(new HaetaeSpecializationConfig
                {
                    Id = specialization.id,
                    DisplayNameKey = specialization.displayNameKey,
                    DescriptionKey = specialization.descriptionKey,
                    Combat = new RobotCombatProfileConfig
                    {
                        PreferredMinRange = specialization.preferredMinRange,
                        PreferredMaxRange = specialization.preferredMaxRange,
                        DashDamageMultiplier = specialization.dashDamageMultiplier,
                        BiteDamageMultiplier = specialization.biteDamageMultiplier,
                        RangedDamage = specialization.rangedDamage,
                        RangedCooldownSeconds = specialization.rangedCooldownSeconds,
                        CleaveRadius = specialization.cleaveRadius,
                        MaximumTargets = specialization.maximumTargets,
                        IncomingDamageMultiplier = specialization.incomingDamageMultiplier,
                        CombatBatteryMultiplier = specialization.combatBatteryMultiplier
                    }
                });
            }
            foreach (var zombie in source.zombies)
            {
                result.Zombies.Add(new ZombieConfig
                {
                    Type = zombie.type,
                    HaetaeExperienceReward = zombie.haetaeExperienceReward,
                    MaxHealth = zombie.maxHealth,
                    MoveSpeed = zombie.moveSpeed,
                    BaseDamage = zombie.baseDamage,
                    PlayerDamage = zombie.playerDamage,
                    RobotDamage = zombie.robotDamage,
                    AttackInterval = zombie.attackInterval,
                    AttackRange = zombie.attackRange,
                    PathVariationFraction = zombie.pathVariationFraction,
                    SeparationRadius = zombie.separationRadius,
                    SeparationStrength = zombie.separationStrength,
                    ThreatCost = zombie.threatCost,
                    FirstPhase = zombie.firstPhase,
                    TargetPriority = (TargetKind[])zombie.targetPriority.Clone()
                });
            }
            foreach (var phase in source.phases)
            {
                result.Phases.Add(new PhaseConfig
                {
                    Number = phase.number,
                    ThreatBudget = phase.threatBudget,
                    TargetDurationSeconds = phase.targetDurationSeconds,
                    OpenRoutes = (RouteId[])phase.openRoutes.Clone(),
                    OpensNewRoute = phase.opensNewRoute,
                    NewlyOpenedRoute = phase.newlyOpenedRoute,
                    RunnerCount = Range(phase.runnerCount),
                    BruiserCount = Range(phase.bruiserCount),
                    RipperCount = Range(phase.ripperCount),
                    LearningTotal = Range(phase.learningTotal),
                    RunnerMinimum = phase.runnerMinimum,
                    BruiserMinimum = phase.bruiserMinimum,
                    RipperMinimum = phase.ripperMinimum,
                    TrimOrder = (SpawnTrimTarget[])phase.trimOrder.Clone(),
                    PhaseStartDelaySeconds = phase.phaseStartDelaySeconds,
                    GroupIntervalSeconds = phase.groupIntervalSeconds,
                    GroupSize = Range(phase.groupSize),
                    MaxAliveConcurrent = phase.maxAliveConcurrent,
                    RouteWeights = RouteWeights(phase.routeWeights),
                    ZombieTypeRouteWeights = ZombieRouteWeights(phase.zombieTypeRouteWeights)
                });
            }
            foreach (var route in source.routes)
            {
                var points = new Float3[route.waypoints.Length];
                for (var index = 0; index < points.Length; index++)
                    points[index] = new Float3(route.waypoints[index].x, route.waypoints[index].y, route.waypoints[index].z);
                result.Routes.Add(new RouteConfig
                {
                    Id = route.id,
                    OpenPhase = route.openPhase,
                    DisplayNameKey = route.displayNameKey,
                    Waypoints = points,
                    Width = route.width
                });
            }
            foreach (var profile in source.simPlayerProfiles)
            {
                result.SimPlayerProfiles.Add(new SimPlayerProfileConfig
                {
                    Id = profile.id,
                    AimAccuracy = profile.aimAccuracy,
                    HeadshotRate = profile.headshotRate,
                    ReactionDelaySeconds = profile.reactionDelaySeconds,
                    FireIntervalSeconds = profile.fireIntervalSeconds,
                    RoutePriorityPolicy = profile.routePriorityPolicy,
                    RipperFocus = profile.ripperFocus,
                    RobotChargeThresholdFraction = profile.robotChargeThresholdFraction,
                    GrenadeUsePolicy = profile.grenadeUsePolicy,
                    GrenadeClusterThreshold = profile.grenadeClusterThreshold,
                    DefaultSpecializationLoadout = new HaetaeSpecializationPair(
                        profile.defaultSpecializationLoadout.Haetae1,
                        profile.defaultSpecializationLoadout.Haetae2)
                });
            }
            return result;
        }

        public static void Validate(MvpContentCatalog source)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source.game == null || source.baseConfig == null || source.ammo == null || source.weapon == null || source.grenade == null || source.battery == null ||
                source.robot == null || source.medical == null || source.barrier == null || source.warnings == null ||
                source.world == null || source.commands == null || source.hud == null || source.playerSettings == null || source.telemetry == null ||
                source.validation == null || source.haetaeProgression == null || source.strings == null || source.runtimeMaterialTemplate == null ||
                source.runtimeMaterialTemplate.shader == null)
                throw new InvalidOperationException("Catalog is missing a required shared asset.");
            if (source.haetaeSpecializations == null || source.haetaeSpecializations.Length != 3)
                throw new InvalidOperationException("Exactly three Haetae specialization definitions are required.");
            if (source.haetaeProgression.experiencePerLevel <= 0 ||
                source.haetaeProgression.readyAlertSeconds <= 0f ||
                source.haetaeProgression.powerDamageBonusPerRank <= 0f ||
                source.haetaeProgression.armorDamageReductionPerRank <= 0f ||
                source.haetaeProgression.efficiencyBatteryReductionPerRank <= 0f ||
                source.haetaeProgression.attackSpeedBonusPerRank <= 0f ||
                source.haetaeProgression.minimumReductionMultiplier <= 0f ||
                source.haetaeProgression.minimumReductionMultiplier > 1f)
                throw new InvalidOperationException("Haetae progression configuration is invalid.");
            if (source.haetaeProgression.specializations == null ||
                source.haetaeProgression.specializations.Length != source.haetaeSpecializations.Length)
                throw new InvalidOperationException("Haetae progression must reference all three specializations.");
            if (source.zombies == null || source.zombies.Length != 3) throw new InvalidOperationException("Exactly three zombie definitions are required.");
            if (source.phases == null || source.phases.Length != 8) throw new InvalidOperationException("Exactly eight phase definitions are required.");
            if (source.simPlayerProfiles == null || source.simPlayerProfiles.Length != 3)
                throw new InvalidOperationException("Exactly three simulation player profiles are required.");
            if (source.routes == null || source.routes.Length != 3) throw new InvalidOperationException("Exactly three route definitions are required.");
            if (source.supplyPoints == null || source.supplyPoints.Length != 2 ||
                Array.FindAll(source.supplyPoints, item => item != null && item.kind == SupplyKind.Safe).Length != 1 ||
                Array.FindAll(source.supplyPoints, item => item != null && item.kind == SupplyKind.Risky).Length != 1)
                throw new InvalidOperationException("Exactly one Safe and one Risky supply point are required.");
            if (source.commands.commands == null || source.commands.commands.Length != 3)
                throw new InvalidOperationException("Exactly three robot commands are required.");
            foreach (var required in RobotCommandSystem.AllowedCommands)
                if (Array.IndexOf(source.commands.commands, required) < 0) throw new InvalidOperationException("Missing robot command: " + required);
            if (source.hud.elements == null || source.hud.elements.Length != 7)
                throw new InvalidOperationException("HUD must declare all seven required elements.");
            if (source.grenade.innerRadius > source.grenade.radius || source.grenade.edgeDamage > source.grenade.centerDamage)
                throw new InvalidOperationException("Grenade falloff configuration is invalid.");
            if (source.warnings.batteryYellowFraction <= source.warnings.batteryRedFraction)
                throw new InvalidOperationException("Battery yellow threshold must exceed red threshold.");
            if (source.baseConfig.maxHealth <= 0f || source.baseConfig.phaseRecoveryFraction <= 0f ||
                source.baseConfig.warningFraction <= 0f || source.baseConfig.warningFraction >= 1f || source.baseConfig.allowPlayerRepair)
                throw new InvalidOperationException("Base configuration is invalid.");
            if (source.ammo.startReserveAmmo <= 0 || source.ammo.reserveAmmoMax < source.ammo.startReserveAmmo ||
                source.ammo.resupplyUseSeconds <= 0f || source.ammo.resupplyCooldownSeconds < 0f ||
                source.ammo.grenadeResupplyPolicy != GrenadeResupplyPolicy.PhaseResetOnly)
                throw new InvalidOperationException("Ammo reserve configuration is invalid.");
            if (source.robot.dashDamage <= 0f || source.robot.biteDamage <= 0f || source.robot.biteCooldownSeconds <= 0f ||
                source.robot.dashCooldownSeconds <= 0f || source.robot.engageRange <= 0f || source.robot.detectionRadius <= 0f ||
                source.robot.separationRadius <= 0f || source.robot.separationStrength <= 0f ||
                source.robot.formationSpacing < source.robot.separationRadius ||
                source.robot.defendLeashRadius <= source.robot.engageRange)
                throw new InvalidOperationException("Robot attack configuration is invalid.");
            if (source.telemetry.requiredFields == null || Array.IndexOf(source.telemetry.requiredFields, "simProfileId") < 0 ||
                source.telemetry.sampleIntervalSeconds <= 0f || source.telemetry.routePressureSampleIntervalSeconds <= 0f ||
                source.telemetry.batteryEmitIntervalSeconds <= 0f)
                throw new InvalidOperationException("Telemetry sampling configuration is invalid.");
            if (source.game.jumpHeight <= 0f || source.game.gravity <= 0f)
                throw new InvalidOperationException("Jump height and gravity must both be positive.");
            if (source.game.firstPersonFieldOfView < 30f || source.game.firstPersonFieldOfView > 120f ||
                source.game.thirdPersonFieldOfView < 30f || source.game.thirdPersonFieldOfView > 120f)
                throw new InvalidOperationException("Camera field of view must be between 30 and 120 degrees.");
            if (source.game.cameraCollisionRadius <= 0f || source.game.cameraCollisionPadding < 0f)
                throw new InvalidOperationException("Camera collision settings must be non-negative and use a positive radius.");
            if (source.game.sprintMultiplier < 1f || source.game.sprintMultiplier > 3f)
                throw new InvalidOperationException("Sprint multiplier must be between 1 and 3.");
            if (source.world.supplyInteractionRadius <= 0f || source.world.supplyExitTolerance < 0f ||
                source.world.baseChargingRadius <= 0f || source.world.chargingArrivalRadius <= 0f)
                throw new InvalidOperationException("Supply and base charging radii are invalid.");
            if (source.hud.lowAmmoThreshold < 0 || source.hud.lowAmmoThreshold >= source.weapon.magazineSize ||
                source.hud.damageIndicatorSeconds <= 0f || source.hud.hitMarkerSeconds <= 0f ||
                source.hud.headshotLabelSeconds <= 0f)
                throw new InvalidOperationException("HUD feedback tuning is invalid.");
            if (source.playerSettings.minimumMouseSensitivity <= 0f ||
                source.playerSettings.maximumMouseSensitivity < source.playerSettings.minimumMouseSensitivity ||
                source.playerSettings.defaultMouseSensitivity < source.playerSettings.minimumMouseSensitivity ||
                source.playerSettings.defaultMouseSensitivity > source.playerSettings.maximumMouseSensitivity ||
                source.playerSettings.defaultMasterVolume < 0f || source.playerSettings.defaultMasterVolume > 1f ||
                source.playerSettings.defaultEffectsVolume < 0f || source.playerSettings.defaultEffectsVolume > 1f ||
                source.playerSettings.minimumResolutionWidth <= 0 || source.playerSettings.minimumResolutionHeight <= 0 ||
                source.playerSettings.defaultResolutionWidth < source.playerSettings.minimumResolutionWidth ||
                source.playerSettings.defaultResolutionHeight < source.playerSettings.minimumResolutionHeight ||
                !Enum.IsDefined(typeof(CameraPerspective), source.playerSettings.defaultPerspective))
                throw new InvalidOperationException("Player settings defaults are invalid.");
            if (source.weapon.fireIntervalSeconds <= 0f || source.weapon.recoilPitchMinimumDegrees < 0f ||
                source.weapon.recoilPitchMaximumDegrees < source.weapon.recoilPitchMinimumDegrees ||
                source.weapon.recoilYawMaximumDegrees < 0f ||
                source.weapon.recoilRecoveryDegreesPerSecond <= 0f || source.weapon.muzzleFlashSeconds <= 0f ||
                source.weapon.muzzleFlashSize <= 0f || source.weapon.impactPulseSize <= 0f ||
                source.weapon.fireSoundFrequency < 20f || source.weapon.bodyHitSoundFrequency < 20f ||
                source.weapon.headshotSoundFrequency < 20f || source.weapon.combatSoundSeconds <= 0f ||
                source.weapon.fireSoundVolume < 0f || source.weapon.fireSoundVolume > 1f ||
                source.weapon.hitSoundVolume < 0f || source.weapon.hitSoundVolume > 1f ||
                Math.Abs(source.weapon.headshotSoundFrequency - source.weapon.bodyHitSoundFrequency) < 1f)
                throw new InvalidOperationException("Weapon feedback tuning is invalid.");

            foreach (var zombie in source.zombies)
            {
                if (zombie == null || zombie.hitFlashSeconds <= 0f || zombie.deathEffectSeconds <= 0f ||
                    zombie.deathPulseSize <= 0f || zombie.pathVariationFraction <= 0f ||
                    zombie.pathVariationFraction > 0.45f || zombie.separationRadius <= 0f ||
                    zombie.separationStrength <= 0f || zombie.haetaeExperienceReward <= 0)
                    throw new InvalidOperationException("Zombie presentation tuning is invalid.");
            }

            var targetSessionSeconds = 0f;
            for (var index = 0; index < source.phases.Length; index++)
            {
                var phase = source.phases[index];
                var expectedNumber = index + 1;
                var expectedRouteCount = Math.Min(expectedNumber, source.routes.Length);
                if (phase == null || phase.number != expectedNumber || phase.openRoutes == null ||
                    phase.openRoutes.Length != expectedRouteCount)
                    throw new InvalidOperationException("Phase definitions must be contiguous and routes must remain cumulative.");
                var shouldOpenRoute = expectedNumber <= source.routes.Length;
                if (phase.opensNewRoute != shouldOpenRoute)
                    throw new InvalidOperationException("Only phases that introduce a route may declare a route opening.");
                if (phase.opensNewRoute &&
                    Array.Find(source.routes, route => route != null && route.id == phase.newlyOpenedRoute &&
                        route.openPhase == phase.number) == null)
                    throw new InvalidOperationException("The newly opened route must declare the same phase.");
                if (phase.targetDurationSeconds <= 0f)
                    throw new InvalidOperationException("Phase target duration must be positive.");
                targetSessionSeconds += phase.targetDurationSeconds;
                ValidateRange(phase.runnerCount, "runner composition");
                ValidateRange(phase.bruiserCount, "bruiser composition");
                ValidateRange(phase.ripperCount, "ripper composition");
                ValidateRange(phase.learningTotal, "learning total");
                ValidateRange(phase.groupSize, "spawn group size");
                if (phase.groupSize.Min <= 0 || phase.phaseStartDelaySeconds < 0f || phase.groupIntervalSeconds <= 0f ||
                    phase.maxAliveConcurrent <= 0 || phase.trimOrder == null || phase.routeWeights == null ||
                    phase.zombieTypeRouteWeights == null)
                    throw new InvalidOperationException("Phase spawn schedule is invalid.");
                ValidateRouteWeights(phase.openRoutes, phase.routeWeights, "phase route weights");
                foreach (var typedWeights in phase.zombieTypeRouteWeights)
                {
                    if (typedWeights == null) throw new InvalidOperationException("Zombie route weights are missing.");
                    ValidateRouteWeights(phase.openRoutes, typedWeights.Routes, typedWeights.Type + " route weights");
                }
            }
            if (targetSessionSeconds < source.game.targetSessionMinimumSeconds ||
                targetSessionSeconds > source.game.targetSessionMaximumSeconds)
                throw new InvalidOperationException("Phase target durations must add up to the configured session target.");

            var specializationIds = new HashSet<HaetaeSpecialization>();
            foreach (var specialization in source.haetaeSpecializations)
            {
                if (specialization == null || !IsSelectable(specialization.id) || !specializationIds.Add(specialization.id) ||
                    string.IsNullOrWhiteSpace(specialization.displayNameKey) ||
                    string.IsNullOrWhiteSpace(specialization.descriptionKey) ||
                    specialization.preferredMinRange < 0f ||
                    specialization.preferredMaxRange < specialization.preferredMinRange ||
                    specialization.dashDamageMultiplier < 0f || specialization.biteDamageMultiplier < 0f ||
                    specialization.rangedDamage < 0f || specialization.rangedCooldownSeconds < 0f ||
                    specialization.cleaveRadius < 0f || specialization.maximumTargets < 1 ||
                    specialization.incomingDamageMultiplier <= 0f || specialization.combatBatteryMultiplier <= 0f ||
                    specialization.scaleMultiplier.x <= 0f || specialization.scaleMultiplier.y <= 0f ||
                    specialization.scaleMultiplier.z <= 0f)
                    throw new InvalidOperationException("Haetae specialization definitions are invalid.");
                if ((specialization.id == HaetaeSpecialization.Ranged ||
                     specialization.id == HaetaeSpecialization.Balanced) &&
                    (specialization.rangedDamage <= 0f || specialization.rangedCooldownSeconds <= 0f))
                    throw new InvalidOperationException("Ranged Haetae profiles require positive damage and cooldown.");
                if (specialization.id == HaetaeSpecialization.Melee)
                {
                    if (specialization.cleaveRadius <= 0f || specialization.maximumTargets < 2)
                        throw new InvalidOperationException("Melee Haetae profile requires cleave.");
                }
                else if (specialization.cleaveRadius > 0f)
                {
                    throw new InvalidOperationException("Only the Melee Haetae profile may cleave.");
                }

                if (Array.IndexOf(source.haetaeProgression.specializations, specialization) < 0)
                    throw new InvalidOperationException("Progression specialization references do not match the catalog.");
                RequireString(source, specialization.displayNameKey);
                RequireString(source, specialization.descriptionKey);
            }
            if (!specializationIds.SetEquals(new[]
                {
                    HaetaeSpecialization.Melee,
                    HaetaeSpecialization.Ranged,
                    HaetaeSpecialization.Balanced
                }))
                throw new InvalidOperationException("Melee, Ranged, and Balanced Haetae specializations are required.");

            RequireStringValue(source, "haetae.specialization.melee", "근거리형");
            RequireStringValue(source, "haetae.specialization.ranged", "원거리형");
            RequireStringValue(source, "haetae.specialization.balanced", "균형형");
            foreach (var key in new[]
                {
                    "hud.haetae_level", "hud.haetae_experience", "hud.haetae_general",
                    "hud.haetae_specialization_ready", "hud.haetae_choose_specialization",
                    "hud.haetae_specialization_hint", "hud.haetae_mastery_points",
                    "haetae.specialization.melee.description",
                    "haetae.specialization.ranged.description",
                    "haetae.specialization.balanced.description",
                    "haetae.mastery.panel_title",
                    "haetae.mastery.power", "haetae.mastery.power.description",
                    "haetae.mastery.armor", "haetae.mastery.armor.description",
                    "haetae.mastery.efficiency", "haetae.mastery.efficiency.description",
                    "haetae.mastery.attack_speed", "haetae.mastery.attack_speed.description",
                    "radio.phase1", "radio.phase2", "radio.phase3", "radio.phase4",
                    "radio.phase5", "radio.phase6", "radio.phase7", "radio.phase8"
                })
                RequireString(source, key);

            var profileIds = new HashSet<SimProfileId>();
            foreach (var profile in source.simPlayerProfiles)
            {
                if (profile == null || !profileIds.Add(profile.id) || profile.aimAccuracy < 0f || profile.aimAccuracy > 1f ||
                    profile.headshotRate < 0f || profile.headshotRate > 1f || profile.reactionDelaySeconds < 0f ||
                    profile.fireIntervalSeconds <= 0f || profile.ripperFocus < 0f || profile.ripperFocus > 1f ||
                    profile.robotChargeThresholdFraction < 0f || profile.robotChargeThresholdFraction > 1f ||
                    profile.grenadeClusterThreshold < 1 || profile.defaultSpecializationLoadout == null ||
                    !IsSelectable(profile.defaultSpecializationLoadout.Haetae1) ||
                    !IsSelectable(profile.defaultSpecializationLoadout.Haetae2))
                    throw new InvalidOperationException("Simulation player profiles are invalid.");
            }
        }

        private static bool IsSelectable(HaetaeSpecialization specialization)
        {
            return specialization == HaetaeSpecialization.Melee ||
                   specialization == HaetaeSpecialization.Ranged ||
                   specialization == HaetaeSpecialization.Balanced;
        }

        private static void RequireString(MvpContentCatalog source, string key)
        {
            if (source.strings.entries == null ||
                !source.strings.entries.Exists(item => item != null && item.key == key && !string.IsNullOrWhiteSpace(item.value)))
                throw new InvalidOperationException("Missing required string: " + key);
        }

        private static void RequireStringValue(MvpContentCatalog source, string key, string expected)
        {
            RequireString(source, key);
            if (source.strings.Get(key) != expected)
                throw new InvalidOperationException("Required string value does not match: " + key);
        }

        private static Float3 Point(UnityEngine.Vector3 value)
        {
            return new Float3(value.x, value.y, value.z);
        }

        private static Float3[] Points(UnityEngine.Vector3[] values)
        {
            var result = new Float3[values.Length];
            for (var index = 0; index < values.Length; index++) result[index] = Point(values[index]);
            return result;
        }

        private static IntRangeConfig Range(IntRangeConfig source)
        {
            if (source == null) return null;
            return new IntRangeConfig { Min = source.Min, Max = source.Max };
        }

        private static RouteWeightConfig[] RouteWeights(RouteWeightConfig[] source)
        {
            if (source == null) return null;
            var result = new RouteWeightConfig[source.Length];
            for (var index = 0; index < source.Length; index++)
                result[index] = new RouteWeightConfig { Route = source[index].Route, Weight = source[index].Weight };
            return result;
        }

        private static ZombieRouteWeightConfig[] ZombieRouteWeights(ZombieRouteWeightConfig[] source)
        {
            if (source == null) return null;
            var result = new ZombieRouteWeightConfig[source.Length];
            for (var index = 0; index < source.Length; index++)
                result[index] = new ZombieRouteWeightConfig { Type = source[index].Type, Routes = RouteWeights(source[index].Routes) };
            return result;
        }

        private static void ValidateRange(IntRangeConfig range, string label)
        {
            if (range == null || range.Min < 0 || range.Max < range.Min)
                throw new InvalidOperationException("Invalid " + label + " range.");
        }

        private static void ValidateRouteWeights(RouteId[] openRoutes, RouteWeightConfig[] weights, string label)
        {
            if (weights == null || weights.Length != openRoutes.Length)
                throw new InvalidOperationException(label + " must cover every open route.");
            var sum = 0f;
            foreach (var route in openRoutes)
            {
                var weight = Array.Find(weights, item => item != null && item.Route == route);
                if (weight == null || weight.Weight < 0f) throw new InvalidOperationException("Invalid " + label + ".");
                sum += weight.Weight;
            }
            if (Math.Abs(sum - 1f) > 0.001f) throw new InvalidOperationException(label + " must sum to one.");
        }
    }
}
