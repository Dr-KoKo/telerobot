# Research: Haetae Visibility Foundation

## Decision 1: Identity gameplay root and explicit physical shape

**Decision**: Set the Haetae actor root scale to identity and reproduce the legacy
capsule bounds with explicit data-driven radius, height, and center.

**Rationale**: The old `(1.1, 0.75, 1.5)` transform simultaneously affected
physics and every visual descendant. Moving physical shape into the collider
removes hidden visual multiplication without changing gameplay footprint.

**Alternatives considered**:

- Fold 0.90 into the root: rejected because it shrinks collision and separation.
- Keep both scales but rename them: rejected because visual descendants would
  still inherit non-uniform distortion.
- Store final non-uniform visual scale: rejected because it preserves distortion
  of artist-authored models.

## Decision 2: Keep one uniform 0.90 visual value

**Decision**: Retain `haetaeVisualScale = 0.90` as the only visual-size value and
apply it once to `Presentation Visual`.

**Rationale**: It is already data-driven, applies to authored and fallback paths,
and motion captures it as the stable baseline. With an identity parent it finally
means an actual uniform 90% model scale.

**Alternatives considered**:

- Change the value while restructuring: rejected to avoid mixing architecture
  correction with a new subjective size decision.
- Rescale every FBX: rejected because it duplicates tuning across eight assets.

## Decision 3: Renderer bounds own obstruction

**Decision**: Intersect the central camera ray with each cached, active renderer's
expanded world bounds and ignore gameplay colliders.

**Rationale**: The defect occurs when rendered geometry hides the aiming area.
Renderer bounds cover authored LODs, specialization models, motion, and fallback
without adding colliders or allocations.

**Alternatives considered**:

- Keep the capsule SphereCast: rejected because it demonstrably misses visible
  geometry outside the physical body.
- Project every mesh vertex: rejected as unnecessary per-frame cost.
- Screen-space corner projection: viable but more complex than expanded bounds
  for the current center-ray requirement.

## Decision 4: Make transparency unmistakable

**Decision**: Use obstructing opacity 0.24 and disable preserved specular on
transparent URP variants while retaining the existing fade/restore timing.

**Rationale**: The previous 0.32 treatment was difficult to perceive, especially
on bright metallic and emissive surfaces. The new value retains the silhouette
but provides a clearer view of enemies.

**Alternatives considered**:

- Keep 0.32: rejected based on user playtest feedback.
- Hide the model entirely: rejected because ally location and identity matter.
- Use dithering: rejected because it requires a custom shader and asset scope.

## Decision 5: Test visual and physical boundaries separately

**Decision**: Compare live physical bounds with a legacy reference capsule, prove
renderer-only obstruction, assert real URP material state and alpha, and retain
all lifecycle regressions.

**Rationale**: Previous coverage confirmed data state but used a centered
collider. Separating the boundaries prevents the same false confidence.

**Alternatives considered**:

- Manual-only validation: rejected because the regression is structural.
- Pixel-perfect screenshot threshold as the sole test: rejected as sensitive to
  platform lighting; material-state automation plus manual rendered comparison is
  more stable.
