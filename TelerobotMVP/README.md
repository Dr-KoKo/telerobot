# 텔레로봇: 출격하라 — MVP

Unity 6.3 LTS로 만든 3단계 좀비 거점 방어 게임입니다. 플레이어가 직접 전투하면서 두 대의 해태 로봇에 명령을 내리고, 배터리와 보급을 관리해 세 경로를 방어합니다.

## Unity에서 처음 열기

1. Unity Hub의 **Projects**에서 **Add > Add project from disk**를 누릅니다.
2. 이 파일이 들어 있는 `TelerobotMVP` 폴더를 선택합니다. 올바른 폴더에는 `Assets`, `Packages`, `ProjectSettings`가 함께 있습니다.
3. Unity `6000.3.20f1`로 엽니다. 첫 임포트가 끝날 때까지 기다립니다.
4. Project 창에서 `Assets/Game/Scenes/MainMenu.unity`를 더블클릭합니다.
5. 상단 중앙의 ▶ **Play** 버튼을 누릅니다.

`New project`를 누르거나 `.vsconfig`, `.slnx` 파일을 임포트할 필요는 없습니다. Unity Hub에는 프로젝트의 루트 폴더만 등록하면 됩니다.

씬이나 데이터 자산이 보이지 않을 때는 Unity 메뉴의 **Tools > Telerobot > Build MVP Project**를 한 번 실행하십시오.

## Unity 없이 바로 실행하기

검증된 Windows 플레이테스트 빌드는 `Builds/Windows/TelerobotMVP.exe`입니다. `Builds/Windows` 폴더 전체를 같은 위치에 둔 채 exe를 더블클릭하십시오. 처음에는 시작 화면이 열리며 **설정 → 게임 시작** 순서로 들어가면 됩니다.

Unity에서 새 빌드를 만들려면 **Tools > Telerobot > Build Windows Playtest**를 누릅니다. 결과는 같은 `Builds/Windows/` 폴더에 생성되고, 그 안의 `README-KO.txt`에도 실행 방법이 적혀 있습니다.

이전 빌드에서 `ArgumentNullException: shader` 오류가 보이거나 HUD 한글의 위아래가 잘린다면 현재의 `Builds/Windows` 폴더 전체를 사용하십시오. 실행 파일만 따로 복사하면 필요한 `TelerobotMVP_Data`와 Unity 런타임 파일이 빠집니다. 현재 플레이어 버전은 `0.2.3`입니다.

## 주변 테스터에게 공유하기

Unity 메뉴에서 **Tools > Telerobot > Build Shareable Windows Package**를 누르면 비개발 Windows 빌드와 단일 배포 ZIP이 함께 생성됩니다.

- 공유 파일: `Builds/Distribution/TelerobotMVP-Windows-v0.2.3.zip`
- itch.io 업로드 안내: `Builds/Distribution/ITCH-IO-UPLOAD-KO.md`
- 설문 작성 템플릿: `Builds/Distribution/FEEDBACK-FORM-KO.md`
- 테스터 실행 안내: ZIP 내부 `START-HERE-KO.txt`

공유 ZIP은 일반 실행에 필요한 파일만 포함하고 `DoNotShip` 폴더와 PDB/MDB 디버그 심볼을 제외합니다. 테스터에게는 ZIP을 새 폴더에 전부 푼 뒤 `TelerobotMVP.exe`를 실행하라고 안내하십시오. 처음에는 itch.io의 Restricted+비밀번호 또는 검색 비노출 페이지에 ZIP을 올리고, 페이지 상단에 피드백 설문 링크를 함께 두는 방식이 적합합니다.

## 시작 화면과 설정

시작 화면은 **게임 시작 / 설정 / 게임 종료**를 제공합니다. 설정에서는 마우스 감도, 전체 음량, 효과음 음량, 해상도, 전체 화면, 새 게임의 기본 1인칭·3인칭 시점을 조정할 수 있습니다. **저장하고 적용**을 누르면 이 PC에 자동 저장되어 다음 실행에도 유지됩니다. 게임 중에는 `Esc` → **설정**으로 같은 화면을 열 수 있습니다.

## 조작법

| 입력 | 동작 |
|---|---|
| WASD | 이동 |
| Shift | 달리기 |
| 마우스 | 시점 및 조준 |
| V | 1인칭 / 3인칭 전환 |
| Space | 점프 |
| 왼쪽 클릭 | 사격(누르고 있으면 연속 발사) |
| R | 재장전 |
| G | 수류탄 |
| E | 가까운 탄약 보급 상호작용 시작 |
| 1 / 2 | 해태 로봇 개별 선택 |
| 3 | 두 해태 로봇 전체 선택 전환 |
| Tab | 로봇 명령 메뉴 열기 / 닫기 |
| Q | 명령 메뉴에서 대상 경로 변경 |
| Esc | 일시정지 / 계속하기 |

