# Data Model: Base Visibility and Walkable Access

## `WorldLayoutAsset` additions

| Field | Type | Meaning | Default |
|---|---|---|---|
| `baseOuterRadius` | float | Radius of the widest walkable terrace and zombie exclusion footprint | `4.0` |
| `baseTerraceCount` | int | Number of concentric walkable levels | `3` |
| `baseTerraceRise` | float | Top-height increase between adjacent levels | `0.25` |
| `baseTerraceDepth` | float | Radial inset between adjacent levels | `0.75` |
| `baseTerraceSlopeRun` | float | Horizontal run of the climbable slope leading to each level | `0.50` |
| `baseBeaconDiameter` | float | Maximum width of the identity element above the broad body | `1.0` |
| `baseAttackEdgePadding` | float | Clear radial gap between footprint and first attack row | `0.15` |
| `baseAttackRowSpacing` | float | Radial separation between successive attack rows | `0.75` |
| `baseAttackLateralSpacing` | float | Tangential separation between attackers in a row | `0.95` |

Validation invariants:

- All dimensions and spacings are finite and positive except edge padding, which may
  be zero.
- `baseTerraceCount` is between 1 and 8.
- `baseTerraceCount * baseTerraceRise` is at most `0.75`.
- `baseTerraceSlopeRun` is positive and no greater than `baseTerraceDepth`.
- `baseOuterRadius - (baseTerraceCount - 1) * baseTerraceDepth -
  baseTerraceSlopeRun` remains positive and greater than half the beacon diameter.
- `baseBeaconDiameter` is at most `1.0`.

The fields map one-to-one to PascalCase members in `WorldLayoutConfig`.

## `CentralBasePlatform`

One component on the scale-one `Central Base` runtime anchor.

| Runtime property | Meaning |
|---|---|
| `OuterRadius` | Configured horizontal footprint |
| `TerraceCount` | Number of successfully constructed levels |
| `TopHeight` | Highest broad walkable surface |
| `BeaconDiameter` | Configured maximum presentation beacon width |
| `SurfaceColliders` | The one enabled static mesh collider matching the continuous visible surface |

### Terrace derivation

For zero-based terrace index `i`:

```text
radius(i) = outerRadius - terraceDepth * i
rampTopRadius(i) = radius(i) - terraceSlopeRun
topHeight(i) = terraceRise * (i + 1)
```

The unified surface connects `radius(i)` to `rampTopRadius(i)` while rising to
`topHeight(i)`, then remains level until the next terrace radius. All rings share one
closed mesh, preventing internal collision seams.

## `BasePerimeterSlotRequest`

A pure value set passed to `BasePerimeterRules`:

- base center;
- route approach vector;
- outer radius;
- zombie ordinal;
- edge padding;
- row spacing;
- lateral spacing.

The rule normalizes the horizontal approach, chooses a row from the ordinal, applies
the row's radial distance, and adds a tangent offset. Output height is provided by
the caller so the core rule has no physics dependency.

## Ownership and invariants

- `WorldLayoutAsset` owns tuning and `MvpDataMapper` rejects invalid profiles.
- `CentralBasePlatform` owns only static geometry construction and observable
  hierarchy metadata.
- `WorldArtBuilder` owns materials, trim, and beacon visuals; it does not own
  walkable collision.
- `BasePerimeterRules` owns attack-slot geometry but not target priority, movement,
  attack cadence, or damage.
- `MvpGameController` remains the adapter that supplies route approach and actor
  ordinal and converts the core result to Unity coordinates.
