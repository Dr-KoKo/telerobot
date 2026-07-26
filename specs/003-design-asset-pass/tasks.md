# Tasks: 디자인 에셋 패스

**Input**: Design documents from `specs/003-design-asset-pass/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Required by FR-028, the project constitution, and the acceptance-validation contract.

**Organization**: Tasks are grouped by user story and preserve current gameplay roots, colliders, data and deterministic results.

**Current state (2026-07-27)**: 66/69 tasks complete. Feature 002 is merged into `main`; the detail-revision-2 General haetae has been regenerated and visually reviewed at 26,694 source vertices, then passed Unity rebuild, EditMode `99/99`, PlayMode `63/63`, Windows build and standalone smoke. The earlier screenshot/performance/final-checklist tasks remain open.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel when dependencies named in the description are complete
- **[Story]**: Maps the task to the corresponding spec user story
- Every task includes an exact file path

## Phase 1: Setup and Baseline

**Purpose**: Establish the current regression floor and art-pipeline folders before presentation changes.

- [X] T001 Run the current Unity `6000.3.20f1` EditMode and PlayMode suites and record the actual pre-art baseline in `specs/003-design-asset-pass/quickstart.md`
- [X] T002 Create project-local art and documentation folder metadata through `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs` for `Assets/Game/Art/{Fonts,Materials,Menu,SourceRecords}` and `TelerobotMVP/Documentation/Art/`
- [X] T003 [P] Create the human-readable style guide skeleton and semantic palette in `TelerobotMVP/Documentation/Art/STYLE-GUIDE.md`
- [X] T004 [P] Create the inventory and third-party notice skeletons in `TelerobotMVP/Documentation/Art/ASSET-CATALOG.md` and `TelerobotMVP/Documentation/Art/THIRD-PARTY-NOTICES.md`

---

## Phase 2: Foundational Presentation Infrastructure

**Purpose**: Add data-owned theme/catalog models and replaceable presentation services shared by every story.

**⚠️ CRITICAL**: User-story implementation starts only after this phase.

- [X] T005 [P] Write failing theme/catalog contract tests for required IDs, valid decision/status combinations, source requirements, fallbacks and semantic colors in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`
- [X] T006 [P] Define visual theme, semantic color/material/effect records and presentation role keys in `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs`
- [X] T007 [P] Define design asset item, source record, decision/status/category enums and catalog validation surface in `TelerobotMVP/Assets/Game/Data/Definitions/DesignAssetCatalogAsset.cs`
- [X] T008 Add visual theme and design catalog references to `TelerobotMVP/Assets/Game/Data/Definitions/MvpContentCatalog.cs`
- [X] T009 Implement the shared URP material cache, property-block accent support and explicit disposal/fallback behavior in `TelerobotMVP/Assets/Game/Runtime/Presentation/PresentationMaterialLibrary.cs`
- [X] T010 [P] Implement cached code-native status/route/command/specialization icon textures in `TelerobotMVP/Assets/Game/Runtime/Presentation/RuntimeIconLibrary.cs`
- [X] T011 Implement the common compound primitive builder, safe child visual roots and silhouette signatures in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T012 Implement bounded transient pulse/tracer/impact construction and cleanup in `TelerobotMVP/Assets/Game/Runtime/Presentation/VisualEffectFactory.cs`
- [X] T013 Generate `VisualTheme.asset`, `DesignAssetCatalog.asset`, shared materials and all required first-pass inventory entries in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T014 Complete T005 by validating generated theme/catalog assets, unique roles, fallbacks and no gameplay-balance fields in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`

**Checkpoint**: Theme, catalog, material, icon, model and effect infrastructure is rebuildable and validated.

---

## Phase 3: User Story 1 - 한눈에 읽히는 전장 (Priority: P1) 🎯 MVP

**Goal**: Replace greybox world/interactables with readable guardian-city landmarks without changing paths or colliders.

**Independent Test**: Build the current session with only world art enabled and identify the central base, three routes and three interactables while all existing gameplay tests remain green.

### Tests for User Story 1

- [X] T015 [P] [US1] Write failing PlayMode tests for world landmark creation, route shape signatures, interactable distinctions, fallback and unchanged root colliders in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 1

- [X] T016 [P] [US1] Implement central base, charging station, safe/risky supply and emergency barrier compound visuals in `TelerobotMVP/Assets/Game/Runtime/Presentation/WorldArtBuilder.cs`
- [X] T017 [P] [US1] Implement north chevron tower, east alley pylons, south tunnel arches and restrained road-edge dressing in `TelerobotMVP/Assets/Game/Runtime/Presentation/WorldArtBuilder.cs`
- [X] T018 [US1] Integrate the theme, material library and world art builder without changing world coordinates or colliders in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T019 [US1] Preserve the old root renderer when theme/world construction fails and emit one development warning in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T020 [US1] Complete T015 and record the grayscale/5-second battlefield review procedure in `TelerobotMVP/Documentation/Art/STYLE-GUIDE.md`

**Checkpoint**: The world is visually identifiable and remains gameplay-equivalent.

---

## Phase 4: User Story 2 - 정체성이 드러나는 해태 팀 (Priority: P1)

**Goal**: Give the two haetae, three staged specializations and medical robot distinct guardian silhouettes and state effects.

**Independent Test**: Instantiate the visual-role gallery without progression and distinguish unit 1, unit 2, General, Melee, Ranged, Balanced and Medical roles.

### Tests for User Story 2

- [X] T021 [P] [US2] Write failing PlayMode tests for haetae unit markers, four role signatures, medical signature and fallback root preservation in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 2

- [X] T022 [US2] Add general quadruped haetae body, paired horns, crest and unit-1/unit-2 markers to `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T023 [US2] Add melee armor/ram, ranged turret/barrel and balanced mixed attachment variants without progression logic in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T024 [US2] Add a non-combat medical support silhouette, halo and heal-range presentation in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T025 [US2] Integrate General/unit identity visuals and theme-driven damaged/destroyed accents in `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs` and `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T026 [US2] Integrate the medical visual without changing spawn, target or healing rules in `TelerobotMVP/Assets/Game/Runtime/Robots/MedicalRobotActor.cs` and `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T027 [US2] Add an editor-only visual gallery command for General/Melee/Ranged/Balanced/Medical roles in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T028 [US2] Complete T021 and document the presentation-only integration boundary with feature 002 in `TelerobotMVP/Documentation/Art/ASSET-CATALOG.md`

