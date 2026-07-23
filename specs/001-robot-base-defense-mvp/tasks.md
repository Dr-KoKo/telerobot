# Tasks: 「텔레 로봇팀, 출격하라」 MVP 수직 슬라이스

**Input**: Design documents from `specs/001-robot-base-defense-mvp/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: 프로젝트 Constitution III–V에 따라 순수 규칙, 장면 통합, 결정론적 시뮬레이션 검증을 필수로 포함한다.

**Organization**: 사용자 스토리별로 구현과 독립 검증을 묶고, 동일 파일을 수정하는 작업은 순차 실행한다.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Unity 버전·패키지·어셈블리 경계를 확정한다.

- [X] T001 Confirm Unity 6000.3.20f1 and required package pins in `TelerobotMVP/ProjectSettings/ProjectVersion.txt` and `TelerobotMVP/Packages/manifest.json`
- [X] T002 Create planned Game/Core, Game/Data, Game/Runtime, Game/Simulation, Game/Scenes, Tests folder and assembly-definition structure under `TelerobotMVP/Assets/`
- [X] T003 Verify Unity-generated artifacts, test results, builds, telemetry, and IDE files are ignored in `TelerobotMVP/.gitignore`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 모든 사용자 스토리가 공유하는 순수 코어, 데이터 자산, 이벤트, 시간·난수, 텔레메트리 토대를 만든다.

**⚠️ CRITICAL**: 이 단계 완료 전에는 사용자 스토리 구현을 시작하지 않는다.

- [X] T004 [P] Define pure configuration records and enums in `TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`
- [X] T005 [P] Define pure session, combatant, route, phase, robot, zombie, and upgrade state models in `TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`
- [X] T006 [P] Define command, player-input, domain-event, telemetry, clock, RNG, and movement contracts in `TelerobotMVP/Assets/Game/Core/Events/GameContracts.cs`
- [X] T007 Implement deterministic RNG, fixed simulation clock, and waypoint progression in `TelerobotMVP/Assets/Game/Core/Rng/DeterministicPrimitives.cs`
- [X] T008 [P] Define ScriptableObject balance, content, route, upgrade, and string assets in `TelerobotMVP/Assets/Game/Data/Definitions/MvpDataDefinitions.cs`
- [X] T009 Implement data-to-core mapping and validation in `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`
- [X] T010 Implement domain event bus and structured telemetry record creation in `TelerobotMVP/Assets/Game/Core/Events/DomainEventBus.cs`
- [X] T011 Implement development JSON-lines telemetry sink in `TelerobotMVP/Assets/Game/Simulation/Telemetry/JsonLinesTelemetrySink.cs`
- [X] T012 Create EditMode and PlayMode test assemblies plus shared test fixtures in `TelerobotMVP/Assets/Tests/`

**Checkpoint**: `Game.Core` has no UnityEngine reference and shared configuration can be supplied entirely from assets.

---

## Phase 3: User Story 1 - 거점 방어 핵심 전투 루프 (Priority: P1) 🎯 MVP

**Goal**: North Road에서 플레이어 사격, 두 해태의 자동 전투, 거점 피해, 페이즈 클리어와 즉시 패배가 가능한 단일 페이즈를 제공한다.

**Independent Test**: North Road만 연 장면에서 러너 몸통 3발/헤드샷 1–2발, 해태 1–2초 처치, 거점 피격 −8, 거점·플레이어 사망 패배, 전멸 클리어를 검증한다.

### Tests for User Story 1

- [X] T013 [P] [US1] Add damage, health, ammo, grenade, and defeat EditMode tests in `TelerobotMVP/Assets/Tests/EditMode/CombatAndHealthTests.cs`
- [X] T014 [P] [US1] Add Phase 1 transition and spawn-completion EditMode tests in `TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`
- [X] T015 [P] [US1] Add Phase 1 boot, combat, clear, and loss PlayMode tests in `TelerobotMVP/Assets/Tests/PlayMode/PhaseOnePlayModeTests.cs`

### Implementation for User Story 1

- [X] T016 [US1] Implement health, damage, headshot, ammo, resupply, and grenade rules in `TelerobotMVP/Assets/Game/Core/Combat/CombatRules.cs`
- [X] T017 [US1] Implement session win/loss and seven-step phase transition rules in `TelerobotMVP/Assets/Game/Core/Phase/PhaseSystem.cs`
- [X] T018 [US1] Implement budget-bounded Runner composition and base-directed target selection in `TelerobotMVP/Assets/Game/Core/Spawning/SpawnAndTargeting.cs`
- [X] T019 [US1] Implement third-person movement, aiming, hitscan fire, reload, and grenade input adapter in `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`
- [X] T020 [US1] Implement waypoint zombie actor, damage hitboxes, base attacks, and death events in `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`
- [X] T021 [US1] Implement Haetae detection, chase, dash/bite, and Runner kill timing adapter in `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`
- [X] T022 [US1] Implement runtime session composition, spawning, combat registration, and win/loss flow in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T023 [US1] Implement minimum combat HUD and data-driven radio caption/audio adapter in `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T024 [US1] Create the central base, North Road, player, two Haetae, charging station, and safe/risky supply greybox in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`

**Checkpoint**: US1 is playable and independently testable as a complete Phase 1 game.

---

## Phase 4: User Story 2 - 해태 로봇 지휘와 배터리 전략 (Priority: P1)

**Goal**: 정확히 세 가지 개별 로봇 명령과 활동별 배터리 소모, 저전력 페널티, 고갈·회복·기지 자동 충전을 제공한다.

**Independent Test**: 명령 메뉴가 3개만 제공되는지, 명령별 상태와 0.3/0.8/2.5초당 소모, Low Power/Critical, 0에서 Disabled, 5초 후 0.5/초 Recovery, 배터리 5에서 자동 복귀, 기지 자동 충전 4/초와 위협 감지 시 Engage 전환을 검증한다.

### Tests for User Story 2

- [X] T025 [P] [US2] Add battery bands, drain, charge, Ripper drain, and recovery state-machine EditMode tests in `TelerobotMVP/Assets/Tests/EditMode/BatteryTests.cs`
- [X] T026 [P] [US2] Add exactly-three command contract and base auto-charge PlayMode tests in `TelerobotMVP/Assets/Tests/PlayMode/RobotCommandPlayModeTests.cs`

### Implementation for User Story 2

- [X] T027 [US2] Implement battery bands, activity drain, penalties, charging, disabled hold, recovery, and return threshold in `TelerobotMVP/Assets/Game/Core/Battery/BatterySystem.cs`
- [X] T028 [US2] Implement exactly-three robot commands and command state transitions in `TelerobotMVP/Assets/Game/Core/Robots/RobotCommandSystem.cs`
- [X] T029 [US2] Integrate commands, charging-station movement, disabled/recovery behavior, and combat lockout in `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`
- [X] T030 [US2] Implement robot selection, route targeting, and three-command quick menu in `TelerobotMVP/Assets/Game/Runtime/HUD/RobotCommandMenu.cs`

**Checkpoint**: US2 battery decisions create an observable charge-versus-defense tradeoff without breaking US1.

---

## Phase 5: User Story 3 - 측면 경로 개방과 페이즈 보상 (Priority: P2)

**Goal**: Phase 1 후 업그레이드 3택 1, 거점 15% 회복, East Alley 개방, 브루저와 두 경로 압박을 제공한다.

**Independent Test**: Phase 1 클리어 후 정확히 세 후보에서 하나를 선택하고, 거점 +150, Phase 2 두 경로와 브루저 −60 피해, 선택 효과 적용을 검증한다.

### Tests for User Story 3

- [X] T031 [P] [US3] Add threat-budget composition, base recovery, upgrade offer/application, and barrier EditMode tests in `TelerobotMVP/Assets/Tests/EditMode/PhaseTwoAndUpgradeTests.cs`
- [X] T032 [P] [US3] Add Phase 2 route-open, radio, Bruiser, and upgrade UI PlayMode tests in `TelerobotMVP/Assets/Tests/PlayMode/PhaseTwoPlayModeTests.cs`

### Implementation for User Story 3

- [X] T033 [US3] Extend deterministic spawn composition for Bruiser minimums and multi-route distribution in `TelerobotMVP/Assets/Game/Core/Spawning/SpawnAndTargeting.cs`
- [X] T034 [US3] Implement 3-of-9 offer, maximum-two selection, all effect handlers, reservation, piercing, and barrier rules in `TelerobotMVP/Assets/Game/Core/Upgrades/UpgradeSystem.cs`
- [X] T035 [US3] Create nine upgrade assets and provisional barrier configuration in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T036 [US3] Implement phase reward selection UI, base recovery, East Alley opening, and next-phase start in `TelerobotMVP/Assets/Game/Runtime/HUD/UpgradeSelectionView.cs`
- [X] T037 [US3] Extend greybox/runtime spawning with East Alley, Bruiser presentation, and route assignment in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`