기본값은 3인칭이며 설정에서 시작 시점을 바꿀 수 있습니다. 3인칭 카메라는 벽을 통과하지 않고, `V`를 누르면 두 시점이 즉시 전환됩니다. `Shift`를 누른 채 이동하면 1.5배 속도로 달립니다. 화면 중앙 조준점과 명중 표식이 사격 결과를 알려주고, 헤드샷에는 별도 표시가 나옵니다. 공격받으면 적이 있는 방향에 붉은 표시가 나타납니다. 보급지에서는 E를 누른 뒤 1.5초 동안 반경에 머물러야 예비 탄약이 최대치까지 회복됩니다. 판정은 높이를 제외한 지면 거리이며, 진행 중 작은 경계 흔들림에는 0.75m의 여유가 있습니다. 탄창이 6발 이하일 때는 재장전 경고가 표시됩니다. `1`·`2`로 로봇을 따로 선택하거나 `3`으로 전체 선택을 전환할 수 있습니다. Tab 명령은 거점 사수·경로 순찰·기지 복귀 3개이며, 해태는 기지 반경 6m 안에서 자동 충전됩니다. 충전 중 기지 주변 적을 발견하면 배정 경로와 관계없이 충전을 중단하고 해당 표적과 교전합니다. 파괴된 로봇은 명령을 받지 않고 다음 페이즈 시작 때 복구됩니다. 일시정지 및 승리·패배 화면에서는 세션 재시작이나 시작 화면 복귀가 가능합니다.

왼쪽 클릭을 누르고 있으면 약 분당 500발 속도로 연속 발사됩니다. 매 발마다 정해진 범위 안에서 반동 방향과 크기가 조금씩 달라지며, 총구 섬광·발사음이 함께 발생합니다. 좀비는 피격 시 흰색으로 번쩍이며 일반 명중과 헤드샷의 확인음이 다릅니다. 사망 시 붉은 효과와 함께 축소·침하하고, 재장전 중에는 조준점 아래 진행 막대가 표시됩니다. 현재 효과음은 외부 오디오 파일 없이 런타임에서 생성하는 MVP용 임시 음원입니다.

## 폴더 구조

- `Assets/Game/Core`: Unity에 의존하지 않는 전투, 체력, 배터리, 페이즈, 업그레이드 규칙
- `Assets/Game/Data`: ScriptableObject 데이터 정의와 코어 설정 매퍼
- `Assets/Game/Runtime`: 플레이어, 카메라, 로봇, 좀비, HUD, 게임 진행 어댑터
- `Assets/Game/Simulation`: 고정 시간축과 seed를 쓰는 결정론적 전체 세션 시뮬레이션
- `Assets/Tests`: EditMode 규칙 테스트와 PlayMode 씬 통합 테스트

## 테스트

Unity에서 **Window > General > Test Runner**를 열고 EditMode와 PlayMode의 **Run All**을 각각 누릅니다. 배치 자동화 전에는 같은 프로젝트의 Editor를 닫고 Unity Hub에 로그인합니다. Personal 라이선스도 사용할 수 있으며, 종료 코드 198은 라이선스 등급이 아니라 headless 프로세스의 로컬 인증 실패를 뜻합니다.

```powershell
Unity.exe -batchmode -quit -projectPath . -executeMethod Telerobot.Game.Editor.MvpProjectBuilder.BuildAll
Unity.exe -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml
Unity.exe -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults TestResults/playmode.xml
Unity.exe -batchmode -projectPath . -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch -quit
Unity.exe -batchmode -projectPath . -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildShareableWindowsPlaytestBatch -quit
```

2026-07-22 Unity `6000.3.20f1` 전체 검증 기준선은 EditMode `51/51`, PlayMode `38/38`(실패·건너뜀·미결정 없음), Windows x64 빌드, `TELEROBOT_STANDALONE_SMOKE_READY` 독립 실행 마커와 종료 코드 0입니다. 생성된 `Game.Runtime`, `Game.Tests.EditMode`, `Game.Tests.PlayMode` 프로젝트도 경고·오류 없이 컴파일됩니다.

개발용 텔레메트리는 `Application.persistentDataPath/Telerobot/Telemetry/` 아래 JSON Lines 형식으로 기록됩니다.

## Microsoft Store 배포

Windows의 “인식할 수 없는 앱” 경고 없이 외부 테스터에게 제공하려면 Unity 메뉴 **Tools > Telerobot > Build Microsoft Store MSIX**를 사용합니다. 이 메뉴는 Partner Center에 예약된 `Dr-Ko.telerobot` / `Dr-Ko` ID로 Windows x64 패키지를 만듭니다.

처음 한 번은 Visual Studio Installer에서 Windows 10/11 SDK를 설치해야 합니다. 결과 파일은 `Builds/Store/TelerobotMVP-Store-v0.2.3.0-x64.msix`이며, 이 무서명 파일을 직접 배포하지 않고 Partner Center에 업로드합니다. 자세한 순서는 `Documentation/Store/STORE-SUBMISSION-KO.md`를 따릅니다.

## 현재 그래픽 범위

현재 버전은 최종 아트·애니메이션 대신 색상, 기본 프리미티브와 절차형 임시 효과음을 사용하는 기능 검증용 그레이박스입니다. 전투, 로봇 명령, 배터리, 보급, 업그레이드, 세 페이즈와 승리·패배 흐름은 플레이할 수 있습니다.
