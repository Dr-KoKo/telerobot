# Research: Haetae Wave Cleanup Targeting

## Decision 1: Define cleanup from the authoritative spawn queue

**Decision**: Cleanup is active when the current phase has a spawn queue and its
next-spawn index has reached the queue count.

**Rationale**: This distinguishes true exhaustion from a temporary concurrency-cap
pause. It is also available immediately to robot acquisition without relying on
the frame order in which a mirrored phase-state flag is refreshed.

**Alternatives considered**:

- Treat low alive count as cleanup: rejected because more scheduled groups may
  still be pending.
- Add a timer after the last spawn: rejected because it delays recovery and adds
  a new tuning value.
- Read only the mirrored `PhaseState.AllSpawned`: rejected because the robot and
  controller update order can differ within the completion frame.

## Decision 2: Relax only defend-route eligibility

**Decision**: During cleanup, `DefendPosition` may acquire any-route targets.
`PatrolRoute` remains assigned-route-only. The existing base-relative defend leash,
non-defend detection radius, availability, and combat checks remain unchanged.

**Rationale**: All routes converge at the base, so a defender must protect the
shared objective once no future lane pressure is pending. Patrol is an explicit
lane assignment and must retain its player-facing meaning.

**Alternatives considered**:

- Remove route filtering for all commands: rejected because patrol robots would
  abandon assigned lanes.
- Permanently make defend cross-route: rejected because it changes active-spawn
  deployment behavior beyond the reported bug.
- Teleport or retask robots at phase end: rejected because it bypasses command,
  movement, and battery rules.

## Decision 3: Share one pure eligibility rule

**Decision**: Add a Unity-free rule that accepts command, assigned route,
candidate route, and spawn-complete state. Runtime spatial acquisition calls it,
and a deterministic EditMode matrix verifies every route/command transition.

**Rationale**: The route exception affects combat outcome and phase progression.
A pure rule gives deterministic coverage without moving transform queries into
the core. The full-session simulator intentionally models routes as separate
one-dimensional axes and cannot calculate a truthful cross-route world-space
distance at the converged base.

**Alternatives considered**:

- Patch only the runtime `if` statement: rejected because the cleanup decision
  would not have scene-free deterministic coverage.
- Move all spatial ranking into the pure core: rejected as unnecessary for this
  bug; scene distances and transforms remain adapter concerns by established
  architecture.
- Apply cross-route acquisition directly to the scalar full-session simulator:
  rejected after validation because it compares unrelated route coordinates as
  one distance and changes specialization-dependent phase reach. A world-space
  simulation expansion is outside this bug fix.

## Decision 4: Reproduce the exact failure in PlayMode

**Decision**: The integration test exhausts the real phase queue, removes all but
one living cross-route zombie, places that survivor inside the base defense area,
and observes the real Haetae actor damage it and allow phase completion.

**Rationale**: Existing tests cover cross-route charging interruption and
post-kill follow-up, but not initial acquisition by a non-charging defender after
the final spawn. The new test closes that exact gap.

**Alternatives considered**:

- Test `FindRobotTarget` directly only: rejected because it would not prove actor
  combat or phase progression.
- Depend on a long manual phase: rejected because the final composition and
  timing are nondeterministic for regression diagnosis.

## Decision 5: Keep telemetry and balance data unchanged

**Decision**: Reuse existing `zombie_killed`, `phase_cleared`, robot battery, and
session telemetry. Add no values or content assets.

**Rationale**: This is a state/eligibility correction. Existing events already
show whether cleanup ended and the phase advanced.

**Alternatives considered**:

- Add a cleanup-start event: deferred because the user did not request new
  diagnostics and existing validation can infer the state from spawn and phase
  records.
