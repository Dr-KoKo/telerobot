# Feature Specification: Haetae Visibility Foundation

**Feature Branch**: `main` (user-authorized direct implementation)

**Created**: 2026-07-31

**Status**: Implemented

**Input**: User request to consolidate Haetae sizing so it cannot become tangled later, make the intended obstruction transparency visibly and reliably activate in a player build, then reduce the robot slightly further after playtesting.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - One predictable visual size (Priority: P1)

As the field commander, I see every Haetae model at one consistently controlled
size and at the proportions authored by the artist.

**Why this priority**: Stacked parent and child scaling obscures the true size,
distorts the model, and makes later tuning error-prone.

**Independent Test**: Start a session, inspect both general Haetae and each
specialization, and confirm one shared size setting controls their undistorted
visible presentation while their physical gameplay footprint remains unchanged.

**Acceptance Scenarios**:

1. **Given** a general or specialized Haetae, **When** its presentation is
   created or replaced, **Then** exactly one uniform visual size setting applies.
2. **Given** the previous gameplay footprint, **When** sizing is consolidated,
   **Then** collision, movement, separation, targeting, and combat outcomes remain
   unchanged.
3. **Given** animation or repeated model replacement, **When** the model updates,
   **Then** visual size neither compounds nor resets.

---

### User Story 2 - Visible obstruction transparency (Priority: P1)

As the field commander, I clearly see through a Haetae whenever its rendered body
covers the central aiming area, even when its physical footprint does not cross
that area.

**Why this priority**: The previous physical-footprint test can miss the larger
visible model, causing the transparency feature to appear absent during play.

**Independent Test**: Offset a Haetae so its visible model covers the crosshair
while its physical body remains outside the old corridor, then confirm it visibly
fades and restores when the rendered body moves clear.

**Acceptance Scenarios**:

1. **Given** third-person view, **When** any active Haetae renderer overlaps the
   central aiming corridor within range, **Then** that Haetae reaches 10% opacity
   within 0.15 seconds.
2. **Given** the visible model blocks the corridor but its physical footprint does
   not, **When** obstruction is evaluated, **Then** transparency still activates.
3. **Given** a faded Haetae, **When** all of its visible renderers clear the
   corridor, **Then** it restores full opacity within 0.25 seconds.

---

### User Story 3 - Stable readable presentation (Priority: P2)

As the field commander, I get the same clear visibility behavior for both Haetae,
all specializations, LODs, motion states, and procedural fallback presentation.

**Why this priority**: A visibility fix must survive the model lifecycle without
leaking materials or changing status and combat behavior.

**Independent Test**: Exercise two robots through specialization replacement,
animation, ten fade cycles, first-person switching, and fallback presentation,
then confirm independent and reversible behavior.

**Acceptance Scenarios**:

1. **Given** two obstructing Haetae, **When** their rendered bounds overlap the
   aiming corridor, **Then** they fade independently.
2. **Given** first-person view, **When** a Haetae is centered, **Then** it remains
   fully opaque.
3. **Given** specialization, LOD, animation, damage tint, or fallback changes,
   **When** visibility transitions repeat, **Then** the current model restores its
   original appearance without accumulating materials.

### Edge Cases

- The physical body and rendered model occupy different screen regions.
- A renderer is disabled or inactive because another LOD is selected.
- A model is replaced while partially transparent.
- The player switches perspective during a fade.
- The visible bounds touch only the margin around the crosshair.
- A robot is destroyed or disabled while faded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every Haetae presentation MUST have exactly one uniform,
  designer-adjustable visual size value defaulting to `0.80`.
- **FR-002**: Parent gameplay state MUST NOT apply a second visual scale or
  distort authored proportions.
- **FR-003**: The existing physical gameplay footprint MUST remain unchanged and
  MUST be independently defined from visual size.
- **FR-004**: General, melee, ranged, balanced, authored LOD, and procedural
  fallback presentations MUST use the same sizing rule.
- **FR-005**: Obstruction MUST be determined from active visible presentation,
  not solely from the physical gameplay footprint.
- **FR-006**: Obstructing opacity MUST default to `0.10`, fade duration to `0.15`
  seconds, and restore duration to `0.25` seconds, all adjustable as presentation
  data.
- **FR-007**: First-person and clear third-person views MUST remain fully opaque.
- **FR-008**: Each Haetae MUST transition independently and restore the exact
  original materials when clear.
- **FR-009**: Repeated transitions and presentation replacements MUST NOT create
  cumulative size changes, stale material assignments, or unbounded material
  growth.
- **FR-010**: Movement, separation, navigation, targeting, attacks, damage,
  health, battery, progression, spawning, telemetry, and status UI MUST remain
  unchanged.
- **FR-011**: Player, zombie, medical robot, base, prop, effect, and environment
  presentation MUST remain unaffected.
- **FR-012**: The Windows player build MUST visibly render Haetae obstruction
  transparency, and luminous accents MUST dim with the obstructing opacity rather
  than remaining visually opaque.

### Key Entities

- **Haetae visual presentation**: The current rendered model, its one uniform
  size, active renderer bounds, and opaque/transparent material state.
- **Haetae physical footprint**: The independently configured collision shape
  used by gameplay and preserved through visual tuning.
- **Occlusion tuning**: Opacity, transition durations, corridor margin, range,
  and enablement used by every Haetae.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of general and specialized Haetae models use exactly one
  `0.80` uniform visual size setting after creation, animation, and replacement.
- **SC-002**: Physical bounds before and after consolidation differ by no more
  than `0.001` world units on any axis.
- **SC-003**: 100% of test cases where visible Haetae bounds cover the central
  aiming corridor reach 10% opacity within 0.15 seconds, including cases where
  the physical footprint does not cover it.
- **SC-004**: Clear and first-person cases restore 100% opacity within 0.25
  seconds and restore the original material references.
- **SC-005**: Ten fade/restore cycles plus a model replacement produce no
  additional retained material variants and no scale drift.
- **SC-006**: Complete automated suites, Windows build, standalone smoke, and a
  rendered transparency comparison complete with zero new failures.

## Assumptions

- The uniform visual size is reduced from `0.85` to `0.80` after playtest feedback;
  the identity parent and independent physical footprint remain unchanged.
- Existing physical bounds are preserved by explicit physical-footprint data.
- A 10% opacity is intentionally more obvious than the previous 16% value while
  retaining enough silhouette to identify the ally.
- All resources required for transparency are included in the Windows player
  build rather than depending on editor-only state.
- Central obstruction includes a small world-space margin around the camera aim
  ray and evaluates only active renderers in front of the camera.
- Camera distance, field of view, formation behavior, enemy outlines, and
  through-wall rendering remain out of scope.
