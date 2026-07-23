# Quickstart: 텔레로봇 MVP

**Feature**: `001-robot-base-defense-mvp`

**Unity**: `6000.3.20f1`

**Data version**: `mvp-1.4.5`

**Project root**: `<repo>/TelerobotMVP/`

## 1. 프로젝트 열기

1. Unity Hub에서 **Add > Add project from disk**를 선택한다.
2. `Assets`, `Packages`, `ProjectSettings` 폴더가 들어 있는 `<repo>/TelerobotMVP/`를 지정한다.
3. Unity 6.3 LTS로 열고 첫 임포트가 끝날 때까지 기다린다.
4. 필요하면 **Tools > Telerobot > Build MVP Project**를 실행한다.
5. `Assets/Game/Scenes/MainMenu.unity`를 열고 Play를 누른다.

Unity Hub의 **New project**로 만들거나 개별 C# 파일을 임포트하는 프로젝트가 아니다.

### Unity 없이 Windows 빌드 실행

1. `<repo>/TelerobotMVP/Builds/Windows/` 폴더를 연다.
2. 폴더 안의 파일과 하위 폴더를 그대로 둔 채 `TelerobotMVP.exe`를 더블클릭한다.
3. 시작 화면에서 **설정**을 확인하고 **게임 시작**을 누른다.

Unity에서 다시 빌드할 때는 **Tools > Telerobot > Build Windows Playtest**를 실행한다. 결과 폴더의 `README-KO.txt`에도 같은 안내가 들어 있다.

이전 빌드에서 `ArgumentNullException: shader`가 발생하거나 HUD 한글의 위아래가 잘린다면 현재 `Builds/Windows` 폴더 전체로 교체한다. 수정된 플레이어 버전은 `0.2.2`이며, exe만 복사하지 말고 `TelerobotMVP_Data`와 나머지 런타임 파일을 함께 둔다.

### 주변 테스터용 ZIP 만들기

1. Unity 메뉴에서 **Tools > Telerobot > Build Shareable Windows Package**를 실행한다.
2. `Builds/Distribution/TelerobotMVP-Windows-v0.2.2.zip`을 itch.io에 Windows 다운로드 파일로 올린다.
3. 같은 폴더의 `ITCH-IO-UPLOAD-KO.md`에 따라 접근 범위와 페이지를 설정한다.
4. `FEEDBACK-FORM-KO.md` 문항으로 설문을 만들고 itch.io 페이지에 링크한다.

ZIP에는 `START-HERE-KO.txt`가 들어 있으며 `DoNotShip` 폴더와 PDB/MDB 디버그 심볼은 포함되지 않는다. 공유 빌드는 `BuildOptions.None`을 쓰고, 기존 `Builds/Windows` 개발용 빌드는 진단용으로 별도 유지한다.

## 2. 기본 조작

| 입력 | 기능 |
|---|---|
| WASD / 마우스 | 이동 / 조준 |
| Shift | 달리기 |
| V | 1인칭·3인칭 전환 |
| Space | 점프 |
| LMB / R | 사격(누르고 있으면 연속 발사) / 재장전 |
| G / E | 수류탄 / 탄약 보급 상호작용 시작 |
| 1·2·3 / Tab / Q | 개별 로봇 선택·전체 로봇 선택 / 명령 메뉴 / 대상 경로 전환 |
| Esc | 일시정지·계속하기 |

확인할 플레이 경험:

