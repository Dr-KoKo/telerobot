# Telerobot Microsoft Store 제출 안내

이 문서는 Windows의 “인식할 수 없는 앱” 경고 없이 주변 사람들에게 Telerobot을 설치하게 하는 과정을 설명합니다. 핵심은 개발자가 만든 무서명 파일을 직접 전달하는 것이 아니라, Microsoft Store에 패키지를 제출하고 인증된 Store 설치 링크를 공유하는 것입니다.

## 현재 연결된 제품 ID

- Package/Identity/Name: `Dr-Ko.telerobot`
- Package/Identity/Publisher: `CN=D7C3F8A8-2C26-4CBC-BEDF-193632AAF7DC`
- Package/Properties/PublisherDisplayName: `Dr-Ko`
- Unity 게임 버전: `0.2.2`
- Store 패키지 버전: `0.2.2.0`
- 대상: Windows Desktop x64, Windows 10 2004(빌드 19041) 이상

이 세 ID 값은 Partner Center에서 복사한 공개 패키지 식별자입니다. 계정 암호나 인증 키가 아니므로 프로젝트 설정에 저장해도 됩니다.

## 1. Windows SDK를 한 번만 설치하기

패키지를 만드는 `MakeAppx.exe`는 Unity가 아니라 Windows 10/11 SDK에 들어 있습니다.

1. 시작 메뉴에서 **Visual Studio Installer**를 엽니다.
2. 설치된 **Visual Studio Community** 오른쪽의 **수정**을 누릅니다.
3. 상단에서 **개별 구성 요소**를 누릅니다.
4. 검색창에 `Windows SDK`를 입력합니다.
5. 최신 **Windows 11 SDK** 하나를 선택합니다. 가능하면 `10.0.26100.0` 이상을 사용합니다.
6. 오른쪽 아래 **수정**을 눌러 설치를 마칩니다.
7. Unity가 열려 있었다면 완전히 닫았다가 다시 엽니다.

설치 확인용 PowerShell 명령:

```powershell
Get-ChildItem 'C:\Program Files (x86)\Windows Kits\10\bin' -Filter MakeAppx.exe -Recurse
```

한 개 이상의 경로가 나오면 준비가 끝난 것입니다.

## 2. Unity에서 Store 패키지 만들기

1. Unity Hub에서 기존 `TelerobotMVP` 프로젝트를 엽니다.
2. Unity 상단 메뉴에서 **Tools > Telerobot > Build Microsoft Store MSIX**를 누릅니다.
3. Console에 `Microsoft Store MSIX completed`가 나올 때까지 기다립니다.
4. 프로젝트의 `Builds/Store` 폴더를 엽니다.

성공하면 다음 파일이 만들어집니다.

- `Builds/Store/TelerobotMVP-Store-v0.2.2.0-x64.msix`: Partner Center에 올릴 파일
- `Builds/Store/Staging/AppxManifest.xml`: 실제 ID와 실행 파일 설정
- `Builds/Store/STORE-SUBMISSION-KO.md`: 빌드 버전에 맞춘 짧은 안내

SDK가 없다는 오류가 나더라도 `Builds/Store/Staging`은 만들어집니다. SDK를 설치한 뒤 같은 메뉴를 다시 누르면 됩니다.

## 3. 로컬에서 선택적으로 확인하기

생성된 `.msix`는 Store 제출용 무서명 패키지이므로 더블클릭 설치가 실패하는 것이 정상입니다. 자체 인증서를 만들 필요 없이, 패키징 전 내용은 느슨한 등록 방식으로 확인할 수 있습니다.

일반 PowerShell에서 다음을 실행합니다. 관리자 권한은 필요하지 않습니다.

```powershell
Add-AppxPackage -Register 'C:\Users\dongh\Documents\workspaces\telerobot\TelerobotMVP\Builds\Store\Staging\AppxManifest.xml'
```

등록 후 시작 메뉴에서 `Telerobot`을 실행합니다. 테스트가 끝나면 **설정 > 앱 > 설치된 앱 > Telerobot > 제거**를 선택합니다.

## 4. Partner Center에 제출하기

1. [Partner Center](https://partner.microsoft.com/dashboard)에서 `telerobot` 제품을 엽니다.
2. 새 제출을 시작합니다.
3. **Packages** 단계에 `TelerobotMVP-Store-v0.2.2.0-x64.msix`를 끌어 놓습니다.
4. 자동 분석이 끝나면 오류가 없는지 확인합니다.
5. **Properties**, **Age ratings**, **Store listings**를 채웁니다.
6. Store listing에 게임 설명, 지원 연락처, 아이콘, 스크린샷을 넣습니다.
7. 처음에는 원하는 테스터 공개 범위를 선택합니다. 제한된 공개를 사용한다면 테스터의 Microsoft 계정 이메일을 등록합니다.
8. 제출 검사를 실행하고 **Submit to the Store**를 누릅니다.

Private audience로 시작한 제품은 공개 전환 정책에 제약이 있을 수 있으므로 Partner Center에 표시되는 경고 문구를 제출 전에 반드시 읽습니다. 장기적으로 일반 공개할 계획이라면 공개 범위를 확정하기 전에 정책을 다시 확인합니다.

## 5. 테스터에게 전달할 것

인증 완료 후 Partner Center가 제공하는 Microsoft Store 링크만 전달합니다. 테스터는 링크를 열어 **설치**를 누르면 되고, ZIP을 풀거나 보안 경고에서 “계속 실행”을 찾을 필요가 없습니다.

Store에서 설치한 빌드는 Microsoft가 서명합니다. 이 배포 경로가 기존 ZIP/EXE에서 나타나던 “인식할 수 없는 앱” 경고를 없애는 경로입니다. 개발 PC에서 만든 무서명 `.msix` 자체에는 이 보장이 적용되지 않습니다.

## 업데이트할 때

새 패키지는 이전 제출보다 높은 4자리 버전이어야 합니다.

- 현재: Unity `0.2.2` → Store `0.2.2.0`
- 예시 업데이트: Unity `0.2.3` → Store `0.2.3.0`

Unity의 **Project Settings > Player > Version**을 먼저 올린 뒤 Store 빌드 메뉴를 다시 실행합니다. Package Identity와 Publisher 값은 바꾸지 않습니다.

## 문제 해결

- `MakeAppx.exe was not found`: Windows SDK를 설치하고 Unity를 다시 시작합니다.
- `The package version is invalid`: Unity Version을 숫자 1~4개와 점만 사용해 입력합니다. 예: `0.2.3`.
- `.msix` 더블클릭 설치 실패: 무서명 제출본에는 정상입니다. 로컬은 `Add-AppxPackage -Register`, 외부 배포는 Store 인증 링크를 사용합니다.
- Partner Center에서 identity mismatch: 이 문서의 Name/Publisher와 Partner Center의 **Product identity** 값을 다시 비교합니다.
