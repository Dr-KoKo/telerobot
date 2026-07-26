---

description: "Phase 2 해태 성장·전문화 구현 작업 목록"
---

# Tasks: Phase 2 해태 성장·전문화

**Input**: `specs/002-haetae-build-progression/`의 `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Prerequisites**: 활성 기능 경로 `.specify/feature.json`, Unity `6000.3.20f1`, 기존 검증 기준 EditMode 51/51 및 PlayMode 38/38

**Tests**: Constitution III–V에 따라 순수 규칙의 EditMode 테스트, 런타임의 PlayMode 테스트, 밸런스 변경의 결정론적 시뮬레이션 검증을 필수로 포함한다. 각 사용자 스토리의 테스트 작업을 구현보다 먼저 수행하고 실패를 확인한다.

**Organization**: 사용자 스토리별로 독립 구현·검증할 수 있도록 구성한다. 현재 구현된 반동, 스폰 간격, 그룹 크기, 동시 생존 상한은 회귀 기준이며 재조정 대상이 아니다.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 선행 작업 완료 후 다른 파일을 수정하므로 병렬 실행 가능
- **[Story]**: `spec.md`의 사용자 스토리 추적 라벨
- 모든 작업은 수정하거나 생성할 정확한 파일 경로를 포함한다.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 변경 전 기준선을 고정하고 Phase 2 구현이 기존 난이도·사격감을 훼손했는지 비교할 수 있게 한다.

- [X] T001 Unity `6000.3.20f1`에서 기존 EditMode, PlayMode, Windows 빌드 및 standalone smoke 기준을 재실행하고 실제 결과와 실행 날짜를 Prerequisites 및 Automated Validation 절에 기록한다 (`specs/002-haetae-build-progression/quickstart.md`)

**Checkpoint**: 구현 전 기준선이 기록되고 이후 회귀 결과와 비교할 수 있다.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 모든 사용자 스토리가 공유하는 성장 상태, 데이터 계약, ScriptableObject 매핑과 테스트 픽스처를 마련한다.

**⚠️ CRITICAL**: 이 단계가 완료되기 전에는 사용자 스토리 구현을 시작하지 않는다.

- [X] T002 [P] 데이터 계약의 필수 전문화 3종, 양수 XP, 유효 범위·배율과 문자열 키 검증을 먼저 실패하는 테스트로 작성하되 최종 `mvp-2.0.0` cutover 검증은 T050까지 보류한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs`)
- [X] T003 `HaetaeSpecialization`, `DamageSourceKind`, `RobotMovementIntent`, `RobotAttackKind`, ordered specialization pair와 `SimulationRunOptions`를 포함한 성장·전문화·전투 프로필 순수 설정 타입을 추가한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`)
- [X] T004 해태별 `HaetaeProgressionState`, 좀비별 `CombatContributionState`, typed `DamageSource`, 공격 결과/결정 모델을 추가하고 `RobotState`와 `ZombieState`에 합성한다 (`TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`)
- [X] T005 [P] 진행도와 전문화 ScriptableObject 정의를 생성한다 (`TelerobotMVP/Assets/Game/Data/Definitions/HaetaeProgressionDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/HaetaeSpecializationDefinitionAsset.cs`)
- [X] T006 좀비 XP, 시뮬레이션 프로필의 ordered 기본 전문화 pair, 진행도/전문화 카탈로그 참조를 추가한다 (`TelerobotMVP/Assets/Game/Data/Definitions/ZombieDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/SimPlayerProfileAsset.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/MvpContentCatalog.cs`)
- [X] T007 데이터 계약에 따라 진행도·전문화·좀비 XP·시뮬레이션 선택을 순수 설정으로 매핑하고 누락·중복·범위 오류를 거부한다 (`TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`)
- [X] T008 `MvpProjectBuilder`에 75 XP, Runner/Bruiser/Ripper `5/25/20`, 전문화 3종, 필수 문자열 및 진행도 이벤트를 staging하고 자산을 재생성하되 active upgrade flow가 제거되는 T049 전에는 catalog를 `mvp-2.0.0`으로 표시하지 않는다 (`TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeProgression.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeMelee.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeRanged.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeBalanced.asset`, `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`, `TelerobotMVP/Assets/Game/Data/Assets/StringTable.asset`, `TelerobotMVP/Assets/Game/Data/Assets/TelemetryConfig.asset`)
- [X] T009 테스트용 설정에 실제 자산과 동일한 XP, 전문화 프로필, 두 해태의 결정론적 선택 기본값을 추가한다 (`TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`)
- [X] T010 T002의 데이터 매핑 테스트를 통과시키고 builder 재실행 뒤 생성 자산이 동일한 값을 유지하는지 검증한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs`, `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`)

**Checkpoint**: 두 해태가 독립 성장 상태를 가질 수 있고 모든 밸런스 값이 데이터에서 순수 코어로 전달된다.

---

## Phase 3: User Story 1 - 해태별 독립 성장 (Priority: P1) 🎯 MVP

**Goal**: 각 해태가 자신이 피해를 준 좀비의 전체 XP를 독립적으로 받고, 기여하지 않은 해태는 변하지 않으며 레벨 2에서 정확히 정지한다.

**Requirement coverage**: FR-001–FR-010, FR-030–FR-031, FR-033; US1 acceptance 1–5; 관련 edge cases 전체