**Checkpoint**: All known robot visual assets exist and can be reviewed without duplicating 002 progression semantics.

---

## Phase 5: User Story 3 - 위협 단계가 보이는 좀비 (Priority: P1)

**Goal**: Replace same-shape enemy capsules with readable Runner, Bruiser and Ripper silhouettes and feedback.

**Independent Test**: Spawn all three enemy roles with labels/color removed and distinguish them by body signature while hit/headshot/death feedback still cleans up.

### Tests for User Story 3

- [X] T029 [P] [US3] Write failing PlayMode tests for three enemy silhouette signatures, head marker continuity and transient death-effect cleanup in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 3

- [X] T030 [US3] Implement the narrow forward Runner, wide armored Bruiser and tall blade-arm Ripper compound roles in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T031 [US3] Attach enemy child visuals while retaining root movement, body collider and head hit region in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs` and `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`
- [X] T032 [US3] Route hit, headshot, death and Ripper telegraph presentation through bounded theme effects in `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs` and `TelerobotMVP/Assets/Game/Runtime/Presentation/VisualEffectFactory.cs`
- [X] T033 [US3] Complete T029 and add grayscale silhouette examples/checks to `TelerobotMVP/Documentation/Art/STYLE-GUIDE.md`

**Checkpoint**: Enemy type and threat are readable without labels or hue.

---

## Phase 6: User Story 4 - 지휘 정보를 정리한 인터페이스 (Priority: P2)

**Goal**: Apply one menu/HUD/command/settings/result visual language with editable Korean text.

**Independent Test**: Navigate all six interface surfaces, find critical state groups and confirm no required Korean text is clipped or baked into art.

### Tests for User Story 4

- [X] T034 [P] [US4] Write failing PlayMode tests for menu backdrop fallback, cached icon/style creation, Korean glyph-safe font fallback and no text-bearing raster dependency in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 4

- [X] T035 [P] [US4] Generate and save the original text-free 16:9 menu key art plus prompt record in `TelerobotMVP/Assets/Game/Art/Menu/guardian-night-menu.png` and `TelerobotMVP/Assets/Game/Art/SourceRecords/guardian-night-menu.md`
- [X] T036 [P] [US4] Record and, if locally available with verified OFL evidence, adopt Noto Sans KR with fallback in `TelerobotMVP/Assets/Game/Art/Fonts/` and `TelerobotMVP/Assets/Game/Art/SourceRecords/noto-sans-kr.md`
- [X] T037 [US4] Add cached themed styles, backdrop overlay, title hierarchy and button states to `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MainMenuController.cs`
- [X] T038 [US4] Reorganize the combat HUD into player, base/route, haetae and feedback regions using theme styles/icons in `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T039 [P] [US4] Apply the same theme to robot commands and specialization-ready surface without changing strings or inputs in `TelerobotMVP/Assets/Game/Runtime/HUD/RobotCommandMenu.cs` and `TelerobotMVP/Assets/Game/Runtime/HUD/HaetaeSpecializationView.cs`
- [X] T040 [P] [US4] Apply the same theme to settings/pause/result surfaces in `TelerobotMVP/Assets/Game/Runtime/Settings/SettingsOverlay.cs` and `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [ ] T041 [US4] Complete T034 and record the six-screen screenshot review in `TelerobotMVP/Documentation/Art/STYLE-GUIDE.md`

**Checkpoint**: All interface surfaces share the visual system and retain exact data-owned Korean strings.

---

## Phase 7: User Story 5 - 출처와 교체 비용이 관리되는 에셋 (Priority: P2)

**Goal**: Make the complete asset list, source decisions, provenance and fallback status auditable.

**Independent Test**: A new developer can identify every required category, current make/find/defer decision, license and replacement path from the catalog and notices.

### Tests for User Story 5

- [X] T042 [P] [US5] Extend the failing EditMode audit to reject incomplete external provenance, rejected active references and deferred items without fallback in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`

