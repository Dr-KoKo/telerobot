# Feature Specification: Haetae Scale Tuning

**Feature Branch**: `009-haetae-scale-tuning`

**Created**: 2026-07-28

**Status**: Implemented; user playtest pending

**Input**: User description: "해태 로봇의 크기를 조금만 작게 만들 수 있을까"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Slightly smaller Haetae silhouette (Priority: P1)

As the field commander, I see both Haetae robots at a slightly smaller visual
size so they occupy less of the combat view while remaining immediately
recognizable.

**Why this priority**: The authored Haetae models are visually prominent and can
cover nearby enemies or terrain. A modest reduction improves readability without
losing their guardian presence.

**Independent Test**: Compare the current and tuned Haetae silhouettes in the same
camera position and confirm that every visible dimension is 90% of the previous
size while identity markers and model detail remain readable.

**Acceptance Scenarios**:

1. **Given** a new session with two general Haetae robots, **When** gameplay
   begins, **Then** both visible models use the same 90% linear scale.
2. **Given** a Haetae changes to melee, ranged, or balanced specialization,
   **When** its role-specific model appears, **Then** the same 90% visual reduction
   remains applied on top of its role silhouette.
3. **Given** a Haetae moves, attacks, takes damage, or is destroyed, **When** its
   animation state changes, **Then** the tuned scale remains stable.

---

### User Story 2 - Preserve gameplay and UI footprint (Priority: P1)

As the field commander, I get the visual size improvement without changes to
movement, collision, spacing, attack range, targeting, health, battery, or status
information.

**Why this priority**: A visual adjustment must not silently rebalance combat or
make robots overlap because their physical footprint changed.

**Independent Test**: Record each robot's physical bounds and gameplay values,
apply all Haetae presentation roles, and confirm that only the visible model size
changes while status bars remain populated.

**Acceptance Scenarios**:

1. **Given** a Haetae's physical footprint before the tuning, **When** the smaller
   model is attached or refreshed, **Then** its physical footprint and separation
   behavior remain unchanged.
2. **Given** any Haetae role, **When** combat is active, **Then** movement speed,
   attack distance, targeting, health, battery, and damage behavior are unchanged.
3. **Given** the HUD displays Haetae health, battery, and experience, **When** the
   model becomes smaller, **Then** every existing status bar and label remains
   visible and populated.

## Edge Cases

- Presentation is refreshed repeatedly after specialization or phase restore.
- An authored model is unavailable and the procedural fallback is used.
- Two Haetae robots with different identity-marker counts are shown together.
- Hit, attack, death, and locomotion animation temporarily modifies pose.
- A specialized role already has a non-uniform role scale.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: All general and specialized Haetae visible models MUST use a
  configurable uniform visual scale of `0.90` relative to the current baseline.
- **FR-002**: The scale MUST apply to authored models and procedural fallback
  models.
- **FR-003**: The visual reduction MUST NOT alter the gameplay object's physical
  footprint, movement, separation, detection, targeting, attack range, damage,
  health, battery, or progression behavior.
- **FR-004**: Existing specialization-specific proportions MUST be preserved; the
  global reduction MUST multiply rather than replace their role silhouette.
- **FR-005**: Animation and repeated presentation refreshes MUST preserve the tuned
  scale without cumulative shrinking.
- **FR-006**: Haetae identity markers, LOD behavior, materials, and authored model
  selection MUST remain unchanged.
- **FR-007**: Existing HUD health, battery, experience, and specialization
  presentation MUST remain unchanged.
- **FR-008**: No zombie, player, medical robot, base, prop, or effect scale MUST be
  changed by this feature.

## Success Criteria *(mandatory)*

- **SC-001**: Both live general Haetae and all three specialized Haetae roles
  display at exactly 90% of their previous linear visual dimensions.
- **SC-002**: Physical bounds before and after presentation attachment differ by
  no more than `0.001` world units on any axis.
- **SC-003**: Reattaching or animating a Haetae presentation 10 times produces no
  cumulative scale change.
- **SC-004**: Existing identity, authored-model, LOD, animation, collision,
  separation, HUD/status-bar, combat, and phase tests have zero new failures.
- **SC-005**: The complete automated validation suites, Windows build, and
  standalone gameplay-ready smoke check pass.

## Assumptions

- "조금만 작게" is interpreted as a 10% uniform linear reduction.
- The requested change is visual only; the gameplay capsule remains the current
  size to avoid unrequested balance changes.
- The same reduction applies to both Haetae units and all specialization forms.
- No camera, FOV, animation timing, status-bar layout, or model geometry change is
  included.
