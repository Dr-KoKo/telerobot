# Feature Specification: Base Visibility and Walkable Access

**Feature Branch**: `007-base-visibility-access`

**Created**: 2026-07-28

**Status**: Implemented; user visual acceptance pending

**Input**: User description: "거점의 원통 구조물이 시야를 가리는 문제가 있다. 구조물을 그냥 옆으로 옮겨버리거나 구조물 형태 그대로 사람이 올라갈 수 있도록 변경되어야 할 것 같다."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See combat around the base (Priority: P1)

As the field commander, I can read enemies and routes on the opposite side of the
base without a broad, tall cylinder covering the combat area. The base remains an
obvious circular landmark, but its wide opaque mass stays below the normal combat
view and only a narrow identity beacon rises above it.

**Why this priority**: The central structure currently hides enemies at the exact
location where route pressure converges, making aiming and defense decisions
unnecessarily difficult.

**Independent Test**: Observe the base from ground level at the north, east, south,
and west perimeter positions and confirm that the broad platform stays below the
combat sightline while the opposite approach remains readable around the narrow
beacon.

**Acceptance Scenarios**:

1. **Given** the player is standing immediately outside any cardinal side of the
   base, **When** the player looks across the center, **Then** the broad opaque
   structure remains below the player's normal sightline and the opposite approach
   is visible on both sides of the narrow beacon.
2. **Given** zombies occupy distributed attack positions around the base, **When**
   the player rotates around the perimeter, **Then** the structure does not conceal
   an entire attack row behind a full-height wall.
3. **Given** the base is viewed alongside routes, charging, supplies, and status
   bars, **When** combat is active, **Then** the base remains visually identifiable
   without obscuring those gameplay indicators.

---

### User Story 2 - Climb the circular base (Priority: P1)

As the field commander, I can walk onto and off the retained circular base form from
every major approach without a jump, invisible wall, or collision exploit. The
structure behaves as a low terraced platform rather than an impassable cylinder.

**Why this priority**: Keeping the landmark in place avoids relocating charging,
rally, and defense anchors, while walkable access turns the former obstruction into
useful defensive terrain.

**Independent Test**: Starting just outside each cardinal side, walk straight toward
the center at normal speed, reach the highest terrace without jumping, then walk off
the opposite side without becoming stuck or falling through the geometry.

**Acceptance Scenarios**:

1. **Given** the player starts on level ground outside the base, **When** they walk
   toward the center from north, east, south, or west, **Then** they ascend every
   terrace without jumping and can stand stably on the top.
2. **Given** the player stands on the highest terrace, **When** they walk toward any
   cardinal edge, **Then** they descend to the ground without snagging, tunnelling,
   or receiving damage.
3. **Given** the player traverses the base diagonally or along a terrace edge,
   **When** movement continues, **Then** collision remains stable and does not eject
   or trap the player.

---

### User Story 3 - Preserve base-defense behavior (Priority: P1)

As the player, I receive the visibility and traversal improvement without changes to
base health, charging, zombie attack distribution, route logic, combat rules, or the
existing HUD/status bars.

**Why this priority**: The structure is a presentation and traversal correction, not
a redesign of the defense loop.

**Independent Test**: Run a wave with multiple attackers, cross the base, command a
Haetae to charge, and verify that distributed perimeter attacks still damage the
base while all existing status information remains visible and accurate.

**Acceptance Scenarios**:

1. **Given** multiple zombies target the base, **When** they reach attack range,
   **Then** they remain distributed outside the base footprint and continue damaging
   base health at the existing cadence.
2. **Given** a Haetae returns to the base charging area, **When** it is eligible to
   charge, **Then** the existing charging position and radius still apply.
3. **Given** the player enters combat around or on the base, **When** health,
   battery, phase, or route pressure changes, **Then** the existing status bars
   remain present and update as before.

### Edge Cases

- The player approaches exactly between two cardinal directions or skims a circular
  terrace edge.
- The player stops with one foot-equivalent over a terrace boundary, changes
  direction, or descends backward.
