# itch.io 배포 체크리스트

빌드 버전: `{{BUILD_VERSION}}`  
업로드 파일: `{{ARCHIVE_NAME}}`

## 처음 한 번만 설정

1. <https://itch.io>에서 계정을 만들고 Dashboard의 **Create new project**를 누른다.
2. 제목은 `텔레로봇 MVP 플레이테스트`, Kind of project는 **Downloadable**로 설정한다.
3. Release status는 **In development**, 가격은 **No payments** 또는 무료 다운로드로 설정한다.
4. 간단한 게임 설명, 조작법, 스크린샷 3장 이상과 피드백 설문 링크를 넣는다.
5. Visibility는 다음 중 하나를 고른다.
   - 주변 사람만: **Restricted**와 비밀번호
   - 링크를 받은 사람: **Public**에서 검색/탐색 비노출

Draft 상태에서는 다른 사람이 정상적으로 내려받을 수 없으므로 테스트를 시작하기 전에 접근 설정을 확인한다.

## 새 빌드 업로드

1. `Builds/Distribution/{{ARCHIVE_NAME}}`을 업로드한다.
2. 업로드 파일의 플랫폼에서 **Windows**를 체크한다.
3. 파일 설명에 `Windows 64-bit · v{{BUILD_VERSION}}`를 적는다.
4. 페이지 본문 맨 위에 현재 버전과 피드백 설문 링크를 표시한다.
5. 브라우저의 로그아웃/시크릿 창에서 링크, 다운로드, 압축 해제, 실행까지 한 번 확인한다.

## 테스터에게 보낼 메시지 예시

텔레로봇 MVP 테스트를 부탁드려요.
1. 아래 링크에서 Windows ZIP을 다운로드해 압축을 전부 풀어주세요.
2. START-HERE-KO.txt를 읽고 TelerobotMVP.exe를 실행해 주세요.
3. 플레이 후 페이지의 짧은 설문을 작성해 주세요.
4. Windows 경고가 나오면 다운로드 주소와 제가 보낸 파일이 맞는지 먼저 확인하고, 출처가 불분명하면 실행하지 마세요.

다운로드: [itch.io 링크]
피드백: [설문 링크]

## 업데이트가 잦아진 뒤

처음에는 ZIP을 웹에서 직접 교체하면 충분하다. 업데이트 빈도가 높아지면 itch.io의 butler를 설치하고 다음 형태로 공유 빌드 폴더를 올린다.

```text
butler push Builds/Shareable/Windows 사용자명/프로젝트명:windows-alpha
```

공식 안내:
- 프로젝트 만들기: <https://itch.io/docs/creators/getting-started>
- 접근 제한: <https://itch.io/docs/creators/access-control>
- butler 업로드: <https://itch.io/docs/butler/pushing.html>
