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
| `GameConfig` | playerMaxHp, baseMaxHp, basePhaseRecoveryPct, baseWarningPct, targetSessionMinutes, simFixedStepSeconds | 0 < pct ≤ 1; positive HP |
| `WeaponDef` | baseDamage, headshotMultiplier, magazineSize, reloadSeconds, startGrenades | all > 0; mult ≥ 1 |
| `GrenadeDef` | radiusMeters, centerDamage, innerFullDamageRadiusMeters, edgeDamageAtRadius, maxAffectedZombies, affects=Zombies | inner ≤ radius; edge ≤ center; cap ≥ 1 |
| `BaseConfig` | maxHp, phaseRecoveryPct, warningPct, allowPlayerRepair=false | warningPct in (0,1) |
| `RouteDef` (×3) | id, openPhase, character flags, waypoints[], chokeAnchor | waypoints ordered, terminate at base; openPhase ∈ {1,2,3} |
| `ZombieDef` (×3) | hp, moveSpeed, baseDamage, playerDamage, robotDamage, targetPriority[3], threatCost, special, firstAppears, distinctAV | priority is a permutation of {base,player,robot}; cost>0 |
| `PhaseDef` (×3) | number, openRoutes[], threatBudget, recommendedComposition, specialMinimums, targetDifficulty, targetDuration, deploysUnit | budget>0; minimums ≥ 0; sum(spawn cost) ≤ budget |
| `RobotDef` | hp, maxBattery, moveSpeed, attack, killRunnerSeconds, killBruiserSeconds | hp,battery>0 |
| `BatteryConfig` | max, band thresholds, drainIdle/Patrol/Combat, ripperHitDrain, chargeRate, lowPowerMoveMult, lowPowerAttackMult, disabledHoldSeconds, recoveryRate, moveEnableThreshold, warnYellowPct, warnRedPct | bands partition 0..max; rates>0; mults in (0,1] |
| `MedicalRobotDef` | hp, healRate, radiusMeters, healsPlayerFirst=true, attacks=false, regenAfterDestroy=false | hp,heal,radius>0 |
| `UpgradeDef` (×9) | id, displayNameKey, effectType, effectParams, applyTiming, targetSystem | exactly 9 ids; ids unique |
| `BarrierConfig` | hp, perOpenRoute=true, placement=ChokeAnchor, durationPolicy=UntilDestroyedOrPhaseEnd, blocksNavPermanently=false | hp>0; blocksNavPermanently MUST be false |
| `AmmoConfig` | magazineSize, reloadSeconds | match WeaponDef |
| `SupplyPointConfig` (×2) | id, kind∈{Safe,Risky}, location | exactly one Safe + one Risky |
| `CommandConfig` | commands = [DefendPosition, PatrolRoute, ReturnToBase, Charge] | exactly these 4, no more (FR-085) |
| `WarningConfig` | batteryYellowPct, batteryRedPct, baseWarningPct, ripperCalloutEnabled | yellow>red |
| `HudConfig` | element list, infoPriority order, combatBundle, awarenessBundle | priority = base HP > robot battery > route alert |
| `RadioEventDef` + `StringTable` | eventId → stringKey → verbatim Korean text + clip ref | strings verbatim (see strings.contract.md) |
| `TelemetryConfig` | enabledEvents[], sinkPath, requiredFields | includes constitution minimum set |
| `ValidationConfig`/`SimParams` | seeds[], fixedStep, per-scenario params | seeds explicit |

## Acceptance

- [ ] No balance value appears inline in code (grep for magic numbers in `Game.Runtime`/`Game.Core` rule classes returns only references to config fields).
- [ ] All 9 `UpgradeDef` ids present; `CommandConfig` has exactly 4 commands; exactly one Safe + one Risky supply point.
- [ ] `Game.Core` compiles with no `UnityEngine` reference (assembly definition enforced).
