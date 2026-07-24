# Implementation Plan: Phase 2 해태 성장·전문화

**Branch**: `main` | **Date**: 2026-07-23 | **Spec**: [spec.md](./spec.md)

**Active Spec Identity**:
- Path: `specs/002-haetae-build-progression/spec.md`
- Feature: Phase 2 해태 성장·전문화 (`002-haetae-build-progression`)
- Status/date: Draft, 2026-07-23
- Baseline dependency: implemented `001-robot-base-defense-mvp`, Unity validation baseline EditMode 51/51 and PlayMode 38/38 on 2026-07-22

**Input**: Feature specification from `specs/002-haetae-build-progression/spec.md`

## Summary

Phase 2 replaces the phase-end 3-of-9 upgrade interruption with session-local, per-Haetae progression. Each of the two Haetae starts at level 1 with its own experience ledger. A Haetae that contributed damage to a defeated zombie receives that zombie type's full experience reward; on reaching level 2 it becomes independently eligible for one irreversible session specialization: **근거리형**, **원거리형**, or **균형형**. Specialization changes attack mode, engagement distance, and approach behavior while preserving the existing three commands, nine robot modes, battery pressure, destruction/recovery rules, three-phase session, recoil, and current spawn-pressure baseline.

The technical approach extends the existing pure C# core with typed damage sources, per-zombie combat contribution, `HaetaeProgressionState`, a progression service, and a data-driven combat-policy resolver shared by runtime and deterministic simulation. Unity adapters continue to own transforms, range queries, VFX, input, and HUD. The full-screen upgrade gate is removed from active runtime and simulation flow; level-up only raises a non-modal notification, and the player explicitly opens a specialization panel without pausing world time. The deterministic simulator models specialization-specific engagement distance and uses fixed, profile-owned choices that do not consume spawn RNG.

## Technical Context

**Language/Version**: C# under Unity scripting runtime, .NET Standard 2.1 profile; pure core remains free of `UnityEngine`

**Primary Dependencies**: Unity `6000.3.20f1`; Input System `1.19.0`; AI Navigation `2.0.13`; Unity Test Framework `1.6.0`; existing URP/UGUI runtime baseline

**Storage**: ScriptableObject data assets for XP, specialization, presentation, and string definitions; in-memory session-only progression; development JSON Lines telemetry under the existing local telemetry folder; no permanent progression save and no external service

**Testing**: Unity Test Framework EditMode and PlayMode suites; deterministic full-session simulation with seeded RNG and fixed timestep; quickstart manual playtest checks; current baseline 51 EditMode and 38 PlayMode tests remains a regression floor

**Target Platform**: Windows PC x64, keyboard and mouse first

**Project Type**: Single Unity desktop game project

**Performance Goals**: Preserve the existing 60 fps greybox target at the Phase 3 cap of 24 simultaneously alive zombies; XP/contribution processing is bounded by two Haetae contributors per zombie; no additional scene load or phase-transition stall

**Constraints**:
- Current recoil, muzzle/impact feedback, phase composition, spawn intervals, group sizes `3–4 / 3–5 / 4–6`, and concurrent caps `15 / 20 / 24` are regression baselines, not rebalance targets.
- Progression is session-local and caps at level 2.
- The specialization panel must not change `Time.timeScale` or gate phase transitions.
- All balance-affecting rules must be data-driven and deterministic-simulation covered.
- Player-facing specialization names remain exactly `근거리형`, `원거리형`, and `균형형`.
- `.specify/templates/` and `.specify/memory/constitution.md` remain unchanged.

