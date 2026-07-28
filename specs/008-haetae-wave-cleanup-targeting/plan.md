# Implementation Plan: Haetae Wave Cleanup Targeting

**Branch**: `008-haetae-wave-cleanup-targeting` | **Date**: 2026-07-28 |
**Spec**: [spec.md](./spec.md)

**Input**: Feature specification from
`/specs/008-haetae-wave-cleanup-targeting/spec.md`

**Active product reference**: `specs/002-haetae-build-progression/spec.md`
(implemented baseline), with command/state-machine ownership from
`specs/001-robot-base-defense-mvp/research.md` and
`specs/001-robot-base-defense-mvp/data-model.md`.

**Implementation baseline**: commit `d8de302` on
`007-base-visibility-access`.

## Summary

Fix the phase-ending stall by treating an exhausted spawn schedule as a bounded
cleanup state. A defending Haetae may acquire a cross-route survivor only while
that state is active and only inside its existing defend leash.
One pure route-eligibility rule will be deterministic and scene-free; runtime
target acquisition will consume it, while PlayMode coverage reproduces the
complete "all spawned + cross-route survivor" failure through the real robot
actor.

## Technical Context

**Language/Version**: C# on Unity `6000.3.20f1`

**Primary Dependencies**: Existing Unity runtime, Input System, NUnit, Unity Test
Framework; no new packages

**Storage**: Existing ScriptableObject configuration and local JSONL development
telemetry; no schema or asset change

**Testing**: NUnit EditMode pure-rule tests, Unity PlayMode scene integration
tests, existing deterministic full-session regressions, Windows player smoke
launch

**Target Platform**: Windows x86_64 desktop player

**Project Type**: Single Unity desktop game

**Performance Goals**: Preserve current frame-rate behavior; target filtering
remains one linear pass over the bounded alive-zombie collection

**Constraints**: No new pursuit distance, no patrol semantic change, no balance or
data-version change, no new player-facing strings or telemetry events

**Scale/Scope**: One pure targeting rule, one runtime call site, one EditMode
fixture, one PlayMode fixture update, and feature documentation

## Constitution Check

*GATE: Passed before research and re-checked after design.*

- **I / IX — Spec traceability and scope**: PASS. The active feature is
  `specs/008-haetae-wave-cleanup-targeting/spec.md`, dated 2026-07-28 and based
  on commit `d8de302`. The design only corrects phase cleanup acquisition.
- **II — Data-driven gameplay**: PASS. No tunable value is introduced; detection
  radius, defend leash, commands, routes, and phase schedule remain in existing
  configuration.
- **III — Testable pure core**: PASS. Spawn-complete route eligibility is a
  scene-free rule. Unity adapters continue to own transforms and distance
  queries.
- **IV — Deterministic simulation**: PASS. The cleanup decision is covered by a
  deterministic pure-rule matrix, while existing same-seed full-session
  simulation remains unchanged. The simulator's independent one-dimensional
  route axes cannot represent cross-route spatial convergence without a separate
  model expansion.
- **V — Verifiable acceptance**: PASS. US1 scenarios map to the PlayMode cleanup
  test and phase transition assertion; US2 maps to EditMode rule matrices and
  existing PlayMode command/range regressions.
- **VI / VII — Strings and assets**: PASS. No player-facing text or art changes.
- **VIII — Telemetry**: PASS. Existing spawn, kill, and phase-clear events remain
  sufficient; the feature adds no event name or schema.
- **X — Recorded decisions**: PASS. Core/runtime ownership, cleanup boundary,
  and alternatives are recorded in [research.md](./research.md).

Post-design re-check: PASS. No complexity exception is required.

## Acceptance Validation Map

| Scenario | Validation |
|----------|------------|
| US1.1 cross-route survivor acquired after all spawned | PlayMode regression with a defend-position Haetae and an East Alley survivor |
| US1.2 combat continues on the valid survivor | PlayMode health and target assertions across multiple frames |
| US1.3 final kill advances phase | PlayMode transition assertion after the survivor is killed |
| US2.1 active spawning remains route-bound | Deterministic EditMode route matrix plus PlayMode target query assertion |
| US2.2 patrol remains route-bound after all spawned | Deterministic EditMode route matrix plus PlayMode patrol assertion |
| US2.3 defend leash remains enforced | Existing and extended PlayMode boundary assertions |
| US2.4 unavailable/returning states remain authoritative | Existing battery, destruction, and command suites |

## Project Structure

### Documentation (this feature)

```text
specs/008-haetae-wave-cleanup-targeting/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── cleanup-targeting.contract.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
TelerobotMVP/Assets/Game/
├── Core/Robots/
│   └── RobotTargetingRules.cs
├── Runtime/Bootstrap/
│   └── MvpGameController.cs

TelerobotMVP/Assets/Tests/
├── EditMode/
│   └── RobotTargetingRulesTests.cs
└── PlayMode/
    └── RobotCommandPlayModeTests.cs
```

**Structure Decision**: Keep the established Unity assembly split. The Core file
owns only command/route/schedule eligibility and Runtime owns scene distances and
actor lookup. The scalar full-session simulator remains route-local because it
has no common world-space coordinate for cross-route convergence; the new core
rule itself supplies deterministic coverage.

## Complexity Tracking

No constitution violations or approved exceptions.
