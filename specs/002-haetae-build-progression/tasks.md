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

- [ ] T001 Unity `6000.3.20f1`에서 기존 EditMode, PlayMode, Windows 빌드 및 standalone smoke 기준을 재실행하고 실제 결과와 실행 날짜를 Prerequisites 및 Automated Validation 절에 기록한다 (`specs/002-haetae-build-progression/quickstart.md`)

**Checkpoint**: 구현 전 기준선이 기록되고 이후 회귀 결과와 비교할 수 있다.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 모든 사용자 스토리가 공유하는 성장 상태, 데이터 계약, ScriptableObject 매핑과 테스트 픽스처를 마련한다.

**⚠️ CRITICAL**: 이 단계가 완료되기 전에는 사용자 스토리 구현을 시작하지 않는다.

- [ ] T002 [P] 데이터 계약의 필수 전문화 3종, 양수 XP, 유효 범위·배율, 문자열 키, `mvp-2.0.0` 검증을 먼저 실패하는 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs`)
- [ ] T003 `HaetaeSpecialization`, `DamageSourceKind`, `RobotMovementIntent`, `RobotAttackKind`와 성장·전문화·전투 프로필 순수 설정 타입을 추가한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`)
- [ ] T004 해태별 `HaetaeProgressionState`, 좀비별 `CombatContributionState`, typed `DamageSource`, 공격 결과/결정 모델을 추가하고 `RobotState`와 `ZombieState`에 합성한다 (`TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`)
- [ ] T005 [P] 진행도와 전문화 ScriptableObject 정의를 생성한다 (`TelerobotMVP/Assets/Game/Data/Definitions/HaetaeProgressionDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/HaetaeSpecializationDefinitionAsset.cs`)
- [ ] T006 좀비 XP, 시뮬레이션의 해태별 전문화 선택, 진행도/전문화 카탈로그 참조를 추가한다 (`TelerobotMVP/Assets/Game/Data/Definitions/ZombieDefinitionAsset.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/SimPlayerProfileAsset.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/MvpContentCatalog.cs`)
- [ ] T007 데이터 계약에 따라 진행도·전문화·좀비 XP·시뮬레이션 선택을 순수 설정으로 매핑하고 누락·중복·범위 오류를 거부한다 (`TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`)
- [ ] T008 `MvpProjectBuilder`에 100 XP, Runner/Bruiser/Ripper `5/25/20`, 전문화 3종, 필수 문자열 및 진행도 이벤트를 정의하고 `mvp-2.0.0` 자산을 재생성한다 (`TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeProgression.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeMelee.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeRanged.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeBalanced.asset`, `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`, `TelerobotMVP/Assets/Game/Data/Assets/StringTable.asset`, `TelerobotMVP/Assets/Game/Data/Assets/TelemetryConfig.asset`)
- [ ] T009 테스트용 설정에 실제 자산과 동일한 XP, 전문화 프로필, 두 해태의 결정론적 선택 기본값을 추가한다 (`TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`)
- [ ] T010 T002의 데이터 매핑 테스트를 통과시키고 builder 재실행 뒤 생성 자산이 동일한 값을 유지하는지 검증한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeDataConfigurationTests.cs`, `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`)

**Checkpoint**: 두 해태가 독립 성장 상태를 가질 수 있고 모든 밸런스 값이 데이터에서 순수 코어로 전달된다.

---

## Phase 3: User Story 1 - 해태별 독립 성장 (Priority: P1) 🎯 MVP

**Goal**: 각 해태가 자신이 피해를 준 좀비의 전체 XP를 독립적으로 받고, 기여하지 않은 해태는 변하지 않으며 레벨 2에서 정확히 정지한다.

**Requirement coverage**: FR-001–FR-010, FR-030–FR-031, FR-033; US1 acceptance 1–5; 관련 edge cases 전체

**Independent Test**: 한 해태만 Runner에 피해를 준 뒤 플레이어가 처치하면 그 해태만 5 XP를 얻는다. 두 해태가 Bruiser에 피해를 주면 각각 25 XP를 받고, 중복 타격·파괴된 기여자·초과 보상에서도 한 번만 지급되고 100 XP에 고정된다.

### Tests for User Story 1

- [ ] T011 [P] [US1] 초기 상태, 단독/공동 기여, 전체 보상, player-only 제외, 중복 기여 제거, 파괴된 기여자, XP clamp, 동시 레벨업, 100회 상태 격리를 먼저 실패하는 EditMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeProgressionTests.cs`)
- [ ] T012 [P] [US1] 플레이어·해태의 typed damage와 실제 좀비 사망이 올바른 로봇 상태에만 XP를 적용하는 장면 통합 테스트를 먼저 실패하도록 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionPlayModeTests.cs`)
- [ ] T013 [P] [US1] 동일 seed/기여/처치 순서의 XP·레벨·이벤트 재현성과 공동 기여 전체 보상을 먼저 실패하는 시뮬레이션 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

### Implementation for User Story 1

- [ ] T014 [US1] 기여 기록, ordinal contributor 정렬, 사망 보상 guard, 전체 XP 지급, 100 XP clamp와 XP→레벨→ready 이벤트 결과를 순수 규칙으로 구현한다 (`TelerobotMVP/Assets/Game/Core/Progression/HaetaeProgressionSystem.cs`)
- [ ] T015 [US1] 문자열 damage source를 typed `DamageSource`로 교체하고 양수 applied damage만 좀비 기여 상태에 기록한다 (`TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`, `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`, `TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`)
- [ ] T016 [US1] 좀비 사망 시 기여자별 보상을 처리하고 `haetae_xp_gained`→`haetae_level_reached`→`haetae_specialization_ready`→`zombie_killed` 순서로 publish하는 런타임 연결을 구현한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Runtime/Zombies/ZombieActor.cs`)
- [ ] T017 [US1] `SimZombie`에 기여 상태와 보상 guard를 추가하고 런타임과 같은 순수 진행도 규칙과 이벤트 순서를 사용한다 (`TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [ ] T018 [US1] 새 진행도 이벤트 이름과 `zombie_killed`의 안정 정렬 contributor payload를 JSONL에 반영하고 zero-applied XP 이벤트를 일관되게 생략한다 (`TelerobotMVP/Assets/Game/Simulation/Telemetry/JsonLinesTelemetrySink.cs`, `TelerobotMVP/Assets/Game/Core/Events/DomainEventBus.cs`)
- [ ] T019 [US1] T011–T013을 통과시키고 한 해태의 처리로 다른 해태가 변하지 않는 독립 테스트를 100회 성공시킨다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeProgressionTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: 전문화 UI 없이도 해태별 XP와 레벨 2 준비 상태를 독립적으로 검증할 수 있다.

---

## Phase 4: User Story 2 - 레벨 2 전문화 선택 (Priority: P1)

**Goal**: 레벨 2가 된 각 해태에 대해 근거리형·원거리형·균형형을 명시적으로 한 번 선택하고, 같은 역할 또는 서로 다른 역할을 독립적으로 유지한다.

**Requirement coverage**: FR-012–FR-015, FR-032; US2 acceptance 1–6

**Independent Test**: 두 해태를 레벨 2 ready 상태로 만든 뒤 각각 다른 전문화를 선택하고 상태가 섞이지 않는지 확인한다. 새 세션 초기화, 같은 역할 중복, 동일 로봇 재선택 거부를 별도로 검증한다.

### Tests for User Story 2

- [ ] T020 [P] [US2] 레벨 제한, 유효 3종, same/mixed 선택, 명시적 선택 전 General 유지, 세션 내 불변, 새 세션 초기화를 먼저 실패하는 순수 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeSpecializationTests.cs`)
- [ ] T021 [P] [US2] 대상 로봇 한 대만 선택되고 Disabled/Destroyed 상태에서도 선택이 보존되며 복원 후 적용되는 런타임 통합 테스트를 먼저 실패하도록 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationPlayModeTests.cs`)

### Implementation for User Story 2

- [ ] T022 [US2] `SelectSpecialization`의 `Selected`, `NotLevelTwo`, `AlreadySelected`, `InvalidChoice` 결과와 상태 불변 조건을 구현한다 (`TelerobotMVP/Assets/Game/Core/Progression/HaetaeProgressionSystem.cs`)
- [ ] T023 [US2] 한 robot ID에만 명시적 선택을 적용하고 성공 시 phase/time/ready duration을 포함한 `haetae_specialization_selected`를 emit하는 controller API를 추가한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`)
- [ ] T024 [US2] 각 `SimPlayerProfile`에 ordered two-entry specialization loadout을 저장하고 ready 시 즉시 선택하되 spawn RNG를 소비하지 않게 한다 (`TelerobotMVP/Assets/Game/Core/Config/GameConfig.cs`, `TelerobotMVP/Assets/Game/Data/Definitions/SimPlayerProfileAsset.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [ ] T025 [US2] T020–T021을 통과시키고 같은 seed에서 전문화 선택 조합을 바꿔도 spawn stream이 동일한지 검증한다 (`TelerobotMVP/Assets/Tests/EditMode/HaetaeSpecializationTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: UI를 통하지 않고도 해태별 전문화 선택의 독립성·불변성·결정성을 검증할 수 있다.

---

## Phase 5: User Story 3 - 전투 역할이 달라지는 세 가지 빌드 (Priority: P1)

**Goal**: 전문화마다 공격 방식, 교전 거리와 접근 행동이 달라지고 기존 명령·배터리·파괴 규칙 안에서 강점과 약점을 드러낸다.

**Requirement coverage**: FR-011, FR-016–FR-022; US3 acceptance 1–5

**Independent Test**: 동일 명령·경로·적 배치에서 근거리형은 접근 후 최대 3대 cleave, 원거리형은 6–12 m 유지 및 ranged 공격, 균형형은 접근 중 ranged 후 2 m 이내 melee 전환을 보인다.

### Tests for User Story 3

- [ ] T026 [P] [US3] General/Melee/Ranged/Balanced의 movement intent, attack kind, 거리 band, cooldown, cleave target cap, damage·방어·배터리 배율을 먼저 실패하는 순수 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/RobotCombatPolicyTests.cs`)
- [ ] T027 [P] [US3] 동일 경로에서 역할별 접근·공격·tracer/cleave cue, battery 0 Disabled, Ripper drain, Destroyed/restore 후 전문화 유지를 먼저 실패하는 PlayMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeCombatRolePlayModeTests.cs`)
- [ ] T028 [P] [US3] 9개 ordered loadout의 거리·공격·배터리·피해·Destroyed 지표와 동일 입력 재현성을 먼저 실패하는 결정론적 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