**Checkpoint**: Phase 1→2 progression and rewards are independently verifiable.

---

## Phase 6: User Story 5 - 전황 인지: HUD·경보·무전 (Priority: P2)

**Goal**: 전체 HUD, 경로 경보, 임계값 시각 경고, 특수 적 표시, 정확한 한국어 무전을 우선순위에 맞게 제공한다.

**Independent Test**: HUD 7요소, 배터리 <25/<10, 거점 ≤30%, 경로 개방, 리퍼 출현 조건을 강제로 발생시켜 경고·콜아웃을 확인한다.

### Tests for User Story 5

- [X] T038 [P] [US5] Add warning-threshold and HUD-priority EditMode tests in `TelerobotMVP/Assets/Tests/EditMode/WarningTests.cs`
- [X] T039 [P] [US5] Add verbatim Korean string and HUD/radio integration PlayMode tests in `TelerobotMVP/Assets/Tests/PlayMode/HudAndRadioPlayModeTests.cs`

### Implementation for User Story 5

- [X] T040 [US5] Implement warning evaluation and event de-duplication in `TelerobotMVP/Assets/Game/Core/Events/WarningSystem.cs`
- [X] T041 [US5] Expand HUD with phase progress, route pressure/minimap, warning priority, battery flashes, base-edge alert, and Ripper icon in `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T042 [US5] Create the exact radio, command, route, and upgrade Korean StringTable data in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`

