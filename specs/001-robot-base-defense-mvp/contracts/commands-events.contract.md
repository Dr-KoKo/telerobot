# Contract: Command & Event Interfaces

**Feature**: `001-robot-base-defense-mvp` | Non-REST contract. Defines the interfaces between `Game.Core` (pure rules) and `Game.Runtime` (adapters). Adapters translate Unity input/physics/UI into these calls and render the resulting domain events; the core never references Unity.

## Core math types (plain structs — NO `UnityEngine`)

The core is UnityEngine-free (Constitution III), so all geometry in these interfaces uses **plain C# structs**, not `Vector2`/`Vector3`:

```
CoreVec2   { float x, y }              // 2D input axes
CoreVec3   { float x, y, z }           // world position/direction in core space
WorldPoint = CoreVec3                  // a position in the simulation world
RoutePoint { RouteId routeId; float progress }   // position along a route waypoint chain (arc-distance)
```
Adapters convert between these and `UnityEngine.Vector*` at the boundary; the core never sees Unity types.

## Robot command interface (FR-085, FR-086, FR-087)

```
ICommandInput (issued by adapter → core)
  IssueCommand(robotId, RobotCommand, CommandTarget?)

RobotCommand enum = { DefendPosition, PatrolRoute, ReturnToBase }   // EXACTLY these 3 (FR-085)
CommandTarget   = { RouteId? routeId , WorldPoint? point }   // PatrolRoute requires open routeId; DefendPosition takes point/route (Assumptions)
```
Invariants:
- **Individual selection MUST be supported** and **a select-all toggle MUST be supported** (FR-087). Commands are always applied **per robot**, so the two robots may run different routes/commands.
- Select-all is an adapter/UI convenience: internally it **fans out to one `IssueCommand(robotId, …)` per robot** — the core has no batch command; there is no shared/global robot command state.
- No command outside the 3 may exist; `Charge` is not a command.
- A robot below maximum battery and without a valid target automatically charges inside the base charging radius; detecting a new target interrupts charging before combat resumes (FR-035, FR-097).
- A `Destroyed` robot (FR-081) **ignores commands** until it is restored at next phase start.

## Player intent interface (input abstraction, research.md §2)

```
IPlayerInput (adapter → core/runtime)
  ReadFrame() -> PlayerInputFrame

PlayerInputFrame
  move(CoreVec2), look(CoreVec2), firePressed, fireHeld, reloadPressed,
  grenadePressed, interactPressed, jumpPressed, sprintHeld,
  togglePerspectivePressed, pausePressed

Robot selection/command-menu input remains adapter state and fans out through ICommandInput;
individual selection + select-all are both required by FR-087.
```
Testable: the simulation harness and PlayMode tests inject synthetic intent without hardware.

## Domain command/query surface (current core API, EditMode-tested)

```
CombatRules (static)
  CalculateBulletDamage(config, hitRegion); ApplyDamage/Heal/RecoverBase(...)
  TryFire/BeginReload/TickReload/Resupply(...); ApplyGrenade(config, candidates)
BatterySystem
  Drain(robot, activity, dt, multiplier); Charge(robot, dt, multiplier)
  ApplyRipperHit(robot); TickDisabledRecovery(robot, dt); BandFor/MoveMultiplier/AttackMultiplier(robot)
PhaseSystem
  Evaluate(session, phase, base, player); StartNext(session, nextPhaseConfig)
SpawnSystem / ContinuousSpawnScheduler
  Compose(phaseConfig, rng); Advance(dt, aliveCount, remainingCount)
TargetingSystem (static)
  Select(zombieConfig, candidates)   // per ZombieDef.targetPriority
RobotCommandSystem / RobotSelectionModel
  IssueCommand(robot, command, route); SelectOnly(robotId); ToggleAll(selected); IsSelected(robotId)
RobotAttackSystem / RobotDurabilitySystem
  Advance/BeginEngagement/EndEngagement(...); ApplyDamage(...); RestoreAtPhaseStart(...)
UpgradeSystem
  Offer(rng, selectedUpgradeIds) -> 3 of (9 minus already-selected); Apply(...)
  // exclude already-selected ids; no stacking (SessionState.SelectedUpgrades)
```

## Domain events (core → adapter; also feed telemetry)

```
DomainEvent { Name, SimTime, Phase, Payload<string,string> }
IDomainEventSink.Publish(DomainEvent)
DomainEventBus : IDomainEventSink
  EventPublished, History, Publish(event), Clear()
TelemetryBridge
  subscribes to EventPublished and writes a TelemetryRecord with required envelope fields
```

Canonical event identifiers are lower snake case and data-enabled through `TelemetryConfig`; the full required set and payloads are normative in `telemetry.contract.md`. Gameplay/UI additions include `radio_event`, `route_opened`, `base_warning`, `battery_warning`, `camera_perspective_changed`, `player_jumped`, `player_hit_confirmed`, `game_paused`, `session_restarted`, and `returned_to_main_menu`. Robot lifecycle events distinguish `robot_disabled` (battery) from `robot_destroyed` (HP) and use `robot_auto_charge_started` for the automatic base-zone transition.

Invariants:
- Pure numeric/state rules stay in `Game.Core`; adapters render events and may perform scene-dependent physics/distance queries before invoking core services.
- Each event maps to zero or more telemetry records (see telemetry.contract.md) and/or radio/string events (strings.contract.md).
- `radio_event` triggers fire at their gameplay milestone (not deferred): GameStart at boot, Phase1/2/3 on route/phase start, BatteryWarning on battery threshold, BaseDanger on base warning, PhaseClear on phase clear, Victory on win.

## Acceptance

- [x] Core exposes the pure types/services above with no `UnityEngine` types in signatures.
- [x] Command menu can issue exactly the 3 commands and nothing else.
- [x] Every domain event has at least one subscriber path to HUD/audio and/or telemetry.
