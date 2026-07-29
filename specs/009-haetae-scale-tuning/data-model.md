# Data Model: Haetae Scale Tuning

## VisualThemeDefinitionAsset

New presentation field:

| Field | Type | Default | Valid range | Meaning |
|-------|------|---------|-------------|---------|
| `haetaeVisualScale` | float | `1.0` for newly constructed themes | greater than `0` and at most `2.0` | Uniform multiplier applied only to Haetae presentation children |

The generated project theme sets the field to `0.90`.

## Composition rules

```text
Gameplay root transform
├── existing capsule collider and actor components (unchanged)
└── Presentation Visual
    ├── local scale = (haetaeVisualScale, haetaeVisualScale, haetaeVisualScale)
    └── authored LOD or procedural fallback hierarchy
```

For specialized live actors:

```text
world visual size =
    existing gameplay root scale
    × existing specialization scaleMultiplier
    × haetaeVisualScale
    × authored mesh local transforms
```

The feature changes only `haetaeVisualScale`.

## Role applicability

The global multiplier applies to:

- Haetae general unit 1
- Haetae general unit 2
- Haetae melee
- Haetae ranged
- Haetae balanced

It does not apply to player, rifle, medical robot, zombie, structure, route, prop,
or effect roles.

## Validation invariants

- `0 < haetaeVisualScale <= 2.0`.
- A presentation refresh sets the scale absolutely; it never multiplies the
  previous presentation scale.
- Gameplay root transform and collider bounds do not change when a presentation
  is attached.
- The character-motion driver captures the tuned scale as its baseline.
- Authored and procedural Haetae paths use the same presentation root.
