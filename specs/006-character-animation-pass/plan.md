# Implementation Plan: Character Animation Pass

**Branch**: `006-character-animation-pass` | **Date**: 2026-07-28 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/006-character-animation-pass/spec.md`

## Summary

Add role-readable, presentation-only motion to the three authored zombie models and
the general/melee/ranged/balanced Haetae models. A single data-driven runtime motion
driver will animate only the `Presentation Visual` hierarchy, synchronize all active
LOD rigs, expose its state for tests, and degrade to root motion when optional bones
are absent. Zombie organic-shell weights will be refined in the Blender source so
the existing armatures deform the silhouettes cleanly. Existing actors remain the
sole owners of navigation, attacks, damage, death timing, targeting, and telemetry.

## Technical Context

**Language/Version**: C# supported by Unity 6000.3.20f1; Python 3 via Blender 4.5.11 for authored-source regeneration

**Primary Dependencies**: UnityEngine, Unity Test Framework, existing URP project, Blender Python API

**Storage**: Unity `VisualThemeDefinitionAsset` motion profiles and existing FBX/prefab assets

**Testing**: Unity EditMode and PlayMode suites, deterministic state probes, Windows player build and smoke launch

**Target Platform**: Windows x86_64 desktop player

**Project Type**: Single Unity game project with Blender-authored source assets

**Performance Goals**: Preserve the existing combat load with no more than 10% average frame-time regression

**Constraints**: Presentation hierarchy only; no gameplay-root, collider, navigation, damage, timing, targeting, battery, spawning, headshot, or telemetry changes

**Scale/Scope**: Eight presentation roles, two LODs per authored model, one runtime driver per supported actor

## Constitution Check

*GATE: Passed before Phase 0 research and re-checked after Phase 1 design.*

| Principle | Result | Evidence |
|---|---|---|
| I. Spec-first delivery | PASS | `spec.md`, this plan, research, data model, contract, quickstart, and `tasks.md` precede implementation. |
| II. Deterministic simulation | PASS | No simulation rule changes; the driver reads actor movement/events and writes only child visuals using scaled game time. |
| III. Data-driven tuning | PASS | Role-specific motion amplitudes and timing live in `VisualThemeDefinitionAsset`. |
| IV. State-machine ownership | PASS | `ZombieActor` and `HaetaeRobotActor` continue to own combat state and emit presentation triggers only. |
| V. Verifiable acceptance | PASS | Each scenario maps to EditMode/PlayMode checks plus a manual visual and Windows-build checklist. |
| VI. Telemetry contract | PASS | No telemetry names, fields, or emission policy changes. |
| VII. Unity baseline | PASS | Unity 6000.3.20f1, current packages, and existing build pipeline remain unchanged. |

## Motion Architecture

1. `LowPolyModelFactory.Attach` creates or replaces the presentation hierarchy,
   then binds the existing `CharacterMotionDriver` on the gameplay root.
2. The driver snapshots root and named joint local transforms for both LODs.
3. Each frame it infers locomotion from gameplay-root displacement and applies a
   role profile to the child hierarchy only.
4. Actors trigger attack, hit, and death reactions after their existing gameplay
   decisions. Trigger methods cannot apply damage or change gameplay state.
5. State priority is `Death > Hit > Attack > Locomotion > Idle`. Rebinding resets
   the snapshots and keeps one driver component.
6. If named joints are absent, the visual root still receives a conservative pose;
   missing targets never stop combat.

## Acceptance Validation Map

| Spec behavior | Validation |
|---|---|
| Zombie role-readable locomotion | EditMode role-profile assertions and PlayMode displacement/state tests |
| Attack timing without gameplay drift | PlayMode attack-trigger and health/result comparison tests |
| Hit is non-blocking | PlayMode hit-state precedence and continued displacement test |
| Death respects existing cleanup | PlayMode death state plus existing death/collider regression tests |
| Haetae role-specific attacks | EditMode pose samples for melee/ranged/balanced and PlayMode actor trigger tests |
| Independent instances | PlayMode phase/state isolation test |
| LOD phase synchronization | EditMode binding/pose comparison across duplicated named joints |
| Missing-rig fallback | EditMode root-only model binding test |
| No duplicate driver on reattach | EditMode repeated-attach component-count test |
| Performance/build | PlayMode frame-time sample, full suites, Windows build and smoke launch |

## Project Structure

### Documentation (this feature)

```text
specs/006-character-animation-pass/
|-- spec.md
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   `-- character-motion.contract.md
|-- checklists/
|   `-- requirements.md
`-- tasks.md
```

### Source Code (repository root)

```text
TelerobotMVP/
|-- ArtSource/Zombies/
|   `-- create_zombie_models.py
|-- Assets/Game/
|   |-- Data/
|   |   |-- Assets/VisualTheme.asset
|   |   `-- Definitions/VisualThemeDefinitionAsset.cs
|   |-- Runtime/
|   |   |-- Presentation/
|   |   |   |-- CharacterMotionDriver.cs
|   |   |   `-- LowPolyModelFactory.cs
|   |   |-- Robots/HaetaeRobotActor.cs
|   |   `-- Zombies/ZombieActor.cs
|   |-- Editor/MvpProjectBuilder.cs
|   `-- Tests/
|       |-- EditMode/CharacterMotionEditModeTests.cs
|       `-- PlayMode/CharacterMotionPlayModeTests.cs
`-- Assets/Game/Art/Models/
    `-- Zombies/*.fbx
```

**Structure Decision**: Extend the existing single Unity project and authored-source
pipeline. No new assembly, package, Animator Controller, or gameplay subsystem is
introduced.

## Complexity Tracking

No constitution violations or exceptions.