**Checkpoint**: US5 communicates off-screen threats without replacing or paraphrasing spec strings.

---

## Phase 7: User Story 4 - 종합 국면: 리퍼·메디컬 로봇 (Priority: P3)

**Goal**: Phase 3 세 경로 압박, 메디컬 치료, 리퍼의 로봇 우선 공격·배터리 고갈, 최종 승리를 제공한다.

**Independent Test**: Phase 2 후 두 번째 업그레이드, South Tunnel, 6m/8HPs 치료, 리퍼 타격 추가 배터리 −5와 무력화, Phase 3 전멸 승리를 검증한다.

### Tests for User Story 4

- [X] T043 [P] [US4] Add medical healing, Ripper target priority/battery hit, and final-victory EditMode tests in `TelerobotMVP/Assets/Tests/EditMode/PhaseThreeTests.cs`
- [X] T044 [P] [US4] Add Phase 3 deployment, three-route, Ripper, medical, destruction, and victory PlayMode tests in `TelerobotMVP/Assets/Tests/PlayMode/PhaseThreePlayModeTests.cs`

### Implementation for User Story 4

- [X] T045 [US4] Implement medical heal/destruction and Ripper robot-first targeting rules in `TelerobotMVP/Assets/Game/Core/Robots/MedicalAndRipperRules.cs`
- [X] T046 [US4] Implement non-combat medical zone and Ripper distinct presentation/battery attack adapters in `TelerobotMVP/Assets/Game/Runtime/Robots/MedicalRobotActor.cs`
- [X] T047 [US4] Extend South Tunnel composition, medical deployment, Ripper spawning, and Phase 3 victory flow in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T048 [US4] Extend the greybox with South Tunnel limited sightlines and medical-zone anchor in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`

**Checkpoint**: The full Phase 1→2→3 session ends in spec-compliant victory or immediate defeat.

---

## Phase 8: Deterministic Simulation, Telemetry & Polish

**Purpose**: 동일 코어로 전체 세션을 재현하고, 밸런스 데이터를 남기며, 문서 기준의 완성도를 검증한다.

- [X] T049 Implement fixed-step headless full-session simulation and summary metrics in `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`
- [X] T050 [P] Add same-seed event-stream reproducibility and balance-telemetry tests in `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`
- [X] T051 Instrument all constitution-minimum and spec-specific telemetry events through `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T052 Generate `TelerobotMVP/Assets/Game/Scenes/MVP.unity`, default data assets, and build settings through `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T053 Tune Recovery, grenade, barrier, and phase composition values from simulation output and record them in `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`
- [X] T054 [P] Update controls, run/test instructions, confirmed editor patch, architecture, and telemetry notes in `TelerobotMVP/README.md` and `specs/001-robot-base-defense-mvp/quickstart.md`
- [X] T055 Run all Unity EditMode and PlayMode tests and record results in `TelerobotMVP/TestResults/`
- [X] T056 Validate all eight Korean radio strings byte-for-byte and run the complete quickstart walkthrough in `specs/001-robot-base-defense-mvp/quickstart.md`

---

## Phase 9: Player View, Movement & Session UX

**Purpose**: 1인칭·3인칭 전환, 점프, 안정적인 카메라, 조준 피드백과 기본 세션 제어를 추가한다.

- [X] T057 Extend player input contracts and data-driven camera/jump settings in `TelerobotMVP/Assets/Game/Core/Events/GameContracts.cs`, `TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, and `TelerobotMVP/Assets/Game/Data/Definitions/GameConfigAsset.cs`
- [X] T058 Implement first-person/third-person switching and grounded jump movement in `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`
- [X] T059 Add third-person sphere-cast camera collision and test hooks in `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`
- [X] T060 Add crosshair, hit/headshot feedback, and perspective HUD status in `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T061 Add Esc pause, cursor management, resume, and session restart flow in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T062 Add EditMode configuration safety checks and PlayMode player-experience tests in `TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs` and `TelerobotMVP/Assets/Tests/PlayMode/PlayerExperiencePlayModeTests.cs`
- [X] T063 Regenerate camera/jump defaults, telemetry names, strings, and the MVP scene in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T064 Document beginner opening steps and controls, then verify Unity EditMode 21/21 and PlayMode 13/13 in `TelerobotMVP/README.md` and `specs/001-robot-base-defense-mvp/quickstart.md`

---

## Phase 10: Combat Readability & Movement Feedback

**Purpose**: 플레이어가 이동·피격·탄약·보급 상태를 즉시 이해하도록 핵심 피드백을 보강한다.

- [X] T065 Add Shift sprint input and data-driven sprint multiplier in `TelerobotMVP/Assets/Game/Core/Events/GameContracts.cs`, `TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, and `TelerobotMVP/Assets/Game/Runtime/Player/InputSystemPlayerInput.cs`
- [X] T066 Apply sprint speed and expose runtime movement verification in `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`
- [X] T067 Propagate zombie source position and directional player-damage telemetry in `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs` and `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`
- [X] T068 Add directional damage, low-ammo, and nearby-supply HUD feedback in `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T069 Add reusable nearby-supply detection and successful-resupply results in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T070 Add data validation and PlayMode coverage for sprint, damage direction, supply prompts, and ammo warnings in `TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs` and `TelerobotMVP/Assets/Tests/PlayMode/PlayerExperiencePlayModeTests.cs`
- [X] T071 Regenerate data version `mvp-1.1.0`, document controls, and verify EditMode 22/22 plus PlayMode 16/16 in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/README.md`, and `specs/001-robot-base-defense-mvp/quickstart.md`

