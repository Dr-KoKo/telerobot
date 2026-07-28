# Tasks: Character Animation Pass

**Input**: Design documents from `/specs/006-character-animation-pass/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`

## Phase 1: Setup

- [X] T001 Add character-motion profile types and validation to `TelerobotMVP/Assets/Game/Data/Definitions/VisualThemeDefinitionAsset.cs`
- [X] T002 Populate all eight supported role profiles in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs` and `TelerobotMVP/Assets/Game/Data/Assets/VisualTheme.asset`

## Phase 2: Foundational

- [X] T003 Implement the cached, child-only state/pose engine in `TelerobotMVP/Assets/Game/Runtime/Presentation/CharacterMotionDriver.cs`
- [X] T004 Bind or reuse one motion driver after model creation in `TelerobotMVP/Assets/Game/Runtime/Presentation/LowPolyModelFactory.cs`
- [X] T005 Add shared EditMode coverage for profile validation, pose invariants, fallback, LOD synchronization, and repeated binding in `TelerobotMVP/Assets/Tests/EditMode/CharacterMotionEditModeTests.cs`

## Phase 3: User Story 1 - Read Zombie Behavior From Motion (P1)

**Goal**: Runner, Bruiser, and Ripper expose readable locomotion, attack, hit, and death presentation without changing gameplay.

**Independent Test**: Exercise all five states for each zombie role and compare gameplay-root/collider/combat results to the baseline.

- [X] T006 [US1] Refine organic zombie skin weights and regenerate authored source/FBX assets from `TelerobotMVP/ArtSource/Zombies/create_zombie_models.py`
- [X] T007 [US1] Emit attack, hit, and death presentation triggers from existing decisions in `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`
- [X] T008 [US1] Add zombie motion/state and gameplay non-interference tests in `TelerobotMVP/Assets/Tests/PlayMode/CharacterMotionPlayModeTests.cs`

## Phase 4: User Story 2 - Read Haetae Role From Motion (P1)

**Goal**: General, melee, ranged, and balanced Haetae show distinct idle, locomotion, and attack motion.

**Independent Test**: Trigger the same action across each role and verify distinct pose signatures, timing alignment, and per-instance state isolation.

- [X] T009 [US2] Emit role-specific attack and hit presentation triggers from `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`
- [X] T010 [US2] Add Haetae role and independent-instance tests in `TelerobotMVP/Assets/Tests/PlayMode/CharacterMotionPlayModeTests.cs`

## Phase 5: User Story 3 - Fallback and Combat-Scale Safety (P2)

**Goal**: Missing rig targets, LOD changes, repeated attachment, and maximum combat load stay safe.

**Independent Test**: Bind incomplete and dual-LOD models repeatedly, run scaled/paused time and multi-character samples, and confirm no errors or gameplay drift.

- [X] T011 [US3] Complete paused/scaled-time, missing-target, LOD, rebind, and performance validation in the character-motion EditMode/PlayMode tests

## Phase 6: Polish and Validation

- [X] T012 Run the Blender asset-source checks and Unity content builder; update generated metadata where required
- [X] T013 Run all EditMode and PlayMode tests and resolve regressions
- [X] T014 Build and smoke-launch the Windows player following `specs/006-character-animation-pass/quickstart.md`
- [X] T015 Mark completed tasks, record final validation evidence in `specs/006-character-animation-pass/quickstart.md`, and commit the feature on `006-character-animation-pass`

## Dependencies and Execution Order

- T001-T002 establish profile data required by T003-T004.
- T003-T005 block both P1 stories.
- T006-T008 complete zombie behavior independently.
- T009-T010 complete Haetae behavior independently using the shared foundation.
- T011 follows both stories so it can validate cross-character load and isolation.
- T012-T015 are the final integrated gate.