- 3인칭으로 시작하고 `V`를 누르면 몸체가 숨겨진 1인칭으로 전환된다.
- 3인칭 카메라는 뒤쪽 벽이나 지형 앞에서 거리를 자동으로 줄인다.
- 점프 중에는 중력이 적용되고 지면에 다시 착지한다.
- Shift를 누른 채 이동하면 기본 속도의 1.5배로 달린다.
- 조준점이 화면 중앙에 있으며 일반 명중과 헤드샷 피드백을 구분한다.
- 발사 버튼을 누르고 있으면 0.12초 간격으로 연속 발사되며, 매 발 범위 내 무작위 반동·총구 섬광·효과음이 나타난다.
- 좀비가 피격되면 흰색으로 번쩍이고 일반 명중과 헤드샷 확인음이 다르게 재생된다.
- 좀비 사망 시 붉은 효과와 함께 몸체가 축소·침하한다.
- R로 재장전하면 조준점 아래 진행 막대가 완료까지 증가한다.
- 공격받은 방향에 붉은 피격 표시가 나타난다.
- 보급지 근처에서는 `[E] 탄약 보급` 안내가 나타난다. E를 누른 뒤 1.5초 동안 보급지 반경에 머물면 예비 탄약이 최대치까지 회복된다. 진입은 높이를 제외한 수평 거리로 판정하며, 진행 중에는 0.75m의 이탈 여유가 있어 점프나 작은 경계 흔들림으로 취소되지 않는다.
- `1` 또는 `2`로 해태를 개별 선택하고 `3`으로 두 로봇 전체 선택을 전환한다. Tab 메뉴는 거점 사수·경로 순찰·기지 복귀 3개만 제공한다. 해태는 기지 중심 반경 6m 안에서 유효한 표적이 없고 배터리가 부족하면 자동 충전한다. 충전 중 탐지 반경 안에 기지 위협이 나타나면 배정 경로와 무관하게 충전을 중단하고 교전하며, 파괴된 로봇은 명령을 거부하고 다음 페이즈 시작 때 복구된다.
- 해태가 적을 처치하면 탐지 반경 안의 가장 가까운 적을 이어서 공격하고, `거점 사수` 중에는 방어 반경 밖까지 추격하지 않는다.
- 긴급 방벽은 East Alley와 South Tunnel에서도 각 통로의 거점 진입선을 가로지르는 각도로 생성된다.
- Esc 일시정지 중에는 게임 시간이 멈추고 마우스 커서가 풀린다.
- 일시정지 화면에서 감도·음량·해상도·전체 화면·기본 시점을 설정할 수 있다.
- 일시정지 및 결과 화면에서 같은 세션 재시작 또는 시작 화면 복귀를 선택할 수 있다.
- 설정은 로컬 PC에 저장되며 새 게임을 시작할 때 선택한 기본 시점이 적용된다.

## 3. 자동 테스트

Editor에서는 **Window > General > Test Runner**를 연다. 자동 배치 실행 전에는 같은 프로젝트를 연 Unity Editor를 종료하고, Unity Hub에 로그인해 로컬 라이선스가 활성화되어 있는지 확인한다. **Personal 라이선스도 이 자동화에 사용할 수 있으며**, 이전 종료 코드 198은 라이선스 등급이 아니라 headless 프로세스가 인증된 로컬 라이선스를 찾지 못해서 발생했다.

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'
$project = (Resolve-Path 'TelerobotMVP').Path
$results = Join-Path $project 'TestResults'
New-Item -ItemType Directory -Force -Path $results | Out-Null

