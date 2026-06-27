# Contract: Telemetry Event Schema

**Feature**: `001-robot-base-defense-mvp` | Non-REST contract. Development-only local telemetry (Constitution VIII). Same event interface used by runtime gameplay and the deterministic simulation harness. Sink = local structured files (JSON Lines / CSV), dev flag only; no external analytics (research.md §8).

## Required fields on every event

Per Constitution VIII, every event MUST include:

| Field | Notes |
|-------|-------|
| `buildVersion` | build/app version |
| `dataVersion` | config-asset snapshot version |
| `sessionId` or `runId` | join key (`runId` for sim runs) |
| `seed` | deterministic seed |
| `phase` | 1/2/3, or `null`/`session` for session-level events |
| `timestamp` / `simTime` | wall time (runtime) or sim clock (sim) |

## Constitution minimum event set (MUST emit)

`session_started`, `session_ended`, `phase_started`, `phase_cleared`, `phase_failed`, `zombie_spawned`, `zombie_killed`, `base_damaged`, `player_damaged`, `player_died`, `robot_battery_changed`, `robot_charge_commanded`, `robot_disabled`, `ripper_attacked_robot`, `upgrade_selected`, `route_pressure_sampled`, `simulation_run_completed`.

## Spec-specific events / payloads (MUST emit for this MVP)

| Event | Key payload | Covers planning telemetry requirement |
|-------|-------------|----------------------------------------|
| `session_started` | seed, dataVersion | deterministic simulation seed |
| `session_ended` | durationSeconds, result, defeatReason∈{BaseDestroyed,PlayerDeath,null} | session duration; defeat reason |
| `phase_started` / `phase_cleared` / `phase_failed` | phase, simTime, result | phase start/end timestamps; clear/fail |
| `base_hp_sampled` | phase, hp, simTime | base HP over time / at phase end |
| `player_hp_at_phase_end` | phase, hp | player HP at phase end |
| `robot_battery_changed` | robotId, value, state | robot battery over time + threshold events |
| `robot_disabled` | robotId | robot Depleted count (count of events) |
| `robot_charge_commanded` | robotId | Charge command count |
| `ripper_attacked_robot` | robotId, batteryDrained=5 | Ripper hits on robots |
| `upgrade_selected` | upgradeId, rewardStep | upgrade choices |
| `grenade_used` | center, affectedCount | grenade usage |
| `ammo_resupplied` | supplyKind∈{Safe,Risky} | ammo resupply usage by point |
| `barrier_damaged` / `barrier_destroyed` | routeId, hp | barrier damage/destruction (if upgrade #6) |
| `route_pressure_sampled` | routeId, aliveCount, distanceToBase | route pressure over time |
| `simulation_run_completed` | runId, seed, summary metrics | deterministic sim summary |

## Invariants

- Events tied to not-yet-implemented systems MAY be marked not-applicable in a partial milestone but MUST be added when that system enters scope (Constitution VIII) — e.g. `barrier_*` only once Emergency Barrier exists; `ripper_attacked_robot` from Phase 3.
- Names are identifiers, MAY be extended, MUST NOT be silently dropped.
- Same `seed` + `dataVersion` ⇒ identical event stream for a sim run (reproducibility, Constitution IV).

## Acceptance

- [ ] All constitution-minimum events emit with required fields.
- [ ] Defeat reason distinguishes base-destroyed vs player-death (supports SC-012).
- [ ] Sim run writes a telemetry file keyed by seed; re-running same seed reproduces it.