---

## Phase 11: Combat Feel & Reload Readability

**Purpose**: 총기 발사·명중·처치·재장전의 시청각 반응을 보강해 전투 결과를 즉시 체감하게 한다.

- [X] T072 Add data-driven recoil, muzzle/impact, procedural sound, hit-flash, death-effect, and marker timing fields in `TelerobotMVP/Assets/Game/Data/Definitions/WeaponDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/ZombieDefinitionAsset.cs`, and `TelerobotMVP/Assets/Game/Data/Definitions/HudConfigAsset.cs`
- [X] T073 Implement deterministic runtime-generated combat clips in `TelerobotMVP/Assets/Game/Runtime/Combat/ProceduralCombatAudio.cs`
- [X] T074 Implement rifle muzzle flash, camera recoil/recovery, impact pulses, and body/headshot sound routing in `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`
- [X] T075 Implement zombie hit flash and delayed shrink/sink death presentation in `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`
- [X] T076 Extend reusable pulse presentation with lifetime, effect name, and optional point light in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`
- [X] T077 Add data-driven hit-marker timing and reload progress UI in `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T078 Add PlayMode verification for shot feedback, headshot audio routing, zombie death presentation, and advancing reload progress in `TelerobotMVP/Assets/Tests/PlayMode/PhaseOnePlayModeTests.cs` and `TelerobotMVP/Assets/Tests/PlayMode/PlayerExperiencePlayModeTests.cs`
- [X] T079 Regenerate data version `mvp-1.2.0`, update documentation, and verify EditMode 22/22 plus PlayMode 18/18 in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/README.md`, and `specs/001-robot-base-defense-mvp/quickstart.md`

---

## Phase 12: Playtest Front Door, Saved Settings & Windows Build

**Purpose**: Unity 편집기를 몰라도 시작 화면에서 설정을 조정하고, 독립 실행형 Windows 플레이테스트 빌드를 실행할 수 있게 한다.

- [X] T080 Add PlayMode verification for persistent settings, main-menu scene flow, preferred starting perspective, and pause-menu settings in `TelerobotMVP/Assets/Tests/PlayMode/MenuAndSettingsPlayModeTests.cs` and `TelerobotMVP/Assets/Tests/PlayMode/PlayerExperiencePlayModeTests.cs`
- [X] T081 Add data-driven preference defaults and validation for sensitivity, audio levels, resolution, fullscreen, and initial perspective in `TelerobotMVP/Assets/Game/Data/Definitions/PlayerSettingsAsset.cs` and `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`
- [X] T082 Implement local PlayerPrefs persistence, settings application, and a reusable settings overlay in `TelerobotMVP/Assets/Game/Runtime/Settings/PlayerPreferences.cs` and `TelerobotMVP/Assets/Game/Runtime/Settings/SettingsOverlay.cs`
- [X] T083 Implement the start/settings/quit main menu and pause/result return-to-menu flow in `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MainMenuController.cs`, `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, and `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T084 Apply saved sensitivity, initial perspective, master volume, and effects volume to player, combat, and radio presentation in `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs` and `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`
- [X] T085 Generate `MainMenu.unity` before `MVP.unity`, add all Korean menu/settings strings as StringTable data, and advance data version `mvp-1.3.0` in `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`
- [X] T086 Add a Windows x64 playtest build command and beginner-readable output guide in `TelerobotMVP/Assets/Game/Editor/WindowsBuildPipeline.cs`
- [X] T087 Regenerate assets/scenes, verify all EditMode/PlayMode tests, create the Windows playtest build, and update `TelerobotMVP/README.md`, `specs/001-robot-base-defense-mvp/quickstart.md`, and data/string contracts

---

## Phase 13: Windows Standalone Rendering Reliability

**Purpose**: Prevent runtime-created greybox materials from losing their shader during Windows player shader stripping, and make the gameplay scene part of the automated standalone smoke test.

- [X] T088 Reference a build-included runtime material template, remove dynamic runtime shader lookup, and add PlayMode regression coverage in `TelerobotMVP/Assets/Game/Data/Definitions/MvpContentCatalog.cs`, `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, and `TelerobotMVP/Assets/Tests/PlayMode/PhaseOnePlayModeTests.cs`
- [X] T089 Add gameplay-scene standalone smoke mode, rebuild Windows x64, and verify its player log has the success marker with no runtime exception