**Independent Test**: 한 해태만 Runner에 피해를 준 뒤 플레이어가 처치하면 그 해태만 5 XP를 얻는다. 두 해태가 Bruiser에 피해를 주면 각각 25 XP를 받고, 중복 타격·파괴된 기여자·초과 보상에서도 한 번만 지급되고 75 XP에 고정된다.

### Tests for User Story 1

- [X] T011 [P] [US1] 초기 상태, 단독/공동 기여, 전체 보상, player-only 제외, 중복 기여 제거, 파괴된 기여자, XP clamp, 동시 레벨업, 100회 상태 격리를 먼저 실패하는 EditMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeProgressionTests.cs`)
- [X] T012 [P] [US1] 플레이어·해태의 typed damage와 실제 좀비 사망이 올바른 로봇 상태에만 XP를 적용하는 장면 통합 테스트를 먼저 실패하도록 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionPlayModeTests.cs`)
- [X] T013 [P] [US1] 동일 seed/기여/처치 순서의 XP·레벨·이벤트 재현성과 공동 기여 전체 보상을 먼저 실패하는 시뮬레이션 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

### Implementation for User Story 1

- [X] T014 [US1] 기여 기록, ordinal contributor 정렬, 사망 보상 guard, 전체 XP 지급, 75 XP clamp와 XP→레벨→ready 이벤트 결과를 순수 규칙으로 구현한다 (`TelerobotMVP/Assets/Game/Core/Progression/HaetaeProgressionSystem.cs`)
- [X] T015 [US1] 문자열 damage source를 typed `DamageSource`로 교체하고 양수 applied damage만 좀비 기여 상태에 기록한다 (`TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`, `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`, `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`)
- [X] T016 [US1] 좀비 사망 시 기여자별 보상을 처리하고 `haetae_xp_gained`→`haetae_level_reached`→`haetae_specialization_ready`→`zombie_killed` 순서로 publish하는 런타임 연결을 구현한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`)
- [X] T017 [US1] `SimZombie`에 기여 상태와 보상 guard를 추가하고 런타임과 같은 순수 진행도 규칙과 이벤트 순서를 사용한다 (`TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [X] T018 [US1] 새 진행도 이벤트 이름과 `zombie_killed`의 안정 정렬 contributor payload를 JSONL에 반영하고 zero-applied XP 이벤트를 일관되게 생략한다 (`TelerobotMVP/Assets/Game/Simulation/Telemetry/JsonLinesTelemetrySink.cs`, `TelerobotMVP/Assets/Game/Core/Events/DomainEventBus.cs`)
- [X] T019 [US1] T011–T013을 통과시키고 한 해태의 처리로 다른 해태가 변하지 않는 독립 테스트를 100회 성공시킨다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeProgressionTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: 전문화 UI 없이도 해태별 XP와 레벨 2 준비 상태를 독립적으로 검증할 수 있다.

---

## Phase 4: User Story 2 - 레벨 2 전문화 선택 (Priority: P1)

**Goal**: 레벨 2가 된 각 해태에 대해 근거리형·원거리형·균형형을 명시적으로 한 번 선택하고, 같은 역할 또는 서로 다른 역할을 독립적으로 유지한다.

**Requirement coverage**: FR-012–FR-015, FR-032; US2 acceptance 1–6

**Independent Test**: 두 해태를 레벨 2 ready 상태로 만든 뒤 각각 다른 전문화를 선택하고 상태가 섞이지 않는지 확인한다. 새 세션 초기화, 같은 역할 중복, 동일 로봇 재선택 거부를 별도로 검증한다.

### Tests for User Story 2

- [X] T020 [P] [US2] 레벨 제한, 유효 3종, same/mixed 선택, 명시적 선택 전 General 유지, 세션 내 불변, 새 세션 초기화를 먼저 실패하는 순수 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeSpecializationTests.cs`)
- [X] T021 [P] [US2] 대상 로봇 한 대만 선택되고 Disabled/Destroyed 상태에서도 선택이 보존되며 복원 후 적용되는 런타임 통합 테스트를 먼저 실패하도록 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationPlayModeTests.cs`)

### Implementation for User Story 2