& $unityEditor -batchmode -nographics -projectPath $project -runTests -testPlatform EditMode -testResults (Join-Path $results 'editmode-cli.xml') -logFile (Join-Path $results 'editmode-cli.log')
& $unityEditor -batchmode -nographics -projectPath $project -runTests -testPlatform PlayMode -testResults (Join-Path $results 'playmode-cli.xml') -logFile (Join-Path $results 'playmode-cli.log')
& $unityEditor -batchmode -nographics -projectPath $project -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch -quit -logFile (Join-Path $results 'windows-build-cli.log')
& (Join-Path $project 'Builds\Windows\TelerobotMVP.exe') -batchmode -nographics -telerobot-smoke -logFile (Join-Path $results 'standalone-smoke-cli.log')
```

- EditMode: 순수 전투·체력·탄약·수류탄·배터리·업그레이드·스폰·페이즈·결정론적 시뮬레이션 규칙
- PlayMode: 시작 화면·설정 저장, 세 페이즈 진행, 완화된 스폰 상한·재개, 개체별 접근 경로·충돌 회피, 경유지 원거리 피해 방지, 경로 진입선에 맞춘 긴급 방벽 회전, 해태 자동 공격·처치 후 근거리 연속 교전·로봇 간 최소 간격·고유 대기 위치·거점 근접 적 우선순위·방어 반경 복귀·기지 자동 충전, 홀드 연속 발사·범위 내 무작위 반동·수평 거리 및 이탈 여유를 적용한 1.5초 보급, 3개 개별·전체 로봇 명령·파괴·다음 페이즈 복구, 의료 로봇·리퍼, 승패, HUD·라디오, 시점·점프·달리기·카메라 충돌·피격 방향·총기 피드백·재장전 UI·한글 HUD 세로 여백·일시정지
- 2026-07-22 Unity `6000.3.20f1` 전체 검증 기준선: EditMode `51/51`, PlayMode `38/38`, 실패·건너뜀·미결정 없음. 생성된 `Game.Runtime`, `Game.Tests.EditMode`, `Game.Tests.PlayMode` 프로젝트도 경고·오류 없이 컴파일됨
- Windows x64 개발 빌드 성공 및 `-telerobot-smoke` 독립 실행에서 실제 MVP 게임 월드 준비 마커 확인, 종료 코드 0

## 4. 수동 수용 검증

| 항목 | 방법 | 기대 결과 |
|---|---|---|
| Phase 1 | North Road 방어 | 필드의 적이 사라지고 거점·플레이어 생존 시 업그레이드 진입 |
| Phase 2 | 첫 업그레이드 선택 | East Alley 개방, Bruiser 등장, 거점 HP 15% 회복 |
| Phase 3 | 두 번째 업그레이드 선택 | South Tunnel 개방, 의료 로봇과 Ripper 등장 |
| 승리 | Phase 3 전멸 | 승리 화면과 다시 시작 버튼 표시 |
| 패배 | 거점 또는 플레이어 HP 0 | 즉시 패배, 정확한 `defeatReason` 기록 |
| 배터리 | 순찰·전투 후 기지 복귀, 충전 중 타 경로 기지 위협 접근 | 경고 임계값, 무력화·회복, 기지 반경 자동 충전이 동작; 위협 탐지 시 충전을 중단하고 교전; Tab에 별도 충전 명령 없음 |
| 해태 연속 교전 | 가까운 두 적을 배치하고 첫 적 처치 관찰 | 거점으로 복귀하기 전에 탐지 반경 안의 다음 적을 획득; 거점 사수 반경은 유지 |
| 긴급 방벽 | 업그레이드 선택 후 Phase 2·3 시작 | 동·남쪽 방벽이 각 통로 진입 방향을 가로질러 배치 |
| 보급 | 안전·위험 보급지에서 E를 누르고 1.5초간 반경 유지 | 최대 예비 탄약 회복과 Safe/Risky 텔레메트리 기록; 높이 차이 무시, 0.75m 이탈 여유 밖에서만 취소 |
| 전투 피드백 | 클릭 및 홀드 사격·헤드샷·처치·재장전 | 0.12초 연속 발사·범위 내 무작위 반동·총구 섬광·구분된 명중음·피격/사망 효과·재장전 막대 표시 |
| 카메라 | V 전환, 벽에 등지고 이동 | 두 시점의 FOV 변경, 3인칭 벽 관통 방지 |
| 접근성 | Esc | 시간 정지, 커서 해제, 계속하기·다시 시작 제공 |
| 시작 화면 | MainMenu에서 설정 후 게임 시작 | 저장한 기본 시점·감도·음량이 MVP에 적용 |
| Windows 실행 | `Builds/Windows/TelerobotMVP.exe` | 시작 화면 기동, 전체 빌드 폴더 단독 실행 가능 |

상세 시나리오는 `contracts/validation-scenarios.contract.md`, 데이터 기본값은 `contracts/data-config.contract.md`, 텔레메트리 스키마는 `contracts/telemetry.contract.md`를 참조한다.

## 5. 결정론적 시뮬레이션과 텔레메트리

동일 seed, `dataVersion`, `simProfileId` 조합은 동일한 이벤트 스트림을 생성한다. 개발용 런타임 이벤트는 `Application.persistentDataPath/Telerobot/Telemetry/` 아래 JSONL로 기록된다. 2026-07-22 검증에서는 같은 seed×프로필의 두 고정 스텝 실행이 바이트 단위로 동일한 텔레메트리 스트림을 생성했다.

## 6. 참고

- 현재 그래픽과 오디오는 기능 검증용 그레이박스·절차형 플레이스홀더다.
- 밸런스, 카메라·점프, 반동·효과 시간·절차형 음 높이는 ScriptableObject 데이터로 관리하므로 코드 수정 없이 조정할 수 있다.
- 플레이 중 빠른 페이즈 확인이 필요하면 개발용 `F10`으로 현재 웨이브를 정리할 수 있다.

## 7. Microsoft Store 패키지

1. Visual Studio Installer에서 Windows 10/11 SDK를 설치한다.
2. Unity에서 **Tools > Telerobot > Build Microsoft Store MSIX**를 실행한다.
3. `Builds/Store/TelerobotMVP-Store-v0.2.2.0-x64.msix`를 Partner Center의 Packages 단계에 업로드한다.
4. 인증 후 Microsoft Store 설치 링크를 테스터에게 공유한다.

무서명 `.msix`를 테스터에게 직접 보내지 않는다. 로컬 확인은 `Builds/Store/Staging/AppxManifest.xml`을 `Add-AppxPackage -Register`로 등록하고, 외부 배포는 Microsoft가 서명한 Store 설치본만 사용한다. 상세 절차는 `TelerobotMVP/Documentation/Store/STORE-SUBMISSION-KO.md`에 있다.