**Scale/Scope**: Two independently progressing Haetae; one level transition; three specializations; three existing phases/routes/zombie types; no currency, shop, player build, additional robot, time-attack mission, permanent progression, or level 3+

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| # | Principle | Gate result | Plan compliance |
|---|-----------|-------------|-----------------|
| I | Spec is the product source of truth | PASS | Active spec is identified above. The plan implements only per-Haetae XP, level-2 specialization, non-modal selection, and removal of the phase-end upgrade gate. |
| II | Data-driven gameplay and balance | PASS | XP rewards, level threshold, specialization combat profiles, battery/defense multipliers, presentation cues, and strings are owned by data assets and mapped to pure configs. |
| III | Testable pure gameplay core | PASS | Contribution, XP, level transition, selection validation, attack decisions, and phase transition rules live in `Game.Core`; Unity adapters only bind presentation, input, geometry, and range queries. |
| IV | Deterministic simulation | PASS | Runtime and simulation share progression/combat policy. Simulation uses fixed step, seeded spawn RNG, stable ID ordering, and specialization choices that do not consume spawn RNG. |
| V | Acceptance scenarios verifiable | PASS | [validation-scenarios.contract.md](./contracts/validation-scenarios.contract.md) maps every acceptance scenario to EditMode, PlayMode, simulation, or manual playtest validation. |
| VI | Player-facing text preserved | PASS | Role names and new HUD labels are string-table data; the three role names are preserved verbatim from the spec. |
| VII | Greybox first | PASS | Role colors, scale accents, pulses, and tracers are validation cues. Production robot models, animations, and final audio are not prerequisites. |
| VIII | Development telemetry | PASS WITH RECORDED EXCEPTIONS | New XP/level/specialization events cover progression. The obsolete constitution events `robot_charge_commanded` and `upgrade_selected` use recorded replacements described in Complexity Tracking and the telemetry contract. |
| IX | Scope discipline | PASS | Currency, shops, player weapons, melee weapons, timed missions, extra robots, persistent growth, and spawn/recoil redesign remain out of scope. |
| X | Explicit technical decisions | PASS | [research.md](./research.md) records state ownership, attribution, combat policy, UI, simulation, telemetry, migration, and test decisions with alternatives. |

**Initial gate**: PASS WITH RECORDED TELEMETRY EXCEPTIONS.

**Post-design gate**: PASS WITH THE SAME RECORDED EXCEPTIONS. Phase 1 artifacts preserve pure-core ownership, deterministic coverage, data-driven tuning, validation traceability, and scope boundaries.

## Project Structure

### Documentation (this feature)

```text
specs/002-haetae-build-progression/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── data-config.contract.md
│   ├── progression-events.contract.md
│   ├── specialization-ui.contract.md
│   ├── telemetry.contract.md
│   └── validation-scenarios.contract.md
├── checklists/
│   └── requirements.md
└── tasks.md                         # created later by /speckit-tasks
```

### Source Code (repository root)

```text
TelerobotMVP/
├── Assets/
│   ├── Game/
│   │   ├── Core/                    # pure C#, no UnityEngine
│   │   │   ├── Config/
│   │   │   │   └── GameConfig.cs   # XP/specialization config and typed damage source
│   │   │   ├── GameState/
│   │   │   │   └── GameModels.cs   # Robot progression + zombie contribution state
│   │   │   ├── Progression/
│   │   │   │   └── HaetaeProgressionSystem.cs
│   │   │   ├── Robots/
│   │   │   │   ├── RobotAttackSystem.cs
│   │   │   │   ├── RobotCombatPolicy.cs
│   │   │   │   └── RobotCommandSystem.cs
│   │   │   ├── Phase/
│   │   │   │   └── PhaseSystem.cs
│   │   │   └── Events/
│   │   │       └── GameContracts.cs
│   │   ├── Data/
│   │   │   ├── Definitions/
│   │   │   │   ├── HaetaeProgressionDefinitionAsset.cs
│   │   │   │   ├── HaetaeSpecializationDefinitionAsset.cs
│   │   │   │   ├── RobotDefinitionAsset.cs
│   │   │   │   ├── ZombieDefinitionAsset.cs
│   │   │   │   └── MvpContentCatalog.cs
│   │   │   ├── Assets/
│   │   │   │   ├── HaetaeProgression.asset
│   │   │   │   ├── HaetaeMelee.asset
│   │   │   │   ├── HaetaeRanged.asset
│   │   │   │   ├── HaetaeBalanced.asset
│   │   │   │   └── StringTable.asset
│   │   │   └── MvpDataMapper.cs
│   │   ├── Runtime/
│   │   │   ├── Robots/
│   │   │   │   └── HaetaeRobotActor.cs
│   │   │   ├── Zombies/
│   │   │   │   └── ZombieActor.cs
│   │   │   ├── HUD/
│   │   │   │   ├── CombatHud.cs
│   │   │   │   ├── RobotCommandMenu.cs
│   │   │   │   └── HaetaeSpecializationView.cs
│   │   │   └── Bootstrap/
│   │   │       └── MvpGameController.cs
│   │   ├── Simulation/
│   │   │   └── SimRunner/
│   │   │       └── DeterministicSessionSimulator.cs
│   │   └── Editor/
│   │       └── MvpProjectBuilder.cs
│   └── Tests/
│       ├── EditMode/
│       │   ├── HaetaeProgressionTests.cs
│       │   ├── HaetaeSpecializationTests.cs
│       │   ├── PhaseOneTests.cs
│       │   ├── PhaseTwoAndUpgradeTests.cs
│       │   └── DeterministicSimulationTests.cs
│       ├── PlayMode/
│       │   ├── HaetaeProgressionPlayModeTests.cs
│       │   ├── HaetaeSpecializationPlayModeTests.cs
│       │   ├── PhaseOnePlayModeTests.cs
│       │   ├── PhaseTwoPlayModeTests.cs
│       │   └── PhaseThreePlayModeTests.cs
│       └── Shared/
│           └── TestConfigFactory.cs
├── Packages/
└── ProjectSettings/
```

