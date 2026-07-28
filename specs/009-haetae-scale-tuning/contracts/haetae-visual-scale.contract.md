# Contract: Haetae Visual Scale

## Theme contract

- `haetaeVisualScale` is finite, greater than `0`, and at most `2.0`.
- The committed generated theme value is exactly `0.90`.
- Validation rejects invalid values before runtime world construction.

## Presentation factory contract

When attaching a Haetae role:

1. Create or replace exactly one `Presentation Visual` child.
2. Set its local position and rotation to identity.
3. Set its local scale uniformly from `haetaeVisualScale`.
4. Build either the authored LOD hierarchy or procedural fallback beneath it.
5. Bind character motion after the scale is assigned.
6. Do not modify the gameplay root transform or collider.

When attaching any non-Haetae role, keep the presentation root scale at identity.

## Refresh contract

Repeated attachment, specialization changes, phase restoration, and animation
sampling must restore the configured absolute scale without compounding it.

## Integration acceptance

With the generated theme:

- both live general Haetae visual roots equal `(0.9, 0.9, 0.9)`;
- melee, ranged, and balanced visual roots equal `(0.9, 0.9, 0.9)`;
- procedural Haetae fallback also equals `(0.9, 0.9, 0.9)`;
- gameplay collider bounds are unchanged to within `0.001`;
- a Runner presentation root remains `(1, 1, 1)`.
