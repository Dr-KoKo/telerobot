# Data Model: 「텔레 로봇팀, 출격하라」 MVP

**Feature**: `001-robot-base-defense-mvp` | **Date**: 2026-06-27 | **Plan**: [plan.md](./plan.md)

All tunable values live in **ScriptableObject data assets** (Constitution II) that map to **plain config structs** consumed by the pure `Game.Core` (Constitution III). Values cite their spec FR; values labeled **(PLANNING — to balance)** are introduced by the plan (see research.md §4–5) and are not spec-derived. Runtime/simulation entity state is held in pure C# state objects driven by the core.

---

## Data assets (ScriptableObject definitions)

### GameConfig
Session-level aggregate only — **owns `playerMaxHp` and holds references to the other config assets**; it does NOT duplicate base/weapon values (single-source-of-truth, see note below).
| Field | Value | Source |
|-------|-------|--------|
| playerMaxHp | 100 | FR-017 (owner; no PlayerConfig split in MVP) |
| targetSessionMinutes | 10–15 | FR-006, SC-001 |
| simFixedStepSeconds | 1/60 (tunable) | research.md §3 |
| baseConfigRef / weaponDefRef / … | asset refs | base HP/recovery/warning live in **BaseConfig**; magazine/reload live in **WeaponDef** |

**Single source of truth (owners):** base maxHp/recovery/warning → `BaseConfig` only; magazine/reload → `WeaponDef` only; per-zombie base damage → `ZombieDef` only. Other assets reference these, never re-declare them. Any unavoidable mirror MUST be tagged `mirrored` and covered by a validation rule asserting equality (see data-config.contract.md).

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
maxHp 1000 (FR-020); phaseRecoveryPct 0.15 (FR-021); no player repair (FR-022); warning ≤30% (FR-023); defeat at 0 (FR-024). **Damage dealt to the base lives on `ZombieDef.baseDamage` only** (single source of truth — do not duplicate zombie damage numbers here, to avoid tuning drift).

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
| robotDamage (qualitative, spec) | low | medium | medium |
| **robotDamageNumeric** (PLANNING — to balance) | 5 | 25 | 20 |
| targetPriority | base > player > robot | base > robot > player | robot > player > base |
| threatCost | 1 | 5 | 4 |
| **attackIntervalSeconds** (PLANNING — to balance) | 1.0 | 1.5 | 1.0 |
| **attackMode** | RepeatedUntilKilled | RepeatedUntilKilled | RepeatedUntilKilled |
| **targetAttackRange** (PLANNING — to balance) | melee | melee | melee |
| special | — | — | +5 robot battery drain per hit (FR-045) |
| firstAppears | Phase 1 | Phase 2 | Phase 3 |
| distinct A/V | — | — | required (FR-047): special icon + voice callout |
Source: FR-040..047, FR-052. Only these 3 types (FR-040, FR-140).

**Attack semantics** (resolves data-model ambiguity): a zombie that reaches its current target (base/player/robot) attacks **repeatedly** every `attackIntervalSeconds` for that type's damage until it is killed or the target is destroyed — **not** a one-shot on arrival. This is what makes "base HP drops 8씩" (US1.4) and robot HP 300 / medical HP 150 attrition testable. `robotDamage` is left as the spec's qualitative label; `robotDamageNumeric` is a **planning value** introduced so robot/medical attrition is quantifiable (see research.md §10) — to be balanced.

