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
                    BaseMaxHealth = source.game.baseMaxHealth,
                    BasePhaseRecoveryFraction = source.game.baseRecoveryFraction,
                    BaseWarningFraction = source.game.baseWarningFraction,
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
                    MinimumSpawnInterval = source.game.minimumSpawnInterval,
                    DataVersion = source.dataVersion
                },
                Weapon = new WeaponConfig
                {
                    BaseDamage = source.weapon.baseDamage,
                    HeadshotMultiplier = source.weapon.headshotMultiplier,
                    MagazineSize = source.weapon.magazineSize,
                    ReserveAmmo = source.weapon.reserveAmmo,
                    ReloadSeconds = source.weapon.reloadSeconds,
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
                    AttackDamage = source.robot.attackDamage,
                    AttackInterval = source.robot.attackInterval,
                    DetectionRadius = source.robot.detectionRadius,
                    AttackRange = source.robot.attackRange
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
                    BatteryRedFraction = source.warnings.batteryRedFraction,
                    BaseWarningFraction = source.warnings.baseWarningFraction
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
                    ChargingArrivalRadius = source.world.chargingArrivalRadius
                },
                Commands = new CommandConfig { Commands = (RobotCommand[])source.commands.commands.Clone() },
                Telemetry = new TelemetryConfig
                {
                    EnabledEvents = (string[])source.telemetry.enabledEvents.Clone(),
                    SinkFolder = source.telemetry.sinkFolder
                },
                Validation = new ValidationConfig
                {
                    Seeds = (int[])source.validation.seeds.Clone(),
                    FixedStepSeconds = source.validation.fixedStepSeconds
                }
            };

            foreach (var zombie in source.zombies)
            {
                result.Zombies.Add(new ZombieConfig
                {
                    Type = zombie.type,
                    MaxHealth = zombie.maxHealth,
                    MoveSpeed = zombie.moveSpeed,
                    BaseDamage = zombie.baseDamage,
                    PlayerDamage = zombie.playerDamage,
                    RobotDamage = zombie.robotDamage,
                    AttackInterval = zombie.attackInterval,
                    AttackRange = zombie.attackRange,
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
                    RunnerTarget = phase.runnerTarget,
                    BruiserTarget = phase.bruiserTarget,
                    RipperTarget = phase.ripperTarget
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
            foreach (var upgrade in source.upgrades)
            {
                result.Upgrades.Add(new UpgradeConfig
                {
                    Id = upgrade.id,
                    DisplayNameKey = upgrade.displayNameKey,
                    EffectType = upgrade.effectType,
                    Amount = upgrade.amount
                });
            }
            return result;
        }

        public static void Validate(MvpContentCatalog source)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (source.game == null || source.weapon == null || source.grenade == null || source.battery == null ||
                source.robot == null || source.medical == null || source.barrier == null || source.warnings == null ||
                source.world == null || source.commands == null || source.hud == null || source.playerSettings == null || source.telemetry == null ||
                source.validation == null || source.strings == null || source.runtimeMaterialTemplate == null ||
                source.runtimeMaterialTemplate.shader == null)
                throw new InvalidOperationException("Catalog is missing a required shared asset.");
            if (source.zombies == null || source.zombies.Length != 3) throw new InvalidOperationException("Exactly three zombie definitions are required.");
            if (source.phases == null || source.phases.Length != 3) throw new InvalidOperationException("Exactly three phase definitions are required.");
            if (source.routes == null || source.routes.Length != 3) throw new InvalidOperationException("Exactly three route definitions are required.");
            if (source.upgrades == null || source.upgrades.Length != 9) throw new InvalidOperationException("Exactly nine upgrade definitions are required.");
            if (source.supplyPoints == null || source.supplyPoints.Length != 2 ||
                Array.FindAll(source.supplyPoints, item => item != null && item.kind == SupplyKind.Safe).Length != 1 ||
                Array.FindAll(source.supplyPoints, item => item != null && item.kind == SupplyKind.Risky).Length != 1)
                throw new InvalidOperationException("Exactly one Safe and one Risky supply point are required.");
            if (source.commands.commands == null || source.commands.commands.Length != 4)
                throw new InvalidOperationException("Exactly four robot commands are required.");
            foreach (var required in RobotCommandSystem.AllowedCommands)
                if (Array.IndexOf(source.commands.commands, required) < 0) throw new InvalidOperationException("Missing robot command: " + required);
            if (source.hud.elements == null || source.hud.elements.Length != 7)
                throw new InvalidOperationException("HUD must declare all seven required elements.");
            if (source.grenade.innerRadius > source.grenade.radius || source.grenade.edgeDamage > source.grenade.centerDamage)
                throw new InvalidOperationException("Grenade falloff configuration is invalid.");
            if (source.warnings.batteryYellowFraction <= source.warnings.batteryRedFraction)
                throw new InvalidOperationException("Battery yellow threshold must exceed red threshold.");
            if (source.game.jumpHeight <= 0f || source.game.gravity <= 0f)
                throw new InvalidOperationException("Jump height and gravity must both be positive.");
            if (source.game.firstPersonFieldOfView < 30f || source.game.firstPersonFieldOfView > 120f ||
                source.game.thirdPersonFieldOfView < 30f || source.game.thirdPersonFieldOfView > 120f)
                throw new InvalidOperationException("Camera field of view must be between 30 and 120 degrees.");
            if (source.game.cameraCollisionRadius <= 0f || source.game.cameraCollisionPadding < 0f)
                throw new InvalidOperationException("Camera collision settings must be non-negative and use a positive radius.");
            if (source.game.sprintMultiplier < 1f || source.game.sprintMultiplier > 3f)
                throw new InvalidOperationException("Sprint multiplier must be between 1 and 3.");
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
            if (source.weapon.recoilPitchDegrees < 0f || source.weapon.recoilYawDegrees < 0f ||
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
                    zombie.deathPulseSize <= 0f)
                    throw new InvalidOperationException("Zombie presentation tuning is invalid.");
            }

            var ids = new HashSet<string>();
            foreach (var upgrade in source.upgrades)
            {
                if (upgrade == null || string.IsNullOrWhiteSpace(upgrade.id) || !ids.Add(upgrade.id))
                    throw new InvalidOperationException("Upgrade ids must be non-empty and unique.");
            }
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
    }
}