**Structure Decision**: Preserve the existing `Game.Core` → `Game.Data`/`Game.Runtime`/`Game.Simulation` assembly boundary. Progression is a new pure-core feature folder rather than controller-owned state. Specialization values are per-robot profiles, never global `RuntimeModifiers`, so two Haetae can hold different builds simultaneously.

## System Decomposition

| Concern | Pure core owner | Data owner | Runtime adapter | Simulation/validation |
|---------|-----------------|------------|-----------------|-----------------------|
| Damage attribution | `DamageSource`, `CombatContributionState` | N/A | `ZombieActor` records applied Haetae damage | `SimZombie` records the same typed source |
| XP and level | `HaetaeProgressionSystem` | progression threshold + zombie XP | controller forwards core events to HUD/telemetry | same system used in full-session sim |
| Specialization selection | `HaetaeProgressionSystem.SelectSpecialization` | three specialization definitions | non-modal specialization view | scripted per-robot profile choice |
| Combat role | `RobotCombatPolicy`, `RobotAttackResult` | range, damage, cooldown, cleave, battery/defense multipliers | actor performs movement, range query, direct hit/tracer/VFX | headless range and movement model |
| Phase transition | `PhaseSystem` | existing phase definitions unchanged | controller handles `NextPhase` immediately | simulator removes upgrade gate |
| HUD and strings | progression state is authoritative | string table + HUD timing | HUD rows, ready alert, selection panel | PlayMode/manual |
| Telemetry | domain events | enabled-event declaration | JSONL bridge | deterministic event stream and summary |

## Core Processing Order

For each damaging hit:

1. Resolve typed damage source.
2. Apply damage and calculate positive applied amount.
3. If the source is a Haetae and applied amount is positive, record its ID once in the zombie contribution state.
4. If the zombie dies, sort contributor IDs with ordinal comparison.
5. Award the full configured XP reward to each contributor, clamp at the level-2 threshold, and publish XP/level/ready events.
6. Publish `zombie_killed` and remove the zombie.
7. Allow the next update to evaluate phase completion.

This order preserves the spec edge case where the final kill and phase clear happen together, and allows a Haetae destroyed before the kill to receive credit for its existing contribution.

## Specialization Combat Strategy

- **Level 1 / level 2 unselected**: current dash + bite general behavior.
- **근거리형**: approach to contact, retain dash/bite, cleave up to three nearby targets within the configured radius, reduce incoming damage, and increase combat battery drain.
- **원거리형**: hold a configured 6–12 m band, use direct deterministic ranged damage with a runtime tracer, retreat when too close, and accept higher incoming damage.
- **균형형**: use lower-power ranged harassment while approaching, switch to reduced-power dash/bite inside the existing chassis melee range (2 m), and receive no cleave or specialist defensive advantage.

`RobotCombatPolicy` returns movement intent (`Approach`, `Hold`, `Retreat`) and `RobotAttackResult` (`None`, `Dash`, `Bite`, `Ranged`, damage, radius, max targets). Unity performs spatial queries and visual effects; the deterministic simulator performs stable route/distance queries. No specialization adds a `RobotMode` or player command.

## Progression and Balance Baseline

Initial planning values, all data-driven and subject to deterministic/playtest balancing:

| Value | Initial baseline |
|-------|------------------|
| Level-2 threshold | 100 XP |
| Runner reward | 5 XP |
| Bruiser reward | 25 XP |
| Ripper reward | 20 XP |
| 근거리형 cleave | radius 2.5 m, maximum 3 targets |
| 근거리형 incoming damage multiplier | 0.80 |
| 근거리형 combat battery multiplier | 1.20 |
| 원거리형 attack | 30 damage every 0.6 s; preferred band 6–12 m |
| 원거리형 incoming damage multiplier | 1.15 |
| 균형형 ranged harassment | 15 damage every 1.0 s |
| 균형형 close damage multiplier | 0.85 |

