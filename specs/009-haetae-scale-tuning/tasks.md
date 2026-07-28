# Tasks: Haetae Scale Tuning

**Input**: Design documents from `/specs/009-haetae-scale-tuning/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/haetae-visual-scale.contract.md`, `quickstart.md`

**Tests**: Required by the project constitution and feature specification.
Contract and presentation tests precede implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: May run in parallel because it touches a different file and has no
  dependency on another incomplete task.
- **[Story]**: Maps the task to the matching user story in `spec.md`.

## Phase 1: Setup and baseline

**Purpose**: Preserve the validated presentation and build baseline.

- [x] T001 Confirm branch, clean baseline, Unity version, and 119/119 EditMode plus 79/79 PlayMode counts in `specs/009-haetae-scale-tuning/quickstart.md`
- [x] T002 Verify Unity-generated Library, Logs, TestResults, and Builds remain ignored in `.gitignore`

**Checkpoint**: The visual-only feature begins from the passing 008 build.

---

## Phase 2: Foundational data contract

**Purpose**: Make the global Haetae presentation scale explicit, validated, and
reproducible.

- [x] T003 Add failing default, invalid-range, and generated-0.90 theme assertions in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`
- [x] T004 Add and validate the `haetaeVisualScale` presentation field in `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs`
- [x] T005 Seed `haetaeVisualScale` to 0.90 in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs` and `TelerobotMVP/Assets/Game/Data/Assets/VisualTheme.asset`

**Checkpoint**: The theme contract rejects invalid scales and regenerates the
requested value.

---

## Phase 3: User Story 1 - Slightly smaller Haetae silhouette (Priority: P1)

**Goal**: Apply the 90% scale to every general and specialized Haetae visual.

**Independent Test**: Attach every Haetae presentation role and verify one uniform
0.90 visual root for authored and fallback models after repeated refresh.

### Tests for User Story 1

- [x] T006 [US1] Add failing live-general, three-specialization, repeated-attachment, and authored/fallback scale assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 1

- [x] T007 [US1] Apply the configured scale absolutely to Haetae presentation roots before model construction and motion binding in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`

**Checkpoint**: All five Haetae roles use 0.90 without cumulative scaling.

---

## Phase 4: User Story 2 - Preserve gameplay and UI footprint (Priority: P1)

**Goal**: Prove the change remains isolated from collision, other roles, motion,
combat, and status UI.

**Independent Test**: Compare collider bounds before and after Haetae attachment,
sample character motion, attach a Runner control, and execute existing HUD/combat
regressions.

### Tests for User Story 2

- [x] T008 [US2] Add collider-bound, motion-baseline, and non-Haetae identity-scale assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`
- [x] T009 [US2] Verify existing HUD status bars, specialization, robot separation, targeting, combat, and phase tests require no gameplay adaptation in `TelerobotMVP/Assets/Tests/PlayMode/`

**Checkpoint**: Only the replaceable Haetae presentation child changes size.

---

## Phase 5: Regeneration, validation, and handoff

**Purpose**: Produce a reproducible tested Windows build for visual confirmation.

- [x] T010 Regenerate content with `MvpProjectBuilder.BuildAll`, verify `TelerobotMVP/Assets/Game/Data/Assets/VisualTheme.asset`, and discard unrelated scene/catalog serialization noise
- [x] T011 Run complete EditMode and PlayMode suites and record counts in `specs/009-haetae-scale-tuning/quickstart.md`
- [x] T012 Build Windows x86_64, smoke to `TELEROBOT_STANDALONE_SMOKE_READY`, and record results in `specs/009-haetae-scale-tuning/quickstart.md`
- [x] T013 Mark `specs/009-haetae-scale-tuning/spec.md` implemented, complete `specs/009-haetae-scale-tuning/tasks.md`, run `git diff --check`, review scope, and commit on `009-haetae-scale-tuning`

---

## Dependencies & Execution Order

- Phase 1 establishes the baseline.
- Phase 2 blocks presentation implementation.
- US1 depends on the theme field and delivers the visual change.
- US2 depends on US1 and proves gameplay/UI isolation.
- Phase 5 depends on both user stories.

## Parallel Opportunities

- T003 and T006 can be authored independently before implementation.
- T008 boundary tests can be designed while T007 is implemented.
- Documentation review can run while long Unity suites execute.

## Implementation Strategy

1. Lock the theme data contract.
2. Write failing visual-role and physical-boundary tests.
3. Apply one absolute visual-root multiplier before motion binding.
4. Regenerate content and run presentation/gameplay regressions.
5. Build, smoke, document, and commit the new branch.
