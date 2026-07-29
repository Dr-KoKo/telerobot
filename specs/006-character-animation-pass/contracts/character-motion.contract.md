# Contract: Character Motion Presentation

## Binding

`LowPolyModelFactory.Attach(actor, role)` MUST:

1. replace only the actor's `Presentation Visual` child;
2. resolve the role profile from the active visual theme;
3. create or reuse exactly one `CharacterMotionDriver` for supported roles;
4. bind both authored LOD hierarchies, or bind root-only fallback when joints are
   unavailable;
5. leave the gameplay renderer fallback enabled if model construction fails.

## Runtime input

Actors MAY call:

- `TriggerAttack(CharacterAttackMotion kind)` after an existing attack produces damage;
- `TriggerHit()` after accepted damage;
- `TriggerDeath(duration)` after existing death state begins.

The driver MUST NOT call gameplay damage, targeting, movement, spawning, battery, or
telemetry APIs.

## Runtime output

The driver MAY write local transforms under `Presentation Visual`. It MUST NOT write:

- the gameplay actor transform;
- gameplay colliders or hit regions;
- navigation or route data;
- health, battery, attack cooldowns, damage, or target state.

## Diagnostics

Tests and debugging MUST be able to read the bound role, profile ID, current state,
normalized phase, bound-target count, and bind count. A deterministic pose-sampling
entry point MAY be exposed for EditMode tests but MUST share the production pose code.

## Failure behavior

Missing profiles, bones, optional role parts, or LODs MUST result in an inert or
root-only presentation fallback. No such failure may interrupt actor gameplay.
