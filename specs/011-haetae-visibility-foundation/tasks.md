# Tasks: Haetae Visibility Foundation

**Input**: Design documents from `/specs/011-haetae-visibility-foundation/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/haetae-visibility.contract.md`, `quickstart.md`

**Tests**: Required by the project constitution and feature specification. Test
changes precede their corresponding implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: May run in parallel because it touches a different file.
- **[Story]**: Maps to a user story in `spec.md`.

## Phase 1: Setup and baseline

- [x] T001 Confirm `main` commit `3a72357`, Unity `6000.3.20f1`, baseline EditMode 119/119 and PlayMode 82/82 in `specs/011-haetae-visibility-foundation/quickstart.md`
- [x] T002 Verify Unity outputs remain ignored in `.gitignore`

---

## Phase 2: Foundational data

- [x] T003 Add failing robot physical-footprint mapping and validation tests in `TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs` and `TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`
- [x] T004 Add data-driven radius, height, and center to `TelerobotMVP/Assets/Game/Data/Definitions/RobotDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, and `TelerobotMVP/Assets/Game/Data/Assets/HaetaeRobot.asset`

---

## Phase 3: User Story 1 - One predictable visual size (Priority: P1)

**Independent Test**: Every role uses one uniform 0.85 presentation scale under
an identity actor root while the legacy physical bounds remain unchanged.

- [x] T005 [US1] Add failing identity-root, legacy-bounds, all-role, motion, and repeated-replacement assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [x] T006 [US1] Normalize Haetae spawn roots and configure mapped capsule data in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [x] T007 [US1] Verify `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs` remains the sole Haetae visual-scale owner and remove obsolete stacked-scale expectations

---

## Phase 4: User Story 2 - Visible obstruction transparency (Priority: P1)

**Independent Test**: A centered visible model fades with an off-corridor
collider, while a centered collider with clear renderers stays opaque.

- [x] T008 [US2] Add failing renderer-only obstruction, collider-only clear, opacity 0.16, active-renderer, and transparent-surface assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [x] T009 [US2] Replace physics-collider obstruction with cached active-renderer bounds in `TelerobotMVP/Assets/Game/Runtime/Presentation/HaetaeCameraOcclusionFader.cs`
- [x] T010 [US2] Set opacity 0.16 and non-preserved specular transparent variants in `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/VisualTheme.asset`, and `TelerobotMVP/Assets/Game/Runtime/Presentation/HaetaeCameraOcclusionFader.cs`

---

## Phase 5: User Story 3 - Stable readable presentation (Priority: P2)

**Independent Test**: Two robots, all replacement paths, first-person, motion,
tint, fallback, and ten cycles restore exact stable state.

- [x] T011 [US3] Update independence, lifecycle, fallback, first-person, material restoration, and no-growth coverage in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [x] T012 [US3] Complete renderer cache lifecycle and material cleanup in `TelerobotMVP/Assets/Game/Runtime/Presentation/HaetaeCameraOcclusionFader.cs`

---

## Phase 6: Regeneration and handoff

- [x] T013 Run `MvpProjectBuilder.BuildAll`, verify generated physical/visual/opacity data, and remove unrelated serialization noise
- [x] T014 Run complete EditMode and PlayMode suites, Windows build, standalone smoke, and record results in `specs/011-haetae-visibility-foundation/quickstart.md`
- [x] T015 Mark `specs/011-haetae-visibility-foundation/spec.md` implemented, complete this task list, run `git diff --check`, review scope, and commit on `main`

---

## Phase 7: Follow-up visibility tuning

- [x] T016 [US1] Change generated and loaded theme expectations to uniform scale `0.85`
- [x] T017 [US2] Change generated and loaded obstruction opacity expectations to `0.16`
- [x] T018 [US1] [US2] Update theme defaults, generated data, and runtime assets without changing physical footprint or transition timing
- [x] T019 Run targeted and complete Unity suites, regenerate, build Windows, run standalone smoke, exclude serialization noise, and record results
- [x] T020 Mark the tuning delta implemented, complete tasks, review, and commit on `main`

---

## Phase 8: Player-build transparency and second size pass

- [x] T021 [US1] [US2] Amend the feature artifacts for uniform scale `0.80`, opacity `0.10`, retained player-build transparency, and emissive fading
- [x] T022 [US1] Add failing generated/default scale `0.80` assertions in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs` and `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [x] T023 [US2] Add failing retained transparent-template, render-state, and emissive-opacity assertions in the same EditMode and PlayMode fixtures
- [x] T024 [US1] [US2] Generate the transparent material asset, map it through `VisualThemeDefinitionAsset`, apply scale/opacity values, and derive runtime faded materials from the retained template
- [x] T025 Run targeted red/green coverage, regenerate assets, complete suites, Windows build, standalone smoke, and exclude unrelated serialization noise
- [x] T026 Mark the follow-up implemented, record validation, review scope, and commit the explicit files on `main`

---

## Phase 9: First-person playtest amendment

- [x] T027 [US2] Add a failing PlayMode assertion that a Haetae covering the
  first-person aiming corridor uses the same conditional fade and restore rule
  in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [x] T028 [US4] Add failing configuration and PlayMode assertions for a
  first-person first-launch default and saved-perspective precedence in
  `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`,
  `TelerobotMVP/Assets/Tests/PlayMode/PlayerExperiencePlayModeTests.cs`, and
  `TelerobotMVP/Assets/Tests/PlayMode/MenuAndSettingsPlayModeTests.cs`
- [x] T029 [US2] Allow renderer-bound obstruction evaluation in either camera
  perspective in `TelerobotMVP/Assets/Game/Runtime/Presentation/HaetaeCameraOcclusionFader.cs`
- [x] T030 [US4] Change the data-defined and generated first-launch perspective
  to first-person while retaining saved preference precedence in
  `TelerobotMVP/Assets/Game/Data/Assets/PlayerSettings.asset` and
  `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [x] T031 [US2] [US4] Run targeted red/green tests, complete EditMode and
  PlayMode regression suites, build Windows x86_64, run standalone smoke, and
  record validation in `specs/011-haetae-visibility-foundation/quickstart.md`

## Dependencies & Execution Order

- Phase 1 establishes the committed baseline.
- Phase 2 blocks identity-root spawning.
- US1 establishes scale and physical ownership before US2 changes obstruction.
- US2 establishes renderer-bound activation before US3 lifecycle coverage.
- T027 and T028 establish the amended red tests before T029 and T030 implementation.
- T031 depends on all first-person amendment tasks.
- Phase 6 depends on all stories.

## Parallel Opportunities

- EditMode physical-data tests and PlayMode visual tests touch separate fixtures.
- Documentation review may run while Unity suites or builds execute.
- Runtime tasks are sequential because spawn, scale, and fader boundaries interact.

## Implementation Strategy

1. Lock physical footprint data and tests.
2. Normalize the root and prove physical invariance.
3. Prove the old collider detection fails the renderer-only scenario.
4. Implement renderer-bound activation and clearer material state.
5. Re-run lifecycle, full regression, build, smoke, and commit.
6. Extend the proven obstruction rule to first-person, change only the unsaved
   perspective default, and repeat full validation.
