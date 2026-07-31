# Data Model: Haetae Visibility Foundation

## Haetae Physical Footprint

Stored in `RobotDefinitionAsset` and mapped to `RobotConfig`:

| Field | Default | Validation | Meaning |
|-------|---------|------------|---------|
| `bodyColliderRadius` | `0.75` | finite and greater than zero | Capsule radius with identity actor transform |
| `bodyColliderHeight` | `1.50` | finite and at least twice radius | Capsule total height |
| `bodyColliderCenterY` | `0.0` | finite | Vertical center offset |

These values reproduce the bounds of the legacy default capsule beneath
`(1.1, 0.75, 1.5)` while allowing the actor transform to remain identity.

## Haetae Visual Presentation

`VisualThemeDefinitionAsset.haetaeVisualScale` remains a uniform scalar with
generated value `0.85`. It is the only transform scale used to size a Haetae
presentation.

```text
Haetae actor root: scale (1, 1, 1)
|-- CapsuleCollider: radius/height/center from RobotConfig
`-- Presentation Visual: scale (0.85, 0.85, 0.85)
    `-- Authored LOD or procedural fallback: identity attachment transform
```

## Occlusion State

Existing per-Haetae state remains:

- current presentation root;
- cached active renderers and opaque material arrays;
- owned transparent material variants;
- current and target opacity;
- obstruction state.

The activation source changes from physical hits to expanded active-renderer
bounds intersecting the camera aim ray. Generated opacity becomes `0.16`.

## Invariants

- Actor root scale is identity after spawn.
- Collider world bounds match the legacy footprint within `0.001`.
- Presentation root scale always equals uniform theme scale.
- Inactive/disabled renderers do not activate obstruction.
- No collider location is required for renderer obstruction.
- Opaque restoration reinstates exact original material references.
