# Contract: Acceptance and Balance Validation

**Feature**: `002-haetae-build-progression`  
**Validation codes**: **E** EditMode/unit, **P** PlayMode/integration, **S** deterministic simulation, **Q** quickstart/manual playtest

## Acceptance Scenario Map

### US1 — 해태별 독립 성장

| Scenario | Methods | Verification |
|----------|---------|--------------|
| 1 both start level 1 / XP 0 / General | E, P | separate progression objects initialized at session creation |
| 2 only Haetae 1 contributed | E, P | Haetae 1 receives full reward; Haetae 2 unchanged |
| 3 both contributed | E, P, S | both receive the full reward in stable ID order |
| 4 player-only kill | E, P, S | no Haetae XP event/state change |
| 5 reward crosses/overshoots threshold | E, P, S | preserve overflow; level 2; ready true; other robot unchanged |
| 6 post-level-2 reward crosses another threshold | E, P, S | cumulative XP and level 3+ advance; no repeated ready event |

### US2 — 레벨 2 전문화 선택

| Scenario | Methods | Verification |
|----------|---------|--------------|
| 1 only ready robot receives three choices; combat continues | P, Q | target ID visible; three roles; `Time.timeScale == 1`; spawn/AI advance |
| 2 selecting Haetae 1 does not mutate Haetae 2 | E, P | independent state diff |
| 3 mixed Melee/Ranged selection | E, P, S | both choices coexist |
| 4 same specialization on both | E, P, S | duplicate role allowed |
| 5 selected role is immutable for session | E, P | second selection rejected without mutation |
| 6 new session resets previous choice | E, P | fresh progression state |

### US3 — 세 가지 전투 역할

| Scenario | Methods | Verification |
|----------|---------|--------------|
| 1 Melee approaches, uses dash/bite, and cleaves | E, P, S | movement intent Approach; melee attack; 2–3 valid clustered targets |
| 2 Ranged maintains distance and uses ranged primary attack | E, P, S | Hold/Retreat in configured band; no normal dash |
| 3 Balanced harasses then transitions to melee | E, P, S | ranged result at distance; dash/bite inside switch range |
| 4 different roles obey same command/route | P, S | no command/route mutation; different combat decisions |
| 5 all roles remain subject to battery/Ripper/destroy rules | E, P, S | no bypass of Disabled/Destroyed/charge state machine |

### US4 — 성장 상태와 비차단 선택

| Scenario | Methods | Verification |
|----------|---------|--------------|
| 1 HUD shows different HP/battery/current-level XP for correct robots and stable selected/unselected layout | P, Q | two ID-bound labeled bar triplets; identical three-line row structure |
| 2 level notification does not pause | P | `Time.timeScale == 1`; simulation/runtime tick advances |
| 3 readiness survives phase transition | E, P, S | ready remains true after `NextPhase` |
| 4 simultaneous readiness permits separate targeting | P | panel switches target; one choice never applies to both |
| 5 selected name/role visibly attached to robot | P, Q | exact role name and distinct combat cue |

### US5 — 페이즈 보상에서 지속 성장으로 전환

| Scenario | Methods | Verification |
|----------|---------|--------------|
| 1 Phase 1/2 clear shows no old upgrade view | E, P, S | transition is `NextPhase`; old view closed/not instantiated; no upgrade event |
| 2 base recovery, route open, next phase remain | E, P, S | event/state sequence preserved |
| 3 mid-phase level-up can be selected immediately | P, Q | readiness and panel available before phase end |

### US6 — 전문화 빌드를 활용하는 후반 페이즈

| Scenario | Methods | Verification |
|----------|---------|--------------|
| 1 Phase 1–3 pacing remains unchanged | E, P | original composition, cadence, group, cap, and route-opening assertions remain exact |
| 2 Phase 3 clear starts Phase 4 | E, P, S | transition is `NextPhase`; all three routes remain open |
| 3 Phase 4–7 continue without upgrades | E, P, S | numeric phase sequence, recovery, no upgrade event/view |
| 4 Phase 8 clear wins | E, P, S | only final configured phase returns `Victory` |
| 5 late-phase death still loses | E, P, S | base/player death takes priority over clear |
| 6 Phase 3 medical announcement is not repeated | P | `radio.phase3` once; distinct `radio.phase4`–`radio.phase8` keys thereafter |

### US7 — 레벨 3+ 반복 강화

| Scenario | Methods | Verification |
|----------|---------|--------------|
| 1 level 3 grants one point | E, P, S | matching robot gains one unspent point; other robot unchanged |
| 2 specialization is required | E, P | pre-specialization spend rejected without mutation |
| 3 repeat one choice | E, P | same choice increases the same rank once per point |
| 4 exact combat modifiers | E, P, S | Power +10% damage; Armor/Efficiency -8% per rank; Attack Speed -10% Dash/Bite/Ranged interval per rank; all reductions floor at 0.50 |
| 5 shared B panel remains non-blocking | P | specialization/mastery mode switches; `Time.timeScale == 1` |
| 6 deterministic auto-spend | E, S | stable round-robin ranks/events; spawn RNG unchanged |

## Edge-Case Map

