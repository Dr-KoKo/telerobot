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
| `cmd.charge` | 충전 | Charge |

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
- Battery warning callout for US5.2 reuses `radio.battery_warning` verbatim.

## Acceptance

- [ ] All 8 radio strings present verbatim and byte-exact to the spec.
- [ ] No player-facing Korean string is hard-coded in a MonoBehaviour/adapter.
- [ ] Displayed text equals spec text with no paraphrase/translation/shortening.