---

## Phase 14: Korean HUD Vertical Metrics

**Purpose**: Prevent Korean glyphs from being vertically clipped by fixed-height IMGUI label rectangles.

- [X] T090 Add PlayMode coverage for overflow clipping, measured line heights, and status-panel row capacity
- [X] T091 Use measured GUIStyle row heights with vertical padding and cursor-based status/route layouts, then rebuild and smoke-test Windows x64

---

## Phase 15: Shareable Playtest Distribution

**Purpose**: Let non-Unity testers download one clean Windows archive, launch it from a beginner-readable guide, and return structured feedback.

- [X] T092 Add EditMode coverage for distribution archive naming, required tester-document templates, and exclusion of `DoNotShip`/debug-symbol files
- [X] T093 Add a non-Development Windows share build plus automatic ZIP packaging, tester guide, itch.io upload checklist, and feedback-form template
- [X] T094 Build and inspect the shareable archive, run standalone D3D12 smoke validation, and document the distribution workflow

---

## Phase 16: Microsoft Store MSIX Distribution

**Purpose**: Package the verified Windows x64 build with the reserved Microsoft Store identity so certified Store installs are publisher-trusted and easy for external testers to install.

- [X] T095 Add EditMode contract coverage for the exact Store identity, four-part package version, package naming, manifest fields, and required visual assets
- [X] T096 Generate a Store icon asset set and implement a reproducible Windows x64 Store staging/MSIX build command using the Partner Center identity
- [X] T097 Validate the Store staging/package output, rerun Unity tests and standalone smoke checks, and document the beginner submission workflow and unsigned-local-package limitation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 즉시 시작 가능.
- **Foundational (Phase 2)**: Setup 완료 후 진행하며 모든 사용자 스토리를 차단한다.
- **US1 (Phase 3)**: Foundational 이후 최소 플레이 가능 슬라이스.
- **US2 (Phase 4)**: US1의 해태 런타임 위에 명령·배터리를 통합한다.
- **US3 (Phase 5)**: US1의 페이즈 전환과 US2의 로봇 압박을 확장한다.
- **US5 (Phase 6)**: US1–US3 이벤트를 표현하며 US4 리퍼 아이콘 연결 지점을 제공한다.
- **US4 (Phase 7)**: US1–US3의 전체 진행과 US5 표시 계층을 통합한다.
- **Simulation/Polish (Phase 8)**: 모든 게임플레이 스토리 완료 후 전체 세션을 검증한다.