- [X] T022 [US2] `SelectSpecialization`의 `Selected`, `NotLevelTwo`, `AlreadySelected`, `InvalidChoice` 결과와 상태 불변 조건을 구현한다 (`TelerobotMVP/Assets/Game/Core/Progression/HaetaeProgressionSystem.cs`)
- [X] T023 [US2] 한 robot ID에만 명시적 선택을 적용하고 성공 시 phase/time/ready duration을 포함한 `haetae_specialization_selected`를 emit하는 controller API를 추가한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`)
- [X] T024 [US2] 각 `SimPlayerProfile`의 ordered 기본 pair와 run-scoped `SimulationRunOptions.SpecializationLoadout` override를 구현하고 ready 시 override 우선으로 즉시 선택하되 spawn RNG를 소비하지 않게 한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/SimPlayerProfileAsset.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [X] T025 [US2] T020–T021을 통과시키고 같은 seed에서 전문화 선택 조합을 바꿔도 spawn stream이 동일한지 검증한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeSpecializationTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: UI를 통하지 않고도 해태별 전문화 선택의 독립성·불변성·결정성을 검증할 수 있다.

---

## Phase 5: User Story 3 - 전투 역할이 달라지는 세 가지 빌드 (Priority: P1)

**Goal**: 전문화마다 공격 방식, 교전 거리와 접근 행동이 달라지고 기존 명령·배터리·파괴 규칙 안에서 강점과 약점을 드러낸다.

**Requirement coverage**: FR-011, FR-016–FR-022; US3 acceptance 1–5

**Independent Test**: 동일 명령·경로·적 배치에서 근거리형은 접근 후 최대 3대 cleave, 원거리형은 6–12 m 유지 및 ranged 공격, 균형형은 접근 중 ranged 후 2 m 이내 melee 전환을 보인다.

### Tests for User Story 3

- [X] T026 [P] [US3] General/Melee/Ranged/Balanced의 movement intent, attack kind, 거리 band, cooldown, cleave target cap, damage·방어·배터리 배율을 먼저 실패하는 순수 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/RobotCombatPolicyTests.cs`)
- [X] T027 [P] [US3] 동일 경로에서 역할별 접근·공격·tracer/cleave cue, battery 0 Disabled, Ripper drain, Destroyed/restore 후 전문화 유지를 먼저 실패하는 PlayMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeCombatRolePlayModeTests.cs`)
- [X] T028 [P] [US3] 9개 ordered loadout의 거리·공격·배터리·피해·Destroyed 지표와 동일 입력 재현성을 먼저 실패하는 결정론적 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

### Implementation for User Story 3

- [X] T029 [US3] 현재 거리와 해태별 활성 프로필로 `Approach/Hold/Retreat` 및 `Dash/Bite/Ranged/None`을 반환하는 순수 전투 정책을 구현한다 (`TelerobotMVP/Assets/Game/Core/Robots/RobotCombatPolicy.cs`)
- [X] T030 [US3] 기존 float damage 반환을 `RobotAttackResult` 기반으로 확장하고 General의 기존 dash/bite cadence를 보존한다 (`TelerobotMVP/Assets/Game/Core/Robots/RobotAttackSystem.cs`)
- [X] T031 [US3] 해태별 전투 정책을 이동·공격에 연결하고 Melee cleave, Ranged direct hit/tracer, Balanced 2 m 전환, incoming/combat-battery 배율과 전문화 시각 cue를 적용한다 (`TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`)
- [X] T032 [US3] 동일 route의 유효 대상만 progress와 ID로 안정 정렬해 cleave하고 ranged tracer/pulse를 생성하는 런타임 query/presentation helper를 추가한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`)
- [X] T033 [US3] `SimRobotRuntime`의 route position/target distance를 추가하고 shared 전투 정책으로 접근·유지·후퇴, cleave, 배율과 로봇별 damage/battery/Disabled/Destroyed summary를 계산한다 (`TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [X] T034 [US3] T026–T028을 통과시키고 기존 세 명령, battery/charge, Ripper 및 Destroyed 복원 테스트가 모두 유지되는지 확인한다 (`TelerobotMVP/Assets/Tests/EditMode/RobotCombatPolicyTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeCombatRolePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: 동일 명령에서도 세 역할의 전투 행동과 trade-off가 자동 테스트와 greybox 장면에서 구분된다.

---

## Phase 6: User Story 4 - 성장 상태 확인과 비차단 선택 (Priority: P2)

**Goal**: 플레이어가 해태별 레벨·XP·현재 역할·선택 대기를 확인하고 전투를 멈추지 않은 채 원하는 로봇을 전문화한다.

**Requirement coverage**: FR-023–FR-026; US4 acceptance 1–5; SC-004–SC-005의 구현 전제

**Independent Test**: 서로 다른 XP와 ready 상태의 두 해태를 표시하고 `B`로 패널을 열어 대상 한 대에만 선택한다. 패널을 열어 둔 동안 `Time.timeScale == 1`이며 좀비·해태·스폰·페이즈가 계속 진행된다.

### Tests for User Story 4

- [X] T035 [P] [US4] 두 HUD row의 ID별 level/XP 진행도/role/ready 표시, 알림 지속, 정확한 문자열을 먼저 실패하는 PlayMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`)
- [X] T036 [P] [US4] `B` 패널 열기/닫기, ready robot 전환, one-target 선택, same/mixed 표시, `Time.timeScale == 1`, phase 진행, click-through 차단을 먼저 실패하는 PlayMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationUiPlayModeTests.cs`)

### Implementation for User Story 4

- [X] T037 [US4] `일반형`, `근거리형`, `원거리형`, `균형형`, level/XP/ready/panel 설명과 `B` 입력 힌트를 StringTable builder와 생성 자산에 추가한다 (`TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/StringTable.asset`)
- [X] T038 [US4] 각 해태 row에 level, XP 진행 상태, 역할, ready highlight와 데이터 기반 4초 알림을 표시한다 (`TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`)
- [X] T039 [US4] 대상 robot ID, 세 역할 설명/trade-off, ready robot 전환, 닫기/재열기 및 성공 후 다음 ready robot 이동을 제공하는 비차단 패널을 구현한다 (`TelerobotMVP/Assets/Game/Runtime/HUD/HaetaeSpecializationView.cs`)
- [X] T040 [US4] `Player/Specialization`의 keyboard `B` 바인딩과 입력 프레임을 추가하고 Pause보다 후, 일반 combat 입력 차단보다 먼저 panel toggle을 처리한다 (`TelerobotMVP/Assets/InputSystem_Actions.inputactions`, `TelerobotMVP/Assets/Game/Core/Events/GameContracts.cs`, `TelerobotMVP/Assets/Game/Runtime/Player/InputSystemPlayerInput.cs`, `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`)
- [X] T041 [US4] 전문화 view를 생성·연결하고 command/pause/settings/전문화 패널의 cursor lock/visibility를 controller 한 곳에서 조정하며 “전체 선택”이 일괄 전문화를 유발하지 않게 한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Runtime/HUD/RobotCommandMenu.cs`)
- [X] T042 [US4] T035–T036을 통과시키고 선택을 미룬 ready 상태가 phase 변경 및 Disabled/Destroyed를 거쳐 유지되는지 검증한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationUiPlayModeTests.cs`)

