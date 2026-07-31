# Data Model: Haetae Camera Occlusion

## HaetaeOcclusionFadeDefinition

Designer-authored presentation tuning nested in `VisualThemeDefinitionAsset`.

| Field | Type | Default | Validation |
|-------|------|---------|------------|
| `enabled` | Boolean | `true` | Always valid |
| `obstructingOpacity` | Float | `0.32` | Finite, `0.05..0.95` |
| `fadeSeconds` | Float | `0.15` | Finite, `0.01..2.0` |
| `restoreSeconds` | Float | `0.25` | Finite, `0.01..2.0` |
| `aimCorridorRadius` | Float | `0.45` | Finite, `0.01..3.0` world units |
| `maxDistance` | Float | `35.0` | Finite, `1.0..200.0` world units |

The definition owns presentation values only. It cannot change actor, physics,
combat, navigation, or simulation values.

## HaetaeOcclusionPresentationState

Runtime state owned independently by each `HaetaeCameraOcclusionFader`.

| Field | Meaning |
|-------|---------|
| `presentationRoot` | Current named replaceable visual hierarchy |
| `trackedRenderers` | Renderers captured from the current hierarchy |
| `opaqueMaterials` | Exact shared material arrays to restore |
| `transparentMaterials` | Component-owned transparent variants |
| `currentOpacity` | Current interpolated opacity in `[configured, 1]` |
| `isObstructing` | Result of the latest third-person corridor evaluation |
| `player` | Camera/perspective provider; read-only |
| `tuning` | Validated theme definition; read-only |

## State Transitions

```text
Unbound --presentation found--> Opaque
Opaque --third-person obstruction--> Fading
Fading --opacity reaches configured value--> Faded
Fading/Faded --corridor clears or first-person--> Restoring
Restoring --opacity reaches 1--> Opaque
Any bound state --presentation replaced--> Unbound -> Opaque/Fading
Any state --component destroyed--> Restore originals -> Released
```

## Invariants

- `currentOpacity` never leaves `[obstructingOpacity, 1]`.
- Full opacity uses the exact original opaque material references.
- Transparent materials are never shared between Haetae fader instances.
- Presentation replacement releases all variants owned for the obsolete model.
- Collider enabled state, bounds, transform, and gameplay values are never
  written by the fader.
- Non-Haetae renderers are never captured.
