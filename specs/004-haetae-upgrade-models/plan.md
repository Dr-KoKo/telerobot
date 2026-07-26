# Implementation Plan: 해태 업그레이드 모델

**Branch**: `004-haetae-upgrade-models` | **Date**: 2026-07-27 | **Spec**: [spec.md](./spec.md)

**Active Spec Identity**:
- Path: `specs/004-haetae-upgrade-models/spec.md`
- Feature: 해태 업그레이드 모델 (`004-haetae-upgrade-models`)
- Status/date: Draft, 2026-07-27
- Baseline reference: `003-design-asset-pass` commit `8f09b427e070a7912b773295d40ebfa7b1360953`
- Domain dependency: feature 002 remains the sole owner of specialization state, combat rules and upgrade selection

**Input**: Feature specification from `specs/004-haetae-upgrade-models/spec.md`

## Summary

Create production-facing Melee, Ranged and Balanced haetae models that share the General model's authored body language while carrying unmistakable role silhouettes. A shared Blender 4.5 LTS recipe derives each variant from the validated General source, adds role-specific hard-surface equipment, exports two FBX LODs and renders individual plus gallery previews. Unity stores the three role-keyed model pairs in the visual theme, selects them from the existing feature-002 specialization, and retains the current procedural role as a per-variant failure fallback.

## Technical Context

**Language/Version**: C# under Unity .NET Standard 2.1; Python 3 through Blender `4.5 LTS`

**Primary Dependencies**: Unity `6000.3.20f1`, URP `17.3.0`, Unity Test Framework `1.6.0`, Blender FBX export/import, feature-002 specialization state, feature-003 presentation factory and material library

**Storage**: Project-local `.blend`, deterministic Python recipe, FBX LOD0/LOD1, PNG previews, ScriptableObject model references and Markdown provenance

**Testing**: EditMode import/contract tests, PlayMode authored-role selection/marker/fallback/cleanup tests, Blender round-trip metrics, same-source render review, Windows build and standalone smoke

**Target Platform**: Windows PC x64

**Project Type**: Unity desktop game plus offline DCC authoring pipeline

**Performance Goals**: Preserve the 60 fps target; every variant supplies an LOD1 below 70% of LOD0 and uses the existing shared five-material theme mapping

**Constraints**:
- No change to specialization values, unlock timing, attacks, targeting, gameplay root transforms or colliders.
- General and all upgrade variants use the same named rig hierarchy and separately addressable unit markers.
- Each variant uses exactly five populated semantic material groups.
- Runtime and player builds consume checked-in FBX files and do not require Blender.
- Missing role assets select the existing role-specific procedural fallback without affecting other roles.
- Existing Korean strings and feature-002 progression ownership remain unchanged.
- `.specify/templates/` and `.specify/memory/constitution.md` remain unchanged.

**Scale/Scope**: Three authored variants, six FBX outputs, three editable sources, three individual previews, one gallery preview, one role-keyed runtime mapping, contract and regression coverage

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| # | Principle | Gate result | Plan compliance |
|---|-----------|-------------|-----------------|
| I | Spec is product source of truth | PASS | Feature 004 spec defines three visual variants and explicitly excludes gameplay expansion. |
| II | Data-driven gameplay and balance | PASS | No balance data changes; authored references remain in the visual theme asset. |
| III | Testable pure gameplay core | PASS | Core/domain assemblies remain unchanged; Unity code is presentation-only. |
| IV | Deterministic simulation | PASS | The change cannot affect simulation outcomes; existing deterministic suite remains the regression oracle. |
| V | Acceptance scenarios verifiable | PASS | Import contracts, PlayMode selection/fallback and manual gallery review cover all scenarios. |
| VI | Player-facing text preserved | PASS | No string changes are planned. |
| VII | Greybox first | PASS | Existing procedural roles remain fallbacks and are never removed. |
| VIII | Development telemetry | PASS | Existing upgrade telemetry is unchanged because no gameplay event semantics change. |
| IX | Scope discipline | PASS | Only Melee/Ranged/Balanced presentation assets and mapping are included. |
| X | Explicit technical decisions | PASS | Shared derivation, data mapping, LOD/material contracts and fallback decisions are recorded in research. |

