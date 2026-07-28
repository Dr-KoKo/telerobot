# Research: Haetae Scale Tuning

## Decision 1: Use a 0.90 uniform linear scale

**Decision**: Interpret "slightly smaller" as 90% of the current visible size on
all three axes.

**Rationale**: A 10% reduction is noticeable beside the player and zombies but
does not erase the Haetae's guardian silhouette or authored detail.

**Alternatives considered**:

- 95%: rejected as likely too subtle for a build-to-build visual check.
- 80%: rejected as more than a small adjustment and likely to weaken presence.
- Different values per specialization: rejected because the user asked for the
  Haetae robot as a class and existing role multipliers already express
  specialization differences.

## Decision 2: Scale the presentation child, not the gameplay root

**Decision**: Apply the multiplier to the `Presentation Visual` child created by
the presentation factory. Leave the primitive gameplay root transform and capsule
collider unchanged.

**Rationale**: The gameplay root currently owns physical bounds, separation, actor
movement, effects, and specialization root multipliers. Scaling it would silently
change collision and spacing. The child already isolates authored/procedural
visuals and is rebound by the motion driver on each refresh.

**Alternatives considered**:

- Reduce `SpawnRobot` root scale: rejected because it also shrinks the collider
  and changes physical gameplay.
- Rescale every FBX: rejected because it duplicates the adjustment across eight
  authored LOD assets and excludes procedural fallback.
- Change each model instance separately: rejected because repeated refreshes
  could drift and role coverage would be brittle.

## Decision 3: Store the value in the visual theme

**Decision**: Add one validated `haetaeVisualScale` field to the existing visual
theme, seed it to `0.90` in the project builder, and commit the generated asset.

**Rationale**: This keeps presentation tuning editable and reproducible without
hard-coding a content value in runtime behavior.

**Alternatives considered**:

- Runtime constant: rejected by the project's data-driven content principle.
- Gameplay robot config: rejected because this value has no gameplay meaning.
- Per-model definition fields: rejected as unnecessary duplication for one global
  Haetae adjustment.

## Decision 4: Apply before character-motion binding

**Decision**: Set the presentation root scale before the motion driver binds and
captures its baseline.

**Rationale**: Attack, hit, locomotion, and death poses restore the captured
baseline. Binding after scale assignment makes 0.90 the stable baseline and
prevents animation or reattachment from resetting it.

**Alternatives considered**:

- Apply after binding: rejected because baseline restoration could revert to 1.0.
- Multiply scale on every frame: rejected as unnecessary work and a source of
  cumulative shrink errors.

## Decision 5: Validate boundaries in existing presentation suites

**Decision**: Extend theme contract tests and presentation PlayMode tests to cover
all Haetae roles, authored/fallback paths, repeated attachment, animation, collider
bounds, and a non-Haetae control.

**Rationale**: The change is presentation-only, so deterministic balance
simulation is not the correct validation path. Existing scene and factory tests
can prove both visual effect and gameplay isolation directly.
