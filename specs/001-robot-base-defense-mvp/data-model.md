# Data Model: 「텔레 로봇팀, 출격하라」 MVP

**Feature**: `001-robot-base-defense-mvp` | **Date**: 2026-06-27 | **Plan**: [plan.md](./plan.md)

All tunable values live in **ScriptableObject data assets** (Constitution II) that map to **plain config structs** consumed by the pure `Game.Core` (Constitution III). Values cite their spec FR; values labeled **(PLANNING — to balance)** are introduced by the plan (see research.md §4–5) and are not spec-derived. Runtime/simulation entity state is held in pure C# state objects driven by the core.

---

## Data assets (ScriptableObject definitions)

### GameConfig
| Field | Value | Source |
|-------|-------|--------|
| playerMaxHp | 100 | FR-017 |
| baseMaxHp | 1000 | FR-020 |
| basePhaseRecoveryPct | 0.15 (15%) | FR-021 |
| baseWarningPct | 0.30 (≤30%) | FR-023, FR-125 |
| targetSessionMinutes | 10–15 | FR-006, SC-001 |
| simFixedStepSeconds | 1/60 (tunable) | research.md §3 |

### WeaponDef (Assault Rifle)
| Field | Value | Source |
|-------|-------|--------|
| baseDamage | 30 | FR-012 |
| headshotMultiplier | 2.5 | FR-013 |
| magazineSize | 30 | FR-014 |
| reloadSeconds | 2 | FR-015 |
| startGrenades | 2 (reset each phase) | FR-018, Assumptions |
Validation: Runner body ≈3 hits, head 1–2 (FR-042 sanity); headshot applies only to head-hitbox zombies, not robots (Assumptions).

### GrenadeDef **(PLANNING — to balance)**
| Field | Value | Source |
|-------|-------|--------|
| radiusMeters | 5 | research.md §4 |
| centerDamage | 150 | research.md §4 |
| innerFullDamageRadiusMeters | 2 | research.md §4 |
| edgeDamageAtRadius | 60 (at 5 m) | research.md §4 |
| falloff | linear 150→60 between 2 m→5 m | research.md §4 |
| maxAffectedZombies | 10 | research.md §4 |
| affects | zombies only | research.md §4 |

### BaseConfig
maxHp 1000 (FR-020); phaseRecoveryPct 0.15 (FR-021); no player repair (FR-022); warning ≤30% (FR-023); defeat at 0 (FR-024). Bruiser deals 60 (FR-043), Runner 8 (FR-042), Ripper 10 (FR-044) on reaching base.

### RouteDef ×3
| Field | North Road | East Alley | South Tunnel |
|-------|-----------|-----------|--------------|
| id | NorthRoad | EastAlley | SouthTunnel |
| openPhase | 1 | 2 | 3 |
| character | wide, high readability, larger groups (FR-032) | shorter, faster base pressure (FR-033) | limited sightlines, Ripper-favored (FR-034) |
| waypoints | ordered list → base | ordered list → base | ordered list → base |
| chokeAnchor | base-side entry point | base-side entry point | base-side entry point |
All routes converge on the central base (FR-025, FR-030). Waypoints are the authoritative path/progression model (research.md §3).

### ZombieDef ×3
| Field | Runner | Bruiser | Ripper |
|-------|--------|---------|--------|
| hp | 90 | 500 | 180 |
| moveSpeed | fast | slow | fast |
| baseDamage | 8 | 60 | 10 |
| playerDamage | 12 | 30 | 18 |
| robotDamage | low | medium | medium |
| targetPriority | base > player > robot | base > robot > player | robot > player > base |
| threatCost | 1 | 5 | 4 |
| special | — | — | +5 robot battery drain per hit (FR-045) |
| firstAppears | Phase 1 | Phase 2 | Phase 3 |
| distinct A/V | — | — | required (FR-047): special icon + voice callout |
Source: FR-040..047, FR-052. Only these 3 types (FR-040, FR-140).

### PhaseDef ×3
| Field | Phase 1 | Phase 2 | Phase 3 |
|-------|---------|---------|---------|
| number | 1 | 2 | 3 |
| openRoutes | NorthRoad | +EastAlley | +SouthTunnel |
| threatBudget | 40 | 60 | 80 |
| recommendedComposition | Runner 20–30 | total 35–50 incl. Bruiser 2–3 | total 55–75 incl. Ripper 3–5 |
| specialMinimums | — | Bruiser ≥2 | Ripper ≥3 |
| targetDifficulty/duration | low / 2–3 min | medium / 3–4 min | high / 4–5 min |
| deploysUnit | 2 Haetae (start) | — | Medical robot |
Source: FR-003, FR-031, FR-051..054, FR-064. Budget-vs-target reconciliation: budget is a hard cap; preserve special minimums, trim cheapest unit (Runner) to fit (Assumptions "위협 예산 vs 목표 마릿수").

