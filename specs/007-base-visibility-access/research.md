# Research: Base Visibility and Walkable Access

## Decision 1: Retain the anchor and replace the tall body with low terraces

Use three concentric circular levels, rising by 0.25 metres to a maximum broad
height of 0.75 metres.

**Rationale**: The current anchor is shared by zombie targeting, rally, charging,
health, presentation, and tests. Retaining it prevents a cosmetic correction from
moving gameplay systems. The progressively lower radii preserve the cylinder motif
and create useful traversable terrain below the combat sightline.

**Alternatives considered**:

- Move the cylinder beside the anchor: improves visibility but separates the visible
  landmark from the object zombies attack and robots use.
- Add a ramp around the existing tall cylinder: makes the top reachable but leaves
  the tall opaque wall and camera occlusion in place.
- Add a mantle or ladder: expands player abilities and requires new input,
  animation, and edge-case handling outside the requested scope.

## Decision 2: Use one continuous visible terrace mesh as the collision source

Generate one circular mesh that alternates a configured inward slope with a level
band for each of the three elevations, then attach one static, non-convex mesh
collider using that same mesh and transform.

**Rationale**: A cylinder primitive's default capsule approximation does not match
flat terrace tops. Separate overlapping terrace colliders also create internal seams
that can catch a `CharacterController` while descending. One continuous surface
provides exact visible/collision agreement, preserves the three-band silhouette,
supports stable ascent/descent, and has no per-frame work.

**Alternatives considered**:

- Keep the hidden 8-by-3 box: directly violates visibility and traversal needs.
- Shrink the hidden box under visual cylinders: leaves invisible square corners and
  mismatched collision.
- Use separate overlapping frustums: visually correct, but PlayMode validation found
  that their internal collision seams could catch diagonal and descending movement.

## Decision 3: Store all movement-affecting geometry in world-layout data

Add outer radius, terrace count, rise, depth, beacon diameter, and perimeter-slot
spacing to `WorldLayoutAsset`, validate them in `MvpDataMapper`, and map them into
`WorldLayoutConfig`.

**Rationale**: Footprint and step height affect movement and zombie placement, so
they are gameplay tuning rather than incidental presentation constants. Existing
world positions and interaction radii already use this data path.

## Decision 4: Move perimeter geometry to a pure deterministic rule

Implement `BasePerimeterRules.AttackSlot` in the core assembly. It normalizes the
route approach, selects a radial row outside the footprint, and applies a
deterministic tangent offset from the zombie ordinal.

**Rationale**: The current box-bounds calculation lives inside
`MvpGameController`. Changing the footprint is an opportunity to make the rule
scene-free, directly testable, and independent of renderer/collider timing.

**Alternatives considered**:

- Query child collider bounds at runtime: couples target selection to presentation
  object construction and produces a square envelope for a circular platform.
- Keep four face-specific branches: preserves box assumptions and makes diagonal
  route approaches less natural.

## Decision 5: Keep the guardian beacon narrow and non-colliding

The beacon may rise above the terraces but is limited to a 1.0 metre diameter and
does not contribute a blocking collider.

**Rationale**: A narrow emissive landmark keeps the Haetae guardian identity while
allowing targets to remain visible on both sides. Removing collision prevents a
small decorative element from trapping the player at the top.

## Decision 6: Build gameplay geometry before presentation styling

`CentralBasePlatform` creates the walkable hierarchy with a greybox material first.
`WorldArtBuilder` then styles renderers and adds optional trim/beacon pieces.

**Rationale**: If theme validation or material creation fails, traversal and zombie
targeting must still work. This also keeps presentation failure from changing
gameplay.

## Decision 7: Validate through pure rules, real controller traversal, and regressions

Use EditMode tests for data validation and exact slot output properties; use PlayMode
tests for geometry bounds, cardinal/diagonal traversal, attacker distribution,
charging, HUD/status bars, and hierarchy duplication. Finish with full suites,
Windows build, smoke launch, and a four-side visual inspection.

**Rationale**: Collision feel requires the real Unity `CharacterController`, while
geometry math and invalid configurations are faster and clearer as scene-free tests.
