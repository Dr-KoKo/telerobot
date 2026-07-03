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

## Robot command interface (FR-085, FR-086)

```
ICommandInput (issued by adapter → core)
  IssueCommand(robotId, RobotCommand, CommandTarget?)

RobotCommand enum = { DefendPosition, PatrolRoute, ReturnToBase, Charge }   // EXACTLY these 4 (FR-085, FR-140)
CommandTarget   = { RouteId? routeId , WorldPoint? point }   // PatrolRoute requires open routeId; DefendPosition takes point/route (Assumptions)
```
Invariants:
- Robots are individually selectable; a command applies per robot (Assumptions). A "select all" convenience MAY fan out the same command to both.
- No command outside the 4 may exist.
- `Charge` only progresses while the robot is at the charging station; charging robot cannot fight (FR-097).

## Player intent interface (input abstraction, research.md §2)

```
IPlayerInput (adapter → core/runtime)
  Move(CoreVec2), Look(CoreVec2), Fire(bool), Reload(), ThrowGrenade(),
  OpenCommandMenu(), SelectRobot(robotId)
```
Testable: the simulation harness and PlayMode tests inject synthetic intent without hardware.

## Domain command/query surface (core API, EditMode-tested)

```
IGameLoop
  Tick(dt)                          // advance one fixed step (ISimClock / FixedUpdate)
ICombat
  ApplyHit(targetId, hitRegion)     // hitRegion ∈ {Body, Head}
  ApplyGrenade(WorldPoint center, GrenadeDef)  // returns affected list (≤ maxAffected)
IBatteryService
  Drain(robotId, activity, dt); Charge(robotId, dt); RipperHit(robotId)
IPhaseService
  EvaluateTransition()              // runs 7-step rule (FR-061)
ISpawnService
  ComposeSpawns(phaseDef, rng)      // budget-bounded composition
ITargetingService
  SelectTarget(zombie, perceived)   // per ZombieDef.targetPriority
IUpgradeService
  Offer(rng, selectedUpgradeIds) -> 3 of (9 minus already-selected); Apply(upgradeId)
  // clarify-confirmed: exclude already-selected ids; no stacking (SessionState.selectedUpgradeIds)
```

## Domain events (core → adapter; also feed telemetry)

```
IDomainEvents (publish/subscribe)
  PhaseStarted(phase), PhaseCleared(phase), PhaseFailed(phase, reason)
  RouteOpened(routeId)
  ZombieSpawned(type, routeId), ZombieKilled(id, by)
  BaseDamaged(amount, newHp), BaseWarning(on)
  PlayerDamaged(amount, newHp), PlayerDied()
  RobotBatteryChanged(robotId, newValue, newState)
  RobotStateChanged(robotId, robotState)
  RobotDamaged(robotId, amount, newHp), RobotDestroyed(robotId)   // HP-0 destruction, distinct from Disabled
  RobotChargeCommanded(robotId), RobotDisabled(robotId), RobotRecovered(robotId)
  RipperAttackedRobot(robotId, batteryDrained)
  UpgradeOffered(ids[3]), UpgradeSelected(id)
  GrenadeUsed(center, affectedCount)
  AmmoResupplied(supplyKind)         // Safe | Risky
  BarrierDamaged(routeId, newHp), BarrierDestroyed(routeId)
  RadioEvent(eventId)                // drives caption + audio
  MedicalHealApplied(amount)
  MedicalRobotDamaged(amount, newHp), MedicalRobotDestroyed(), MedicalZoneDisabled()   // FR-107; no regen this session
  GameWon(), GameLost(reason)
```

Invariants:
- Adapters MUST render events (HUD, audio, VFX) but MUST NOT contain rule decisions.
- Each event maps to zero or more telemetry records (see telemetry.contract.md) and/or radio/string events (strings.contract.md).
- `RadioEvent` triggers fire at their gameplay milestone (not deferred): RadioEvent(GameStart) at boot, RadioEvent(Phase1/2/3) on RouteOpened/PhaseStarted, RadioEvent(BatteryWarning) on battery threshold, RadioEvent(BaseDanger) on BaseWarning, RadioEvent(PhaseClear) on PhaseCleared, RadioEvent(Victory) on GameWon.

## Acceptance

- [ ] Core exposes only the interfaces above; no `UnityEngine` types in signatures.
- [ ] Command menu can issue exactly the 4 commands and nothing else.
- [ ] Every domain event has at least one subscriber path to HUD/audio and/or telemetry.