- The third-person camera closes in while the player stands beside or on the base.
- Several zombies attack from one route while others arrive from a perpendicular
  route.
- Presentation materials fail to initialize and the greybox fallback is used.
- The base is rebuilt repeatedly during automated scene loading without duplicated
  colliders or decorative roots.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The base MUST remain at its current gameplay anchor.
- **FR-002**: The base MUST retain a circular guardian-landmark identity.
- **FR-003**: The wide opaque portion of the base MUST be low enough to preserve the
  normal ground-level combat sightline across it.
- **FR-004**: Any element rising above the wide platform MUST be narrow enough that
  targets remain visible on both sides.
- **FR-005**: The base MUST provide walkable, progressively elevated surfaces from
  all four cardinal approaches.
- **FR-006**: The player MUST be able to reach the highest walkable surface without
  jumping.
- **FR-007**: The player MUST be able to descend from the highest surface to ground
  level without becoming stuck, tunnelling, or taking damage.
- **FR-008**: Walkable base surfaces MUST have collision matching their visible
  shape; invisible full-height blocking volumes MUST NOT remain.
- **FR-009**: The base's outer footprint and elevation profile MUST be stored as
  editable world-layout data.
- **FR-010**: Zombies targeting the base MUST continue to select distributed attack
  positions outside the configured outer footprint.
- **FR-011**: Base attack-position selection MUST support attackers arriving from
  every existing route.
- **FR-012**: The change MUST NOT alter base health, damage cadence, route
  navigation, spawn rules, charging positions or radii, combat balance, or phase
  progression.
- **FR-013**: The change MUST NOT remove, hide, or change the update behavior of the
  existing HUD and world-space status bars.
- **FR-014**: The walkable structure MUST remain functional when enhanced
  presentation materials are unavailable.
- **FR-015**: Rebuilding the runtime world MUST produce one base hierarchy without
  duplicate active surface colliders.
- **FR-016**: Automated tests, Windows player build, and standalone smoke validation
  MUST continue to pass.

### Key Entities

- **Base Footprint**: The configured horizontal extent used for the visible platform,
  walkable collision, and zombie perimeter attack positions.
- **Base Terrace**: One circular walkable level with an outer radius and top height;
  terraces progress inward and upward.
- **Guardian Beacon**: A narrow, non-walkable identity element above the platform
  that preserves the Haetae guardian silhouette without recreating a broad sight
  blocker.
- **Base Attack Slot**: A distributed position outside the footprint where a zombie
  can damage the base without entering or overlapping its visible structure.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The broad opaque base body is no higher than 0.75 metres above its
  surrounding ground, and any taller central element is no wider than 1.0 metre.
- **SC-002**: From each of the four cardinal approaches, a player at normal walking
  speed reaches the highest terrace without jumping in 3 seconds or less.
- **SC-003**: The player crosses the base in both cardinal and diagonal directions
  with zero stuck, fall-through, or forced-ejection events across 20 automated
  traversals.
- **SC-004**: Six simultaneous attackers from one route occupy at least four distinct
  perimeter positions, remain outside the configured footprint, and damage the base.
- **SC-005**: Existing charging, combat, route, HUD, status-bar, telemetry, and phase
  acceptance tests have zero new failures.
- **SC-006**: The complete automated validation suites, Windows build, and standalone
  gameplay-ready smoke check pass.

## Assumptions

- The retained-form option is preferred over moving the entire structure because the
  base anchor is already shared by rally, charging, attack, and presentation logic.
- "A person can climb it" means normal walk input without requiring a jump, mantle,
  ladder, or new player ability.
- A three-level terraced circular platform with a maximum broad height of 0.75 metres
  is an acceptable refinement of the current cylindrical silhouette.
- A narrow guardian beacon may remain above the platform if it stays within the
  visibility limit in SC-001.
- Zombie attack-slot behavior, rather than physics-driven climbing, keeps zombies
  outside the base footprint.
- No new camera-fade system, player climbing animation, alternate base location, or
  defense mechanic is included in this feature.
