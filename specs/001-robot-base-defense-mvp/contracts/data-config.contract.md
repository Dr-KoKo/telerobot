# Contract: Gameplay Data / Configuration Assets

**Feature**: `001-robot-base-defense-mvp` | Non-REST contract (Unity game). This is the **data-driven balance surface** (Constitution II). Each asset is a ScriptableObject that exposes the fields below and maps them to plain config structs consumed by `Game.Core`. Field values and sources are in [../data-model.md](../data-model.md); this contract fixes the **shape and invariants**.

## Contract rules (apply to all assets)

- Every tunable gameplay/balance value MUST be a serialized field on one of these assets — **never an inline constant** in a MonoBehaviour, adapter, or domain class (Constitution II gate).
- The pure core MUST receive values via plain structs/interfaces, not by referencing `UnityEngine` SO types directly.
- A `dataVersion` string MUST be derivable from the asset set (e.g. a `DataVersion` asset or hash) and stamped into telemetry (Constitution VIII).
- Changing a value MUST NOT require code changes or recompilation of `Game.Core`.

## Assets and required fields

| Asset | Required fields (shape) | Invariants |
|-------|--------------------------|-----------|
| `GameConfig` | playerMaxHp, targetSessionMinutes, simFixedStepSeconds, playerMoveSpeed, sprintMultiplier, gravity, mouseSensitivity, cameraDistance, third/first-person FOV, first-person eye height, camera collision radius/padding, jumpHeight, groundedVelocity, config refs | **owns only playerMaxHp** among entity health values; movement/camera values satisfy FR-010/010A–C; MUST NOT re-declare base HP/recovery/warning (→ BaseConfig) or magazine/reload (→ WeaponDef) |
| `WeaponDef` | baseDamage, headshotMultiplier, magazineSize, reloadSeconds, fireIntervalSeconds, startGrenades, recoilPitchMinimum/Maximum, recoilYawMaximum, recoilRecovery, muzzleFlash duration/size, impact size, fire/body/headshot tone frequencies, feedback volumes | gameplay values > 0; mult ≥ 1; pitch max ≥ min ≥ 0; yaw max ≥ 0; feedback durations/sizes/frequencies > 0; body/headshot tones distinct; volumes in [0,1] |
| `GrenadeDef` | radiusMeters, centerDamage, innerFullDamageRadiusMeters, edgeDamageAtRadius, maxAffectedZombies, affects=Zombies | inner ≤ radius; edge ≤ center; cap ≥ 1 |
| `BaseConfig` | maxHp, phaseRecoveryPct, warningPct, allowPlayerRepair=false | warningPct in (0,1) |
| `RouteDef` (×3) | id, openPhase, character flags, waypoints[], chokeAnchor | waypoints ordered, terminate at base; openPhase ∈ {1,2,3} |
| `ZombieDef` (×3) | hp, moveSpeed, baseDamage, playerDamage, robotDamage(label), robotDamageNumeric, targetPriority[3], threatCost, attackIntervalSeconds, attackMode, targetAttackRange, special, firstAppears, distinctAV, hitFlashSeconds, deathEffectSeconds, deathPulseSize | priority is a permutation of {base,player,robot}; cost>0; robotDamageNumeric>0; interval>0; attackMode∈{OneShot,RepeatedUntilKilled} (MVP=RepeatedUntilKilled); feedback values > 0 |
| `PhaseDef` (×3) | number, **openRoutes[] (cumulative)**, **newlyOpenedRoute**, threatBudget, composition{runner,bruiser,ripper as min–max ranges}, learningTargetTotalRange, specialMinimums, trimOrder, spawnSchedule{phaseStartDelay,groupInterval,groupSizeRange}, maxAliveConcurrent, routeWeights, zombieTypeWeightsByRoute, specialSpawnPolicy, targetDifficulty, targetDuration, deploysUnit | budget>0; minimums ≥ 0; **openRoutes is cumulative** (P1⊂P2⊂P3) and `newlyOpenedRoute ∈ openRoutes`; achievable spawn total (after trimOrder within budget) ∈ learningTargetTotalRange; ripper route weight South > other routes (FR-034); routeWeights sum to 1 over open routes; per-type route weights sum to 1 over open routes |
| `RobotDef` (+ `RobotAttackDef`) | hp, maxBattery, moveSpeed, attack, killRunnerSeconds, killBruiserSeconds; **dashDamage, biteDamage, biteCooldownSeconds, dashCooldownSeconds, engageRange, detectionRadius, separationRadius/Strength, formationSpacing, defendLeashRadius** | hp,battery>0; damages>0; cooldowns/radii/spacing>0; kill*Seconds are validation targets, not inputs |
| `BatteryConfig` | max, band thresholds, drainIdle/Patrol/Combat, ripperHitDrain, chargeRate, lowPowerMoveMult, lowPowerAttackMult, disabledHoldSeconds, recoveryRate, moveEnableThreshold, warnYellowPct, warnRedPct | bands partition 0..max; rates>0; mults in (0,1] |
| `MedicalRobotDef` | hp, healRate, radiusMeters, healsPlayerFirst=true, attacks=false, regenAfterDestroy=false | hp,heal,radius>0 |
| `WorldLayoutConfig` | base/route/station anchors, supplyInteractionRadius, supplyExitTolerance, baseChargingRadius, chargingArrivalRadius | radii > 0; supplyExitTolerance ≥ 0; chargeRate is owned by `BatteryConfig.chargeRate`; `Charging` disallows simultaneous attack but nearby base threats interrupt into `Engage` (FR-035/037/097) |
| `UpgradeDef` (×9) | id, displayNameKey, effectType, effectParams, applyTiming, targetSystem | exactly 9 ids; ids unique |
| `BarrierConfig` | hp, perOpenRoute=true, placement=ChokeAnchor, durationPolicy=UntilDestroyedOrPhaseEnd, blocksNavPermanently=false | hp>0; blocksNavPermanently MUST be false |
| `AmmoConfig` | (references WeaponDef.magazineSize/reloadSeconds — not re-declared), startReserveAmmo, reserveAmmoMax, resupplyPolicy∈{FullReserve,FixedAmount}, resupplyAmount, resupplyUseSeconds, resupplyCooldownSeconds, grenadeResupplyPolicy∈{None,PhaseResetOnly} | 0<startReserve≤max; grenadeResupplyPolicy=PhaseResetOnly (spec); any mirrored magazine/reload MUST equal WeaponDef |
| `SupplyPointConfig` (×2) | id, kind∈{Safe,Risky}, location | exactly one Safe + one Risky |
| `SimPlayerProfile` (×3) | id∈{Novice,Baseline,Skilled}, aimAccuracy, headshotRate, reactionDelaySeconds, routePriorityPolicy, ripperFocus, robotChargeThresholdPct, upgradeSelectionPolicy, grenadeUsePolicy | 0≤accuracy≤1; Baseline = intended MVP average |
| `CommandConfig` | commands = [DefendPosition, PatrolRoute, ReturnToBase] | exactly these 3, no more; Charge is automatic and not a command (FR-085) |
| `WarningConfig` | batteryYellowPct, batteryRedPct, baseWarningPct, ripperCalloutEnabled | yellow>red |
| `HudConfig` | element list, infoPriority order, combatBundle, awarenessBundle, lowAmmoThreshold, damage/hit/headshot feedback durations | priority = base HP > robot battery > route alert; thresholds non-negative; durations > 0 |
| `PlayerSettings` | mouseSensitivity min/max/default, master/effects volume defaults, minimum/default resolution, defaultFullscreen, defaultPerspective | sensitivity default within min/max; volumes in [0,1]; default resolution ≥ minimum; perspective valid |
| `RadioEventDef` + `StringTable` | eventId → stringKey → verbatim Korean text + clip ref | strings verbatim (see strings.contract.md) |
| `TelemetryConfig` | enabledEvents[], sinkPath, requiredFields, sampleIntervalSeconds, routePressureSampleIntervalSeconds, batteryEmitPolicy, batteryEmitIntervalSeconds | includes constitution minimum set; all sampling cadences counted on the sim clock (deterministic) |
| `ValidationConfig`/`SimParams` | seeds[] (fully enumerated), fixedStep, simPlayerProfileId, per-scenario params | seeds explicit + pinned — **no ellipsis; every seed listed** |

## Acceptance

- [x] No balance value appears inline in code; runtime/core rule classes consume config fields, with presentation-only constants excluded from this balance rule.
- [x] All 9 `UpgradeDef` ids present; `CommandConfig` has exactly 3 commands; exactly one Safe + one Risky supply point.
- [x] `Game.Core` compiles with no `UnityEngine` reference (assembly definition enforced).
- [x] **Single source of truth:** base HP/recovery/warning declared only in `BaseConfig`; magazine/reload only in `WeaponDef`; per-zombie base damage only in `ZombieDef`. Any `mirrored` field has a validation rule asserting equality.
- [x] **Cumulative routes:** `PhaseDef.openRoutes` is cumulative (P1⊂P2⊂P3) and `newlyOpenedRoute ∈ openRoutes`.
- [x] **Composition:** achievable spawn total after budget trim ∈ `learningTargetTotalRange`; Ripper South-Tunnel route weight > other routes.
- [x] **Seeds** in `SimParams` are fully enumerated (no ellipsis); telemetry sampling cadences are set and sim-clock-based.