| Edge case | Methods | Verification |
|-----------|---------|--------------|
| both robots level on one shared kill | E, P, S | two ordered XP/level/ready sequences |
| one reward exceeds threshold | E | overflow is preserved; derived level matches cumulative XP |
| later level boundary crossed | E, P, S | level 3+ advances; specialization-ready remains single-shot |
| multi-level reward above level 2 | E, S | one point for every crossed level; points accumulate |
| specialized robot has multiple points | E, P, S | repeated legal spend until zero; no cross-robot mutation |
| contributor destroyed before later kill | E, P, S | destroyed robot still receives reward |
| ready robot becomes Disabled/Destroyed | E, P | ready persists; selection allowed; combat profile visible after recovery |
| unspent choice at Phase 3/session end | E, P | result not blocked; state discarded on next session |
| lethal hit and phase clear same update | E, P, S | progression events precede kill/phase clear |
| robot ID/state isolation | E, S | no cross-mutation across 100 iterations |
| same role selected twice across robots | E, P | valid; same robot reselection invalid |
| repeated damage by same robot | E, P | contributor stored once; one reward |
| duplicate death callback | E, S | no duplicate XP/events |
| specialization after selection while Destroyed | E, P | selection persists through restore and presentation reapplies |
| old upgrade assets still serialized | E, P | no active catalog mapping or reachable UI |

## Success Criteria Map

| Criterion | Methods | Gate |
|-----------|---------|------|
| SC-001 independent mutation 0/100 | E, S | 100 iterations, zero cross-robot changes |
| SC-002 first level 2 within Phase-2 +60s | S | at least 16/20 eligible Baseline runs |
| SC-003 both level 2 before Phase 3 | S | at least 80% of Phase-3-eligible Baseline runs; eligible means Phase 2 cleared and Phase 3 entered |
| SC-004 choose within 15s without instruction | Q | at least 90% of first-time testers |
| SC-005 identify role from combat | Q | at least 80% |
| SC-006 role changes assignment/decision | Q | at least 70% |
| SC-007 each role ≥20% of 30 choices | Q + telemetry | local event aggregation |
| SC-008 session remains 10–15 minutes | Q | time an uninterrupted manual Baseline session from playable Phase 1 start through Victory or Defeat; accelerated simulation duration is not accepted as evidence |
| SC-009 phase transition works without upgrades | E, P, S | 100% scenario pass |
| SC-010 reproducible progression | E, S | identical state and telemetry |
| SC-011 dynamic Phase 3/7/8 transitions | E, P, S | 100% scenario pass |
| SC-012 Phase 1–3 pressure unchanged | E, P | exact composition/group/cap assertions |
| SC-013 continuing levels | E, P, S | level 3+ progression and one-time specialization unlock |
| SC-014 medical radio accuracy | P | medical deployment key exactly once in Phase 3 |
| SC-015 per-Haetae mastery isolation | E, P, S | point/rank mutation affects only selected robot |
| SC-016 labeled current-level XP bars | P, Q | fill and inside fraction match current interval; boundary shows `0 / interval` |
| SC-017 stable row layout and labeled HP bars | P, Q | selected/unselected rows have equal line count; HP fill/fraction match state |
| SC-018 labeled battery bars | P, Q | battery fill/fraction match state; warning color follows existing thresholds |

## Deterministic Simulation Matrix

### Seeds

- Smoke: `1001, 1002, 1003`
- Balance: `1101` through `1120` inclusive
- Golden: `9001`

### Inputs

- Fixed timestep: existing `1/60` baseline
- Player profiles: Novice, Baseline, Skilled
- Run input: `SimulationRunOptions.SpecializationLoadout`; use the selected profile's ordered default pair only when this override is absent
- Ordered specialization loadouts:
  - Melee/Melee
  - Melee/Ranged
  - Melee/Balanced
  - Ranged/Melee
  - Ranged/Ranged
  - Ranged/Balanced
  - Balanced/Melee
  - Balanced/Ranged
  - Balanced/Balanced

Mixed-role results may also be aggregated into six unordered combinations, but ordered results remain available because robot route assignment can make order meaningful.

### Reproducibility

For the same seed × player profile × ordered specialization loadout × data version:

- spawn composition/order/route assignment is identical;
- progression state and events are identical;
- specialization choice does not alter spawn RNG;
- session summary and JSONL output are identical.

## Regression Gates

The following implemented behavior is retained unchanged:

- rifle random recoil and recovery;
- muzzle/impact/hit/death feedback;
- phase group sizes `3–4 / 3–5 / 4–6`;
- concurrent caps `15 / 20 / 24`;
- eight contiguous phases with Phase 4–8 using all three routes, group `4–6`, interval `3s`, and cap `24`;
- continuous spawning pauses at cap and resumes after deaths;
- three robot commands and individual/all selection;
- battery drain/charge/Disabled/Recovery;
- HP Destroyed and next-phase restore;
- Ripper and medical robot rules;
- route opening, base recovery, victory/defeat;
- Windows build and standalone smoke.

## Acceptance

- Every spec acceptance scenario and edge case above has at least one validation method.
- Human-only outcomes are not falsely treated as automated assertions.
- All balance-affecting behavior has EditMode or deterministic-simulation coverage.
- No regression gate changes recoil or spawn tuning as part of this feature.