**Checkpoint**: 전투를 멈추지 않고 어느 해태가 성장했는지 식별하고 한 대씩 전문화할 수 있다.

---

## Phase 7: User Story 5 - 페이즈 보상에서 지속 성장으로 전환 (Priority: P2)

**Goal**: Phase 1/2 종료 시 기존 3택 업그레이드 없이 거점 회복·경로 개방·다음 페이즈가 즉시 이어진다.

**Requirement coverage**: FR-027–FR-029; US5 acceptance 1–3; SC-009

**Independent Test**: Phase 1과 Phase 2를 각각 클리어했을 때 `NextPhase`가 반환되고 old upgrade UI/event 없이 base recovery, route opening, next phase start가 일어난다. ready 선택은 페이즈 중과 전환 후 모두 가능하다.

### Tests for User Story 5

- [X] T043 [P] [US5] Phase 1/2 clear가 `NextPhase`, Phase 3 clear가 `Victory`이고 base recovery와 ready 상태가 유지됨을 먼저 실패하는 EditMode 테스트로 변경한다 (`TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseTwoAndUpgradeTests.cs`)
- [X] T044 [P] [US5] Phase 1/2에서 upgrade view가 생성·표시되지 않고 clear radio, 새 route, 다음 phase가 이어지는 테스트를 먼저 실패하도록 변경한다 (`TelerobotMVP/Assets/Tests/PlayMode/PhaseOnePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseTwoPlayModeTests.cs`)
- [X] T045 [P] [US5] 시뮬레이션에서 upgrade offer/selection RNG 없이 3개 phase가 진행되고 `upgrade_selected`가 없음을 먼저 실패하는 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

### Implementation for User Story 5

