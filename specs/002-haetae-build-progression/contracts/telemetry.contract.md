# Contract: Haetae Progression Telemetry

**Feature**: `002-haetae-build-progression`  
**Contract type**: Development-only domain events bridged to runtime and deterministic-simulation JSON Lines  
**Data version**: `mvp-2.0.0`

## Required Envelope

Every persisted event retains the project envelope:

| Field | Rule |
|-------|------|
| `buildVersion` | build/application version |
| `dataVersion` | `mvp-2.0.0` for this schema |
| `sessionId` or `runId` | stable join key |
| `seed` | session/simulation seed |
| `simProfileId` | profile for simulation; null/empty for runtime play |
| `phase` | 1/2/3 or session value |
| `simTime` / timestamp | fixed sim time for simulation; runtime session time for play |
| `eventName` | identifier below |
| `payload` | event-specific key/value map |

## Minimum Event Set

The inherited MVP events remain required where the corresponding system remains active:

`session_started`, `session_ended`, `phase_started`, `phase_cleared`, `phase_failed`, `zombie_spawned`, `zombie_killed`, `base_damaged`, `player_damaged`, `player_died`, `robot_battery_changed`, `robot_auto_charge_started`, `robot_disabled`, `ripper_attacked_robot`, `route_pressure_sampled`, `simulation_run_completed`.

Recorded constitution exceptions:

- `robot_charge_commanded` is replaced by `robot_auto_charge_started` because manual Charge does not exist.
- `upgrade_selected` is not applicable in `mvp-2.0.0` because FR-027 removes the upgrade selection system. The growth-choice replacement is `haetae_specialization_selected`.

Consumers must branch by `dataVersion` and must not sum old upgrade events with new specialization events.

## New Progression Events

### `haetae_xp_gained`

Emitted once per eligible Haetae contributor when a zombie death is rewarded.

| Payload | Type | Rule |
|---------|------|------|
| `robotId` | string | contributing Haetae |
| `zombieId` | string | defeated zombie |
| `zombieType` | Runner/Bruiser/Ripper | reward source |
| `rewardAmount` | int | configured full reward |
| `appliedAmount` | int | amount actually added after level cap |
| `xpBefore` | int | before award |
| `xpAfter` | int | after award |
| `levelBefore` | int | 1 or 2 |
| `levelAfter` | int | 1 or 2 |

An already-capped robot may omit this event because the feature stops XP acquisition at level 2. If emitted for diagnostics, it must have `appliedAmount = 0`; one policy must be selected and used consistently by runtime and simulation. Planned default: omit zero-applied award events.

### `haetae_level_reached`

Emitted exactly once per robot when crossing to level 2.

| Payload | Type |
|---------|------|
| `robotId` | string |
| `fromLevel` | 1 |
| `toLevel` | 2 |
| `experience` | threshold value |
| `specializationReady` | true |

### `haetae_specialization_ready`

Emitted immediately after `haetae_level_reached`.

| Payload | Type |
|---------|------|
| `robotId` | string |
| `level` | 2 |

### `haetae_specialization_selected`

Emitted only after a successful explicit player/simulated-player choice.

| Payload | Type |
|---------|------|
| `robotId` | string |
| `specialization` | Melee/Ranged/Balanced |
| `level` | 2 |
| `readyDurationSeconds` | non-negative time from ready event to selection |

## Existing Event Changes

### `zombie_killed`

Retains existing type/source payload and adds:

| Payload | Rule |
|---------|------|
| `contributingHaetaeCount` | 0–2 |
| `contributingHaetaeIds` | stable ordinal-delimited IDs |

XP/level/ready events are published before this kill event.

### `phase_cleared`

No reward-step or upgrade payload. It is followed directly by the next phase start for Phase 1/2.

### `simulation_run_completed`

Adds per-robot and specialization metrics:

- `haetae1Level2Phase`, `haetae1Level2SimTime`
- `haetae2Level2Phase`, `haetae2Level2SimTime`
- `firstLevel2WithinPhase2SixtySeconds`
- `bothLevel2BeforePhase3`
- `haetae1Specialization`, `haetae2Specialization`
- damage dealt, kills contributed, combat battery spent, Disabled count, Destroyed count per Haetae
- existing duration, result, defeat reason, phase clears, base HP, and spawn-pressure metrics

## Event Ordering

For one lethal hit:

1. `haetae_xp_gained` in ordinal robot-ID order.
2. `haetae_level_reached` and `haetae_specialization_ready` immediately after the matching XP event.
3. `zombie_killed`.
4. `phase_cleared` if that kill completed the phase.

Specialization choice events occur only on a later explicit command, even if simulation chooses in the same fixed step after readiness.

## Determinism and Sampling

- Progression events are event-driven and are not sampled.
- Existing base/route/battery sampling cadences remain unchanged and use the simulation clock.
- Same seed, data version, player profile, specialization loadout, and input order must produce identical event streams.
- Specialization selection must not advance the spawn RNG.
- Golden telemetry for `mvp-1.4.5` is historical; `mvp-2.0.0` receives a new golden snapshot.

## Playtest Aggregation

The local playtest report derives:

- time from Phase 2 start to first level 2;
- whether both Haetae reached level 2 before Phase 3;
- selection latency;
- specialization pick distribution;
- mixed/same build combinations;
- damage, battery, disable, and destruction outcomes per specialization.

SC-004 through SC-006 still require survey/observation input; telemetry supplies timing and choices, not human comprehension by itself.

## Acceptance

- Required new events are declared in both generated assets and builder defaults.
- Events carry the required envelope and payload.
- A zombie rewards each contributor once and only once.
- Duplicate death processing emits no duplicate progression events.
- Retired `upgrade_selected` is absent from `mvp-2.0.0` sessions.
- Identical simulation runs reproduce progression and summary records byte-for-byte.
