# Research: 해태 업그레이드 모델

**Date**: 2026-07-27
**Feature**: `004-haetae-upgrade-models`

## 1. Variant source strategy

**Decision**: Use one new deterministic variant recipe that imports the validated General construction module, rebuilds the common body for each role, and saves separate editable sources and exports.

**Rationale**: Shared functions keep proportions, rig names, materials, markers and bug fixes identical while separate `.blend` files let an artist open a single production variant without running the whole gallery.

**Alternatives considered**:
- Duplicate the General script three times: rejected because fixes would drift.
- Keep all variants only in one large `.blend`: rejected because independent handoff and regeneration are harder.
- Add runtime primitives to the General FBX: rejected because they would repeat the visual-quality problem feature 003 solved.

## 2. Role silhouettes

**Decision**:
- Melee: forward ram crown, paired swept side horns, enlarged shoulder shields, foreleg bracers and reinforced jaw.
- Ranged: armored dorsal turret, long central barrel, sensor wings, energy pods and rear stabilizers.
- Balanced: compact offset turret, one reinforced jaw/tusk side, opposite sensor shoulder and mixed foreleg armor.

**Rationale**: Each role receives at least two large, grayscale-readable cues that communicate function at gameplay distance. Details remain layered guardian armor rather than arbitrary surface noise.

**Alternatives considered**:
- Color-only variants: rejected by the readability contract.
- Three weapons on an identical body: rejected because the body silhouette would remain ambiguous.
- Completely different bodies: rejected because shared haetae lineage and animation/rig compatibility would be lost.

## 3. Runtime reference shape

**Decision**: Add a role-keyed authored model entry array to the existing visual theme instead of six independent top-level fields.

**Rationale**: The General fields remain backward compatible, while an array keeps LOD pairs, asset IDs and roles together and supports a single resolver/instantiation path.

**Alternatives considered**:
- Six new prefab fields: simple but repetitive and easy to mismatch.
- Store references on gameplay specialization data: rejected because feature 002 owns gameplay, not presentation assets.
- Load by string path at runtime: rejected because builds should use serialized project-local references.

## 4. Geometry, rig and material contract

**Decision**: Require each LOD0 to exceed 18,000 vertices, LOD1 to remain below 70%, exactly five populated semantic materials, the General rig names and two marker children.

**Rationale**: These thresholds reject accidental primitive or empty-material exports while preserving the proven runtime remapping and rigid-bone integration.

**Alternatives considered**:
- File-size checks only: rejected because compressed size does not prove useful geometry.
- Unbounded high-poly outputs: rejected because gameplay models need predictable runtime cost.
- Separate materials per specialization: rejected because shared theme remapping and batching would fragment.

## 5. Fallback and state ownership

**Decision**: Resolve authored visuals from the already-selected feature-002 specialization and fall back per role to the feature-003 procedural representation.

**Rationale**: This feature changes only what is displayed. A missing Melee file must not affect Ranged, Balanced, General or progression state.

**Alternatives considered**:
- Fail the visual build when a model is missing: rejected because art must not block play.
- Recreate specialization selection in the visual layer: rejected because it duplicates domain state.
- Fall back all haetae roles together: rejected because one damaged asset should not disable valid ones.

## 6. Review evidence

**Decision**: Render one neutral preview per role and a same-export gallery image; keep the five-person grayscale recognition survey as the final manual evidence item.

**Rationale**: Individual renders expose topology/material issues, while the gallery proves comparative silhouette. Automated contracts cannot establish human recognition on their own.

**Alternatives considered**:
- Runtime screenshots only: rejected because camera, effects and UI obscure asset inspection.
- Automated pixel comparison: rejected because renderer and driver differences are brittle.