- [X] T046 [US5] Phase 1/2 생존 clear가 항상 `NextPhase`를 반환하게 하고 active flow에서 `AwaitingUpgrade`와 `SessionState.SelectedUpgrades`를 읽거나 쓰지 않게 한다 (`TelerobotMVP/Assets/Game/Core/Phase/PhaseSystem.cs`, `TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`)
- [X] T047 [US5] `UpgradeSystem`/`UpgradeSelectionView` 생성과 reward gate를 제거하고 phase clear 샘플·radio 후 즉시 `BeginPhase(current + 1)`을 호출한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`)
- [X] T048 [US5] 시뮬레이터의 upgrade offer/apply/policy와 관련 RNG 소비를 제거하고 phase clear 다음 step에 다음 phase를 시작한다 (`TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [X] T049 [US5] active catalog/mapper/builder/test fixture에서 9개 upgrade 요구와 mapping을 제거하고 legacy upgrade `.asset` 파일은 비활성 상태로 보존한 뒤, 같은 regeneration에서 catalog `dataVersion`을 원자적으로 `mvp-2.0.0`으로 전환한다 (`TelerobotMVP/Assets/Game/Data/Definitions/MvpContentCatalog.cs`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`, `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`)
- [X] T050 [US5] T002의 최종 version 계약과 T043–T045를 통과시키고 `mvp-2.0.0` catalog/JSONL에 active upgrade dependency 및 `upgrade_selected`가 없으며 `haetae_specialization_selected`가 성장 선택 이벤트로 기록되는지 검증한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseTwoAndUpgradeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseOnePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseTwoPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: old upgrade 시스템은 비활성이고 세션의 빌드 선택은 해태별 전문화만 남는다.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 전체 telemetry, 밸런스, 회귀, 플레이테스트와 배포 가능한 Windows 빌드를 검증한다.

- [X] T051 [P] 진행도 이벤트 payload/envelope, lethal hit ordering, duplicate death guard, `robot_auto_charge_started` 및 `upgrade_selected` 예외를 계약 테스트로 고정한다 (`TelerobotMVP/Assets/Tests/EditMode/ProgressionTelemetryTests.cs`, `TelerobotMVP/Assets/Game/Simulation/Telemetry/JsonLinesTelemetrySink.cs`)
- [X] T052 [P] 반동·총구/명중 피드백, 그룹 크기 `3–4/3–5/4–6`, cap `15/20/24`, 연속 spawn, 세 명령, battery/Destroyed/Ripper/medical 회귀 검증을 기존 테스트에 보강한다 (`TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseThreeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PlayerExperiencePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/RobotCommandPlayModeTests.cs`)
- [X] T053 20개 balance seed와 9개 ordered run loadout을 실행해 SC-002, SC-003, SC-010을 판정하고 SC-008은 가속 모델로 판정할 수 없음을 확인하며, 필요한 경우 spawn/recoil을 건드리지 않고 XP·전문화 값을 조정하고 builder 기본값, serialized assets, test fixture와 문서 기준값을 함께 동기화한다 (`TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeProgression.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeMelee.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeRanged.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeBalanced.asset`, `TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`, `specs/002-haetae-build-progression/plan.md`, `specs/002-haetae-build-progression/contracts/data-config.contract.md`, `specs/002-haetae-build-progression/quickstart.md`)
- [ ] T054 quickstart의 수동 시나리오 A–E를 실행하고 최소 30회 선택으로 SC-004–SC-007의 식별 시간·역할 인지·배치 변화·선택 비율을 기록한다 (`specs/002-haetae-build-progression/playtest-report.md`, `specs/002-haetae-build-progression/quickstart.md`)
- [X] T055 전체 EditMode/PlayMode suite, Windows build, standalone smoke를 실행하고 신규 정확한 통과 개수와 로그 위치를 기록한다 (`specs/002-haetae-build-progression/quickstart.md`, `TelerobotMVP/Assets/Tests/EditMode/`, `TelerobotMVP/Assets/Tests/PlayMode/`)
- [X] T056 active spec·plan·contracts 대비 FR/acceptance/edge-case 추적성, player-facing 문자열, telemetry 예외, out-of-scope 금전·상점·플레이어 무기 미포함을 최종 감사한다 (`specs/002-haetae-build-progression/spec.md`, `specs/002-haetae-build-progression/plan.md`, `specs/002-haetae-build-progression/contracts/validation-scenarios.contract.md`, `specs/002-haetae-build-progression/tasks.md`)
- [X] T057 quickstart의 SC-008 수동 시간 측정 절차로 Windows Baseline 세션을 Phase 1 조작 가능 시점부터 `Victory` 또는 `Defeat`까지 중단 없이 완료하고, 10–15분 목표 충족 여부와 결과를 기록한다 (`specs/002-haetae-build-progression/playtest-report.md`, `specs/002-haetae-build-progression/quickstart.md`)

---

## Phase 9: User Story 6 - 전문화 빌드를 활용하는 후반 페이즈 (Priority: P1)

**Goal**: Phase 1–3의 속도감을 그대로 유지한 채 Phase 4–8 후반 공세를 추가해 전체 세션을 약 10분 15초로 확장한다.

**Requirement coverage**: FR-034–FR-039; US6 acceptance 1–5; SC-008, SC-011–SC-012

**Independent Test**: Phase 1–3 데이터가 기존 값과 정확히 같고, Phase 3/7 클리어는 다음 페이즈로 이어지며 Phase 8 클리어만 승리한다. Phase 4–8은 세 경로, 기존 적, group 4–6, interval 3초, cap 24를 사용하고 전체 target 합계가 615초다.

- [X] T058 [US6] Phase 1–3 구성·간격·그룹·cap 불변, 정확히 8개 연속 phase, Phase 3→4·Phase 7→8 `NextPhase`, Phase 8 전용 `Victory`, late-phase route-open 중복 방지를 먼저 실패하는 EditMode/PlayMode/결정론 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseTwoAndUpgradeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseThreeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseThreePlayModeTests.cs`)
- [X] T059 [US6] `opensNewRoute`, 8개 연속 phase 검증, target 합계 `615s`, Phase 4–8 composition/route emphasis를 data definition·mapper·builder·test fixture에 구현하고 생성 자산을 갱신한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/PhaseDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`, `TelerobotMVP/Assets/Game/Data/Assets/Phase4.asset`, `TelerobotMVP/Assets/Game/Data/Assets/Phase5.asset`, `TelerobotMVP/Assets/Game/Data/Assets/Phase6.asset`, `TelerobotMVP/Assets/Game/Data/Assets/Phase7.asset`, `TelerobotMVP/Assets/Game/Data/Assets/Phase8.asset`, `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`)
- [X] T060 [US6] final phase 숫자 `3` 하드코딩을 제거하고 runtime·deterministic simulation이 검증된 phase catalog 전체를 순회하며 실제 신규 경로가 있는 phase에서만 route-open 이벤트를 내도록 구현한다 (`TelerobotMVP/Assets/Game/Core/Phase/PhaseSystem.cs`, `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [X] T061 [US6] T058 테스트와 전체 EditMode/PlayMode 회귀, builder regeneration, Windows build, standalone smoke를 통과시키고 8-phase 결정론·phase transition 결과를 문서에 기록한다 (`TelerobotMVP/Assets/Tests/EditMode/`, `TelerobotMVP/Assets/Tests/PlayMode/`, `specs/002-haetae-build-progression/quickstart.md`)
- [ ] T062 [US6] 새 Windows Baseline 세션을 중단 없이 플레이해 Phase 1 조작 가능 시점부터 Phase 8 `Victory` 또는 조기 `Defeat`까지 시간을 기록하고 SC-008의 10–15분 목표를 재판정한다 (`specs/002-haetae-build-progression/playtest-report.md`, `specs/002-haetae-build-progression/quickstart.md`)

---

## Phase 10: User Story 1/6 Remediation - 지속 레벨과 정확한 후반 안내 (Priority: P1)

**Goal**: 레벨 2 이후에도 75 XP마다 해태 레벨이 계속 상승하고, Phase 3의 메디컬 로봇 투입 안내가 Phase 4–8에서 반복되지 않게 한다.