### User Story Dependencies

- **US1 (P1)**: Foundational 이후 독립 실행 가능하며 MVP 범위다.
- **US2 (P1)**: US1의 해태 객체를 사용하지만 배터리·명령 규칙은 독립 테스트 가능하다.
- **US3 (P2)**: US1 페이즈 클리어 이벤트에 의존하며 업그레이드·스폰 규칙은 독립 테스트 가능하다.
- **US5 (P2)**: 이벤트 인터페이스에만 의존하고 표시 검증은 독립적이다.
- **US4 (P3)**: Phase 1·2 누적 진행을 전제로 하지만 메디컬·리퍼 규칙은 독립 테스트 가능하다.

### Parallel Opportunities

- T004–T006과 T008은 서로 다른 파일에서 병렬 가능하다.
- 각 스토리의 EditMode/PlayMode 테스트는 서로 다른 파일에서 병렬 작성 가능하다.
- 순수 규칙 구현과 아직 연결되지 않은 런타임 프레젠테이션은 파일이 겹치지 않을 때 병렬 가능하다.
- T050과 T054는 시뮬레이터/문서 파일이 분리되어 병렬 가능하다.

## Parallel Example: User Story 1

```text
Task T013: CombatAndHealthTests.cs에 순수 전투 규칙 테스트 작성
Task T014: PhaseOneTests.cs에 페이즈 전환 테스트 작성
Task T015: PhaseOnePlayModeTests.cs에 장면 통합 테스트 작성
```

## Implementation Strategy

### MVP First

1. Setup과 Foundational을 완료한다.
2. US1을 구현해 단일 Phase 1 슬라이스를 플레이·검증한다.
3. US2를 추가해 핵심 차별 요소인 배터리 결정을 검증한다.

### Incremental Delivery