### Implementation for User Story 5

- [X] T043 [P] [US5] Record official candidates and decisions for Kenney UI/roads/audio, Quaternius environment/zombie/animation, Adobe Mixamo and Noto in `TelerobotMVP/Documentation/Art/ASSET-CATALOG.md`
- [X] T044 [P] [US5] Create source records with official URLs, creator, license, retrieval date, adoption state and restrictions in `TelerobotMVP/Assets/Game/Art/SourceRecords/`
- [X] T045 [US5] Complete the inventory mirror for every required ID and generated-code recipe path in `TelerobotMVP/Documentation/Art/ASSET-CATALOG.md`
- [X] T046 [US5] Complete the build-facing included-file summary in `TelerobotMVP/Documentation/Art/THIRD-PARTY-NOTICES.md`
- [X] T047 [US5] Complete T042 and verify every integrated/adopted/deferred entry against the asset and licensing contracts in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`

**Checkpoint**: Required assets are fully decided and every included external file is auditable.

---

## Phase 8: Polish and Cross-Cutting Validation

**Purpose**: Prove regression safety, performance, reproducibility and handoff quality.

- [ ] T048 [P] Compare fallback and themed Phase 3 frame time, memory, renderer/material/effect counts and record results in `specs/003-design-asset-pass/quickstart.md`
- [X] T049 [P] Add visual theme/catalog rebuild and screenshot capture instructions to `TelerobotMVP/Documentation/Art/STYLE-GUIDE.md`
- [X] T050 Run the full EditMode and PlayMode suites, Windows build and standalone smoke; record exact results in `specs/003-design-asset-pass/quickstart.md`
- [X] T051 Validate repeated **Tools > Telerobot > Build MVP Project** runs produce stable visual asset references and record the result in `specs/003-design-asset-pass/quickstart.md`
- [ ] T052 Run the complete quickstart visual, grayscale, fallback, license and performance checks and mark remaining deferred assets explicitly in `TelerobotMVP/Documentation/Art/ASSET-CATALOG.md`

---

## Phase 9: Latest-main Integration and Playtest Corrections

**Purpose**: Preserve the merged feature-002 HUD/progression behavior and correct physical/readability issues found in the first design-pass build.

- [X] T053 [US1] Add PlayMode regression coverage for blocking central-base collision and perimeter-distributed zombie attack positions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [X] T054 [US1] Configure the central base as an explicit non-trigger blocker for the player in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T055 [US1] Route zombies to deterministic distributed attack positions outside the base collider instead of the base center in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs` and `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`
- [X] T056 [US2] Preserve feature-002 health/battery/experience status bars and connect live specialization state to the design-pass haetae visuals in `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs` and `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`
- [X] T057 Rebuild generated assets, run the merged EditMode/PlayMode suites, create a Windows build and smoke-test the post-playtest fixes

---

## Phase 10: Authored Haetae Production Model

**Purpose**: Establish the first editable, artist-facing character source and replace the visible General haetae primitive assembly without touching gameplay ownership.

