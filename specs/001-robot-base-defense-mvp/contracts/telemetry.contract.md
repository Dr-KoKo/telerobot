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
| `simProfileId` | `Novice`/`Baseline`/`Skilled` — **required for simulation events**; `null` for runtime play (reproducibility is keyed on `seed × profile`) |
| `phase` | 1/2/3, or `null`/`session` for session-level events |
| `timestamp` / `simTime` | wall time (runtime) or sim clock (sim) |

## Required event set (Constitution VIII + recorded feature exception)

`session_started`, `session_ended`, `phase_started`, `phase_cleared`, `phase_failed`, `zombie_spawned`, `zombie_killed`, `base_damaged`, `player_damaged`, `player_died`, `robot_battery_changed`, `robot_auto_charge_started`, `robot_disabled`, `ripper_attacked_robot`, `upgrade_selected`, `route_pressure_sampled`, `simulation_run_completed`.

Constitution VIII still names `robot_charge_commanded`. The user-authorized 2026-07-22 removal of the manual Charge command makes that identifier semantically false, so this feature replaces it one-for-one with `robot_auto_charge_started`. The exception rationale, impact, and follow-up are recorded in `plan.md` Complexity Tracking; the constitution itself is intentionally unchanged pending separate approval. Consumers MUST count charge starts from the replacement event and MUST NOT sum both names.

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
| `robot_destroyed` | robotId, phase | Haetae lost to HP-0 (FR-081; distinct from battery Depleted); emitted **once** per destruction; next-phase restore is not re-emitted here |
| `medical_robot_destroyed` | phase, simTime | Phase-3 rhythm change; zone lost, no regen (FR-107) |
| `robot_auto_charge_started` | robotId | Base-zone automatic charge starts |
| `ripper_attacked_robot` | robotId, batteryDrained=5 | Ripper hits on robots |
| `upgrade_selected` | upgradeId, rewardStep | upgrade choices |
| `grenade_used` | center, affectedCount | grenade usage |
| `ammo_resupplied` | supplyKind∈{Safe,Risky} | ammo resupply usage by point |
| `barrier_damaged` / `barrier_destroyed` | routeId, hp | barrier damage/destruction (if upgrade #6) |
| `route_pressure_sampled` | routeId, aliveCount, distanceToBase | route pressure over time |
| `simulation_run_completed` | runId, seed, **simProfileId**, dataVersion, summary metrics | deterministic sim summary |

## Sampling cadence (required for deterministic diffs — from `TelemetryConfig`)

Sampled/continuous events MUST have a defined, **sim-clock-based** cadence (never wall-time), so two runs of the same `seed × profile` emit identical sample streams:

| Event | Emit rule | Config field |
|-------|-----------|--------------|
| `base_hp_sampled` | every `sampleIntervalSeconds` (default 1.0) + at phase end | `sampleIntervalSeconds` |
| `route_pressure_sampled` | every `routePressureSampleIntervalSeconds` (default 2.0) | `routePressureSampleIntervalSeconds` |
| `robot_battery_changed` | **OnThresholdCrossing** (band/warning) **+ EveryNSeconds** (`batteryEmitIntervalSeconds`, default 1.0) — NOT per frame | `batteryEmitPolicy`, `batteryEmitIntervalSeconds` |

## Invariants

- Events tied to not-yet-implemented systems MAY be marked not-applicable in a partial milestone but MUST be added when that system enters scope (Constitution VIII) — e.g. `barrier_*` only once Emergency Barrier exists; `ripper_attacked_robot` from Phase 3.
- Names are identifiers, MAY be extended, MUST NOT be silently dropped.
- Same `seed` + `dataVersion` (+ `profile`) ⇒ identical event stream for a sim run, **including sample timing** (reproducibility, Constitution IV).

## Acceptance

- [x] All required events emit with required fields, applying the recorded `robot_charge_commanded` → `robot_auto_charge_started` substitution.
- [x] Defeat reason distinguishes base-destroyed vs player-death (supports SC-012).
- [x] Sim run writes a telemetry file keyed by `seed + simProfileId + dataVersion`; re-running the same triple reproduces it byte-for-byte.
- [x] `robot_destroyed` / `medical_robot_destroyed` emit once per destruction; FR-081 next-phase restore is observable in state, not a duplicate destroy event.
