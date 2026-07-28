# Research: Character Animation Pass

## Decision 1: Runtime procedural pose driver

Use one lightweight `CharacterMotionDriver` with data profiles rather than importing
clip/controller graphs for every model and LOD.

**Rationale**: The current assets already expose predictable joint names, but their
role variants and LODs are separate prefabs. A procedural driver keeps state
ownership explicit, makes fallback testable, avoids controller duplication, and can
apply one normalized phase to every LOD hierarchy.

**Alternatives considered**:

- Animator Controllers and authored clips: strong for a large animation library, but
  adds controller/transition duplication and makes missing-rig fallback harder.
- Root-only bobbing: cheap, but cannot make role attacks or gait silhouettes readable.

## Decision 2: Strict presentation ownership

Attach the driver to the actor only as a coordinator; all transform writes target
the `Presentation Visual` child and its joints.

**Rationale**: Gameplay roots own navigation, collision, headshot regions, targeting,
and death cleanup. Child-only offsets guarantee the animation pass cannot move an
actor through the base, alter range, or change collision results.

## Decision 3: Actor events plus displacement inference

Actors emit attack, hit, and death triggers after their existing gameplay actions.
The driver detects idle versus locomotion from root displacement.

**Rationale**: This preserves the current combat state machines and keeps presentation
from deciding when an attack lands. Movement inference needs no new command coupling
and works across patrol, pursuit, retreat, and formation movement.

## Decision 4: Shared phase across all LODs

Bind every matching target name in all instantiated LOD hierarchies and apply the
same pose sample to each.

**Rationale**: Unity may render one LOD while both hierarchies exist. Updating both
from one normalized phase prevents visible pose pops during LOD transitions.

## Decision 5: Named semantic targets with root fallback

Recognize semantic target families such as head, chest/body, arms, legs, tail, and
ranged barrel. Unknown or missing targets are ignored; the visual root still gets
small translation/rotation poses.

**Rationale**: Authored FBX bones and procedural fallback parts do not share a formal
avatar. Case-insensitive name matching supports both without making rig completeness
a runtime dependency.

## Decision 6: Refine zombie organic-shell skin weights

Update the Blender recipe so flesh vertices are distributed to the nearest relevant
torso/limb segments instead of remaining rigidly assigned to the spine.

**Rationale**: The existing armatures are sufficient, but rigid torso weighting would
make limb motion look detached. Source-controlled procedural weighting is repeatable
for all three roles and both LOD exports.

## Decision 7: Scaled time and bounded work

Advance motion with `Time.deltaTime`, cache bindings and baselines at bind time, and
avoid hierarchy searches or allocations in `Update`.

**Rationale**: Motion pauses and slows with the game, while cached target arrays keep
large-wave overhead bounded. The one-driver-per-actor rule prevents duplicated work.

## Decision 8: Layered validation

Use EditMode tests for data, binding, fallback, LOD, and sampled-pose invariants;
PlayMode tests for time/state integration and gameplay non-interference; finish with
full regressions, Windows build/smoke, and manual readability checks.

**Rationale**: Visual taste still needs human inspection, but safety, timing, and
fallback behavior can be enforced automatically.
