# Contract: Haetae Visibility Foundation

## Spawn contract

- Each spawned Haetae actor root has identity local scale.
- Its capsule uses mapped robot radius, height, and center data.
- Its physical world bounds match the legacy footprint within `0.001`.
- Exactly one `Presentation Visual` child owns the uniform theme scale.
- Exactly one occlusion fader binds to the current presentation.

## Obstruction contract

For each Haetae independently:

1. Return clear outside third-person view, without a camera, without a current
   presentation, or outside maximum distance.
2. Inspect only cached renderers that are enabled and active in hierarchy.
3. Expand each renderer world bound by the configured corridor margin.
4. Return obstructing when the camera center ray intersects any expanded bound in
   front of the camera and within range.
5. Do not consult the gameplay collider to decide visual obstruction.

## Material contract

- Obstructing target opacity is `0.24` by default.
- Transparent variants use alpha blending, no depth write, no shadow caster, and
  no preserved full-strength specular contribution.
- Existing property-block RGB values remain unchanged while alpha transitions.
- Fully restored renderers use their exact original material arrays.
- Replacement and destruction release only variants owned by that fader.

## Boundary contract

- Physical footprint, movement, navigation, separation, targeting, combat,
  progression, telemetry, and HUD remain unchanged.
- First-person always resolves clear.
- Non-Haetae presentation and scale remain unchanged.
