# Contract: Player-Facing String Keys (Verbatim Korean)

**Feature**: `001-robot-base-defense-mvp` | Non-REST contract. Player-facing strings are **data** and rendered **verbatim** (Constitution VI). Internal keys are English; **displayed Korean MUST NOT be paraphrased, translated, romanized, shortened, or "cleaned up."** Greybox/debug captions MUST NOT substitute these (Constitution VII). Source: spec FR-130, FR-085, FR-031, FR-113.

## Radio / sound events (FR-130) — exact, do not alter

| key | Korean text (verbatim) | Trigger milestone |
|-----|------------------------|-------------------|
| `radio.game_start` | 텔레 로봇팀, 출격하라. | game boot (FR-001) |
| `radio.phase1` | 감염체 접근. 북쪽 도로 방어 준비. | Phase 1 start |
| `radio.phase2` | 동쪽 골목에서 추가 접근 신호 감지. | Phase 2 / East Alley open |
| `radio.phase3` | 남쪽 터널 개방. 메디컬 로봇 투입. | Phase 3 / South Tunnel open |
| `radio.battery_warning` | 해태 1호, 배터리 위험. | robot battery warning (FR-123) |
| `radio.base_danger` | 거점 방어선 붕괴 임박. | base HP ≤30% (FR-125) |
| `radio.phase_clear` | 위협 제거. 재정비 단계 진입. | phase clear |
| `radio.victory` | 거점 생존 확인. 작전 성공. | victory (FR-004) |

## Robot command labels (FR-085) — verbatim

| key | Korean (verbatim) | internal id |
|-----|-------------------|-------------|
| `cmd.defend` | 거점 사수 | DefendPosition |
| `cmd.patrol` | 경로 순찰 | PatrolRoute |
| `cmd.return` | 기지 복귀 | ReturnToBase |

## Robot selection labels — verbatim

| key | Korean (verbatim) | Trigger |
|-----|-------------------|---------|
| `hud.all_robots` | 전체 로봇 | `3` select-all toggle / command-menu selection summary |

## HUD and session labels — verbatim

| key | Korean text |
|-----|-------------|
| `hud.base` / `hud.phase` / `hud.player` | 거점 / 페이즈 / 플레이어 |
| `hud.ammo` / `hud.grenade` | 탄약 / 수류탄 |
| `hud.routes` / `hud.command` / `hud.target` | 경로 경보 / 로봇 명령 / 대상 경로 |
| `hud.upgrade` / `hud.ripper` | 업그레이드 선택 / 리퍼 출현 |
| `hud.victory` / `hud.defeat` | 작전 성공 / 작전 실패 |
| `hud.pause` / `hud.resume` / `hud.restart` | 일시정지 / 계속하기 / 다시 시작 |
| `hud.first_person` / `hud.third_person` | 1인칭 / 3인칭 |
| `hud.headshot` / `hud.low_ammo` / `hud.reloading` | 헤드샷 / 탄약 부족 / 재장전 중 |
| `hud.resupply` / `hud.safe_supply` / `hud.risky_supply` | 탄약 보급 / 안전 보급지 / 위험 보급지 |

## Route names (FR-031) — verbatim

| key | Korean (verbatim) | internal id |
|-----|-------------------|-------------|
| `route.north` | 북쪽 도로 | NorthRoad |
| `route.east` | 동쪽 골목 | EastAlley |
| `route.south` | 남쪽 터널 | SouthTunnel |

## Upgrade names (FR-113) — verbatim

| key | Korean (verbatim) | internal id |
|-----|-------------------|-------------|
| `upg.battery` | 고효율 배터리 | high_efficiency_battery |
| `upg.powersave` | 전투 절전 모드 | combat_power_save |
| `upg.dash` | 해태 돌진 강화 | haetae_charge_boost |
| `upg.chargefast` | 충전소 고속화 | charge_station_speedup |
| `upg.armor` | 거점 장갑 보강 | base_armor |
| `upg.barrier` | 긴급 방벽 | emergency_barrier |
| `upg.pierce` | 관통탄 | piercing_rounds |
| `upg.mag` | 확장 탄창 | extended_magazine |
| `upg.recovery` | 응급 회복 프로토콜 | emergency_recovery_protocol |

## Rules

- Stored in `StringTable` ScriptableObject; HUD/audio adapters look up by key and display the exact text.
- MVP MAY pair each key with a placeholder/TTS-stub audio clip; **final VO replacement swaps the clip reference only**, never the text.
- Any new player-facing string introduced during implementation MUST be added here as data, not inlined in scene scripts (Constitution VI gate).
- Battery warning callout for US5.2 reuses `radio.battery_warning` verbatim. **Clarify-confirmed:** MVP keeps this single line "해태 1호, 배터리 위험." for either robot; the actually-endangered robot (1호/2호) is disambiguated by the HUD battery widget / `robotId`, not by a per-robot VO line (per-robot lines are post-MVP).

## Main menu and saved settings

| key | Korean text |
|-----|-------------|
| `menu.title` | 텔레 로봇팀, 출격하라 |
| `menu.subtitle` | 세 경로를 방어하고 해태 로봇팀을 지휘하십시오 |
| `menu.play` / `menu.settings` / `menu.quit` | 게임 시작 / 설정 / 게임 종료 |
| `menu.main` | 시작 화면으로 |
| `menu.controls_hint` | WASD 이동 · 마우스 조준 · V 시점 전환 · Space 점프 |
| `settings.title` | 설정 |
| `settings.sensitivity` | 마우스 감도 |
| `settings.master_volume` / `settings.effects_volume` | 전체 음량 / 효과음 음량 |
| `settings.resolution` / `settings.fullscreen` | 해상도 / 전체 화면 |
| `settings.default_perspective` | 기본 시점 |
| `settings.apply` / `settings.cancel` | 저장하고 적용 / 취소 |
| `settings.on` / `settings.off` | 켜기 / 끄기 |

## Acceptance

- [x] All 8 radio strings present verbatim and byte-exact to the spec.
- [x] No player-facing Korean string is hard-coded in a MonoBehaviour/adapter.
- [x] Displayed text equals spec text with no paraphrase/translation/shortening.
