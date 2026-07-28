# Research: 정교한 좀비 모델

## Decision 1: Project-owned deterministic authoring

**Decision**: Create all three models with a checked-in Blender recipe and editable `.blend` outputs, without external model or texture inputs.

**Rationale**: The user asked for a quality increase, and project ownership gives reproducibility, clear redistribution rights and direct control over role silhouettes.

**Alternatives considered**:
- Adopt the cataloged CC0 Quaternius pack: rejected because its visual language would not match the authored Haetae detail level and role requirements without substantial rework.
- Keep expanding runtime primitives: rejected because the existing look is exactly the quality limitation being addressed.

## Decision 2: Shared infection language with role-specific anatomy

**Decision**: Use one humanoid infection language—desaturated flesh, damaged armor, dark tissue, exposed bone and emissive corruption—then vary body proportions and appendages per role.

**Rationale**: Shared materials make the faction cohesive while silhouette changes carry gameplay information without relying on color.

**Alternatives considered**:
- Three unrelated monster species: rejected because it weakens faction cohesion.
- Color-only variants: rejected because it fails grayscale readability.

## Decision 3: Static rigid skinning on a stable humanoid rig

**Decision**: Export a named humanoid armature and rigid vertex groups while retaining the current transform-driven movement and static combat pose.

**Rationale**: The hierarchy is ready for future animation and gives stable validation anchors without expanding the present scope into animation production.

**Alternatives considered**:
- Unrigged meshes: simpler but makes future animation conversion and hierarchy validation harder.
- New animation controller and clips: rejected as out of scope.

## Decision 4: Five populated semantic material families

**Decision**: Every model uses `MAT_ZombieFlesh`, `MAT_ZombieArmor`, `MAT_ZombieTissue`, `MAT_ZombieCorruption` and `MAT_ZombieBone`.

**Rationale**: Five families provide enough surface separation for detail and preserve batching through the existing shared material library.

**Alternatives considered**:
- One atlas/material: rejected because no texture-authoring pass is in scope and material separation carries visual information.
- Unique materials per role: rejected because it increases runtime material count and weakens cohesion.

## Decision 5: Normalize models to the existing gameplay capsule

**Decision**: Author every model around the existing local origin and nominal two-unit capsule height, then inherit the current `displayScale` on the gameplay root.

**Rationale**: Runner, Bruiser and Ripper already use data-driven root scale for collision, headshot threshold and spacing. Reusing it guarantees no gameplay change.

**Alternatives considered**:
- Move scale values into model references: rejected because visual data would begin owning gameplay collision.
- Resize colliders to the new meshes: rejected by scope and regression constraints.

## Decision 6: Role-keyed references with independent fallback

**Decision**: Store three zombie model entries in VisualTheme and resolve them before the existing `BuildEnemy` implementation.

**Rationale**: This mirrors the proven Haetae authored/fallback boundary while keeping every role independently playable.

**Alternatives considered**:
- Replace the procedural code: rejected because it removes resilience.
- Put model references in ZombieDefinitionAsset: rejected because the balance asset should not own DCC presentation dependencies.

## Decision 7: LOD and renderer feedback behavior

**Decision**: LOD0 must exceed 16,000 vertices; LOD1 uses deterministic mesh reduction below 70%. Hit/death feedback continues to tint all child renderers through ZombieActor.

**Rationale**: The density floor prevents another placeholder-quality result while the LOD contract controls battlefield cost. Existing feedback code already addresses child renderers.

**Alternatives considered**:
- LOD0 only: rejected because peak Phase 3 counts make distance reduction valuable.
- Separate feedback shader: rejected because existing material-property-block behavior is sufficient and bounded.