### RobotDef (Haetae) — 2 instances
| Field | Value | Source |
|-------|-------|--------|
| hp | 300 | FR-071 |
| maxBattery | 100 | FR-072, FR-090 |
| moveSpeed | fast | FR-073 |
| attack | melee dash + bite | FR-074 |
| killRunnerSeconds | ~1–2 | FR-075 |
| killBruiserSeconds | ~6–10 | FR-076 |
Strong vs normal zombies, weak to battery pressure + Ripper (FR-077).

### BatteryConfig
| Field | Value | Source |
|-------|-------|--------|
| max | 100 | FR-090 |
| stateNormal | 31–100 | FR-091 |
| stateLowPower | 11–30 | FR-091 |
| stateCritical | 1–10 | FR-091 |
| stateDepleted | 0 | FR-091 |
| drainIdle | 0.3 / s | FR-092 |
| drainPatrol | 0.8 / s | FR-092 |
| drainCombat | 2.5 / s | FR-092 |
| ripperHitDrain | 5 | FR-045, FR-093 |
| chargeRate | 4 / s | FR-094 |
| lowPowerMoveSpeedMult | 0.85 (−15%) | FR-095 |
| lowPowerAttackSpeedMult | 0.90 (−10%) | FR-095 |
| disabledHoldSeconds | 5 (PLANNING — to balance) | Assumptions, FR-080 |
| recoveryRate | 0.5 / s (PLANNING — to balance) | Assumptions |
| moveEnableThreshold | 5 (PLANNING — to balance) | Assumptions |
| warnYellowPct | <25% | FR-123 |
| warnRedPct | <10% | FR-124 |
Note (Assumptions): mechanical state bands (Low Power/Critical) and UI warning thresholds (25%/10%) are intentionally separate layers.

### MedicalRobotDef (Phase 3)
hp 150 (FR-101); deploys Phase 3 (FR-100); stays near base (FR-102); non-combat, no attack (FR-103, Assumptions); heals player first (FR-104); healRate 8 HP/s (FR-105); radiusMeters 6 (FR-106); destructible (FR-107); destroyed zone not regenerated this session (Assumptions).

### UpgradeDef ×9 (FR-110..115)
3-of-9 offered after Phase 1 and Phase 2; max 2 selections/session; same 9 candidates every reward step.

| # | id | Effect | Apply rule |
|---|----|--------|-----------|
| 1 | high_efficiency_battery | all robots maxBattery +20 | adds to current battery too; Haetae only (Assumptions) |
| 2 | combat_power_save | robot combat drain −20% | next phase |
| 3 | haetae_charge_boost | Haetae first-dash damage +40% | first dash per engagement (Assumptions) |
| 4 | charge_station_speedup | charge rate +30% | applies to charging |
| 5 | base_armor | base maxHp +200 | adds to current HP too (Assumptions) |
| 6 | emergency_barrier | spawn 1 one-shot barrier per open route at phase start | see BarrierConfig |
| 7 | piercing_rounds | bullet pierces 1 extra Runner | Runner-only; stops if 2nd target is Bruiser/Ripper (Assumptions) |
| 8 | extended_magazine | magazine +15 | max only now; current ammo fills to new max on next reload (Assumptions) |
| 9 | emergency_recovery_protocol | medical heal +30% | reserved at choice, applied when medical robot exists in Phase 3 (FR-115) |
Mix includes numeric-improvement and playstyle-change types (FR-114).

