# Tasks: 해태 업그레이드 모델

**Input**: Design documents from `specs/004-haetae-upgrade-models/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Required by the feature specification and project constitution. Contract tests precede implementation.

**Organization**: Tasks are grouped by user story and preserve feature-002 gameplay ownership plus feature-003 procedural fallbacks.

**Current state (2026-07-27)**: 13/13 tasks complete.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel when files do not overlap.
- **[Story]**: Maps work to the corresponding feature-004 user story.
- Every task includes an exact file path.

## Phase 1: Setup and Baseline

**Purpose**: Confirm the feature-003 authored General baseline and define upgrade output locations.

- [X] T001 Record the feature-003 baseline commit, current Unity results and expected upgrade output matrix in `specs/004-haetae-upgrade-models/quickstart.md`

---

## Phase 2: Foundational Contracts

**Purpose**: Establish failing structural/runtime contracts and the role-keyed reference model before authoring integration.

- [X] T002 [P] Add failing EditMode contracts for three unique role entries, required FBX paths, vertex/LOD thresholds, five populated submeshes, hierarchy names and markers in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`
- [X] T003 [P] Add failing PlayMode contracts for authored Melee/Ranged/Balanced selection, asset IDs, signatures, marker counts, per-role fallback and replacement cleanup in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [X] T004 Define the serializable role-keyed authored haetae model entry and resolver in `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs`

**Checkpoint**: Data and test contracts describe all three upgrade roles independently.

---

## Phase 3: User Story 1 - 전투 특화 형태를 즉시 구분한다 (Priority: P1)

**Goal**: Deliver three artist-authored role silhouettes with two LODs each.

**Independent Test**: Render all three exported models under one neutral camera and distinguish every role in grayscale from its ram/turret/asymmetric cues.

- [X] T005 [US1] Create the shared upgrade authoring recipe and Melee ram, side-horn, shoulder-shield, foreleg-bracer and jaw geometry in `TelerobotMVP/ArtSource/Haetae/create_haetae_upgrades.py`
- [X] T006 [US1] Add the Ranged turret, long barrel, sensor wing, power-pod and rear-stabilizer geometry in `TelerobotMVP/ArtSource/Haetae/create_haetae_upgrades.py`
- [X] T007 [US1] Add the Balanced compact turret, asymmetric jaw/tusk, sensor shoulder and mixed foreleg armor in `TelerobotMVP/ArtSource/Haetae/create_haetae_upgrades.py`
- [X] T008 [US1] Generate and visually review three `.blend` files, six FBXs, three role previews and `Haetae_Upgrades_Gallery.png` under `TelerobotMVP/ArtSource/Haetae/` and `TelerobotMVP/Assets/Game/Art/Models/Haetae/`

**Checkpoint**: All role models satisfy the authored source, silhouette, LOD and material contracts.

---

## Phase 4: User Story 2 - 기존 해태 정체성과 유닛 구분을 유지한다 (Priority: P1)

**Goal**: Map feature-002 specialization roles to authored models without losing General lineage or unit identity.

**Independent Test**: Instantiate every role with one and two markers and verify the expected asset ID, shared rig/material contract and marker visibility.

- [X] T009 [US2] Assign stable paths and populate the three authored role entries during project rebuild in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T010 [US2] Generalize authored haetae resolution/instantiation for General, Melee, Ranged and Balanced while retaining unique signatures and markers in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T011 [US2] Complete T002/T003 coverage for live specialization mapping and unchanged gameplay roots/colliders in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs` and `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

**Checkpoint**: Existing specialization state selects the matching authored model and both unit identities remain visible.

---

## Phase 5: User Story 3 - 업그레이드 모델 실패에도 계속 플레이한다 (Priority: P2)

**Goal**: Preserve independent procedural fallback and cleanup for every upgrade role.

**Independent Test**: Remove each role reference separately, instantiate that role twice and observe its procedural signature, no authored marker, one presentation root and no leaked LOD group.

- [X] T012 [US3] Complete per-role missing-reference and repeated-replacement fallback validation in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

**Checkpoint**: One missing asset cannot block or downgrade any other role.

---

## Phase 6: Polish and Validation

**Purpose**: Complete provenance, regeneration documentation and end-to-end build evidence.

- [X] T013 Update `TelerobotMVP/Assets/Game/Art/SourceRecords/haetae-upgrade-models.md` and `specs/004-haetae-upgrade-models/quickstart.md`, rebuild Unity assets, run EditMode/PlayMode, create a Windows build and pass standalone smoke

---

## Dependencies & Execution Order

1. T001 establishes the baseline.
2. T002 and T003 run before implementation and may proceed in parallel.
3. T004 blocks runtime reference population.
4. T005 → T006 → T007 share one authoring file and run sequentially.
5. T008 depends on T005-T007.
6. T009 and T010 depend on T004 and generated FBXs from T008.
7. T011 depends on T009-T010.
8. T012 depends on the generalized factory in T010.
9. T013 runs after all story checkpoints.

## Parallel Opportunities

- T002 and T003 are parallel contract-test tasks.
- Source-record drafting for T013 may begin while model generation runs, but final metrics wait for T008.

## Implementation Strategy

### MVP First

1. Establish failing contracts.
2. Produce and integrate Melee as the first complete role.
3. Apply the same shared pipeline to Ranged and Balanced.
4. Validate independent fallback and build readiness.

### Incremental Delivery

Every role retains its feature-003 procedural representation until its authored reference passes the full contract. A partial asset set therefore remains playable.
