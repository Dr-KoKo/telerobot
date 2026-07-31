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
generated value `0.80`. It is the only transform scale used to size a Haetae
presentation.

```text
Haetae actor root: scale (1, 1, 1)
|-- CapsuleCollider: radius/height/center from RobotConfig
`-- Presentation Visual: scale (0.80, 0.80, 0.80)
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
bounds intersecting the camera aim ray. Generated opacity becomes `0.10`. A
serialized transparent material template guarantees that the needed rendering
variant is retained in player builds; each runtime copy preserves source color
and surface detail while its emission is multiplied by current opacity.

Occlusion evaluation is perspective-neutral: the current camera position and
forward direction drive the same renderer-bound rule in first- and third-person.

## Perspective Preference

`PlayerSettingsAsset.defaultPerspective` has generated value `FirstPerson`.
`PlayerPreferences` resolves session perspective in this order:

1. a valid saved first- or third-person preference;
2. the data-defined first-person default when no saved value exists.

No migration or deletion of an existing preference occurs.

## Invariants

- Actor root scale is identity after spawn.
- Collider world bounds match the legacy footprint within `0.001`.
- Presentation root scale always equals uniform theme scale.
- Inactive/disabled renderers do not activate obstruction.
- Camera perspective does not suppress renderer obstruction.
- No collider location is required for renderer obstruction.
- Opaque restoration reinstates exact original material references.
- A valid saved perspective always overrides the first-launch default.
