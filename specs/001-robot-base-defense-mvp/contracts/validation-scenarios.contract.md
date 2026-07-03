# Contract: Validation Scenarios & Deterministic Simulation Parameters

**Feature**: `001-robot-base-defense-mvp` | Non-REST contract. Maps **every** US1–US5 acceptance scenario to a validation method (Constitution V — no silent omissions) and fixes deterministic-simulation parameters (Constitution IV). Validation method codes: **E** = EditMode unit, **P** = PlayMode integration, **S** = deterministic simulation, **Q** = quickstart/manual.

## Acceptance-scenario → validation map

### US1 — Core combat loop (Phase 1)
| Scenario | Method | Check |
|----------|--------|-------|
| 1 boot radio + North Road only open | P, Q | `radio.game_start` fires at boot, then `radio.phase1` ("감염체 접근. 북쪽 도로 방어 준비.") fires at Phase 1 start (order asserted); openRoutes = {NorthRoad} |
| 2 body ~3 / head 1–2 kills Runner | E | damage math: 30 vs 90 hp; headshot 2.5× |
| 3 robot kills Runner ~1–2 s | P, S | engage timing within band |
| 4 Runner at base → base −8 | E, P | base damage = 8 |
| 5 base HP 0 → defeat | E, P | defeat = BaseDestroyed |
| 6 player HP 0 → defeat | E, P | defeat = PlayerDeath |
| 7 all spawned + cleared + base alive → Phase 1 clear | E, P, S | 7-step transition |

### US2 — Robot command & battery
| Scenario | Method | Check |
|----------|--------|-------|
| 1 menu shows exactly 4 commands | P | CommandConfig = 4 |
| 2 combat drains 2.5/s (idle 0.3, patrol 0.8) | E | drain rates |
| 3 Low Power 11–30 → move −15%, attack −10% | E | mults applied |
| 4 Critical 1–10 → clear warning | E, P | warning fired |
| 5 Depleted 0 → Disabled (no move/attack) | E, P | state machine |
| 6 charge +4/s, no combat while charging | E, P | charge + FR-097 |
| 7 charge-vs-fight tradeoff exists | S, Q | sim shows pressure |
| 8 Disabled 5 s → Recovery 0.5/s → battery 5 → auto return-to-charge, no attack | E, P | depletion/recovery (PLANNING values) |

### US3 — Phase 2 routes & reward
| Scenario | Method | Check |
|----------|--------|-------|
| 1 Phase 1 clear → 3 upgrades, pick exactly 1 | E, P | Offer=3 of 9, select 1; **2nd reward step excludes the already-selected id (no re-offer, no stacking)** |
| 2 base +15% (150) on clear | E | recovery math |
| 3 Phase 2 → East Alley highlight + radio + 2 routes active | P, Q | RouteOpened, RadioEvent(Phase2) |
| 4 Bruiser hits base → −60 | E, P | base damage = 60 |
| 5 robot fixed on a route → useful but drains battery | S | battery telemetry |

### US4 — Phase 3 Ripper & Medical
| Scenario | Method | Check |
|----------|--------|-------|
| 1 Phase 2 clear → 2nd (max-2) upgrade | E, P | selection count ≤ 2 |
| 2 Phase 3 → South Tunnel open, radio, 3 routes, medical deployed | P, Q | RadioEvent(Phase3), medical exists |
| 3 player in 6 m & < max HP → +8 HP/s | E, P | heal math |
| 4 Ripper visually/audibly distinct + icon + callout | P, Q | distinct A/V (FR-047) |
| 5 Ripper hits robot → normal dmg + battery −5 | E, P | FR-045 |
| 6 ignored Ripper → robot disabled | S, P | battery depletion |
| 7 all cleared + base ≥1 → victory | E, P, S | GameWon |

### US5 — HUD / warnings / radio
| Scenario | Method | Check |
|----------|--------|-------|
| 1 HUD shows all 7 elements | P, Q | HudConfig elements |
| 2 battery <25% → yellow flash + callout | E, P | WarningConfig |
| 3 battery <10% → red flash + urgent callout | E, P | WarningConfig |
| 4 base ≤30% → edge warning + alarm | E, P | base warning |
| 5 new route open → highlight + radio | P | RouteOpened + RadioEvent |

> Every acceptance scenario above has at least one validation method. No scenario is left unvalidated (Constitution V satisfied).

## Edge-case coverage (spec Edge Cases)
Both robots Depleted simultaneously (S, P); Disabled robot return path (E, P — US2.8); base hit while charging (P); Haetae destroyed at HP 0 vs battery-Disabled distinction (E, P); medical robot destroyed → zone disabled, no regen (E, P); ammo depletion + risky resupply (P); reload-while-hit exposure (P); threat-budget vs target reconciliation + achievable total ∈ learningTargetTotalRange (E, S); **Phase-3 Ripper spawn count on South Tunnel > other routes (E, S — FR-034 weight matrix)**; cumulative openRoutes P1⊂P2⊂P3 (E); Ripper target-switch when no robot near (E); medical robot damaged only incidentally, not actively targeted (E); upgrade-vs-in-progress-state (E — current-value addition, extended-mag next-reload).

## Deterministic simulation parameters (SimParams asset)

| Param | Value | Source |
|-------|-------|--------|
| seeds (smoke) | {1001, 1002, 1003} | fast reproducibility/regression gate |
| seeds (balance sweep) | {1101, 1102, 1103, 1104, 1105, 1106, 1107, 1108, 1109, 1110, 1111, 1112, 1113, 1114, 1115, 1116, 1117, 1118, 1119, 1120} (20 seeds, fully enumerated) | clear-rate distribution vs SC targets |
| seeds (regression / golden) | {9001} | golden telemetry snapshot |
| fixedStepSeconds | 1/60 (tunable) | research.md §3 |
| movementModel | WaypointMovement (no NavMeshAgent) | research.md §3 |
| simPlayerProfile | Novice \| Baseline \| Skilled (data asset) | scripted player agent — clear-rate is meaningless without it |
| rng | single seeded IDeterministicRng for spawn composition + upgrade offer | research.md §3 |
| reproducibility assert | run same seed × same profile twice → identical telemetry | Constitution IV |
| balance targets (Baseline profile) | session 10–15 min (SC-001); P1 clear ≥90% (SC-002); P2 60–75% (SC-003); P3 35–55% (SC-004) | spec Success Criteria |

## Acceptance

- [ ] Each US1–US5 scenario has a passing validation under its listed method(s).
- [ ] Deterministic suite reproduces identical telemetry for a fixed seed across two runs.
- [ ] Simulation produces telemetry enabling SC-001..004 balance review.