### Implementation for User Story 3

- [ ] T029 [US3] 현재 거리와 해태별 활성 프로필로 `Approach/Hold/Retreat` 및 `Dash/Bite/Ranged/None`을 반환하는 순수 전투 정책을 구현한다 (`TelerobotMVP/Assets/Game/Core/Robots/RobotCombatPolicy.cs`)
- [ ] T030 [US3] 기존 float damage 반환을 `RobotAttackResult` 기반으로 확장하고 General의 기존 dash/bite cadence를 보존한다 (`TelerobotMVP/Assets/Game/Core/Robots/RobotAttackSystem.cs`)
- [ ] T031 [US3] 해태별 전투 정책을 이동·공격에 연결하고 Melee cleave, Ranged direct hit/tracer, Balanced 2 m 전환, incoming/combat-battery 배율과 전문화 시각 cue를 적용한다 (`TelerobotMVP/Assets/Game/Runtime/Robots/HaetaeRobotActor.cs`)
- [ ] T032 [US3] 동일 route의 유효 대상만 progress와 ID로 안정 정렬해 cleave하고 ranged tracer/pulse를 생성하는 런타임 query/presentation helper를 추가한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`)
- [ ] T033 [US3] `SimRobotRuntime`의 route position/target distance를 추가하고 shared 전투 정책으로 접근·유지·후퇴, cleave, 배율과 로봇별 damage/battery/Disabled/Destroyed summary를 계산한다 (`TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [ ] T034 [US3] T026–T028을 통과시키고 기존 세 명령, battery/charge, Ripper 및 Destroyed 복원 테스트가 모두 유지되는지 확인한다 (`TelerobotMVP/Assets/Tests/EditMode/RobotCombatPolicyTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeCombatRolePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: 동일 명령에서도 세 역할의 전투 행동과 trade-off가 자동 테스트와 greybox 장면에서 구분된다.

---

## Phase 6: User Story 4 - 성장 상태 확인과 비차단 선택 (Priority: P2)

**Goal**: 플레이어가 해태별 레벨·XP·현재 역할·선택 대기를 확인하고 전투를 멈추지 않은 채 원하는 로봇을 전문화한다.

**Requirement coverage**: FR-023–FR-026; US4 acceptance 1–5; SC-004–SC-005의 구현 전제

**Independent Test**: 서로 다른 XP와 ready 상태의 두 해태를 표시하고 `B`로 패널을 열어 대상 한 대에만 선택한다. 패널을 열어 둔 동안 `Time.timeScale == 1`이며 좀비·해태·스폰·페이즈가 계속 진행된다.

### Tests for User Story 4

- [ ] T035 [P] [US4] 두 HUD row의 ID별 level/XP/role/ready 표시, 알림 지속, 정확한 문자열을 먼저 실패하는 PlayMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`)
- [ ] T036 [P] [US4] `B` 패널 열기/닫기, ready robot 전환, one-target 선택, same/mixed 표시, `Time.timeScale == 1`, phase 진행, click-through 차단을 먼저 실패하는 PlayMode 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationUiPlayModeTests.cs`)

### Implementation for User Story 4

- [ ] T037 [US4] `일반형`, `근거리형`, `원거리형`, `균형형`, level/XP/ready/panel 설명과 `B` 입력 힌트를 StringTable builder와 생성 자산에 추가한다 (`TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Game/Data/Assets/StringTable.asset`)
- [ ] T038 [US4] 각 해태 row에 level, current/required XP, 역할, ready highlight와 데이터 기반 4초 알림을 표시한다 (`TelerobotMVP/Assets/Game/Runtime/HUD/CombatHud.cs`)
- [ ] T039 [US4] 대상 robot ID, 세 역할 설명/trade-off, ready robot 전환, 닫기/재열기 및 성공 후 다음 ready robot 이동을 제공하는 비차단 패널을 구현한다 (`TelerobotMVP/Assets/Game/Runtime/HUD/HaetaeSpecializationView.cs`)
- [ ] T040 [US4] `Player/Specialization`의 keyboard `B` 바인딩과 입력 프레임을 추가하고 Pause보다 후, 일반 combat 입력 차단보다 먼저 panel toggle을 처리한다 (`TelerobotMVP/Assets/InputSystem_Actions.inputactions`, `TelerobotMVP/Assets/Game/Core/Events/GameContracts.cs`, `TelerobotMVP/Assets/Game/Runtime/Player/InputSystemPlayerInput.cs`, `TelerobotMVP/Assets/Game/Runtime/Player/ThirdPersonPlayerController.cs`)
- [ ] T041 [US4] 전문화 view를 생성·연결하고 command/pause/settings/전문화 패널의 cursor lock/visibility를 controller 한 곳에서 조정하며 “전체 선택”이 일괄 전문화를 유발하지 않게 한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`, `TelerobotMVP/Assets/Game/Runtime/HUD/RobotCommandMenu.cs`)
- [ ] T042 [US4] T035–T036을 통과시키고 선택을 미룬 ready 상태가 phase 변경 및 Disabled/Destroyed를 거쳐 유지되는지 검증한다 (`TelerobotMVP/Assets/Tests/PlayMode/HaetaeProgressionHudPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/HaetaeSpecializationUiPlayModeTests.cs`)