**Requirement coverage**: FR-008, FR-040–FR-042; US1 acceptance 5–6; US6 acceptance 6; SC-013–SC-014

**Independent Test**: 95 XP인 해태가 25 XP를 받으면 120 XP와 레벨 2가 되고, 이후 80 XP를 더 받으면 200 XP와 레벨 3이 된다. 전문화 선택 가능 이벤트는 최초 레벨 2에서만 발생한다. Phase 3→4 전환 기록에는 `radio.phase3`가 한 번만 있고 Phase 4부터는 각 phase 전용 키가 사용된다.

- [X] T063 [US1] 누적 XP 경계 초과 보존, 레벨 3+, 레벨 3+ 전문화 선택, one-shot ready 이벤트와 지속 성장 HUD를 먼저 실패하는 EditMode/PlayMode/결정론 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeProgressionTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/HaetaeSpecializationTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/ProgressionTelemetryTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)
- [X] T064 [US1] 진행도 데이터 계약을 `experiencePerLevel = 75`로 전환하고 cumulative XP에서 레벨을 계산하며 최초 level-2 crossing만 specialization unlock으로 반환하도록 순수 core·mapper·builder·fixture·asset을 구현한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, `TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`, `TelerobotMVP/Assets/Game/Core/Progression/HaetaeProgressionSystem.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/HaetaeProgressionDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeProgression.asset`)
- [X] T065 [US1] runtime·simulation telemetry가 모든 level-up을 기록하되 `haetae_specialization_ready`는 level 2 최초 1회만 기록하고 HUD가 현재 레벨 구간 XP 진행도를 표시하도록 연결한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`, `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`)
- [X] T066 [US6] Phase 1–8 전용 radio 문자열을 데이터에 추가하고 현재 phase 번호의 키를 사용해 Phase 3의 메디컬 투입 안내가 Phase 4–8에서 재생되지 않는 PlayMode 검증을 구현한다 (`TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/StringTable.asset`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HudAndRadioPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseThreePlayModeTests.cs`)
- [X] T067 [US7] 레벨 3+ 포인트 누적, 전문화 전 소비 거부, 세 강화의 반복 선택·해태별 격리·정확한 수치 보정을 먼저 실패하는 EditMode/PlayMode/결정론 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeProgressionTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/RobotCombatPolicyTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationUiPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)
- [X] T068 [US7] 강화 enum·선택 결과·포인트·등급 상태와 데이터 기반 등급당 보정/하한을 core·definition·mapper·builder·fixture·asset에 구현한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, `TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`, `TelerobotMVP/Assets/Game/Core/Progression/HaetaeProgressionSystem.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/HaetaeProgressionDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeProgression.asset`)
- [X] T069 [US7] 화력·장갑·효율 등급을 runtime과 deterministic simulation의 공격 피해·받는 피해·전투 배터리 소모에 동일 순서로 적용한다 (`TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`, `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [X] T070 [US7] 기존 비차단 `B` 패널을 전문화/강화 공용 빌드 패널로 확장하고 포인트·등급 HUD, `haetae_mastery_point_gained`·`haetae_mastery_selected` telemetry를 연결한다 (`TelerobotMVP/Assets/Game/Runtime/HUD/HaetaeSpecializationView.cs`, `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`, `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`)
- [X] T071 [US7] builder regeneration, 전체 EditMode/PlayMode 회귀, Windows build, standalone smoke를 통과시키고 지속 레벨·강화 선택·후반 radio의 새 정확한 결과를 quickstart에 기록한다 (`TelerobotMVP/Assets/Tests/`, `specs/002-haetae-build-progression/quickstart.md`)

---

## Phase 11: User Story 7 Remediation - 공격 속도와 빌드 패널 안정성 (Priority: P1)

**Goal**: 반복 강화에 공격 속도를 추가하고, 마지막 포인트 소비로 패널 대상이 사라지는 같은 GUI 프레임에서도 예외 없이 선택 흐름을 종료한다.

**Independent Test**: 전문화된 해태가 공격 속도 1등급을 선택하면 그 해태의 돌진·물기·원거리 공격 간격만 10% 줄어든다. 마지막 포인트를 소비해 패널이 닫힌 뒤 같은 GUI 프레임이 남은 선택지를 그리려 해도 예외가 발생하지 않는다.

- [X] T072 [US7] 공격 속도 enum/등급/데이터 보정·50% 하한과 4종 강화 선택, 마지막 포인트 UI 프레임 안전성을 먼저 실패하는 EditMode/PlayMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeProgressionTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/RobotCombatPolicyTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationUiPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs`)
- [X] T073 [US7] 공격 속도 상태·선택·데이터 매핑·빌더/자산·문자열·telemetry/HUD를 4종 강화로 확장한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, `TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`, `TelerobotMVP/Assets/Game/Core/Progression/HaetaeProgressionSystem.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/HaetaeProgressionDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`, `TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`)
- [X] T074 [US7] 공격 간격 보정을 runtime과 deterministic simulation의 모든 해태 공격에 동일하게 적용하고, 공용 빌드 패널의 마지막 포인트 소비 후 추가 GUI 접근을 안전하게 중단한다 (`TelerobotMVP/Assets/Game/Core/Robots/RobotAttackSystem.cs`, `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`, `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`, `TelerobotMVP/Assets/Game/Runtime/HUD/HaetaeSpecializationView.cs`)
- [X] T075 [US7] builder regeneration, 전체 EditMode/PlayMode 회귀, Windows build, standalone smoke를 통과시키고 공격 속도/패널 예외 수정 결과를 quickstart에 기록한다 (`TelerobotMVP/Assets/Tests/`, `specs/002-haetae-build-progression/quickstart.md`)

