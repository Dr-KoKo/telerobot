# Feature Specification: Haetae Wave Cleanup Targeting

**Feature Branch**: `008-haetae-wave-cleanup-targeting`

**Created**: 2026-07-28

**Status**: Implemented; user playtest pending

**Input**: User description: "phase의 막바지에 더 이상 좀비가 생기지 않는
시점에 남아있는 좀비들을 해태 로봇이 공격하지 않는 버그가 있다."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Finish surviving zombies after spawning ends (Priority: P1)

As the field commander, I can rely on a Haetae assigned to defend the base to
attack any remaining zombie that has entered the defended area after the phase's
spawn schedule is exhausted, even when that zombie arrived from another route.

**Why this priority**: A surviving zombie that is ignored can keep the phase from
ending indefinitely and makes the robot appear broken at the most visible moment
of the wave.

**Independent Test**: Exhaust the phase spawn schedule, leave one living zombie
from a route different from the defending Haetae's assigned route inside its
defended area, and verify that the Haetae acquires and damages that zombie.

**Acceptance Scenarios**:

1. **Given** all scheduled zombies have spawned and a living zombie from another
   route is inside a defending Haetae's valid combat area, **When** the Haetae
   searches for a target, **Then** it acquires that zombie and begins combat.
2. **Given** the cleanup target remains alive and valid, **When** normal combat
   time advances, **Then** the Haetae continues attacking until the target dies or
   becomes invalid.
3. **Given** the final living zombie is killed by cleanup combat, **When** the
   phase evaluates completion, **Then** the phase advances without requiring a
   new command or another spawn.

---

### User Story 2 - Preserve command and safety boundaries (Priority: P1)

As the field commander, I retain the meaning of route patrol, active-wave route
assignment, defend range, and robot availability while the cleanup behavior is
fixed.

**Why this priority**: Removing all route restrictions would fix the symptom by
changing the player's deployment strategy and could make robots abandon their
assigned lanes during active spawning.

**Independent Test**: Compare target selection before and after spawn completion
for defend and patrol commands, then repeat with targets outside the defended area
and with a robot that cannot attack.

**Acceptance Scenarios**:

1. **Given** scheduled zombies are still pending, **When** a defending Haetae
   searches for targets, **Then** its normal assigned-route behavior is unchanged.
2. **Given** the spawn schedule is exhausted, **When** a Haetae is patrolling an
   assigned route, **Then** it continues to respect that route.
3. **Given** a cross-route zombie is outside the existing defend leash,
   **When** cleanup targeting runs, **Then** the defending Haetae does not acquire
   it.
4. **Given** a Haetae is destroyed, disabled, recovering, or returning under an
   explicit command, **When** cleanup begins, **Then** existing movement,
   availability, and battery rules remain authoritative.

## Edge Cases

- The spawn schedule becomes exhausted on the same frame that the current target
  dies.
- More than one cross-route zombie remains; the Haetae must choose deterministically
  and reacquire after a kill.
- Two Haetae robots can select the same cleanup target without losing their
  existing separation and follow-up behavior.
- A surviving zombie moves outside the defend leash before the next attack.
- The only available Haetae is charging when the last cross-route zombie enters
  the base defense area.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The game MUST expose whether the current phase has no scheduled
  zombies left to spawn.
- **FR-002**: A Haetae using the defend-position command MUST be allowed to acquire
  a living zombie from any route when the current phase has no scheduled zombies
  left to spawn.
- **FR-003**: The cleanup exception MUST apply only to zombies inside the existing
  base-relative defend leash.
- **FR-004**: Before the spawn schedule is exhausted, defend-position acquisition
  MUST retain its existing assigned-route restriction.
- **FR-005**: Patrol-route acquisition MUST retain its assigned-route restriction
  before and after spawn completion.
- **FR-006**: Existing target validity, follow-up targeting, combat cadence,
  specialization, battery, charging interruption, disable, recovery, destruction,
  movement, and separation behavior MUST remain unchanged.
- **FR-007**: After a cleanup target dies, an eligible Haetae MUST be able to
  acquire another valid survivor without a player command.
- **FR-008**: Killing the final scheduled survivor MUST allow the existing phase
  completion flow to proceed automatically.
- **FR-009**: The spawn-complete route-relaxation rule MUST be deterministic and
  independently verifiable without scene timing or navigation.
- **FR-010**: The fix MUST NOT add new commands, balance values, player-facing
  text, enemies, routes, or telemetry event names.

### Key Entities

- **Spawn Schedule State**: Whether scheduled entries remain for the current
  phase; cleanup begins only when none remain.
- **Haetae Command State**: The active defend, patrol, or return command plus the
  assigned route and current availability.
- **Target Candidate**: A living zombie's route and whether it is within the
  robot's existing detection and defend boundaries.

## Success Criteria *(mandatory)*

- **SC-001**: In 100% of automated cleanup scenarios, an eligible defending
  Haetae acquires and damages the sole valid cross-route survivor after spawning
  is complete.
- **SC-002**: In 100% of active-spawn and patrol regression scenarios, assigned
  route restrictions remain unchanged.
- **SC-003**: Defend cleanup targets outside the existing defend leash are rejected
  in 100% of boundary checks.
- **SC-004**: A phase with one valid cleanup survivor advances after that survivor
  is eliminated, with no extra command or spawn required.
- **SC-005**: Repeated deterministic runs with identical inputs produce identical
  target-selection and session outcomes.
- **SC-006**: All existing automated gameplay suites, the Windows build, and the
  standalone gameplay-ready smoke check complete with zero new failures.

## Assumptions

- "No more zombies spawn" means every entry in the current phase's scheduled
  spawn queue has been emitted; it does not mean a temporary concurrency pause.
- The bug concerns defend-position Haetae robots near the base. Route patrol must
  remain route-bound because lane assignment is an intentional player decision.
- Cleanup targeting uses the existing base-relative defend leash. Patrol and other
  non-defend acquisition retain their existing robot-relative detection radius; no
  command gains global awareness or unlimited pursuit.
- Existing cross-route charging interruption and post-kill follow-up behavior are
  correct and remain in scope only as regressions.
- No new balancing or presentation work is required.
