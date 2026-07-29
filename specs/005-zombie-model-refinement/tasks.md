# Tasks: 정교한 좀비 모델

**Input**: Design documents from `specs/005-zombie-model-refinement/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Required by the feature specification and project constitution. Contract tests precede implementation.

**Organization**: Tasks are grouped by user story and preserve feature-001 gameplay ownership plus feature-003 procedural fallbacks.

**Current state (2026-07-28)**: 13/13 tasks complete.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel when files do not overlap.
- **[Story]**: Maps work to the corresponding feature-005 user story.
- Every task includes an exact file path.

## Phase 1: Setup and Baseline

**Purpose**: Confirm feature-004 validation and define zombie output locations.

- [x] T001 Record the feature-004 commit, Unity baseline and expected zombie output matrix in `specs/005-zombie-model-refinement/quickstart.md`

---

## Phase 2: Foundational Contracts

**Purpose**: Establish failing structural/runtime contracts and role-keyed model data before authoring integration.

- [x] T002 [P] Add failing EditMode contracts for three unique role entries, paths, vertex/LOD thresholds, five populated submeshes and humanoid hierarchy in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`
- [x] T003 [P] Add failing PlayMode contracts for authored role selection, asset IDs, signatures, LOD behavior, collider preservation, feedback and independent fallback in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [x] T004 Define the serializable role-keyed authored zombie entry, resolver and validation in `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs`

**Checkpoint**: Data and test contracts describe all three roles without changing gameplay data.

---

## Phase 3: User Story 1 - 위협 유형을 실루엣으로 즉시 구분한다 (Priority: P1)

**Goal**: Deliver three production-facing zombie silhouettes with shared infection language and two LODs each.

**Independent Test**: Render all three models under one neutral camera and distinguish every role in grayscale from anatomy alone.

- [x] T005 [US1] Create the shared infected humanoid rig/material recipe and detailed Runner anatomy, pursuit spines, elongated legs and claws in `TelerobotMVP/ArtSource/Zombies/create_zombie_models.py`
- [x] T006 [US1] Add the Bruiser low heavy torso, layered shoulders, massive forearms, armor and asymmetric corruption mass in `TelerobotMVP/ArtSource/Zombies/create_zombie_models.py`
- [x] T007 [US1] Add the Ripper tall hunter anatomy, paired scythe arms, split crest and anti-robot core in `TelerobotMVP/ArtSource/Zombies/create_zombie_models.py`
- [x] T008 [US1] Generate and visually review three `.blend` files, six FBXs, three role previews and `Zombie_Models_Gallery.png` under `TelerobotMVP/ArtSource/Zombies/` and `TelerobotMVP/Assets/Game/Art/Models/Zombies/`

**Checkpoint**: All roles satisfy detail, silhouette, LOD, material and hierarchy contracts.

---

## Phase 4: User Story 2 - 정교한 외형이 기존 전투 판독성을 보존한다 (Priority: P1)

**Goal**: Select authored models at runtime while preserving collision, headshots and feedback.

**Independent Test**: Spawn every type, compare gameplay-root bounds and configuration, then trigger hit/death feedback on authored renderers.

- [x] T009 [US2] Assign stable model paths and populate the three authored zombie entries during project rebuild in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [x] T010 [US2] Resolve and instantiate authored zombie LODs, remap materials, disable presentation colliders and retain role markers in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [x] T011 [US2] Complete live selection, unchanged collider/headshot/config and authored feedback coverage in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs` and `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

**Checkpoint**: Authored visuals replace only presentation and every existing gameplay rule remains intact.

---

## Phase 5: User Story 3 - 모델 문제가 있어도 전투를 계속한다 (Priority: P2)

**Goal**: Preserve independent procedural fallback, LOD0-only display and replacement cleanup.

**Independent Test**: Remove each LOD0 and each LOD1 separately, attach roles repeatedly and inspect markers, presentation roots and LOD groups.

- [x] T012 [US3] Complete missing-LOD0, missing-LOD1 and repeated-replacement validation in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

**Checkpoint**: A partial asset failure cannot block combat or downgrade another role.

---

## Phase 6: Polish and Validation

**Purpose**: Complete provenance, regeneration documentation and end-to-end build evidence.

- [x] T013 Update `TelerobotMVP/Assets/Game/Art/SourceRecords/zombie-production-models.md` and `specs/005-zombie-model-refinement/quickstart.md`, rebuild Unity assets, run EditMode/PlayMode, create a Windows build and pass standalone smoke

---

## Dependencies & Execution Order

1. T001 establishes the baseline.
2. T002 and T003 precede implementation and may proceed in parallel.
3. T004 blocks generated theme entries and runtime resolution.
4. T005 through T007 share one authoring file and run sequentially.
5. T008 depends on T005-T007.
6. T009 and T010 depend on T004 and generated FBXs.
7. T011 depends on T009-T010.
8. T012 depends on the authored enemy factory path in T010.
9. T013 runs after all story checkpoints.

## Parallel Opportunities

- T002 and T003 are independent contract-test files.
- Source-record drafting for T013 may begin during model generation, but final metrics wait for T008.

## Implementation Strategy

### MVP First

1. Establish failing contracts.
2. Produce and integrate Runner as the first complete role.
3. Apply the shared pipeline to Bruiser and Ripper.
4. Validate independent failure paths and build readiness.

### Incremental Delivery

Every role retains its existing procedural representation until its authored reference passes the full contract. Partial output remains playable.