1. US1: 전투·거점·해태·승패.
2. US2: 명령·배터리·충전.
3. US3: Phase 2·업그레이드·다중 경로.
4. US5: 전황 인지·정확한 한국어 무전.
5. US4: Phase 3·리퍼·메디컬·최종 승리.
6. 결정론적 시뮬레이션과 전체 quickstart 검증.

## Notes

- 모든 밸런스 수치는 데이터 자산에서 공급하고 MonoBehaviour·어댑터·순수 도메인 클래스에 인라인하지 않는다.
- 테스트는 대응 구현보다 먼저 작성하고 실패를 확인한 뒤 구현한다.
- 완료된 작업은 즉시 `[X]`로 갱신한다.
- 최종 아트·보이스는 테스트 전제조건이 아니며 그레이박스·플레이스홀더를 사용한다.

---

## Phase 17: Convergence

**Purpose**: 2026-07-03 설계 문서 정합화에서 확정된 규칙과 현재 Unity 구현 사이의 남은 차이를 닫는다.

- [X] T098 CRITICAL: Implement data-driven per-phase spawn schedules, group sizes, concurrent-alive caps, pause/resume behavior, and deterministic runtime/simulation coverage per FR-055 and Constitution IV (partial)
- [X] T099 CRITICAL: Add the distinct Haetae `Destroyed` state, one-shot damage/destruction telemetry, command/move/attack/charge lockout, rubble presentation, and next-phase HP/battery restoration with EditMode and PlayMode coverage per FR-079, FR-081, and Constitution V (missing)
- [X] T100 CRITICAL: Implement `SimPlayerProfile` assets for Novice/Baseline/Skilled and a fixed-step player/enemy simulation that can produce spec-compliant victory or defeat, evaluate SC-001..004 with the Baseline profile, and prove identical telemetry for the same seed × profile per Constitution IV (partial)
- [X] T101 Implement an individual/select-all robot selection model, input/UI toggle, per-robot command fan-out, destroyed-robot rejection, and PlayMode verification for same-command and divergent-command flows per FR-087 (missing)
- [X] T102 Replace fixed Phase 3 composition and modulo route assignment with data-driven composition ranges, special minimums, trim order, route weights, and per-zombie-type route weights; require Bruiser ≥2, Ripper ≥3, and South-Tunnel Ripper count greater than other routes per FR-034, FR-053, and the threat-budget assumption (contradicts)
- [X] T103 Remove the medical robot from active zombie target candidates, implement incidental adjacent damage while zombies pursue higher-priority targets, and verify destruction disables the medical zone without regeneration per FR-107 and the medical-targeting assumption (contradicts)
- [X] T104 Change upgrade offering to accept `selectedUpgradeIds`, exclude already-selected upgrades from later 3-choice offers, preserve the global nine-definition pool, prevent stacking, and add second-reward regression coverage per FR-112, FR-115, and the upgrade-offer assumption (partial)
- [X] T105 Introduce data-driven `RobotAttackDef` dash/bite damage, cooldowns, ranges, first-dash-per-engagement upgrade behavior, and kill-time validation for Runner/Bruiser bands per FR-074..076 and plan: RobotAttackDef decision (partial)
- [X] T106 Add `AmmoConfig` ownership for start reserve 120, reserve cap 240, FullReserve policy, 1.5-second supply interaction, cooldown, and PhaseResetOnly grenades; replace immediate refill and add safe/risky supply tests per plan: reserve-ammo economy (partial)
- [X] T107 Expand `TelemetryConfig` and telemetry records with required fields including `simProfileId`, sim-clock sampling cadences, threshold/periodic battery policy, `RobotDamaged`/single `RobotDestroyed` events, and deterministic sample-stream tests per Constitution IV and Constitution VIII (partial)
- [X] T108 Refactor configuration ownership so `GameConfig` is session-level, base HP/recovery/warning live only in `BaseConfig`, weapon magazine/reload remain in `WeaponDef`, reserve economy lives in `AmmoConfig`, and mapper validation rejects duplicate or mismatched mirrors per Constitution II and plan: single-source-of-truth decision (partial)
- [X] T109 Reconcile the approved Unity editor baseline across `ProjectVersion.txt`, plan, research, quickstart, README, and task records, keeping the validated version explicit and rerunning the full Unity test/build smoke suite per plan: Unity editor baseline (contradicts)

