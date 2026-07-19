# Quickstart: 텔레로봇 MVP

**Feature**: `001-robot-base-defense-mvp`

**Unity**: `6000.3.20f1`

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
| LMB / R | 사격 / 재장전 |
| G / E | 수류탄 / 탄약 보급 |
| 1·2 / Tab / Q | 로봇 선택 / 명령 메뉴 / 대상 경로 전환 |
| Esc | 일시정지·계속하기 |

확인할 플레이 경험:

- 3인칭으로 시작하고 `V`를 누르면 몸체가 숨겨진 1인칭으로 전환된다.
- 3인칭 카메라는 뒤쪽 벽이나 지형 앞에서 거리를 자동으로 줄인다.
- 점프 중에는 중력이 적용되고 지면에 다시 착지한다.
- Shift를 누른 채 이동하면 기본 속도의 1.5배로 달린다.
- 조준점이 화면 중앙에 있으며 일반 명중과 헤드샷 피드백을 구분한다.
- 발사 시 총구 섬광·효과음·가벼운 카메라 반동이 나타난다.
- 좀비가 피격되면 흰색으로 번쩍이고 일반 명중과 헤드샷 확인음이 다르게 재생된다.
- 좀비 사망 시 붉은 효과와 함께 몸체가 축소·침하한다.
- R로 재장전하면 조준점 아래 진행 막대가 완료까지 증가한다.
- 공격받은 방향에 붉은 피격 표시가 나타난다.
- 보급지 근처에서는 `[E] 탄약 보급` 안내가 나타나고 탄창이 6발 이하이면 탄약 부족 경고가 나타난다.
- Esc 일시정지 중에는 게임 시간이 멈추고 마우스 커서가 풀린다.
- 일시정지 화면에서 감도·음량·해상도·전체 화면·기본 시점을 설정할 수 있다.
- 일시정지 및 결과 화면에서 같은 세션 재시작 또는 시작 화면 복귀를 선택할 수 있다.
- 설정은 로컬 PC에 저장되며 새 게임을 시작할 때 선택한 기본 시점이 적용된다.

## 3. 자동 테스트

Editor에서는 **Window > General > Test Runner**를 연다.

```powershell
Unity.exe -batchmode -projectPath TelerobotMVP -runTests -testPlatform EditMode -testResults results-edit.xml
Unity.exe -batchmode -projectPath TelerobotMVP -runTests -testPlatform PlayMode -testResults results-play.xml
Unity.exe -batchmode -projectPath TelerobotMVP -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch -quit
Unity.exe -batchmode -projectPath TelerobotMVP -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildShareableWindowsPlaytestBatch -quit
```

- EditMode: 순수 전투·체력·탄약·수류탄·배터리·업그레이드·스폰·페이즈·결정론적 시뮬레이션 규칙
- PlayMode: 시작 화면·설정 저장, 세 페이즈 진행, 사격·보급, 로봇 명령·충전, 의료 로봇·리퍼, 승패, HUD·라디오, 시점·점프·달리기·카메라 충돌·피격 방향·총기 피드백·재장전 UI·한글 HUD 세로 여백·일시정지
- 2026-07-19 검증: EditMode `39/39`, PlayMode `23/23`, 실패·건너뜀 없음
- Windows x64 공유 ZIP 생성·필수 파일/제외 규칙 검사 및 압축 해제 후 Direct3D 12 독립 실행 스모크에서 실제 MVP 장면 진입 성공, 런타임 오류 서명 없음

## 4. 수동 수용 검증

| 항목 | 방법 | 기대 결과 |
|---|---|---|
| Phase 1 | North Road 방어 | 필드의 적이 사라지고 거점·플레이어 생존 시 업그레이드 진입 |
| Phase 2 | 첫 업그레이드 선택 | East Alley 개방, Bruiser 등장, 거점 HP 15% 회복 |
| Phase 3 | 두 번째 업그레이드 선택 | South Tunnel 개방, 의료 로봇과 Ripper 등장 |
| 승리 | Phase 3 전멸 | 승리 화면과 다시 시작 버튼 표시 |
| 패배 | 거점 또는 플레이어 HP 0 | 즉시 패배, 정확한 `defeatReason` 기록 |
| 배터리 | 순찰·전투·충전 명령 | 경고 임계값, 무력화·회복, 자동 충전 복귀가 동작 |
| 보급 | 안전·위험 보급지에서 E | 탄약 회복과 Safe/Risky 텔레메트리 기록 |
| 전투 피드백 | 사격·헤드샷·처치·재장전 | 총구 섬광·반동·구분된 명중음·피격/사망 효과·재장전 막대 표시 |
| 카메라 | V 전환, 벽에 등지고 이동 | 두 시점의 FOV 변경, 3인칭 벽 관통 방지 |
| 접근성 | Esc | 시간 정지, 커서 해제, 계속하기·다시 시작 제공 |
| 시작 화면 | MainMenu에서 설정 후 게임 시작 | 저장한 기본 시점·감도·음량이 MVP에 적용 |
| Windows 실행 | `Builds/Windows/TelerobotMVP.exe` | 시작 화면 기동, 전체 빌드 폴더 단독 실행 가능 |

상세 시나리오는 `contracts/validation-scenarios.contract.md`, 데이터 기본값은 `contracts/data-config.contract.md`, 텔레메트리 스키마는 `contracts/telemetry.contract.md`를 참조한다.

## 5. 결정론적 시뮬레이션과 텔레메트리

동일 seed와 `dataVersion`은 동일한 이벤트 스트림을 생성한다. 개발용 런타임 이벤트는 `Application.persistentDataPath/Telerobot/Telemetry/` 아래 JSONL로 기록된다. 기존 로컬 코어 검증에서 seed `1001`은 두 번의 고정 스텝 실행에서 동일한 1,696개 이벤트를 생성했다.

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
