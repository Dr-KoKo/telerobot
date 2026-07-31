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

## Decision 2: Tune the one uniform visual value to 0.85

**Decision**: Use `haetaeVisualScale = 0.85` as the only visual-size value and
apply it once to `Presentation Visual`.

**Rationale**: It is data-driven, applies to authored and fallback paths, and
motion captures it as the stable baseline. Playtest feedback found the 0.90 model
still slightly too large, so the single value is reduced without touching physics.

**Alternatives considered**:

- Keep 0.90: rejected after the follow-up playtest found enemy readability still
  needed a small improvement.
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

**Decision**: Use obstructing opacity 0.16 and disable preserved specular on
transparent URP variants while retaining the existing fade/restore timing.

**Rationale**: The intermediate 0.24 treatment was still weaker than desired in
playtesting. The new value retains the silhouette but provides a clearer view of
enemies through bright metallic and emissive surfaces.

**Alternatives considered**:

- Keep 0.24: rejected based on follow-up user playtest feedback.
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

## Decision 6: Retain a transparent player-build material path

**Decision**: Generate and reference a serialized URP transparent material
template, derive runtime obstruction materials from it, scale emission with the
current opacity, and tune the single visual scale to `0.80` and opacity to `0.10`.

**Rationale**: Editor-only material-state tests did not prove that a transparent
shader variant survived player-build stripping. A referenced transparent asset
makes that dependency explicit, while emission scaling prevents luminous accents
from appearing opaque after the body fades. The smaller single scale directly
addresses the latest playtest without changing the physical footprint.

**Alternatives considered**:

- Continue mutating opaque materials only at runtime: rejected because it leaves
  player-build shader retention implicit.
- Disable renderers completely: rejected because the ally silhouette would be
  lost.
- Add a custom shader: deferred because the standard retained URP transparent
  path satisfies the current requirement with less asset and maintenance scope.