### BarrierConfig **(PLANNING — to balance)**
hp 300; one per open route at phase start (upgrade #6 active); placement = base-side choke anchor; lasts until destroyed or phase end; destroyed by cumulative zombie damage; must not permanently block player/robot nav. Source: research.md §5, FR-113#6, Assumptions.

### AmmoConfig / SupplyPointConfig ×2
magazineSize 30, reload 2 s (FR-014/015). Two supply points (FR-037): `safe` (inside/adjacent base), `risky` (outside/near combat). Resupply refills reserve ammo on interaction (depletion + reload timing + resupply tracked in core).

### CommandConfig (Robot Commands)
Exactly 4 commands, no others (FR-085, FR-140): `DefendPosition`, `PatrolRoute`, `ReturnToBase`, `Charge`. Robots individually selectable; commands per-robot (Assumptions). PatrolRoute takes an open-route target; DefendPosition takes a point/route (Assumptions).

### WarningConfig / HudConfig
HUD elements (FR-120): base HP, phase progress, route alert/minimap, robot battery, player HP, ammo, command quick-menu. Info priority: base HP > robot battery > route alert (FR-121). Thresholds: battery <25% yellow flash + callout (FR-123), <10% red flash + urgent callout (FR-124), base ≤30% edge warning + alarm (FR-125), Ripper appearance special icon + callout (FR-126), new route open highlight + radio (FR-122). Minimum combat HUD bundle ships with US1/US2; situational-awareness bundle with US5 (FR-120a). No info overload (FR-127).

### RadioEventDef + StringTable
8 radio/sound events (FR-130); strings stored verbatim Korean — see [contracts/strings.contract.md](./contracts/strings.contract.md). Triggers implemented with their gameplay milestone (research.md / plan Sound section). MVP uses captions + placeholder/TTS-stub audio; final VO swaps clips without changing event logic.

### TelemetryConfig
Event names + required fields per [contracts/telemetry.contract.md](./contracts/telemetry.contract.md); dev-only local file sink (research.md §8).

### ValidationConfig / SimParams
Deterministic-sim seeds, fixed step, per-scenario parameters — see [contracts/validation-scenarios.contract.md](./contracts/validation-scenarios.contract.md).

---

## Runtime/simulation state entities (pure C# state objects)

### SessionState
currentPhase (1–3), result (InProgress/Victory/Defeat), defeatReason (BaseDestroyed/PlayerDeath/null), elapsedSimTime, seed, upgradesSelected (≤2), openRoutes set. Win: Phase 3 cleared with base HP ≥1 (FR-004). Defeat: base HP 0 or player HP 0 (FR-005).

### PhaseState
number, openRoutes, threatBudget, plannedComposition, spawnedCount, aliveCount, cleared(bool). Transition (FR-061, ordered): ① all spawned → ② field cleared → ③ base alive → ④ phase cleared → ⑤ upgrade (if eligible) → ⑥ open next route → ⑦ start next phase.

### PlayerState
hp (0–100), grenades, weapon ammo state (loaded, reserve). Death → defeat (FR-017).

### BaseState
hp (0–1000), warningActive(≤30%). PhaseClear → +15% maxHp recovery (FR-021). 0 → defeat (FR-024).

### RobotState (×2 Haetae)
hp (0–300), battery (0–100), batteryState (Normal/LowPower/Critical/Depleted/Charging), command, robotState (Standby/Patrol/Engage/LowBattery/ReturnToCharge/Charging/Disabled/Recovery — FR-079), currentTargetEntity, engagementFirstDashUsed(bool).

**Battery/robot state machine** (FR-079, FR-080, FR-091, Assumptions):
```
Standby/Patrol/Engage  --drain-->  battery thresholds set batteryState
battery ≤30 (Low Power)            -> movement −15%, attack −10% (FR-095) + LowBattery state
battery ≤10 (Critical)             -> Critical warning (FR-096)
battery = 0 (Depleted)             -> Disabled: no move/attack (FR-080)
Disabled --hold 5 s--> Recovery (+0.5/s, no attack)
Recovery --battery ≥5--> auto ReturnToCharge (movable, no attack)
At charging station --Charge--> Charging (+4/s, cannot fight FR-097)
Charging --battery high / re-command--> Standby/Patrol/Engage
Ripper hit: battery −5 extra (FR-045) [can push toward Depleted]
```

### ZombieState
type (Runner/Bruiser/Ripper), hp, routeId, waypointProgress(arc-distance), currentTarget (per targetPriority selection), alive. Reaching base applies type baseDamage (FR-042/043/044).

### MedicalRobotState (Phase 3)
hp (0–150), position near base, healActivePlayersInRadius(6 m, 8 HP/s, player-first), destroyed(bool, no regen).

### BarrierState (if upgrade #6)
routeId, hp (0–300), placement (choke anchor), alive. Cumulative zombie damage → destroyed; expires at phase end; never permanently blocks nav.

---

## Cross-cutting validation rules (EditMode-tested, Constitution III)

- **Damage/headshot**: damage = baseDamage × (head ? 2.5 : 1); robots/medical not headshot-eligible (Assumptions).
- **Death/defeat**: entity dies at hp ≤0; base 0 or player 0 → immediate defeat (FR-005/024).
- **Base recovery**: on phase clear, hp += 0.15 × maxHp, capped at maxHp (FR-021).
- **Ammo/reload/resupply**: fire decrements loaded; reload (2 s) moves reserve→loaded up to magazineSize; resupply refills reserve at a supply point; extended-magazine edge rule (Assumptions).
- **Grenade**: per zombie in radius, damage = lerp(150→60, dist 2 m→5 m), full 150 within 2 m, 0 beyond 5 m, capped at 10 targets (PLANNING).
- **Battery**: drain by activity, charge +4/s, thresholds set state + effects; Ripper −5; depletion→recovery→return-to-charge (Assumptions).
- **Upgrade application**: numeric add (incl. current-value for +max), reservation for not-yet-present units (FR-115), playstyle effects (관통탄/긴급 방벽).
- **Threat budget**: sum(cost) ≤ budget; preserve special minimums; trim Runners on conflict (FR-050..054, Assumptions).
- **Target priority**: pick highest-priority detected target per type ordering (FR-041..044).
- **Phase transition**: 7-step ordered rule (FR-061); upgrade only after Phase 1 & Phase 2 (FR-110, FR-112).
