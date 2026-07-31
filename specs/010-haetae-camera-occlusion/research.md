# Research: Haetae Camera Occlusion

## Decision 1: Conditional fade instead of permanent transparency

**Decision**: Preserve full opaque materials normally and fade only a Haetae whose
collider enters the central third-person aiming corridor.

**Rationale**: This directly addresses the reported visibility problem while
retaining authored material quality, unit identity, and specialization
readability in normal play.

**Alternatives considered**:

- Permanent transparency: rejected because it weakens the models in every view
  and keeps transparent-sorting costs active continuously.
- Further permanent size reduction: rejected because 90% scale already exists,
  further shrinking reduces guardian presence, and camera-angle obstruction can
  still occur.
- Enemy through-wall outline: deferred because masking it to Haetae-only
  occlusion requires an extra render pass and risks revealing enemies through
  unrelated world geometry.

## Decision 2: Camera-centered non-allocating corridor query

**Decision**: Use one fixed-buffer sphere corridor query per Haetae in third-person
view and activate only when a returned collider belongs to that Haetae.

**Rationale**: A modest radius around the crosshair catches near-center model
obstruction, includes procedural and authored presentations through the stable
gameplay collider, and avoids per-frame arrays.

**Alternatives considered**:

- Exact projected renderer-bounds overlap against every zombie: rejected because
  it scales with zombie count and would be more expensive and brittle across LODs.
- A single center ray: rejected because a large model can obscure the crosshair
  neighborhood without its collider intersecting an infinitesimal ray.
- Formation/navigation changes: rejected because they alter gameplay movement and
  do not guarantee visibility from all camera angles.

## Decision 3: Transparent clones only while obstructing

**Decision**: Cache original renderer material arrays, create component-owned
transparent variants when a presentation is first bound, assign variants only
during a fade, and restore the exact originals at full opacity.

**Rationale**: Current materials are opaque URP Lit materials. Changing a shared
runtime material would affect both robots and other roles. Per-Haetae variants
allow independent opacity while the normal path returns to the original opaque
rendering mode.

**Alternatives considered**:

- Change shared theme materials: rejected because opacity would leak to every
  consumer of the material library.
- Set only base-color alpha on opaque materials: rejected because opaque surface
  mode ignores alpha.
- Add a new custom dither shader: deferred because the first pass can satisfy the
  visibility contract with existing URP material properties and no new shader
  asset.

## Decision 4: Follow the replaceable presentation hierarchy

**Decision**: Keep the fader on the Haetae gameplay root and detect changes to the
named `Presentation Visual` child. Restore and release old bindings, then capture
the new hierarchy.

**Rationale**: Specialization and phase restoration replace the visual child.
Root ownership survives those changes without adding coupling to combat or
progression state.

**Alternatives considered**:

- Put the component on each visual child: rejected because replacement would need
  every model-construction path to recreate and configure it.
- Modify `HaetaeRobotActor`: rejected because actor state should not own camera or
  rendering policy.

## Decision 5: Preserve current tint property blocks

**Decision**: When a renderer has an existing material property block, retain all
its values and change only base-color alpha during the visibility transition.

**Rationale**: Unit accents and destroyed-state tint already use property blocks.
Clearing or replacing them would corrupt identity and damage presentation.

**Alternatives considered**:

- Clear blocks while faded: rejected because specialization markers and rubble
  tint would disappear.
- Reconstruct all colors from theme roles: rejected because it duplicates model
  material mapping and would be fragile for future authored assets.
