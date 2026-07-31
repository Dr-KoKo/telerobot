# Implementation Plan: Haetae Visibility Foundation

**Branch**: `main` (user-authorized direct implementation) | **Date**: 2026-07-31 |
**Spec**: [spec.md](./spec.md)

**Input**: Feature specification from
`/specs/011-haetae-visibility-foundation/spec.md`

**Active product references**: `specs/002-haetae-build-progression/spec.md`,
`specs/009-haetae-scale-tuning/spec.md`, and
`specs/010-haetae-camera-occlusion/spec.md` at main commit `3a72357`.

## Summary

Normalize each Haetae gameplay root to identity, preserve its existing physical
footprint through explicit robot data, and keep `haetaeVisualScale` as the sole
uniform presentation size. Replace collider-based camera obstruction with a
non-allocating camera-ray test against active renderer bounds, reduce obstructing
opacity to 0.24, and validate visible rendering as well as state and material
contracts.

## Technical Context

**Language/Version**: C# on Unity `6000.3.20f1`

**Primary Dependencies**: UnityEngine, URP Lit, ScriptableObject content,
Unity Test Framework; no new package

**Storage**: Existing `RobotDefinitionAsset`/`RobotConfig` for physical body data
and `VisualThemeDefinitionAsset` for the one visual size and occlusion tuning

**Testing**: EditMode mapping/validation; PlayMode physical bounds, uniform scale,
renderer obstruction, material transition, rendered comparison, lifecycle, and
complete regression suites

**Target Platform**: Windows x86_64 desktop player

**Project Type**: Single Unity desktop game

**Performance Goals**: No physics allocation or managed allocation per fader
frame; test only cached active renderer bounds

**Constraints**: Preserve physical bounds and all simulation outcomes; use one
uniform visual scale; no player-facing string, HUD, combat, telemetry, or camera
behavior changes

**Scale/Scope**: Robot body data and mapping, one spawn path, one presentation
factory invariant, one fader, existing EditMode/PlayMode fixtures, generated data,
and Spec Kit artifacts

## Constitution Check

*GATE: Passed before research and re-checked after design.*

- **I / IX – Traceability and scope**: PASS. Feature 011 explicitly supersedes
  the stacked visual-scale boundary while preserving gameplay behavior.
- **II – Data-driven values**: PASS. Collider dimensions remain robot data;
  visual size, opacity, timing, margin, and range remain theme data.
- **III – Core boundary**: PASS. Transform, collider, renderer bounds, and
  materials are Unity presentation/physics adapters; core combat math is unchanged.
- **IV – Determinism**: PASS, not applicable to outcomes because physical bounds
  and simulation behavior are invariant.
- **V – Verifiable acceptance**: PASS. Every scenario maps to EditMode, PlayMode,
  rendered comparison, regression, build, smoke, or manual validation.
- **VI / VII – Strings and assets**: PASS. No strings or model sources change;
  authored proportions are restored by removing inherited distortion.
- **VIII – Telemetry**: PASS. No gameplay event or schema changes.
- **X – Decisions**: PASS. Scale ownership, physical-footprint migration,
  renderer-bound detection, and material visibility are recorded in research.

Post-design re-check: PASS. No complexity exception is required.

## Architecture

1. `RobotDefinitionAsset` stores explicit capsule radius, height, and center Y;
   `MvpDataMapper` maps and validates them into `RobotConfig`.
2. `SpawnRobot` leaves the actor transform at identity and configures the capsule
   from robot data, preserving the legacy physical bounds.
3. `LowPolyModelFactory` remains the only owner of Haetae visual size and applies
   the uniform `haetaeVisualScale` once before motion binding.
4. `HaetaeCameraOcclusionFader` uses cached active renderers. An expanded renderer
   world bound intersecting the camera aim ray within range activates fading.
5. Transparent variants disable preserved specular contribution and use opacity
   0.24 so the change is visually unambiguous while keeping ally identity.
6. Presentation replacement rebuilds cached renderers and releases obsolete
   material variants as before.

## Acceptance Validation Map

| Scenario | Validation |
|----------|------------|
| US1.1 one uniform scale | PlayMode all-role root/presentation scale assertions |
| US1.2 physical footprint preserved | Legacy-reference versus live capsule bounds test |
| US1.3 no drift | Existing motion and ten-reattach coverage |
| US2.1 visible bounds fade | PlayMode renderer-bound central obstruction test |
| US2.2 collider-miss still fades | Offset root and centered visual hierarchy test |
| US2.3 clear restores | Timed restore and exact original material references |
| US3.1 two robots independent | Existing two-fader state test updated to renderer bounds |
| US3.2 first-person opaque | Perspective boundary test |
| US3.3 lifecycle stable | Specialization, motion, fallback, tint, and ten cycles |

## Project Structure

```text
specs/011-haetae-visibility-foundation/
|-- spec.md
|-- plan.md
|-- research.md
|-- data-model.md
|-- quickstart.md
|-- contracts/
|   `-- haetae-visibility.contract.md
|-- checklists/
|   `-- requirements.md
`-- tasks.md

TelerobotMVP/Assets/Game/
|-- Core/Config/GameConfig.cs
|-- Data/Definitions/RobotDefinitionAsset.cs
|-- Data/Assets/HaetaeRobot.asset
|-- Data/Assets/VisualTheme.asset
|-- Data/MvpDataMapper.cs
|-- Editor/MvpProjectBuilder.cs
`-- Runtime/
    |-- Bootstrap/MvpGameController.cs
    `-- Presentation/HaetaeCameraOcclusionFader.cs

TelerobotMVP/Assets/Tests/
|-- EditMode/HaetaeDataConfigurationTests.cs
|-- EditMode/DesignAssetCatalogTests.cs
|-- PlayMode/VisualPresentationPlayModeTests.cs
`-- Shared/TestConfigFactory.cs
```

**Structure Decision**: Keep one visual scale in the existing presentation
factory and represent physical shape with collider data rather than transform
scale, so presentation and gameplay can no longer multiply accidentally.

## Complexity Tracking

No constitution violations or approved exceptions.
