# Tasks: Base Visibility and Walkable Access

**Input**: Design documents from `/specs/007-base-visibility-access/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/base-platform.contract.md`, `quickstart.md`

**Tests**: Required by the project constitution. Rule tests are written before core
implementation, traversal/integration tests before or with their runtime behavior,
and all regressions/build checks are mandatory.

**Organization**: Tasks are grouped by user story while keeping shared data and pure
geometry rules in the blocking foundation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files after dependencies
  are satisfied.
- **[Story]**: Maps the task to a user story in `spec.md`.

## Phase 1: Setup and data contract

**Purpose**: Establish the editable geometry profile shared by every story.

- [x] T001 Add base footprint, terrace, beacon, and attack-slot fields to `TelerobotMVP/Assets/Game/Data/Definitions/WorldLayoutAsset.cs` and mapped members to `TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`
- [x] T002 Map and validate the new profile in `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, including height, radius, spacing, and beacon limits
- [x] T003 Seed the same defaults in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs` and `TelerobotMVP/Assets/Game/Data/Assets/WorldLayout.asset`

**Checkpoint**: A regenerated catalog produces one valid, editable base profile.

---

## Phase 2: Foundational deterministic geometry

**Purpose**: Move footprint-sensitive attack positions out of the Unity scene
adapter before changing the structure.

- [x] T004 [P] Add failing deterministic outside-footprint, spacing, diagonal-approach, and zero-approach tests in `TelerobotMVP/Assets/Tests/EditMode/BasePerimeterRulesTests.cs`
- [x] T005 Implement pure circular slot calculation in `TelerobotMVP/Assets/Game/Core/Gameplay/BasePerimeterRules.cs`
- [x] T006 Replace box-bound attack-position math with `BasePerimeterRules` in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`

**Checkpoint**: Equal input produces equal distributed slots outside the configured
circular radius without any renderer or collider query.

---

## Phase 3: User Story 1 - See combat around the base (Priority: P1)

**Goal**: Replace the broad tall obstruction with a low landmark and narrow beacon.

**Independent Test**: At all four cardinal viewpoints, assert the broad body height
and beacon width limits, then visually confirm the opposite approach remains
readable.

### Tests for User Story 1

- [x] T007 [US1] Replace the legacy full-height blocker test with failing geometry-height, beacon-width, hierarchy, material-fallback, and duplicate-collider checks in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 1

- [x] T008 [US1] Add the scale-one static terrace hierarchy and matching mesh-collider contract in `TelerobotMVP/Assets/Game/Runtime/Presentation/CentralBasePlatform.cs`
- [x] T009 [US1] Build the configured platform instead of the 8-by-3 box in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [x] T010 [US1] Restyle the base as terraces with non-colliding trim and a narrow guardian beacon in `TelerobotMVP/Assets/Game/Runtime/Presentation/WorldArtBuilder.cs`

**Checkpoint**: The base remains identifiable and broad geometry stays at or below
0.75 metres.

---

## Phase 4: User Story 2 - Climb the circular base (Priority: P1)

**Goal**: Let the real player controller ascend, stand on, cross, and descend from
the visible structure without jumping.

**Independent Test**: Traverse from four cardinal directions and diagonally with the
actual `CharacterController`, checking progress, top height, grounding, and escape.

### Tests for User Story 2

- [x] T011 [US2] Add cardinal ascent/descent and repeated diagonal traversal tests to `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 2

- [x] T012 [US2] Tune terrace placement and controller-compatible step geometry in `TelerobotMVP/Assets/Game/Runtime/Presentation/CentralBasePlatform.cs` while preserving exact visible/collider bounds

**Checkpoint**: All required crossings complete without jump requests, traps,
fall-through, or forced ejection.

---

## Phase 5: User Story 3 - Preserve base-defense behavior (Priority: P1)

**Goal**: Keep perimeter attacks, charging, HUD/status bars, and combat outcomes
unchanged.

**Independent Test**: Run multiple perimeter attackers and charging/HUD assertions
while traversing the new platform.

### Tests for User Story 3

- [x] T013 [US3] Update attacker-distribution assertions to use the circular footprint and add explicit charging/HUD/world-status-bar regression assertions in `TelerobotMVP/Assets/Tests/PlayMode/VisualPresentationPlayModeTests.cs`

### Implementation for User Story 3

- [x] T014 [US3] Verify and, only if required by failing tests, adapt base, charging, or status presentation integration without changing their rules in `TelerobotMVP/Assets/Game/Runtime/`

**Checkpoint**: Zombies damage from at least four distinct outside slots; charging
and every existing status presentation remain intact.

---

## Phase 6: Regeneration, validation, and handoff

**Purpose**: Prove the feature is reproducible and ready for a user playtest.

- [x] T015 Regenerate project content with `MvpProjectBuilder.BuildAll` and verify `TelerobotMVP/Assets/Game/Data/Assets/WorldLayout.asset` retains the new values
- [x] T016 Run the complete EditMode suite and record the final passing count in `specs/007-base-visibility-access/quickstart.md`
- [x] T017 Run the complete PlayMode suite and record the final passing count in `specs/007-base-visibility-access/quickstart.md`
- [x] T018 Build the Windows x86_64 player, smoke-launch it to `TELEROBOT_STANDALONE_SMOKE_READY`, and record both results in `specs/007-base-visibility-access/quickstart.md`
- [x] T019 Complete the four-side manual checklist when observable in automation, mark any remaining user visual checks clearly, and update `specs/007-base-visibility-access/spec.md` status
- [x] T020 Mark every completed item in `specs/007-base-visibility-access/tasks.md`, run `git diff --check`, review scope, and commit the feature on `007-base-visibility-access`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1** has no implementation dependency.
- **Phase 2** depends on mapped world data and blocks runtime attack integration.
- **US1** depends on Phase 1 and provides the platform used by US2.
- **US2** depends on the US1 hierarchy but remains independently testable through
  traversal.
- **US3** depends on the platform and perimeter rule so it can validate preserved
  integrations.
- **Phase 6** depends on all selected stories.

### User Story Dependencies

- **US1** delivers the visibility correction and static platform.
- **US2** uses US1's visible geometry and proves access.
- **US3** integrates the foundation and platform with existing defense behavior.

### Within Each User Story

- Add or revise the acceptance test before finalizing implementation.
- Keep gameplay geometry data-driven and attack math in the pure core.
- Keep presentation decoration optional and collider-free.
- Complete the story checkpoint before moving to final validation.

## Parallel Opportunities

- T004 can be prepared while T001-T003 update the data path.
- Once the hierarchy contract is fixed, US1 presentation styling and US2 test design
  touch different files conceptually, but implementation remains serialized in this
  single-agent run to avoid shared-file conflicts.
- Documentation updates can be reviewed while long-running Unity suites execute.

## Implementation Strategy

1. Lock the data contract and pure perimeter rule.
2. Deliver the low visible platform and validate its bounds.
3. Prove no-jump traversal with the real controller.
4. Re-run defense, charging, HUD, and status integrations.
5. Regenerate, run full suites, build, smoke, document, and commit.