- [X] T058 [US2] Generate and retain the General haetae turnaround concept plus prompt/provenance record in `TelerobotMVP/Assets/Game/Art/Concepts/Haetae/haetae-general-turnaround-v1.png` and `TelerobotMVP/Assets/Game/Art/SourceRecords/haetae-general-model.md`
- [X] T059 [US2] Create the reproducible Blender 4.5 LTS hard-surface recipe, editable `.blend`, LOD0/LOD1 FBX and same-source preview in `TelerobotMVP/ArtSource/Haetae/` and `TelerobotMVP/Assets/Game/Art/Models/Haetae/`
- [X] T060 [US2] Add an authored General model reference and deterministic assignment to `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs` and `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T061 [US2] Instantiate the authored General model, remap semantic material slots, preserve unit markers and retain the procedural failure fallback in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T062 [US2] Add EditMode/PlayMode coverage for imported asset references, vertex/LOD contracts, live authored-model selection and fallback behavior in `TelerobotMVP/Assets/Tests/`
- [X] T063 [US2] Update the catalog/style guide/quickstart with the Blender-to-Unity regeneration and review workflow in `TelerobotMVP/Documentation/Art/` and `specs/003-design-asset-pass/quickstart.md`
- [X] T064 Rebuild Unity assets, run EditMode/PlayMode, render-review the model, create a Windows build and smoke-test the authored-model integration

---

## Phase 11: Authored Haetae Detail Refinement

**Purpose**: Replace the remaining block-and-sphere reading of the authored baseline with deliberate custom armor profiles, layered guardian ornament and functional mechanical detail.

- [X] T065 [US2] Tighten the imported-model contract for vertex density, exact semantic material count, LOD reduction and rig hierarchy in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`
- [X] T066 [US2] Add reusable custom profile-plate, converted curve-tube and spiral ornament construction to `TelerobotMVP/ArtSource/Haetae/create_haetae_general.py`
- [X] T067 [US2] Refine the General haetae face, mane, shoulder/flank armor, energy channels, leg pistons, paws and tail scales in `TelerobotMVP/ArtSource/Haetae/create_haetae_general.py`
- [X] T068 [US2] Regenerate and visually review the refined `.blend`, LOD0/LOD1 FBX and preview in `TelerobotMVP/ArtSource/Haetae/` and `TelerobotMVP/Assets/Game/Art/Models/Haetae/`
- [X] T069 [US2] Rebuild Unity assets, run EditMode/PlayMode, update the model source record, create a Windows build and smoke-test the refined integration

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup → Foundational → user-story phases.
- US1, US2 and US3 can proceed independently after Foundation, but all touch `MvpGameController.cs`; sequence integration tasks T018/T025/T031 to avoid conflicts.
- US4 depends on theme/icons, not on world or character completion.
- US5 depends on final make/find decisions but its source-record tasks can start after Foundation.
- Polish depends on all desired story checkpoints.

### User Story Dependencies

- **US1 (P1)**: Foundation only.
- **US2 (P1)**: Foundation plus the merged feature-002 specialization state.
- **US3 (P1)**: Foundation only.
- **US4 (P2)**: Foundation and menu/font deliverables; independent of gameplay visuals.
- **US5 (P2)**: Foundation plus final integrated/deferred status from US1–US4.

### Parallel Opportunities

- T003/T004; T005/T006/T007; T010/T012; T016/T017; T034/T035/T036; T043/T044; T048/T049.
- Story-specific tests can be authored before their implementation tasks.
- Documentation source research does not block project-owned model construction.

## Parallel Example: User Story 4

```text
Task: T034 UI PlayMode contract tests
Task: T035 original menu key art and prompt record
Task: T036 Noto Sans KR source/adoption record
```

## Implementation Strategy

### MVP First

1. Finish Setup and Foundation.
2. Finish US1 so the battlefield no longer reads as greybox.
3. Validate fallback and unchanged gameplay roots.
4. Add US2 and US3 silhouettes.
5. Add UI and provenance.

### Incremental Delivery

Each story retains a playable fallback. A story may be demonstrated at its checkpoint without waiting for animation/audio replacement.

## Notes

- Do not modify `.specify/templates/` or `.specify/memory/constitution.md`.
- Do not alter feature-002 progression semantics while mapping specialization visuals.
- Do not bulk-import candidate packs.
- All external candidates remain documentation-only unless their source record passes the licensing gate.
- Every checkbox must be updated as implementation completes; deferred items are catalog status, not silently skipped tasks.
