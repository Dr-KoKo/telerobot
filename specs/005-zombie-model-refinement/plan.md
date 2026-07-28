# Implementation Plan: 정교한 좀비 모델

**Branch**: `005-zombie-model-refinement` | **Date**: 2026-07-27 | **Spec**: [spec.md](./spec.md)

**Active Spec Identity**:
- Path: `specs/005-zombie-model-refinement/spec.md`
- Feature: 정교한 좀비 모델 (`005-zombie-model-refinement`)
- Status/date: Draft, 2026-07-27
- Baseline reference: `004-haetae-upgrade-models` commit `6e2154e37d7616ee5df0268b9d4430d8014af232`
- Domain dependency: feature 001 remains the sole owner of zombie balance, targeting, spawning, collision and headshot rules

**Input**: Feature specification from `specs/005-zombie-model-refinement/spec.md`

## Summary

Replace the procedural Runner, Bruiser and Ripper presentation with three production-facing, project-owned infected humanoid models. A deterministic Blender 4.5 LTS recipe creates a shared anatomical/rig foundation, role-specific silhouettes, five populated material families, two FBX LODs per role and individual plus gallery previews. Unity stores role-keyed references in the visual theme, instantiates authored models below the unchanged zombie gameplay root and falls back independently to the existing compound model when a role asset is missing.

## Technical Context

**Language/Version**: C# under Unity .NET Standard 2.1; Python through Blender `4.5 LTS`

**Primary Dependencies**: Unity `6000.3.20f1`, URP `17.3.0`, Unity Test Framework `1.6.0`, Blender FBX export/import, feature-003 presentation factory, feature-001 zombie actor and hit-region logic

**Storage**: Project-local `.blend`, deterministic Python recipe, FBX LOD0/LOD1, PNG previews, ScriptableObject role references and Markdown provenance

**Testing**: EditMode import/contract tests, PlayMode authored selection/material/fallback/collider/feedback tests, Blender FBX round-trip metrics, visual gallery review, existing deterministic regression, Windows build and standalone smoke

**Target Platform**: Windows PC x64

**Project Type**: Unity desktop game plus offline DCC authoring pipeline

**Performance Goals**: Preserve the 60 fps target; every zombie supplies an LOD1 below 70% of LOD0 and uses the existing bounded material library and presentation cleanup path

**Constraints**:
- No change to health, speed, damage, target priority, threat cost, pathing, spawn budgets, collision bounds or headshot threshold.
- Models are normalized to the existing capsule-root coordinate space and inherit the current per-type display scale.
- Each role uses exactly five populated material groups and the same named humanoid rig contract.
- Runtime and player builds consume checked-in FBX files and do not require Blender.
- Missing LOD0 selects only that role's existing procedural fallback; missing LOD1 remains playable with one LOD.
- Imported presentation children contribute no gameplay colliders.
- Existing hit, death and Ripper telegraph effects continue to tint all authored renderers.
- Existing strings, telemetry and deterministic simulation outputs remain unchanged.
- `.specify/templates/` and `.specify/memory/constitution.md` remain unchanged.

**Scale/Scope**: Three authored enemies, six FBXs, three editable sources, three individual previews, one comparison gallery, one role-keyed runtime mapping, contract and regression coverage

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| # | Principle | Gate result | Plan compliance |
|---|-----------|-------------|-----------------|
| I | Spec is product source of truth | PASS | Feature 005 identifies the exact three visual roles and excludes gameplay changes. |
| II | Data-driven gameplay and balance | PASS | Existing zombie definitions remain unchanged; authored references live in the visual theme. |
| III | Testable pure gameplay core | PASS | Core rules remain untouched; Unity changes are presentation adapters only. |
| IV | Deterministic simulation | PASS | Presentation cannot affect simulation outcomes; the existing suite remains the regression oracle. |
| V | Acceptance scenarios verifiable | PASS | Import contracts, PlayMode selection/fallback/feedback checks and gallery review cover every scenario. |
| VI | Player-facing text preserved | PASS | No string changes are planned. |
| VII | Greybox first | PASS | Existing procedural enemies remain independent fallbacks. |
| VIII | Development telemetry | PASS | Existing events are unchanged because no gameplay semantics change. |
| IX | Scope discipline | PASS | Only Runner, Bruiser and Ripper presentation assets and mapping are included. |
| X | Explicit technical decisions | PASS | Ownership, rig, materials, LOD, normalization and fallback decisions are recorded in research. |