**Initial gate**: PASS.

**Post-design gate**: PASS. Contracts preserve gameplay ownership, fallbacks and verifiable model constraints without a constitution exception.

## Project Structure

### Documentation (this feature)

```text
specs/004-haetae-upgrade-models/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── authored-upgrade-model.contract.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
TelerobotMVP/
├── ArtSource/Haetae/
│   ├── create_haetae_general.py
│   ├── create_haetae_upgrades.py
│   ├── Haetae_Melee.blend
│   ├── Haetae_Ranged.blend
│   └── Haetae_Balanced.blend
├── Assets/Game/
│   ├── Art/
│   │   ├── Models/Haetae/
│   │   │   ├── Haetae_Melee_LOD0.fbx
│   │   │   ├── Haetae_Melee_LOD1.fbx
│   │   │   ├── Haetae_Ranged_LOD0.fbx
│   │   │   ├── Haetae_Ranged_LOD1.fbx
│   │   │   ├── Haetae_Balanced_LOD0.fbx
│   │   │   ├── Haetae_Balanced_LOD1.fbx
│   │   │   └── Haetae_*_Preview.png / Haetae_Upgrades_Gallery.png
│   │   └── SourceRecords/haetae-upgrade-models.md
│   ├── Data/Definitions/VisualThemeDefinitionAsset.cs
│   ├── Editor/MvpProjectBuilder.cs
│   └── Runtime/Presentation/LowPolyModelFactory.cs
└── Assets/Tests/
    ├── EditMode/DesignAssetCatalogTests.cs
    └── PlayMode/VisualPresentationPlayModeTests.cs
```

**Structure Decision**: Reuse the feature-003 Blender construction functions and runtime presentation boundary. Variant source files stay outside Unity's import tree; FBX and PNG outputs remain under the existing Haetae model folder. The visual theme owns role-to-prefab references, while the factory owns instantiation, material remapping, markers, LOD groups and per-role fallback.

## Authoring Strategy

1. Import the General recipe as a reusable construction module.
2. Rebuild the complete validated General base independently for each variant.
3. Add role-specific parts before mesh consolidation so all equipment shares the five material slots and rigid rig groups.
4. Export each LOD0 at full authored density and LOD1 with the existing deterministic reduction ratio.
5. Save each editable `.blend` and render a neutral individual preview.
6. Re-import the exported FBXs into a clean Blender scene to build the three-model gallery and measure round-trip vertex counts.

## Runtime Integration Strategy

1. Add a serializable authored-role entry containing role, asset ID, LOD0 and LOD1 references.
2. Generate exactly three entries in `VisualTheme.asset`.
3. Resolve General from its existing fields and specialization roles from the new entry list.
4. Use one generic authored-instantiation path for all four role families.
5. Preserve per-role silhouette signatures and requested unit-marker count.
6. If either role entry or LOD0 is absent, return to the existing procedural `BuildHaetae` branch for only that role.
7. Destroy the previous presentation root before replacement; the LOD group and instantiated children remain scoped under that root.

## Validation Strategy

| Spec area | Validation |
|-----------|------------|
| Three distinct authored roles | EditMode import contract + PlayMode gallery role assertions |
| General lineage and rig | Required hierarchy names/material slots/markers on every FBX |
| LOD density | LOD0 >18,000 vertices; LOD1 >500 and <70% of LOD0 |
| Populated materials | Exactly five distinct semantic materials and five non-empty body submeshes |
| Live mapping | Force each existing specialization and assert matching authored asset ID/signature |
| Unit identity | Instantiate every role with marker counts 1 and 2 |
| Fallback | Null each role entry independently and assert procedural signature/no authored marker |
| Gameplay preservation | Existing EditMode, PlayMode and deterministic regressions |
| Build readiness | Windows player build and standalone ready-marker smoke |
| Visual quality | Individual and gallery preview review, plus later five-person grayscale survey |

## Complexity Tracking

No constitution violations are required.
