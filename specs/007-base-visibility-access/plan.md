# Implementation Plan: Base Visibility and Walkable Access

**Branch**: `007-base-visibility-access` | **Date**: 2026-07-28 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/007-base-visibility-access/spec.md`

## Summary

Replace the central 8-by-3 blocking box and its oversized cylinder decoration with a
data-driven, three-level circular terrace whose broad body tops out at 0.75 metres.
The runtime anchor remains unchanged. One continuous static mesh and matching
collider form alternating slopes and level bands so the player can walk across the
base without jumping, while a narrow guardian beacon preserves identity. Zombie
perimeter slots move to a pure, deterministic circular-footprint rule and remain
outside the configured radius.

## Technical Context

**Language/Version**: C# supported by Unity 6000.3.20f1

**Primary Dependencies**: UnityEngine, existing URP materials and procedural world-art pipeline, Unity Test Framework

**Storage**: Existing `WorldLayoutAsset` ScriptableObject and mapped `WorldLayoutConfig`

**Testing**: NUnit EditMode tests, Unity PlayMode traversal/integration tests, full regressions, Windows build and standalone smoke launch

**Target Platform**: Windows x86_64 desktop player

**Project Type**: Single Unity game project

**Performance Goals**: One static hierarchy with one walkable surface collider; no per-frame base allocation or update work

**Constraints**: Keep the base anchor, charging/rally coordinates, health/damage cadence, routes, HUD/status bars, and telemetry unchanged; broad height at most 0.75 m; beacon diameter at most 1.0 m

**Scale/Scope**: One runtime base hierarchy, three terrace surfaces, one beacon, three route approaches, six-attacker distribution check

## Constitution Check

*GATE: Passed before Phase 0 research and re-checked after Phase 1 design.*

| Principle | Result | Evidence |
|---|---|---|
| I. Spec is source of truth | PASS | Active spec is `specs/007-base-visibility-access/spec.md`, dated 2026-07-28, on branch `007-base-visibility-access`; artifacts precede implementation. |
| II. Data-driven gameplay | PASS | Footprint, terrace profile, beacon width, and attack-slot spacing live in `WorldLayoutAsset` and map into core config. |
| III. Testable pure core | PASS | Circular perimeter slot selection is implemented in pure C# and covered in EditMode; Unity owns only geometry and physics adapters. |
| IV. Deterministic simulation | PASS | Slot output is deterministic for the same config, route approach, and ordinal; no random or frame-time input is introduced. Existing full-session simulations remain unchanged. |
| V. Verifiable acceptance | PASS | Every acceptance scenario maps to EditMode, PlayMode, regression, build, or manual checks below. |
| VI. Player-facing text | PASS | No player-facing string is added or changed. |
| VII. Greybox first | PASS | Walkable collision and visibility are validated independently from enhanced presentation materials. |
| VIII. Telemetry | PASS | No telemetry event or schema changes; existing base-damage and session events remain authoritative. |
| IX. Scope discipline | PASS | No camera fade, mantle, ladder, relocation, new defense rule, or animation system is introduced. |
| X. Explicit decisions | PASS | Geometry, collision, rule ownership, fallback, and testing decisions are recorded in `research.md`. |

## Base Architecture

1. `WorldLayoutAsset` owns terrace and attack-slot tuning and maps it into
   `WorldLayoutConfig`.
2. `MvpGameController` creates a scale-one `Central Base` anchor at the existing
   position and asks `CentralBasePlatform` to build the configured terrace profile.
3. One continuous mesh alternates climbable slopes and circular level bands for all
   three levels. Its single static mesh collider uses the same mesh, and there is no
   full-height hidden blocker or overlapping internal collision boundary.
4. `WorldArtBuilder` styles the terrace renderers and adds only non-colliding trim
   plus a narrow guardian beacon.
5. `BasePerimeterRules` calculates deterministic circular attack slots outside the
   outer radius. The Unity adapter converts between `Float3` and `Vector3`.
6. Zombies keep their current state machine, target priority, movement, and attack
   cadence; only the base target coordinates are shaped by the pure footprint rule.

## Acceptance Validation Map

| Spec behavior | Validation |
|---|---|
| Broad body below sightline, narrow beacon | PlayMode geometry bounds assertions plus four-viewpoint manual check |
| Opposite attack row remains readable | PlayMode six-attacker perimeter distribution plus manual orbit check |
| Base identity and indicators remain | PlayMode landmark/material/HUD/status-bar assertions |
| Four-direction no-jump ascent | PlayMode cardinal traversal tests using the real `CharacterController` |
| Stable descent and diagonal traversal | PlayMode repeated crossing tests with grounded/bounds assertions |
| Visible/collision shape agreement | PlayMode mesh/collider bounds and duplicate-collider checks |
| Deterministic outside attack slots | EditMode pure-rule tests for all routes, ordinals, spacing, and invalid input |
| Charging/rally/combat unchanged | Existing PlayMode and EditMode regressions plus explicit base-damage/charging assertions |
| Fallback remains walkable | PlayMode platform creation does not depend on `WorldArtBuilder`; material fallback check |
| Build readiness | Full suites, Windows build, and standalone smoke launch |

## Project Structure

### Documentation (this feature)

```text
specs/007-base-visibility-access/
|-- spec.md
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   `-- base-platform.contract.md
|-- checklists/
|   `-- requirements.md
`-- tasks.md
```

### Source Code (repository root)

```text
TelerobotMVP/Assets/
|-- Game/
|   |-- Core/
|   |   |-- Config/GameConfig.cs
|   |   `-- Gameplay/BasePerimeterRules.cs
|   |-- Data/
|   |   |-- Assets/WorldLayout.asset
|   |   |-- Definitions/WorldLayoutAsset.cs
|   |   `-- MvpDataMapper.cs
|   |-- Runtime/
|   |   |-- Bootstrap/MvpGameController.cs
|   |   `-- Presentation/
|   |       |-- CentralBasePlatform.cs
|   |       `-- WorldArtBuilder.cs
|   `-- Editor/MvpProjectBuilder.cs
`-- Tests/
    |-- EditMode/BasePerimeterRulesTests.cs
    `-- PlayMode/VisualPresentationPlayModeTests.cs
```

**Structure Decision**: Extend the existing single Unity project. The only new
runtime component is a static base-platform builder/marker, and the only new core
type is the deterministic perimeter-slot rule.

## Complexity Tracking

No constitution violations or exceptions.
