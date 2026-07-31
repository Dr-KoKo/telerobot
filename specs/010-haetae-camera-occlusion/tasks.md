# Tasks: Haetae Camera Occlusion

**Input**: Design documents from `/specs/010-haetae-camera-occlusion/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/haetae-camera-occlusion.contract.md`, `quickstart.md`

**Tests**: Required by the project constitution and feature specification. Tests
precede their corresponding implementation work.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: May run in parallel because it touches a different file and has no
  dependency on another incomplete task.
- **[Story]**: Maps the task to the matching user story in `spec.md`.

## Phase 1: Setup and baseline

**Purpose**: Lock the pulled main baseline and generated-file boundaries.

- [x] T001 Confirm clean `main`, commit `1f8256b`, Unity `6000.3.20f1`, EditMode 119/119, and PlayMode 80/80 in `specs/010-haetae-camera-occlusion/quickstart.md`
- [x] T002 Verify Unity-generated Library, Logs, TestResults, and Builds remain ignored in `.gitignore`

**Checkpoint**: Work begins from the merged 009 presentation baseline.

---

## Phase 2: Foundational presentation data

**Purpose**: Define and validate the designer-owned occlusion tuning contract.

- [x] T003 Add failing default, invalid-range, and generated-theme assertions for Haetae occlusion tuning in `TelerobotMVP/Assets/Tests/EditMode/DesignAssetCatalogTests.cs`
- [x] T004 Add `HaetaeOcclusionFadeDefinition`, validation, builder defaults, and serialized data in `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, and `TelerobotMVP/Assets/Game/Data/Assets/VisualTheme.asset`

**Checkpoint**: Theme validation guarantees finite safe presentation values.

---

## Phase 3: User Story 1 - Keep the combat view readable (Priority: P1)

**Goal**: Independently fade every Haetae that blocks the central third-person
aiming corridor.

**Independent Test**: Move one and then two live Haetae through the camera-centered
corridor and verify 0.32 opacity within 0.15 seconds plus independent state.

### Tests for User Story 1

- [x] T005 [US1] Add failing live corridor, transition timing, two-robot independence, and transparent-surface assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 1

- [x] T006 [US1] Implement non-allocating third-person corridor evaluation and smooth per-Haetae opacity in `TelerobotMVP/Assets/Game/Runtime/Presentation/HaetaeCameraOcclusionFader.cs` and `.meta`
- [x] T007 [US1] Attach and initialize exactly one fader per spawned Haetae in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`

**Checkpoint**: Central obstruction fades without changing the robot actor.

---

## Phase 4: User Story 2 - Preserve Haetae presence outside obstruction (Priority: P1)

**Goal**: Restore exact opaque materials and keep side/first-person views,
colliders, scale, and status UI unchanged.

**Independent Test**: Clear the corridor and toggle first-person, then verify full
opacity, original material references, unchanged physical bounds, 0.90 scale, and
populated status bars.

### Tests for User Story 2

- [x] T008 [US2] Add failing clear-view, first-person, exact-material-restore, collider, scale, and status-boundary assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 2

- [x] T009 [US2] Complete opaque restoration, perspective gating, owned-material cleanup, and property-block alpha preservation in `TelerobotMVP/Assets/Game/Runtime/Presentation/HaetaeCameraOcclusionFader.cs`

**Checkpoint**: Normal rendering returns exactly when the view is clear.

---

## Phase 5: User Story 3 - Stay stable across model states (Priority: P2)

**Goal**: Follow specialization, LOD, animation, tint, and fallback presentation
without stale or cumulative material state.

**Independent Test**: Replace a faded presentation, sample motion, run 10
fade/restore cycles, and repeat with the procedural fallback.

### Tests for User Story 3

- [x] T010 [US3] Add failing specialization-rebind, motion, property-block, 10-cycle, and fallback assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 3

- [x] T011 [US3] Implement visual-root change detection, renderer rebinding, and obsolete variant disposal in `TelerobotMVP/Assets/Game/Runtime/Presentation/HaetaeCameraOcclusionFader.cs`

**Checkpoint**: Current authored or fallback visuals own one clean opacity state.

---

## Phase 6: Regeneration, validation, and handoff

**Purpose**: Produce reproducible tests and a Windows playtest build.

- [x] T012 Regenerate with `MvpProjectBuilder.BuildAll`, verify occlusion data plus 0.90 scale in `TelerobotMVP/Assets/Game/Data/Assets/VisualTheme.asset`, and discard unrelated scene/catalog serialization noise
- [x] T013 Run complete EditMode and PlayMode suites and record counts in `specs/010-haetae-camera-occlusion/quickstart.md`
- [x] T014 Build Windows x86_64, smoke to `TELEROBOT_STANDALONE_SMOKE_READY`, check material/shader logs, and record results in `specs/010-haetae-camera-occlusion/quickstart.md`
- [x] T015 Mark `specs/010-haetae-camera-occlusion/spec.md` implemented, complete `specs/010-haetae-camera-occlusion/tasks.md`, run `git diff --check`, review scope, and commit on `main`

---

## Dependencies & Execution Order

- Phase 1 establishes the pulled baseline.
- Phase 2 blocks every user story.
- US1 delivers the minimum visibility improvement.
- US2 depends on US1 material ownership and proves normal rendering boundaries.
- US3 depends on US1/US2 and proves lifecycle stability.
- Phase 6 depends on all stories.

## Parallel Opportunities

- T003 and T005 can be authored independently before implementation.
- Documentation review can run while long Unity suites or the Windows build run.
- User stories are intentionally executed sequentially because they share the
  fader lifecycle and PlayMode fixture.

## Implementation Strategy

1. Lock data-driven tuning and validation.
2. Write central-obstruction tests before the fader.
3. Add restoration and perspective boundaries.
4. Add model-replacement and fallback lifecycle coverage.
5. Regenerate, run full regressions, build, smoke, document, and commit.
