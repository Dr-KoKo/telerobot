# Tasks: Haetae Wave Cleanup Targeting

**Input**: Design documents from
`/specs/008-haetae-wave-cleanup-targeting/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`,
`contracts/cleanup-targeting.contract.md`, `quickstart.md`

**Tests**: Required by the project constitution and the feature specification.
Pure rule and PlayMode regression tests are written before implementation.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: May run in parallel because it touches a different file and has no
  dependency on another incomplete task.
- **[Story]**: Maps the task to the matching user story in `spec.md`.

## Phase 1: Setup and baseline

**Purpose**: Lock the exact failure and preserve the validated baseline.

- [x] T001 Confirm branch, clean baseline, Unity version, and existing test counts in `specs/008-haetae-wave-cleanup-targeting/quickstart.md`
- [x] T002 Verify `.gitignore` continues to exclude Unity-generated Library, Logs, TestResults, and Builds output in `.gitignore`

**Checkpoint**: The feature starts from the known 111/111 EditMode and 75/75
PlayMode baseline with no generated output entering scope.

---

## Phase 2: Foundational pure rule

**Purpose**: Define one deterministic route-eligibility contract shared by the
runtime adapter and scene-free validation.

- [x] T003 Add failing command, route, and spawn-completion matrix tests in `TelerobotMVP/Assets/Tests/EditMode/RobotTargetingRulesTests.cs`
- [x] T004 Implement the scene-free cleanup route rule in `TelerobotMVP/Assets/Game/Core/Robots/RobotTargetingRules.cs`
- [x] T005 Add Unity metadata for the new core and test sources in `TelerobotMVP/Assets/Game/Core/Robots/RobotTargetingRules.cs.meta` and `TelerobotMVP/Assets/Tests/EditMode/RobotTargetingRulesTests.cs.meta`

**Checkpoint**: The pure matrix proves defend-only cleanup relaxation and
unchanged patrol behavior.

---

## Phase 3: User Story 1 - Finish surviving zombies (Priority: P1)

**Goal**: Make a defending Haetae acquire and eliminate a cross-route survivor
after all scheduled entries have spawned.

**Independent Test**: Exhaust the real queue, leave one valid East Alley survivor
for a North Road defender, and observe acquisition, damage, death, and phase
advance.

### Tests for User Story 1

- [x] T006 [US1] Add a failing all-spawned cross-route cleanup and phase-advance regression in `TelerobotMVP/Assets/Tests/PlayMode/RobotCommandPlayModeTests.cs`

### Implementation for User Story 1

- [x] T007 [US1] Expose authoritative spawn-schedule completion and apply the pure route rule during runtime target acquisition in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [x] T008 [US1] Confirm the deterministic route-rule matrix covers cleanup while the route-local full-session model remains reproducible in `TelerobotMVP/Assets/Tests/EditMode/RobotTargetingRulesTests.cs` and `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`

**Checkpoint**: The exact reported phase-ending stall is fixed in the real scene
and its route transition is covered deterministically.

---

## Phase 4: User Story 2 - Preserve command and safety boundaries (Priority: P1)

**Goal**: Prove that cleanup does not weaken patrol, range, leash, battery, or
availability rules.

**Independent Test**: Query cross-route candidates before/after completion under
defend and patrol, then run the existing command and battery regressions.

### Tests for User Story 2

- [x] T009 [US2] Extend PlayMode coverage for pending-spawn defend, completed-spawn patrol, and outside-leash cleanup candidates in `TelerobotMVP/Assets/Tests/PlayMode/RobotCommandPlayModeTests.cs`
- [x] T010 [US2] Verify same-seed reproducibility and eight-phase completion remain covered in `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`

**Checkpoint**: Only the intended defend-position cleanup route restriction
changes.

---

## Phase 5: Validation and handoff

**Purpose**: Produce a tested Windows player for direct user verification.

- [x] T011 Run complete EditMode and PlayMode suites, Windows x86_64 build, standalone smoke, and record results in `specs/008-haetae-wave-cleanup-targeting/quickstart.md`
- [x] T012 Mark implementation status in `specs/008-haetae-wave-cleanup-targeting/spec.md`, complete all tasks in `specs/008-haetae-wave-cleanup-targeting/tasks.md`, run `git diff --check`, review scope, and commit on `008-haetae-wave-cleanup-targeting`

---

## Dependencies & Execution Order

- Phase 1 establishes the baseline.
- Phase 2 blocks runtime integration.
- US1 depends on the pure rule and delivers the reported fix.
- US2 depends on US1 integration and proves the change is bounded.
- Phase 5 depends on both stories.

## Parallel Opportunities

- T003 and T006 can be authored independently before implementation.
- T007 runtime integration and T008 deterministic regression review can proceed
  independently after T004 is complete.
- T009 and deterministic validation preparation can proceed independently after
  runtime integration.

## Implementation Strategy

1. Lock the pure behavior matrix.
2. Reproduce the scene failure before changing acquisition.
3. Integrate the shared rule in runtime and retain deterministic rule coverage.
4. Prove active-spawn, patrol, range, leash, battery, and phase regressions.
5. Run full suites, build and smoke the Windows player, document, and commit.