XP rewards start at five times the existing zombie threat cost. The level threshold is tuned against SC-002/003 rather than treated as final: at least one Haetae should reach level 2 within 60 seconds after Phase 2 starts in 80% of eligible baseline sessions, and both should reach level 2 before Phase 3 in 80% of eligible baseline sessions.

## Phase-End Upgrade Retirement

- `PhaseSystem` returns `NextPhase` after a surviving Phase 1 or Phase 2 clear.
- `MvpGameController` handles `NextPhase` by emitting clear samples/radio and immediately starting the next phase.
- Runtime and simulation stop constructing or invoking `UpgradeSystem`; the full-screen `UpgradeSelectionView` is not instantiated.
- `SelectedUpgrades`, upgrade offer RNG, and simulation upgrade-selection policy are removed from active state.
- Existing serialized upgrade types/assets may remain unreferenced for one data-version migration window, but they are excluded from the active catalog contract and player flow. This avoids a broad destructive asset migration while ensuring no legacy upgrade is reachable.
- `RuntimeModifiers` may temporarily remain at neutral defaults for serialized/code compatibility, but specialization never writes global modifiers.

## Deterministic Simulation Strategy

- Keep the existing fixed-step clock, seeded spawn composition, route allocation, and waypoint progression.
- Add a `SimRobotRuntime` distance/route position so melee approach, ranged hold/retreat, and balanced transition behavior affect outcomes.
- Use the same `HaetaeProgressionSystem` and `RobotCombatPolicy` as runtime.
- Store contributor IDs per simulated zombie and apply stable ordering for XP and cleave targets.
- Move specialization choices into `SimPlayerProfile` as an ordered two-entry choice array. The simulator selects immediately when a robot becomes ready, modeling player input rather than domain auto-selection.
- Specialization choice consumes no RNG, preserving identical spawn streams across build comparisons.
- Compare all six unordered specialization combinations on the same 20 balance seeds; retain ordered results when route assignment makes order meaningful.
- Regenerate the golden telemetry snapshot under data version `mvp-2.0.0`.

## Validation Strategy

- **EditMode**: independent XP, shared contribution/full reward, non-contributor exclusion, duplicate hit de-duplication, destroyed contributor credit, XP cap, same/different choices, reselection rejection, phase-transition readiness persistence, combat policy outputs, telemetry payloads, determinism.
- **PlayMode**: separate HUD rows, non-pausing alert/panel, correct target robot selection, same/different builds, role-specific ranges and attacks, specialization persistence across Destroyed restore, no phase-end upgrade UI, immediate next-phase flow.
- **Simulation**: SC-002/003/008/010, role damage/battery/destroyed metrics, identical telemetry for identical inputs, identical spawn streams across specialization choices.
- **Regression**: retain current recoil test, continuous/group spawn tests, caps `15/20/24`, command/battery/destroyed recovery, medical/ripper, HUD/radio, and Windows smoke validation.
- **Human playtest**: SC-004 through SC-007 use a minimum of 30 recorded specialization choices and a short role-recognition/decision survey.

## Complexity Tracking

| Violation / exception | Why needed | Simpler alternative rejected because |
|-----------------------|------------|---------------------------------------|
| Constitution VIII event `robot_charge_commanded` remains replaced by `robot_auto_charge_started` | The active game has no manual Charge command; charging begins automatically inside the base zone. This is inherited from the approved MVP behavior. | Emitting `robot_charge_commanded` would record an action the player cannot perform and corrupt telemetry. |
| Constitution VIII event `upgrade_selected` becomes not applicable and is replaced by `haetae_specialization_selected` | FR-027 removes the phase-end upgrade system. The new player growth choice is per-Haetae specialization, with additional `haetae_xp_gained` and `haetae_level_reached` events. | Keeping or aliasing `upgrade_selected` would misrepresent the active product and make old/new data indistinguishable. |

**Impact**: Telemetry consumers and the golden snapshot must branch by `dataVersion`; `mvp-2.0.0` uses specialization events and does not count legacy upgrade selections.

**Follow-up**: A separately approved constitution amendment should generalize the minimum growth-choice event instead of naming the retired `upgrade_selected` event. This plan does not modify `.specify/memory/constitution.md`.