---

## Phase 12: User Story 4 Remediation - 해태 경험치 상태 바 (Priority: P2)

**Goal**: HUD의 해태별 누적 경험치 숫자 비율을 제거하고, 현재 레벨 구간의 진행률을 독립된 상태 바로 표시한다.

**Requirement coverage**: FR-023; US4 acceptance 1; SC-016

**Independent Test**: 서로 다른 현재 레벨 구간 경험치를 가진 두 해태의 HUD 텍스트에는 현재/필요 XP 숫자 비율이 없고, 각 상태 바는 자기 진행률을 표시한다. 레벨 경계에 정확히 도달한 해태의 바는 새 구간의 0%로 갱신된다.

- [X] T076 [US4] 해태별 현재 레벨 경험치 진행률, 숫자 XP 제거, 레벨 경계 초기화를 검증하는 PlayMode 테스트를 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`)
- [X] T077 [US4] 로봇 HUD 행을 현재 레벨 구간 경험치 상태 바로 확장하고 숫자 XP 비율을 제거하며 패널 높이와 ready 강조 영역을 새 행 높이에 맞춘다 (`TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`)
- [X] T078 [US4] 전체 EditMode/PlayMode 회귀, Windows build, standalone smoke를 통과시키고 정확한 결과와 수동 확인 절차를 quickstart에 기록한다 (`TelerobotMVP/Assets/Tests/`, `specs/002-haetae-build-progression/quickstart.md`)

---

## Phase 13: User Story 4 Remediation - 고정 행과 수치 포함 체력·경험치 바 (Priority: P2)

**Goal**: 선택 표식을 본문에서 분리해 모든 해태 행의 줄바꿈과 정렬을 통일하고, 실제 체력과 현재 레벨 경험치 수치를 각 상태 바 내부에 표시한다.

**Requirement coverage**: FR-023, FR-051–FR-052; US4 acceptance 1; SC-016–SC-017

**Independent Test**: 선택 대상을 바꿔도 두 해태 행은 각각 기본 정보·현재 상태·강화 정보의 같은 3줄을 유지한다. 서로 다른 체력과 경험치를 설정하면 각 행의 HP 바는 `현재 / 최대`, XP 바는 `현재 레벨 구간 XP / 구간 필요 XP`를 내부에 표시하고 채움 비율도 해당 상태와 일치한다.

- [X] T079 [US4] 선택/미선택 동일 3줄 구조, 해태별 체력 진행률·내부 수치, 현재 레벨 XP 내부 수치와 경계 초기화를 검증하는 PlayMode 테스트를 먼저 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`)
- [X] T080 [US4] 선택 표식을 고정 열로 분리하고 각 해태 행을 3줄 본문과 수치 포함 HP/XP 상태 바로 재배치하며 필요한 데이터 문자열을 추가한다 (`TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/StringTable.asset`)
- [X] T081 [US4] 전체 EditMode/PlayMode 회귀, Windows build, standalone smoke를 통과시키고 정확한 결과와 수동 확인 절차를 quickstart에 기록한다 (`TelerobotMVP/Assets/Tests/`, `specs/002-haetae-build-progression/quickstart.md`)

---

## Phase 14: User Story 4 Remediation - 수치 포함 배터리 상태 바 (Priority: P2)

**Goal**: 해태의 배터리 숫자를 상태 문구에서 분리하고, 실제 잔량과 기존 경고 임계값을 표현하는 독립된 상태 바로 표시한다.

**Requirement coverage**: FR-051, FR-053; US4 acceptance 1; SC-018

**Independent Test**: 선택 대상을 바꿔도 두 해태 행은 같은 3줄 본문을 유지한다. 서로 다른 배터리 잔량을 설정하면 각 행의 배터리 바는 `현재 / 최대`를 내부에 표시하고 채움 비율이 해당 해태의 실제 잔량과 일치하며, 정상·저전력·위험 구간은 기존 임계값에 맞게 구분된다.

- [X] T082 [US4] 해태별 배터리 진행률·내부 수치와 상태 문구에서의 숫자 분리를 검증하는 PlayMode 테스트를 먼저 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`)
- [X] T083 [US4] 각 해태 행에 수치 포함 배터리 상태 바를 추가하고 기존 임계값에 따른 색을 적용하며 패널·행 높이를 재배치한다 (`TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`)
- [X] T084 [US4] 전체 EditMode/PlayMode 회귀와 Windows·배포·Store 빌드 및 standalone smoke를 통과시키고 결과와 수동 확인 절차를 quickstart에 기록한다 (`TelerobotMVP/Assets/Tests/`, `specs/002-haetae-build-progression/quickstart.md`)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: 즉시 시작 가능
- **Phase 2 Foundational**: T001 이후 진행하며 모든 사용자 스토리를 차단
- **US1 (Phase 3)**: Foundational 완료 후 시작; Phase 2 기능의 최소 검증 단위
- **US2 (Phase 4)**: Foundational과 US1의 progression state/system에 의존
- **US3 (Phase 5)**: US2의 per-robot specialization state에 의존
- **US4 (Phase 6)**: US1의 XP/ready 이벤트와 US2의 선택 API에 의존하며, US3와 병렬 구현 가능
- **US5 (Phase 7)**: Foundational 완료 후 core/runtime/simulation의 upgrade 제거를 병렬 진행할 수 있으나 최종 통합은 US1–US2 이벤트 흐름과 함께 검증
- **Phase 8 Polish**: 목표로 하는 US1–US5 완료 후 진행
- **US6 session extension**: 기존 T057 실측과 사용자 승인 후 진행하며 Phase 1–3 회귀 기준을 변경하지 않음
- **US1/US6/US7 remediation**: US6 구현 후 진행하며 T063/T067 테스트를 먼저 실패시킨 뒤 T064–T066과 T068–T070을 통합하고 T071에서 전체 회귀를 수행

### User Story Dependency Graph

```text
Setup → Foundational → US1 → US2 → US3
                         └──→ US4
                   └────────→ US5