### PhaseDef ×3
| Field | Phase 1 | Phase 2 | Phase 3 |
|-------|---------|---------|---------|
| number | 1 | 2 | 3 |
| openRoutes (cumulative array) | [NorthRoad] | [NorthRoad, EastAlley] | [NorthRoad, EastAlley, SouthTunnel] |
| newlyOpenedRoute (this phase) | NorthRoad | EastAlley | SouthTunnel |
| threatBudget | 40 | 60 | 80 |
| composition.runner (range) | 20–30 | 33–45 | 50–58 |
| composition.bruiser (range) | 0 | 2–3 | 2–3 (clarify-confirmed min 2) |
| composition.ripper (range) | 0 | 0 | 3–5 |
| learningTargetTotalRange (FR-053) | 20–30 | 35–50 | 55–75 |
| specialMinimums | — | Bruiser ≥2 | Bruiser ≥2, Ripper ≥3 |
| trimOrder (on budget conflict) | Runner | Runner → Bruiser | Runner → Bruiser (never below specialMinimums) |
| targetDifficulty/duration | low / 2–3 min | medium / 3–4 min | high / 4–5 min |
| deploysUnit | 2 Haetae (start) | — | Medical robot |
| **spawnSchedule** (PLANNING — to balance): phaseStartDelaySeconds | 2 | 2 | 2 |
| — groupIntervalSeconds | 4 | 3.5 | 3 |
| — groupSizeRange | 3–5 | 4–6 | 5–8 |
| **maxAliveConcurrent** (PLANNING — perf + pressure) | 18 | 24 | 30 |
| **routeWeights** (share of spawns) | North 1.0 | North 0.55 / East 0.45 | North 0.4 / East 0.3 / South 0.3 |
| **zombieTypeWeightsByRoute** (per-type route distribution, PLANNING) | — | Ripper — ; Bruiser {N 0.65, E 0.35}; Runner {N 0.6, E 0.4} | **Ripper {N 0.15, E 0.20, S 0.65}**; Bruiser {N 0.5, E 0.3, S 0.2}; Runner {N 0.4, E 0.3, S 0.3} |
| **specialSpawnPolicy** | — | Bruiser min 2 across open routes | Ripper min 3, distributed by the weights above |
Source: FR-003, FR-031, FR-051..055, FR-064, FR-034. **Continuous spawn + concurrent cap is now spec-mandated (FR-055):** the composition is spawned at intervals (`spawnSchedule`), not all at once, and field-alive count is bounded by `maxAliveConcurrent` — on reaching the cap, further spawns are delayed until zombies die; both are data-driven and deterministic-sim-reproducible (the numbers stay planning values, but the mechanism is required). Composition is **per-type ranges** (not a prose string) so spawn tests assert bounds, and the ranges are tuned so the **achievable total after budget trim lands inside `learningTargetTotalRange`** (FR-053): P2 runner 33–45 + bruiser 2–3 ⇒ total 35–48 at cost 43–60 ≤ 60; P3 runner 50–58 + bruiser 2 + ripper 3 (specials at minimum, runners fill to target) ⇒ total 55–63 at cost 72–80 ≤ 80 (budget caps the achievable upper near ~63, below FR-053's 75 — expected per the budget-wins Assumption). Budget-vs-target reconciliation: budget is a hard cap; preserve `specialMinimums`, then trim per `trimOrder` (Assumptions "위협 예산 vs 목표 마릿수"). **`zombieTypeWeightsByRoute` is now a numeric matrix** — FR-034's "Ripper more frequent in South Tunnel" MUST is quantified as Ripper South weight 0.65 > other routes, and a spawn test asserts South-Tunnel Ripper count > other routes. **Note (P1-9, clarify-confirmed):** Phase 3 now requires **Bruiser ≥2 and Ripper ≥3** (spec Assumption "위협 예산 vs 목표 마릿수") so it reads as the all-types 종합 국면. All `spawnSchedule`/weight/matrix numbers are planning values flagged for balancing (research.md §11).

### RobotDef (Haetae) — 2 instances
| Field | Value | Source |
|-------|-------|--------|
| hp | 300 | FR-071 |
| maxBattery | 100 | FR-072, FR-090 |
| moveSpeed | fast | FR-073 |
| attack | melee dash + bite | FR-074 |
| killRunnerSeconds (validation target) | ~1–2 | FR-075 |
| killBruiserSeconds (validation target) | ~6–10 | FR-076 |

**RobotAttackDef** (PLANNING — to balance, research.md §14): the kill-time bands above are validation *targets*; the concrete combat is driven by these fields (also what `haetae_charge_boost` = "첫 돌진 +40%" multiplies):
| Field | Value | Note |
|-------|-------|------|
| dashDamage | 60 | first-hit lunge; `haetae_charge_boost` scales this (×1.4) on the first dash per engagement |
| biteDamage | 40 | sustained melee |
| biteCooldownSeconds | 0.6 | between bites |
| dashCooldownSeconds | 3.0 | between dashes |
| engageRange (m) | 2 | melee reach |
| detectionRadius (m) | 15 | acquire target |
Sanity vs bands: Runner 90 HP → dash 60 + 1 bite 40 ≈ 1 hit-cycle (~1–1.5 s ✓). Bruiser 500 HP → dash 60 then ~66 DPS (bite 40 / 0.6 s) ≈ ~7 s ✓. Strong vs normal zombies, weak to battery pressure + Ripper (FR-077). All values planning, flagged for balancing.

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
3-of-9 offered after Phase 1 and Phase 2; max 2 selections/session; the global candidate pool stays the same 9 definitions at every reward step (no per-phase gating, FR-115). **Offer policy (clarify-confirmed):** an **already-selected upgrade id is excluded from later offers in the same session**; **no stacking** — each upgrade is selectable at most once per session. `UpgradeService.Offer(rng)` draws 3 from the 9 minus already-selected ids; `selectedUpgradeIds` is tracked on `SessionState`.

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
`magazineSize` and `reloadSeconds` are **owned by `WeaponDef`** (FR-014/015) — AmmoConfig references them, does not re-declare. AmmoConfig owns the reserve-ammo economy (PLANNING — to balance, research.md §12):
| Field | Value | Note |
|-------|-------|------|
| startReserveAmmo | 120 (≈4 mags) | reserve at phase/session start |
| reserveAmmoMax | 240 | cap |
| resupplyPolicy | FullReserve | FullReserve \| FixedAmount |
| resupplyAmount | (n/a for FullReserve) | used only if FixedAmount |
| resupplyUseSeconds | 1.5 | interaction time at a supply point |
| resupplyCooldownSeconds | 0 | per-point cooldown (0 = none in MVP) |
| grenadeResupplyPolicy | **PhaseResetOnly** | **spec-determined** — grenades reset to 2 at each phase start (Assumptions "각 페이즈 시작 시 2개로 재설정"); no mid-phase grenade resupply |

Two supply points (FR-037): `safe` (inside/adjacent base), `risky` (outside/near combat). Fire decrements loaded; reload moves reserve→loaded; resupply refills reserve per `resupplyPolicy` after `resupplyUseSeconds`. `grenadeResupplyPolicy` is the only spec-fixed value here; the rest are planning values to balance.

### ChargingStationConfig
id, anchor/location, `radiusMeters` (trigger radius), `maxConcurrentRobots` (MVP: both Haetae may charge — value to balance), `allowsCombat=false` (FR-097). **Charge rate is not here** — it lives in `BatteryConfig.chargeRate` (single-source); `charge_station_speedup` upgrade (#4) scales that rate. The station is otherwise a scene adapter (transform + trigger volume).

### CommandConfig (Robot Commands)
Exactly 4 commands, no others (FR-085, FR-140): `DefendPosition`, `PatrolRoute`, `ReturnToBase`, `Charge`. **Selection (FR-087, MUST):** both **individual selection** and a **select-all toggle** are supported; commands are always applied **per robot** (select-all fans out to per-robot `IssueCommand`), so the two robots may hold different commands/routes. PatrolRoute takes an open-route target; DefendPosition takes a point/route (Assumptions). A `Destroyed` robot ignores commands until next-phase restore (FR-081).

### WarningConfig / HudConfig
HUD elements (FR-120): base HP, phase progress, route alert/minimap, robot battery, player HP, ammo, command quick-menu. Info priority: base HP > robot battery > route alert (FR-121). Thresholds: battery <25% yellow flash + callout (FR-123), <10% red flash + urgent callout (FR-124), base ≤30% edge warning + alarm (FR-125), Ripper appearance special icon + callout (FR-126), new route open highlight + radio (FR-122). Minimum combat HUD bundle ships with US1/US2; situational-awareness bundle with US5 (FR-120a). No info overload (FR-127).

### PlayerSettings (playtest access layer)
Data-driven defaults and bounds for mouse sensitivity, master/effects volume, resolution, fullscreen, and the initial first/third-person perspective. Runtime values are persisted locally with `PlayerPrefs`; they affect presentation and controls only and do not enter deterministic balance simulation.

### RadioEventDef + StringTable
8 radio/sound events (FR-130); strings stored verbatim Korean — see [contracts/strings.contract.md](./contracts/strings.contract.md). Triggers implemented with their gameplay milestone (research.md / plan Sound section). MVP uses captions + placeholder/TTS-stub audio; final VO swaps clips without changing event logic.

### TelemetryConfig
Event names + required fields per [contracts/telemetry.contract.md](./contracts/telemetry.contract.md); dev-only local file sink (research.md §8). **Sampling policy (PLANNING — to balance; MUST be sim-clock-driven for deterministic diffs):**
| Field | Value | Note |
|-------|-------|------|
| sampleIntervalSeconds | 1.0 | `base_hp_sampled` cadence (sim-time, not wall-time) |
| routePressureSampleIntervalSeconds | 2.0 | `route_pressure_sampled` cadence |
| batteryEmitPolicy | OnThresholdCrossing + EveryNSeconds | `robot_battery_changed` — emit on band/warning crossing **and** every `batteryEmitIntervalSeconds` (avoids per-frame flood from 2.5/s drain) |
| batteryEmitIntervalSeconds | 1.0 | periodic component of the above |
All cadences are counted on the fixed sim clock so two runs of the same `seed × profile` produce identical sample streams (Constitution IV).

### SimPlayerProfile ×3 (PLANNING — to balance)
The deterministic simulation needs a **scripted player agent**; otherwise clear-rate / defeat-reason are reproducible but meaningless. Each profile is a data asset driving how the sim's player behaves. Three profiles bracket the intended experience:

| Field | Novice | Baseline | Skilled |
|-------|--------|----------|---------|
| aimAccuracy (hit prob.) | 0.55 | 0.75 | 0.92 |
| headshotRate (of hits) | 0.10 | 0.25 | 0.45 |
| reactionDelaySeconds | 1.2 | 0.6 | 0.25 |
| routePriorityPolicy | reacts late to new route | balanced coverage | pre-empts highest-pressure route |
| ripperFocus | ignores until robot low | targets when convenient | prioritizes Ripper on sight |
| robotChargeThresholdPct | 10 (late) | 25 | 40 (pre-emptive) |
| upgradeSelectionPolicy | random-of-3 | intended-meta | risk-aware optimal |
| grenadeUsePolicy | rarely | on dense clusters (≥4) | on dense clusters + Bruiser softening |

Purpose: run each seed × profile to review SC-001..004 (session length, clear rates) meaningfully. All values are planning defaults flagged for balancing (research.md §13). Baseline represents the intended MVP average player used for the primary balance targets.

### ValidationConfig / SimParams
Deterministic-sim seeds (fixed list), fixed step, `SimPlayerProfile` selection, per-scenario parameters — see [contracts/validation-scenarios.contract.md](./contracts/validation-scenarios.contract.md).

---

## Runtime/simulation state entities (pure C# state objects)

**Entity composition & interactions** (multiplicities from the spec: 3 phases, 2 Haetae, 1 base/player, 0–1 medical, ≤2 upgrades). Solid = owns/contains; dashed = per-tick interaction. Zombie→target edges follow each type's `targetPriority`.

```mermaid
classDiagram
    class SessionState
    class PhaseState
    class RouteState
    class ZombieState
    class RobotState
    class MedicalRobotState
    class BaseState
    class PlayerState
    class UpgradeChoice
    class BarrierState

    SessionState "1" --> "3" PhaseState : phases
    SessionState "1" --> "1" BaseState
    SessionState "1" --> "1" PlayerState
    SessionState "1" --> "2" RobotState : Haetae
    SessionState "1" --> "0..1" MedicalRobotState : Phase 3
    SessionState "1" --> "0..2" UpgradeChoice : selected
    PhaseState "1" --> "1..3" RouteState : open routes
    RouteState "1" --> "*" ZombieState : spawns along
    PhaseState "1" --> "0..*" BarrierState : if 긴급 방벽

    ZombieState ..> BaseState : attacks
    ZombieState ..> RobotState : attacks
    ZombieState ..> PlayerState : attacks
    RobotState ..> ZombieState : engages
    MedicalRobotState ..> PlayerState : heals 8/s
```

### SessionState
currentPhase (1–3), result (InProgress/Victory/Defeat), defeatReason (BaseDestroyed/PlayerDeath/null), elapsedSimTime, seed, selectedUpgradeIds (≤2; excluded from later offers, no stacking), openRoutes set. Win: Phase 3 cleared with base HP ≥1 (FR-004). Defeat: base HP 0 or player HP 0 (FR-005).

### PhaseState
number, openRoutes, threatBudget, plannedComposition, spawnedCount, aliveCount, cleared(bool); spawn is **continuous** (interval-based) and gated by `maxAliveConcurrent` — spawning pauses while `aliveCount ≥ cap` and resumes as zombies die (FR-055). Transition (FR-061, ordered): ① all spawned → ② field cleared → ③ base alive → ④ phase cleared → ⑤ upgrade (if eligible) → ⑥ open next route → ⑦ start next phase. **At each phase start, any `Destroyed` Haetae is restored to hp=300 + usable battery (FR-081).**

### PlayerState
hp (0–100), grenades, weapon ammo state (loaded, reserve). Death → defeat (FR-017).

### BaseState
hp (0–1000), warningActive(≤30%). PhaseClear → +15% maxHp recovery (FR-021). 0 → defeat (FR-024).

### RobotState (×2 Haetae)
hp (0–300), battery (0–100), batteryState (Normal/LowPower/Critical/Depleted/Charging), command, robotState (Standby/Patrol/Engage/LowBattery/ReturnToCharge/Charging/Disabled/Recovery/**Destroyed** — FR-079, 9 states), currentTargetEntity, engagementFirstDashUsed(bool). **HP-0 (Destroyed) vs battery-0 (Disabled) are distinct paths (FR-081):**
- **Destroyed** — entered from any active state when `hp ≤ 0` (zombie damage, `robotDamageNumeric`). During the **current phase** the robot is rubble: **ignores player commands, cannot move / attack / charge**. At **next phase start** it is **restored to `hp = 300` and a usable battery** (same phase-boundary rhythm as grenade reset / base +15%). Emits `RobotDestroyed`; restore is not a `RobotRecovered` event (that's the battery path).
- **Disabled/Recovery** — battery-0 path (FR-080, recoverable in-phase via Recovery). Independent of Destroyed.
Both Haetae Destroyed in one phase ⇒ player defends solo for the phase remainder (spec Edge Case / FR-081). Zombie robot damage also emits `RobotDamaged`.

**Battery/robot state machine** (all 9 FR-079 states incl. `Destroyed`; FR-080/081/091/095/097, Assumptions). Note: `Critical` (battery 1–10) is a *battery band* that raises a warning, not one of the 9 robot states — it is shown as a note on `LowBattery`. `Destroyed` is the HP-0 path (FR-081), distinct from the battery `Disabled`/`Recovery` path.

```mermaid
stateDiagram-v2
    [*] --> Standby
    Standby --> Patrol: Patrol Route
    Standby --> Engage: 좀비 탐지
    Patrol --> Engage: 좀비 탐지
    Engage --> Patrol: 표적 소멸
    Patrol --> Standby

    Standby --> ReturnToCharge: Charge 명령
    Patrol --> ReturnToCharge: Charge 명령
    Engage --> ReturnToCharge: Charge 명령
    ReturnToCharge --> Charging: 충전소 도착
    Charging --> Standby: 충전 완료 / 재명령

    state "LowBattery (배터리 ≤ 30)" as LowBattery
    Standby --> LowBattery: 배터리 ≤ 30
    Patrol --> LowBattery: 배터리 ≤ 30
    Engage --> LowBattery: 배터리 ≤ 30
    LowBattery --> Engage: 충전 후 31↑
    LowBattery --> Disabled: 배터리 0 · Depleted

    Disabled --> Recovery: 5초 경과
    Recovery --> ReturnToCharge: 배터리 ≥ 5

    Standby --> Destroyed: HP ≤ 0
    Patrol --> Destroyed: HP ≤ 0
    Engage --> Destroyed: HP ≤ 0
    LowBattery --> Destroyed: HP ≤ 0
    ReturnToCharge --> Destroyed: HP ≤ 0
    Charging --> Destroyed: HP ≤ 0
    Disabled --> Destroyed: HP ≤ 0
    Recovery --> Destroyed: HP ≤ 0
    Destroyed --> Standby: 다음 페이즈 시작 · HP=300 복원 (FR-081)

    note right of LowBattery
        이동 −15% · 공격 −10% (FR-095)
        배터리 ≤ 10 → Critical 경고 (FR-096)
        리퍼 피격 배터리 −5 → Depleted 가속 (FR-045)
    end note
    note right of Disabled: 이동·공격 불가 (FR-080)
    note right of Recovery: +0.5/s · 공격 불가
    note right of Destroyed: HP 0 (좀비 피해) · 명령/이동/공격/충전 불가 · 현재 페이즈 한정 (FR-081)
    note right of Charging: +4/s · 전투 불가 (FR-097)
```

### ZombieState
type (Runner/Bruiser/Ripper), hp, routeId, waypointProgress(arc-distance), currentTarget (per targetPriority selection), alive. Reaching base applies type baseDamage (FR-042/043/044).

**Target-priority scope (assumption from FR-044/FR-107):** in the `{base, player, robot}` priority ordering, **"robot" = Haetae combat robot only** (the Ripper is explicitly a "로봇 카운터" for Haetae). The **medical robot is NOT an entry in any zombie's target priority** — it takes damage only **incidentally** when a zombie is adjacent to it on the way to a higher-priority target (FR-107 "좀비가 도달하면"), which can destroy it and disable the zone. **Decided (MVP assumption, accepted — not a clarify item):** no zombie ever *actively* paths to the medical robot; incidental damage only. This is consistent with FR-107's "도달하면" wording and the edge-case coverage already asserting "not actively targeted."

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