---

## Phase 18: Runtime Crowd and Robot Combat Fixes

**Purpose**: 묶음 스폰 이후 발생한 좀비 중첩과 경유지 공격 오류를 제거하고 해태 전투 동작을 회귀 검증한다.

- [X] T110 Fix navigation waypoints being treated as attackable base targets so zombies traverse their route before damaging real targets and Haetae can acquire approaching enemies.
- [X] T111 Add data-driven per-zombie route variation, separated spawn placement, local separation steering, and visible Haetae melee impact feedback.
- [X] T112 Make ReturnToBase complete into DefendPosition, add PlayMode coverage for separated groups, waypoint traversal, autonomous Haetae attacks, and rerun the full Unity test/build smoke suite.

---

## Phase 19: Haetae Formation and Base Defense

**Purpose**: 두 해태의 중첩을 방지하고 `거점 사수` 명령이 기지 중심 방어 역할을 유지하도록 한다.

- [X] T113 Add data-driven Haetae separation radius/strength, unique formation slots, avoidance steering, and post-move minimum-distance resolution.
- [X] T114 Make DefendPosition prioritize the same-route zombie closest to the base, enforce a base-centered leash, and return idle defenders to their own rally slots.
- [X] T115 Add PlayMode regressions for same-position separation, shared-target combat, base-priority acquisition, and leash return; rerun EditMode, PlayMode, Windows build, and standalone smoke.

---

## Phase 20: Difficulty and Rifle Handling

**Purpose**: 첫 외부 플레이 감각에 맞춰 페이즈 압박을 낮추고 기본 소총의 조작감을 보강한다.

- [X] T116 Reduce per-phase zombie composition ranges, spawn group sizes, and concurrent-alive caps while preserving threat budgets and special-zombie minimums.
- [X] T117 Add data-driven 0.12-second held-fire cadence and per-shot bounded deterministic random recoil.
- [X] T118 Add EditMode difficulty tuning and PlayMode held-fire/recoil regressions; regenerate balance assets and rerun the full Unity test/build smoke suite.

---

## Phase 21: Barrier Alignment and Haetae Combat Chaining

**Purpose**: 측면 경로 방벽의 시각·충돌 정렬을 바로잡고 해태가 처치 후 가까운 적과 전투를 자연스럽게 이어가도록 한다.

- [X] T119 Align each emergency barrier perpendicular to its route's final base-entry segment, including East Alley and South Tunnel.
- [X] T120 Keep initial DefendPosition route/base-priority targeting, then allow the nearest valid cross-route follow-up target after a kill without exceeding the defend leash.
- [X] T121 Add PlayMode regressions for side-route barrier alignment and post-kill Haetae target chaining; rerun EditMode, PlayMode, Windows build, and standalone smoke.

---

## Phase 22: Stable Resupply and Base Auto-Charging

**Purpose**: 안전 보급지의 간헐적 경계 취소를 제거하고 해태 충전을 기지 기반 자동 흐름으로 단순화한다.

- [X] T122 Use XZ-planar supply range checks and a data-driven 0.75m in-progress exit tolerance; add a PlayMode boundary-drift regression.
- [X] T123 Add a data-driven 6m base charging radius, automatic charging after ReturnToBase/Recovery, and automatic-charge telemetry.
- [X] T124 Remove Charge from the robot command enum, data contract, string table, and Tab menu; verify exactly three commands and base auto-charging in EditMode/PlayMode source.
- [X] T125 Build generated EditMode and PlayMode C# projects with zero warnings/errors and synchronize spec, contracts, quickstart, and README.
- [X] T126 Rerun Unity EditMode/PlayMode, Windows build, and standalone smoke after Hub-authenticated local licensing: EditMode 51/51, PlayMode 38/38, Windows x64 build success, standalone `TELEROBOT_STANDALONE_SMOKE_READY`, exit code 0 (2026-07-22).
- [X] T127 Make Charging acquire cross-route base threats, interrupt into Engage, and retain the target across frames; strengthen the PlayMode state-machine regression and compile with zero warnings/errors.
