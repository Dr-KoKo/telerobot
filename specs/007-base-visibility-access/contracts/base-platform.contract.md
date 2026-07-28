# Contract: Central Base Platform

## Construction input

The runtime constructor receives:

- an existing base anchor position;
- a validated `WorldLayoutConfig`;
- a fallback material/color path;
- an optional presentation decorator.

Invalid layout data must be rejected during catalog mapping before scene
construction.

## Required hierarchy

```text
Central Base
|-- Base Terrace Surface
`-- Presentation Visual
    |-- terrace trim elements
    `-- Guardian Beacon
```

The surface contains the configured number of alternating slope/level bands.
Rebuilding or decorating the same anchor must not duplicate `Presentation Visual`
or active surface colliders.

## Surface contract

- `Base Terrace Surface` has one enabled static mesh collider.
- The collider uses the same continuous mesh and transform as the visible renderer.
- No enabled full-height box, capsule, or hidden wall surrounds the anchor.
- The highest broad collider top equals `terraceCount * terraceRise`.
- The player ascends through each configured slope without relying on controller
  step-up behavior.

## Presentation contract

- The terrace renderers receive the configured world structure/trim material roles.
- The guardian beacon diameter does not exceed configuration.
- Beacon and trim colliders are absent or disabled.
- Missing enhanced presentation leaves the terrace renderers and colliders active.

## Zombie perimeter contract

- Each slot has planar distance from base center of at least
  `outerRadius + edgePadding`.
- Equal input yields equal output.
- Ordinals distribute across tangent positions and radial rows.
- A zero-length route approach uses a stable forward fallback.
- The contract changes coordinates only; damage cadence and target selection are
  unchanged.