**Checkpoint**: 전투를 멈추지 않고 어느 해태가 성장했는지 식별하고 한 대씩 전문화할 수 있다.

---

## Phase 7: User Story 5 - 페이즈 보상에서 지속 성장으로 전환 (Priority: P2)

**Goal**: Phase 1/2 종료 시 기존 3택 업그레이드 없이 거점 회복·경로 개방·다음 페이즈가 즉시 이어진다.

**Requirement coverage**: FR-027–FR-029; US5 acceptance 1–3; SC-009

**Independent Test**: Phase 1과 Phase 2를 각각 클리어했을 때 `NextPhase`가 반환되고 old upgrade UI/event 없이 base recovery, route opening, next phase start가 일어난다. ready 선택은 페이즈 중과 전환 후 모두 가능하다.

### Tests for User Story 5

- [ ] T043 [P] [US5] Phase 1/2 clear가 `NextPhase`, Phase 3 clear가 `Victory`이고 base recovery와 ready 상태가 유지됨을 먼저 실패하는 EditMode 테스트로 변경한다 (`TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseTwoAndUpgradeTests.cs`)
- [ ] T044 [P] [US5] Phase 1/2에서 upgrade view가 생성·표시되지 않고 clear radio, 새 route, 다음 phase가 이어지는 테스트를 먼저 실패하도록 변경한다 (`TelerobotMVP/Assets/Tests/PlayMode/PhaseOnePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseTwoPlayModeTests.cs`)
- [ ] T045 [P] [US5] 시뮬레이션에서 upgrade offer/selection RNG 없이 3개 phase가 진행되고 `upgrade_selected`가 없음을 먼저 실패하는 테스트로 작성한다 (`TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

### Implementation for User Story 5

- [ ] T046 [US5] Phase 1/2 생존 clear가 항상 `NextPhase`를 반환하게 하고 active flow에서 `AwaitingUpgrade`와 `SessionState.SelectedUpgrades`를 읽거나 쓰지 않게 한다 (`TelerobotMVP/Assets/Game/Core/Phase/PhaseSystem.cs`, `TelerobotMVP/Assets/Game/Core/GameState/GameModels.cs`)
- [ ] T047 [US5] `UpgradeSystem`/`UpgradeSelectionView` 생성과 reward gate를 제거하고 phase clear 샘플·radio 후 즉시 `BeginPhase(current + 1)`을 호출한다 (`TelerobotMVP/Assets/Game/Runtime/Bootstrap/MvpGameController.cs`)
- [ ] T048 [US5] 시뮬레이터의 upgrade offer/apply/policy와 관련 RNG 소비를 제거하고 phase clear 다음 step에 다음 phase를 시작한다 (`TelerobotMVP/Assets/Game/Simulation/SimRunner/DeterministicSessionSimulator.cs`)
- [ ] T049 [US5] active catalog/mapper/builder/test fixture에서 9개 upgrade 요구와 mapping을 제거하되 legacy upgrade `.asset` 파일은 한 data-version 동안 비활성 상태로 보존한다 (`TelerobotMVP/Assets/Game/Data/Definitions/MvpContentCatalog.cs`, `TelerobotMVP/Assets/Game/Data/MvpDataMapper.cs`, `TelerobotMVP/Assets/Game/Editor/MvpProjectBuilder.cs`, `TelerobotMVP/Assets/Tests/Shared/TestConfigFactory.cs`, `TelerobotMVP/Assets/Game/Data/Assets/MvpBalanceCatalog.asset`)
- [ ] T050 [US5] T043–T045를 통과시키고 `mvp-2.0.0` JSONL에 `upgrade_selected`가 없으며 `haetae_specialization_selected`가 성장 선택 이벤트로 기록되는지 검증한다 (`TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseTwoAndUpgradeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseOnePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PhaseTwoPlayModeTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/DeterministicSimulationTests.cs`)

**Checkpoint**: old upgrade 시스템은 비활성이고 세션의 빌드 선택은 해태별 전문화만 남는다.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 전체 telemetry, 밸런스, 회귀, 플레이테스트와 배포 가능한 Windows 빌드를 검증한다.

- [ ] T051 [P] 진행도 이벤트 payload/envelope, lethal hit ordering, duplicate death guard, `robot_auto_charge_started` 및 `upgrade_selected` 예외를 계약 테스트로 고정한다 (`TelerobotMVP/Assets/Tests/EditMode/ProgressionTelemetryTests.cs`, `TelerobotMVP/Assets/Game/Simulation/Telemetry/JsonLinesTelemetrySink.cs`)
- [ ] T052 [P] 반동·총구/명중 피드백, 그룹 크기 `3–4/3–5/4–6`, cap `15/20/24`, 연속 spawn, 세 명령, battery/Destroyed/Ripper/medical 회귀 검증을 기존 테스트에 보강한다 (`TelerobotMVP/Assets/Tests/EditMode/PhaseOneTests.cs`, `TelerobotMVP/Assets/Tests/EditMode/PhaseThreeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/PlayerExperiencePlayModeTests.cs`, `TelerobotMVP/Assets/Tests/PlayMode/RobotCommandPlayModeTests.cs`)
- [ ] T053 20개 balance seed와 9개 ordered loadout을 실행해 SC-002, SC-003, SC-008, SC-010을 평가하고 필요한 경우 spawn/recoil을 건드리지 않고 XP·전문화 data asset만 조정한다 (`TelerobotMVP/Assets/Game/Data/Assets/HaetaeProgression.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeMelee.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeRanged.asset`, `TelerobotMVP/Assets/Game/Data/Assets/HaetaeBalanced.asset`, `specs/002-haetae-build-progression/quickstart.md`)
- [ ] T054 quickstart의 수동 시나리오 A–E를 실행하고 최소 30회 선택으로 SC-004–SC-007의 식별 시간·역할 인지·배치 변화·선택 비율을 기록한다 (`specs/002-haetae-build-progression/playtest-report.md`, `specs/002-haetae-build-progression/quickstart.md`)
- [ ] T055 전체 EditMode/PlayMode suite, Windows build, standalone smoke를 실행하고 신규 정확한 통과 개수와 로그 위치를 기록한다 (`specs/002-haetae-build-progression/quickstart.md`, `TelerobotMVP/Assets/Tests/EditMode/`, `TelerobotMVP/Assets/Tests/PlayMode/`)
- [ ] T056 active spec·plan·contracts 대비 FR/acceptance/edge-case 추적성, player-facing 문자열, telemetry 예외, out-of-scope 금전·상점·플레이어 무기 미포함을 최종 감사한다 (`specs/002-haetae-build-progression/spec.md`, `specs/002-haetae-build-progression/plan.md`, `specs/002-haetae-build-progression/contracts/validation-scenarios.contract.md`, `specs/002-haetae-build-progression/tasks.md`)

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

### Incremental Delivery

1. **US1**: 독립 XP와 레벨 2 ready
2. **US2**: 해태별 불변 전문화 선택
3. **US3**: 세 역할의 실제 전투 차이
4. **US4**: 전투를 멈추지 않는 HUD/선택 UX
5. **US5**: old upgrade gate 제거와 phase 연속성
6. **Polish**: telemetry, 20-seed balance, 30-choice playtest, Windows 회귀

### Scope Guard

- 돈, 상점, 시간 수입, 플레이어 XP/무기/근접 빌드, 타임어택, 추가 해태, 레벨 3+, 영구 저장을 구현하지 않는다.
- 기존 recoil 수치와 spawn interval/group/cap은 테스트 기준으로만 다루며 전문화 밸런싱을 이유로 변경하지 않는다.
- legacy upgrade 자산은 active catalog와 player flow에서 제외하되 이번 data-version에서는 삭제 작업을 만들지 않는다.

---

## Notes

- `[P]` 작업은 선행 checkpoint 완료 후 다른 파일을 수정할 때만 병렬 실행한다.
- 각 사용자 스토리는 해당 `Independent Test`를 단독으로 재현할 수 있어야 한다.
- `MvpProjectBuilder`가 생성 자산을 덮어쓰므로 definition, builder, mapper, serialized asset, test fixture를 함께 갱신한다.
- player-facing 전문화 이름은 StringTable에서 정확히 `근거리형`, `원거리형`, `균형형`을 사용한다.
- simulation specialization 선택은 spawn RNG를 소비하지 않는다.
- final art/audio는 테스트를 차단하지 않으며 greybox cue로 먼저 검증한다.
