# Implementation Plan: Haetae Camera Occlusion

**Branch**: `main` (user-authorized direct implementation) | **Date**: 2026-07-31 |
**Spec**: [spec.md](./spec.md)

**Input**: Feature specification from
`/specs/010-haetae-camera-occlusion/spec.md`

**Active product reference**: `specs/002-haetae-build-progression/spec.md`, with
presentation ownership from features 003 through 009.

**Implementation baseline**: merge commit `1f8256b` on `main` after
`git pull --ff-only origin main`.

## Summary

Keep the existing 90% Haetae scale and add conditional third-person visibility
treatment. Each Haetae independently checks whether its gameplay collider enters
a narrow camera-centered aiming corridor. Only while obstructing, its current
presentation renderers use short-lived transparent material variants and smoothly
reach the configured opacity. Presentation replacement refreshes the renderer
bindings, while gameplay roots, colliders, actors, HUD, combat, and telemetry
remain untouched.

## Technical Context

**Language/Version**: C# on Unity `6000.3.20f1`

**Primary Dependencies**: UnityEngine, URP Lit materials, existing
`VisualThemeDefinitionAsset`, authored/procedural presentation hierarchy, NUnit,
Unity Test Framework; no new package

**Storage**: Nested Haetae occlusion presentation definition in the existing
`VisualThemeDefinitionAsset` and generated `VisualTheme.asset`

**Testing**: EditMode theme-contract assertions; PlayMode corridor, transition,
perspective, material restoration, specialization replacement, fallback,
collider, and independence checks; complete Unity suites; Windows smoke build

**Target Platform**: Windows x86_64 desktop player

**Project Type**: Single Unity desktop game

**Performance Goals**: At most one non-allocating physics corridor query per
Haetae per rendered frame; no per-frame managed allocations; transparent material
variants created only when the presentation hierarchy changes

**Constraints**: Presentation-only; preserve 0.90 scale, gameplay-root transform,
colliders, actor state, targeting, combat, LOD selection, motion, status bars,
first-person behavior, and all non-Haetae renderers

**Scale/Scope**: One theme definition, one runtime presentation component, robot
spawn binding, generated theme data, two existing test fixtures, Spec Kit docs

## Constitution Check

*GATE: Passed before research and re-checked after design.*

- **I / IX – Spec traceability and scope**: PASS. Active spec is
  `specs/010-haetae-camera-occlusion/spec.md`, dated 2026-07-31, based on main
  commit `1f8256b`. Scope is Haetae-only third-person visibility.
- **II – Data-driven content**: PASS. Opacity, transition times, corridor radius,
  maximum distance, and enablement live in the visual-theme asset.
- **III – Testable core boundary**: PASS. No gameplay rule changes. Unity physics
  and materials remain a presentation adapter validated in PlayMode.
- **IV – Deterministic simulation**: PASS, not applicable to balance outcomes.
  The feature cannot change combat, spawning, resources, targeting, or session
  results.
- **V – Verifiable acceptance**: PASS. Every acceptance scenario maps to
  EditMode, PlayMode, regression, build, or manual quickstart validation.
- **VI / VII – Strings and assets**: PASS. No player-facing strings or authored
  meshes are replaced; the normal view retains original materials.
- **VIII – Telemetry**: PASS. Existing gameplay telemetry remains unchanged
  because this is presentation-only.
- **X – Recorded decisions**: PASS. Material ownership, non-allocating corridor
  queries, replacement recovery, and rejected alternatives are recorded in
  [research.md](./research.md).

Post-design re-check: PASS. Direct work on `main` follows the user's explicit
instruction and does not violate a constitution principle. No complexity
exception is required.

## Presentation Architecture

1. `MvpGameController` attaches one `HaetaeCameraOcclusionFader` to each spawned
   Haetae after its initial presentation exists.
2. The fader reads tuning from `VisualThemeDefinitionAsset` and reads camera plus
   perspective from `ThirdPersonPlayerController`.
3. In late presentation update, a fixed hit buffer checks the central camera
   corridor. Only a collider belonging to that Haetae can activate its fade.
4. On first fade, current opaque materials remain cached and transparent clones
   are assigned. Existing tint property blocks keep their RGB values while only
   alpha changes.
5. Leaving the corridor or entering first-person restores cached opaque materials
   over the configured duration.
6. If specialization or phase restore replaces `Presentation Visual`, obsolete
   clones are released and the new renderers are rebound before evaluation.
7. Component destruction restores opaque state and destroys only the variants it
   owns.

## Acceptance Validation Map

| Scenario | Validation |
|----------|------------|
| US1.1 third-person obstruction reaches 0.32 | PlayMode forced transition plus live corridor detection |
| US1.2 two robots fade independently | PlayMode two-fader state assertions |
| US1.3 clear view restores within 0.25 s | PlayMode timed restore and opaque material reference check |
| US2.1 outside corridor stays opaque | PlayMode side-position corridor check |
| US2.2 first-person stays opaque | PlayMode perspective-toggle check |
| US2.3 HUD/status unchanged | Existing status-bar and specialization regressions |
| US3.1 motion does not reset opacity | PlayMode motion sampling while faded |
| US3.2 replacement rebinds cleanly | PlayMode specialization hierarchy replacement and 10 cycles |
| US3.3 authored/fallback parity | Existing authored path plus procedural fallback fader assertion |

## Project Structure

### Documentation (this feature)

```text
specs/010-haetae-camera-occlusion/
|-- spec.md
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   `-- haetae-camera-occlusion.contract.md
|-- checklists/
|   `-- requirements.md
`-- tasks.md
```

### Source Code (repository root)

```text
TelerobotMVP/Assets/Game/
|-- Data/Definitions/VisualThemeDefinitionAsset.cs
|-- Data/Assets/VisualTheme.asset
|-- Editor/MvpProjectBuilder.cs
|-- Runtime/Bootstrap/MvpGameController.cs
`-- Runtime/Presentation/
    |-- HaetaeCameraOcclusionFader.cs
    `-- HaetaeCameraOcclusionFader.cs.meta

TelerobotMVP/Assets/Tests/
|-- EditMode/DesignAssetCatalogTests.cs
`-- PlayMode/VisualPresentationPlayModeTests.cs
```

**Structure Decision**: Keep obstruction handling in a dedicated presentation
component on the gameplay root. The component observes but never mutates actor or
simulation state, and it follows the replaceable visual child across model swaps.

## Complexity Tracking

No constitution violations or approved exceptions.
