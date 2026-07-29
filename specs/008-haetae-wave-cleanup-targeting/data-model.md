# Data Model: Haetae Wave Cleanup Targeting

This feature adds no persisted entity or tunable configuration. It formalizes one
derived state and one pure decision.

## Derived state: SpawnScheduleComplete

| Field | Type | Source | Rule |
|-------|------|--------|------|
| `hasSchedule` | boolean | Current phase spawn queue | Queue exists |
| `nextSpawnIndex` | integer | Runtime/simulation phase loop | Number of emitted entries |
| `scheduledCount` | integer | Current phase spawn queue | Total scheduled entries |
| `isComplete` | boolean | Derived | `hasSchedule && nextSpawnIndex >= scheduledCount` |

`isComplete` does not become true while spawning is merely paused by the
maximum-alive cap.

## Decision input: Robot target eligibility

| Field | Type | Meaning |
|-------|------|---------|
| `command` | `RobotCommand` | Current player command |
| `assignedRoute` | `RouteId` | Robot's commanded route |
| `candidateRoute` | `RouteId` | Zombie's originating route |
| `spawnScheduleComplete` | boolean | Derived phase cleanup state |

## Route eligibility transition table

| Command | Same route | Cross-route, pending spawns | Cross-route, all spawned |
|---------|------------|-----------------------------|--------------------------|
| Defend Position | Eligible | Ineligible | Eligible |
| Patrol Route | Eligible | Ineligible | Ineligible |
| Return to Base | Existing state-machine behavior | Existing behavior | Existing behavior |

Route eligibility is necessary but not sufficient. Runtime defend acquisition
continues to use the existing base-relative defend leash, while non-defend
acquisition retains the robot-relative detection radius. Availability and battery
state remain owned by the Haetae actor.

## State flow

```text
Scheduled entries pending
    └─ Defend/Patrol acquisition stays assigned-route-only

Last scheduled entry emitted
    └─ SpawnScheduleComplete = true
        ├─ Defend may acquire a valid survivor from any route
        └─ Patrol stays assigned-route-only

Final survivor killed
    └─ Alive count reaches zero
        └─ Existing phase clear evaluation advances the session
```

## Invariants

- Spawn completion never implies that the field is clear.
- Cleanup never expands the defend leash or changes non-defend detection radius.
- Patrol never gains cross-route acquisition.
- No target rule bypasses destroyed, disabled, recovery, charging, command, or
  combat availability rules.
- Runtime uses the pure route eligibility decision, and deterministic rule tests
  cover the complete transition matrix.
