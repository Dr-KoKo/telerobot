# Feature Specification: Haetae Camera Occlusion

**Feature Branch**: `main` (user-authorized direct implementation)

**Created**: 2026-07-31

**Status**: Implemented; user playtest pending

**Input**: User description: "해태 로봇으로 인해 적이 잘 보이지 않아. 투명하게 만들거나 크기를 줄여야 할 것 같은데 어떤 방향이 좋겠니" followed by approval to proceed from updated `main`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Keep the combat view readable (Priority: P1)

As the field commander using the third-person camera, I can keep enemies and the
aiming area visible when a Haetae robot passes through the center of the combat
view.

**Why this priority**: The Haetae models are large, mobile allies. When one moves
between the camera and the action, it can hide enemies at the exact moment the
player needs to aim and react.

**Independent Test**: Place either Haetae between the third-person camera and the
center aiming corridor, then confirm its body becomes partially transparent
quickly enough to reveal the space behind it while remaining identifiable.

**Acceptance Scenarios**:

1. **Given** a third-person combat view, **When** a Haetae overlaps the central
   aiming corridor, **Then** its visible body fades to 32% opacity within 0.15
   seconds.
2. **Given** two Haetae overlap the central aiming corridor, **When** both obstruct
   the view, **Then** both fade independently so neither hides the combat area.
3. **Given** a faded Haetae, **When** it leaves the central aiming corridor,
   **Then** it returns to full opacity within 0.25 seconds without a visible pop.

---

### User Story 2 - Preserve Haetae presence outside obstruction (Priority: P1)

As the field commander, I still see the authored Haetae models at full quality
whenever they are not blocking the central combat view.

**Why this priority**: Permanent transparency would weaken silhouette,
specialization readability, materials, and the guardian-robot identity even when
no visibility problem exists.

**Independent Test**: Move a Haetae beside or behind the central viewing corridor
and switch camera perspectives, then confirm normal opaque presentation is
preserved outside the third-person obstruction case.

**Acceptance Scenarios**:

1. **Given** a Haetae outside the central aiming corridor, **When** combat
   continues, **Then** its model remains fully opaque.
2. **Given** the first-person camera, **When** a Haetae crosses the aiming
   direction, **Then** conditional ally transparency is not applied.
3. **Given** any opacity transition, **When** health, battery, experience, unit
   identity, or specialization information is displayed, **Then** that information
   remains unchanged and readable.

---

### User Story 3 - Stay stable across model states (Priority: P2)

As the field commander, I get the same obstruction behavior for both Haetae units
and every specialization without animation, model refresh, LOD, damage, or phase
restore corrupting their materials.

**Why this priority**: Haetae presentations are replaced during specialization
and restored across phases. Visibility treatment must follow the new model and
must not accumulate duplicate materials or permanent tint changes.

**Independent Test**: Exercise general, melee, ranged, and balanced models through
fade and restore cycles, including a presentation replacement, and confirm the
current model alone owns one stable opacity state.

**Acceptance Scenarios**:

1. **Given** a fading Haetae, **When** its locomotion, attack, hit, or death pose is
   sampled, **Then** the opacity transition remains stable and motion remains
   unchanged.
2. **Given** a Haetae presentation is replaced by a specialization or phase
   restore, **When** obstruction is evaluated again, **Then** the replacement model
   fades and restores correctly without retaining obsolete material state.
3. **Given** authored or procedural fallback presentation, **When** obstruction
   occurs, **Then** both follow the same opacity and restoration rules.

### Edge Cases

- A Haetae enters the aiming corridor while another Haetae is already faded.
- The player switches between third-person and first-person during a fade.
- The current presentation hierarchy is replaced while partially transparent.
- The Haetae uses either LOD level or a procedural fallback model.
- The game is paused during a transition and later resumed.
- A robot is disabled, destroyed, or restored while occupying the aiming corridor.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The game MUST apply conditional transparency only to Haetae visual
  presentations that obstruct the central third-person aiming corridor.
- **FR-002**: The configured obstructing opacity MUST default to `0.32` and MUST
  remain adjustable as presentation data.
- **FR-003**: An obstructing Haetae MUST reach the configured opacity within
  `0.15` seconds, and a non-obstructing Haetae MUST restore full opacity within
  `0.25` seconds.
- **FR-004**: Each Haetae MUST evaluate and transition independently when multiple
  allies overlap the combat view.
- **FR-005**: The feature MUST preserve the existing 90% Haetae visual scale and
  MUST NOT further change gameplay-root scale or collider bounds.
- **FR-006**: The feature MUST NOT change movement, separation, navigation,
  targeting, attacks, damage, health, battery, progression, spawning, or telemetry
  outcomes.
- **FR-007**: First-person view and non-obstructing third-person views MUST retain
  fully opaque Haetae presentation.
- **FR-008**: Health, battery, experience, specialization, unit identity, and all
  HUD/status presentation MUST remain unchanged.
- **FR-009**: General, melee, ranged, balanced, both-unit identity variants,
  authored LODs, and procedural fallbacks MUST follow the same obstruction rules.
- **FR-010**: Presentation replacement, animation, damage tint, destruction, and
  phase restoration MUST NOT leave obsolete, duplicated, or permanently
  transparent material state.
- **FR-011**: Player, zombie, medical robot, base, prop, effect, and environment
  presentation MUST remain unaffected.

### Key Entities

- **Haetae obstruction presentation**: The current replaceable visual model for
  one Haetae and its full-opacity, target-opacity, transition, and obstruction
  state.
- **Occlusion tuning**: Designer-adjustable enablement, opacity, transition times,
  aiming-corridor width, and maximum evaluation distance.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of automated central-obstruction scenarios, every obstructing
  Haetae reaches 32% opacity within 0.15 seconds.
- **SC-002**: In 100% of clear-view scenarios, each Haetae returns to 100% opacity
  within 0.25 seconds.
- **SC-003**: After at least 10 fade/restore cycles and one presentation
  replacement, no Haetae remains transparent outside the obstruction condition.
- **SC-004**: Gameplay collider bounds differ by no more than 0.001 world units
  before and after visibility transitions.
- **SC-005**: Existing scale, authored-model, LOD, animation, specialization,
  status-bar, combat, targeting, phase, and build validation has zero new failures.
- **SC-006**: The Windows playtest build reaches the standalone gameplay-ready
  marker with no material or shader errors.

## Assumptions

- The visibility problem is primarily a third-person center-view obstruction;
  the default behavior does not make allies permanently transparent.
- The already-implemented 90% Haetae scale remains the baseline.
- A 32% obstructing opacity balances enemy readability with Haetae identity and
  can be tuned after user playtesting.
- The central aiming corridor includes a modest margin around the crosshair so
  near-center obstruction is handled before the exact center is fully covered.
- No camera distance, field of view, formation, gameplay collider, enemy outline,
  or through-wall visibility change is included.