US1 + US2 + US3 + US4 + US5 → Polish
```

### Within Each User Story

1. 테스트 작업을 먼저 작성하고 의도한 이유로 실패하는지 확인한다.
2. 순수 core 모델/규칙을 구현한다.
3. Unity runtime adapter와 data mapping을 연결한다.
4. deterministic simulation에 동일 규칙을 연결한다.
5. 스토리 테스트와 기존 회귀 테스트를 통과시킨다.

---

## Parallel Opportunities

- T002와 T005는 서로 다른 data-test/definition 파일에서 병렬 진행 가능
- US1의 T011–T013, US2의 T020–T021, US3의 T026–T028, US4의 T035–T036, US5의 T043–T045는 각 스토리 안에서 테스트를 병렬 작성 가능
- Foundational 완료 후 US5의 phase-upgrade 제거는 US1 구현과 병렬 착수 가능
- US2 선택 API가 안정되면 US3 전투 정책과 US4 UI를 서로 다른 파일군에서 병렬 구현 가능
- T051과 T052는 telemetry와 회귀 테스트 파일이 분리되어 병렬 실행 가능

## Parallel Example: User Story 3

```text
Task T026: RobotCombatPolicy 순수 테스트 작성
Task T027: 역할별 PlayMode 테스트 작성
Task T028: 9개 ordered loadout 결정론적 테스트 작성
```

테스트가 실패하는 상태를 확인한 뒤:

```text
Task T029–T030: 순수 전투 정책과 공격 결과 구현
Task T031–T032: Unity 해태 actor와 runtime query/presentation 연결
Task T033: deterministic simulator 거리/전투 모델 연결
```

## Parallel Example: User Story 4 and User Story 5

```text
Developer A: T035–T042 성장 HUD와 비차단 전문화 UI
Developer B: T043–T050 phase-end upgrade 제거와 즉시 phase 전환
```

---

## Implementation Strategy

### MVP First: Independent Progression

1. T001로 변경 전 기준선을 고정한다.
2. T002–T010으로 shared data/core 기반을 완성한다.
3. T011–T019로 US1 독립 XP와 레벨 2 ready 상태를 구현한다.
4. **STOP AND VALIDATE**: scene 없이 독립 성장 규칙, runtime typed damage, deterministic XP/event를 검증한다.

이 단계는 전문화 전투가 없어도 기존 phase-end 보상 대신 지속 성장의 핵심 가설을 검증하는 최소 범위다.
이 checkpoint는 개발 중간 상태이며 release 가능한 `mvp-2.0.0`으로 간주하지 않는다. 최종 version cutover는 T049–T050에서만 수행한다.

### Incremental Delivery

1. **US1**: 독립 XP와 레벨 2 ready
2. **US2**: 해태별 불변 전문화 선택
3. **US3**: 세 역할의 실제 전투 차이
4. **US4**: 전투를 멈추지 않는 HUD/선택 UX
5. **US5**: old upgrade gate 제거와 phase 연속성
6. **Polish**: telemetry, 20-seed balance, 30-choice playtest, Windows 회귀

### Scope Guard

- 돈, 상점, 시간 수입, 플레이어 XP/무기/근접 빌드, 타임어택, 추가 해태, 분기형 스킬 트리·활성 강화 능력, 영구 저장을 구현하지 않는다.
- 기존 recoil 수치와 spawn interval/group/cap은 테스트 기준으로만 다루며 전문화 밸런싱을 이유로 변경하지 않는다.
- legacy upgrade 자산은 active catalog와 player flow에서 제외하되 이번 data-version에서는 삭제 작업을 만들지 않는다.

---

## Notes

- `[P]` 작업은 선행 checkpoint 완료 후 다른 파일을 수정할 때만 병렬 실행한다.
- 각 사용자 스토리는 해당 `Independent Test`를 단독으로 재현할 수 있어야 한다.
- `MvpProjectBuilder`가 생성 자산을 덮어쓰므로 definition, builder, mapper, serialized asset, test fixture를 함께 갱신한다.
- player-facing 전문화 이름은 StringTable에서 정확히 `근거리형`, `원거리형`, `균형형`을 사용한다.
- simulation specialization 선택은 run-scoped loadout override를 우선하며 spawn RNG를 소비하지 않는다.
- final art/audio는 테스트를 차단하지 않으며 greybox cue로 먼저 검증한다.
