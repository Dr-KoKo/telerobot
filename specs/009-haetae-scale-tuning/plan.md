# Implementation Plan: Haetae Scale Tuning

**Branch**: `009-haetae-scale-tuning` | **Date**: 2026-07-28 |
**Spec**: [spec.md](./spec.md)

**Input**: Feature specification from
`/specs/009-haetae-scale-tuning/spec.md`

**Active product reference**: `specs/002-haetae-build-progression/spec.md`, with
presentation architecture from the implemented design-asset and character-motion
features.

**Implementation baseline**: commit `75d41cb` on
`008-haetae-wave-cleanup-targeting`.

## Summary

Reduce every general and specialized Haetae presentation to 90% uniform visual
scale through one designer-editable visual-theme value. Apply the multiplier only
to the replaceable presentation child so the gameplay root, capsule collider,
separation, combat, and HUD contracts remain unchanged. Validate authored,
fallback, animation, reattachment, and non-Haetae boundaries before rebuilding
the Windows player.

## Technical Context

**Language/Version**: C# on Unity `6000.3.20f1`

**Primary Dependencies**: Existing Unity runtime, ScriptableObject content,
authored FBX presentation pipeline, NUnit, Unity Test Framework; no new packages

**Storage**: Existing `VisualThemeDefinitionAsset` and generated
`VisualTheme.asset`

**Testing**: EditMode theme-contract tests, PlayMode presentation/collider/motion
tests, complete Unity suites, Windows player smoke launch

**Target Platform**: Windows x86_64 desktop player

**Project Type**: Single Unity desktop game

**Performance Goals**: No additional per-frame work or allocations; scale is
assigned once whenever a presentation root is created

**Constraints**: Presentation-only change; keep gameplay root and collider scale,
specialization multipliers, LODs, animation, status bars, and all non-Haetae
roles unchanged

**Scale/Scope**: One theme field, one presentation-factory assignment, generated
theme data, two existing test fixtures, feature documentation

## Constitution Check

*GATE: Passed before research and re-checked after design.*

- **I / IX — Spec traceability and scope**: PASS. Active spec is
  `specs/009-haetae-scale-tuning/spec.md`, dated 2026-07-28 and based on commit
  `75d41cb`; scope is a Haetae-only visual reduction.
- **II — Data-driven gameplay/content**: PASS. The 0.90 presentation value lives
  in the visual-theme asset and builder, not inline in a runtime actor.
- **III — Testable core boundary**: PASS. No gameplay rule changes. The Unity
  adapter owns the presentation transform and tests prove physical bounds stay
  unchanged.
- **IV — Deterministic simulation**: PASS, not applicable to outcome changes. This
  presentation-only feature changes no combat, resources, target priority, phase,
  or balance result.
- **V — Verifiable acceptance**: PASS. US1 maps to live/specialized/fallback scale
  assertions; US2 maps to collider, non-Haetae, HUD, motion, and full regression
  tests.
- **VI / VII — Strings and assets**: PASS. No player-facing string or model
  geometry is replaced; existing authored assets remain authoritative.
- **VIII — Telemetry**: PASS. No gameplay telemetry change is required for a
  presentation-only scale adjustment.
- **X — Recorded decisions**: PASS. Visual-root ownership and rejected root-scale
  alternatives are recorded in [research.md](./research.md).

Post-design re-check: PASS. No complexity exception is required.

## Acceptance Validation Map

| Scenario | Validation |
|----------|------------|
| US1.1 both live general Haetae use 90% | PlayMode live-robot visual-root assertions |
| US1.2 all specializations retain 90% | PlayMode role loop across melee/ranged/balanced |
| US1.3 animation preserves scale | PlayMode motion sampling and repeated refresh |
| US2.1 physical footprint unchanged | PlayMode collider bounds before/after attachment |
| US2.2 gameplay behavior unchanged | Existing full combat, targeting, command, and phase suites |
| US2.3 status information unchanged | Existing HUD text/bar assertions in full PlayMode suite |

## Project Structure

### Documentation (this feature)

```text
specs/009-haetae-scale-tuning/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── haetae-visual-scale.contract.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
TelerobotMVP/Assets/Game/
├── Data/Definitions/VisualThemeDefinitionAsset.cs
├── Data/Assets/VisualTheme.asset
├── Editor/MvpProjectBuilder.cs
└── Runtime/Presentation/LowPolyModelFactory.cs

TelerobotMVP/Assets/Tests/
├── EditMode/DesignAssetCatalogTests.cs
└── PlayMode/VisualPresentationPlayModeTests.cs
```

**Structure Decision**: Keep visual tuning in the existing theme content path.
`LowPolyModelFactory` applies it to the disposable `Presentation Visual` child
before motion binding; gameplay actors and their colliders remain unchanged.

## Complexity Tracking

No constitution violations or approved exceptions.
