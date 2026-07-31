# Contract: Haetae Camera Occlusion

## Configuration Contract

The active visual theme MUST provide a non-null validated occlusion definition.
Invalid opacity, timing, radius, or distance prevents the presentation theme from
passing its existing validation contract.

## Runtime Binding Contract

For each live Haetae gameplay root:

1. Exactly one fader is initialized with the current player camera provider and
   visual theme.
2. The fader observes only the child named `Presentation Visual`.
3. Renderer/material bindings are rebuilt only when that child reference changes.
4. The gameplay root, collider, actor state, and status UI are read-only to the
   fader.

## Obstruction Contract

A Haetae is obstructing only when all conditions are true:

- occlusion fading is enabled;
- the camera perspective is third-person;
- a valid camera and current presentation exist;
- the central aiming corridor intersects a collider belonging to that Haetae
  within the configured maximum distance.

First-person view or a clear corridor always targets full opacity.

## Material Ownership Contract

- Normal opaque renderer material arrays are retained exactly.
- Transparent variants are private to one fader instance.
- Existing property-block RGB and non-color properties are preserved; only alpha
  is adjusted during the transition.
- At full opacity, renderers use their original opaque materials.
- Rebinding or destruction restores originals and releases owned variants.

## Timing Contract

- Obstruction target: `0.32` opacity in at most `0.15` seconds.
- Clear-view target: `1.0` opacity in at most `0.25` seconds.
- Each Haetae advances independently.
- Pause may hold the current interpolation; resuming continues toward the current
  target without accumulating opacity error.

## Boundary Contract

The feature MUST NOT modify:

- Haetae 0.90 presentation scale or specialization proportions;
- gameplay-root scale, collider bounds, movement, navigation, separation,
  targeting, combat, health, battery, progression, or telemetry;
- camera distance/FOV or first-person body policy;
- player, zombie, medical robot, base, prop, effect, environment, or HUD
  presentation.