**Initial gate**: PASS.

**Post-design gate**: PASS. The design keeps all gameplay ownership on the existing zombie root and adds no constitution exception.

## Project Structure

### Documentation (this feature)

```text
specs/005-zombie-model-refinement/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── authored-zombie-model.contract.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
TelerobotMVP/
├── ArtSource/Zombies/
│   ├── create_zombie_models.py
│   ├── Zombie_Runner.blend
│   ├── Zombie_Bruiser.blend
│   └── Zombie_Ripper.blend
├── Assets/Game/
│   ├── Art/
│   │   ├── Models/Zombies/
│   │   │   ├── Zombie_Runner_LOD0.fbx
│   │   │   ├── Zombie_Runner_LOD1.fbx
│   │   │   ├── Zombie_Bruiser_LOD0.fbx
│   │   │   ├── Zombie_Bruiser_LOD1.fbx
│   │   │   ├── Zombie_Ripper_LOD0.fbx
│   │   │   ├── Zombie_Ripper_LOD1.fbx
│   │   │   └── Zombie_*_Preview.png / Zombie_Models_Gallery.png
│   │   └── SourceRecords/zombie-production-models.md
│   ├── Data/Definitions/VisualThemeDefinitionAsset.cs
│   ├── Editor/MvpProjectBuilder.cs
│   └── Runtime/Presentation/LowPolyModelFactory.cs
└── Assets/Tests/
    ├── EditMode/DesignAssetCatalogTests.cs
    └── PlayMode/VisualPresentationPlayModeTests.cs
```

**Structure Decision**: Keep editable DCC sources outside Unity's import tree and runtime FBXs under a dedicated Zombies model folder. VisualTheme owns references; LowPolyModelFactory owns presentation instantiation, material remapping, LOD groups and fallback. ZombieActor and its capsule remain the only owners of movement, collision, damage and hit feedback.

## Authoring Strategy

1. Build a normalized humanoid infection base in a local coordinate space centered on the existing capsule.
2. Create a stable humanoid armature with hips, spine, chest, neck, head, two arm chains and two leg chains.
3. Construct layered anatomy, torn armor, dark tissue, exposed bone and emissive corruption as authored profiles, tapered limbs, converted curves and beveled shells.
4. Apply role-specific proportions and equipment before consolidation: lean pursuit anatomy for Runner, low massive asymmetry for Bruiser, tall scythe anatomy for Ripper.
5. Consolidate the body into one rigidly skinned renderer with five non-empty material submeshes.
6. Export LOD0 and deterministic LOD1, save each editable source and render individual previews.
7. Re-import outputs to measure round-trip vertices and render a shared comparison gallery.

## Runtime Integration Strategy

1. Add a serializable zombie role entry containing role, asset ID, LOD0, LOD1 and signature.
2. Populate exactly three entries in `VisualTheme.asset` from stable project paths.
3. Resolve authored enemy presentation before invoking the existing procedural `BuildEnemy`.
4. Instantiate authored children at identity under the scaled gameplay root, strip or disable any imported colliders, remap the five material names and attach a scoped LOD group.
5. Preserve the existing role marker counts and expose an `AuthoredModelMarker` for tests.
6. If an entry or LOD0 is absent, return false so only that role enters the existing procedural path.
7. If LOD1 is absent, keep LOD0 active without adding an LOD group.
8. Reuse Attach cleanup so repeated replacement removes the previous authored model and LOD group.

## Validation Strategy

| Spec area | Validation |
|-----------|------------|
| Three distinct roles | EditMode mapping contract + same-camera gallery review |
| Detailed geometry | Per-role LOD0 vertex floor and five populated submeshes |
| Shared infected language | Five required material names and visual inspection |
| Stable humanoid hierarchy | Required bone names on every LOD0 |
| Runtime selection | Spawn all three types and assert authored IDs/signatures |
| Gameplay preservation | Compare capsule bounds, display scale, hit threshold and zombie config references before/after attach |
| Feedback preservation | Trigger hit/death tint on authored renderers; retain Ripper telegraph regression |
| LOD behavior | Two-LOD group when both references exist; LOD0-only when LOD1 is missing |
| Independent fallback | Null each LOD0 separately and verify only that role uses its procedural signature |
| Build readiness | Full EditMode/PlayMode, Windows player build and standalone ready-marker smoke |
| Visual quality | Individual and gallery review; later five-person grayscale survey |

## Complexity Tracking

No constitution violations are required.
