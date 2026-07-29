# Data Model: 디자인 에셋 패스

**Date**: 2026-07-26  
**Feature**: `003-design-asset-pass`

## 1. Design Asset Catalog

Represents the complete make/find/defer inventory.

| Field | Type | Rules |
|------|------|-------|
| `catalogVersion` | string | required; changes when required inventory or provenance rules change |
| `items` | Asset Item[] | required; unique `id`; must cover contract-required roles |
| `sources` | Source Record[] | source IDs unique; every sourced item resolves to one record |
| `fallbackTheme` | Visual Theme reference | required |

## 2. Asset Item

| Field | Type | Rules |
|------|------|-------|
| `id` | stable string | lowercase dot notation; unique |
| `displayName` | string | development-facing; may be Korean |
| `category` | enum | Character, Enemy, Environment, Equipment, UI, VFX, Animation, Audio, Font |
| `usageRoles` | string[] | at least one game/UI role |
| `priority` | enum | P1, P2, P3 |
| `decision` | enum | Make, Find, Adopt, Defer |
| `status` | enum | Missing, Candidate, InProduction, Integrated, Validated, Deferred, Rejected |
| `assetReferences` | object reference[] | optional until Integrated; project-local only |
| `sourceId` | string? | required for Find/Adopt after Candidate; forbidden for purely project-owned Make unless recording a generation source |
| `fallbackId` | string? | required when the item can be absent at runtime |
| `validationTags` | string[] | at least one automated or manual validation key |
| `notes` | string | rationale, normalization or dependency notes |

### Validation combinations

- `Integrated` requires at least one valid local reference or an explicitly code-generated role.
- `Validated` requires all declared validation tags to have evidence.
- `Adopt` requires a complete source record.
- `Rejected` requires a reason and must not retain an active runtime reference.
- `Deferred` requires a playable fallback.
- `Find` may be `Candidate` or `Deferred`; it may not be `Validated` without adoption and provenance.

## 3. Source Record

| Field | Type | Rules |
|------|------|-------|
| `id` | stable string | unique |
| `title` | string | official asset/package name |
| `creator` | string | required |
| `officialUrl` | URI | HTTPS official distribution or official license page |
| `licenseId` | string | SPDX-like value where possible, e.g. CC0-1.0, OFL-1.1 |
| `licenseEvidencePath` | project path or URI | required before adoption |
| `retrievedOn` | date | required before adoption |
| `originalFiles` | string[] | exact imported filenames/checksums when adopted |
| `modifications` | string[] | scale, material, mesh, audio, font or format changes |
| `attributionRequired` | bool | derived from license/evidence |
| `noticeText` | string | required when attribution/notice is required |
| `redistributionNotes` | string | raw-source restrictions and build inclusion notes |

## 4. Visual Theme

| Field | Type | Rules |
|------|------|-------|
| `themeId` | string | `guardian-night-v1` for this pass |
| `colors` | Style Color[] | required semantic keys; accessible contrast target |
| `materials` | Material Role[] | all required surface roles resolve or use fallback |
| `typography` | Typography Set | body/title/mono fallback chain |
| `iconStyle` | Icon Style | stroke, corner and fill conventions |
| `vfx` | Effect Style[] | duration, size, color, max concurrent; presentation-only |
| `menuBackdrop` | texture reference? | optional; fallback solid gradient required |
| `haetaeGeneralModel` | GameObject reference? | authored LOD0 model; null selects the documented procedural fallback |

### Required semantic color keys

- `world.ground`, `world.structure`, `world.trim`
- `ally.energy`, `ally.haetae`, `ally.unit2`, `ally.medical`
- `enemy.body`, `enemy.corruption`, `enemy.ripper`
- `route.north`, `route.east`, `route.south`
- `state.safe`, `state.caution`, `state.danger`
- `ui.panel`, `ui.line`, `ui.text`, `ui.muted`

## 5. Visual Role

Presentation-only key resolved by the model factory.

| Role group | Required roles |
|------------|----------------|
| Player | commander, assault-rifle |
| Haetae | general-1, general-2, melee-preview, ranged-preview, balanced-preview |
| Support | medical |
| Enemy | runner, bruiser, ripper |
| Base/interactable | central-base, charging-station, safe-supply, risky-supply, emergency-barrier |
| Route | north-road, east-alley, south-tunnel |

Each important unit role declares:

- a silhouette signature (part count/type tags);
- a primary and secondary material role;
- at least one non-color marker;
- optional VFX attachment points;
- a fallback primitive role.

Authored character roles additionally declare:

- a project-local imported model reference;
- named semantic material slots;
- optional LOD assets;
- separately addressable unit-marker children;
- an editable DCC source and deterministic export recipe outside the Unity import tree.

## 6. Generated Asset Record

For image-generated or editor-generated project assets.

| Field | Type | Rules |
|------|------|-------|
| `assetItemId` | string | resolves to catalog |
| `generator` | string | tool/workflow identifier |
| `createdOn` | date | required |
| `promptOrRecipePath` | project path | required |
| `sourcePath` | project path | retained when useful for regeneration |
| `outputPath` | project path | required |
| `postProcessing` | string[] | crop, resize, alpha, compression, import settings |
| `approvedUse` | string[] | screens/roles where used |

## 7. Relationships

```text
Design Asset Catalog
├── 1 ── * Asset Item
│          ├── 0..1 ── Source Record
│          ├── 0..1 ── Generated Asset Record
│          └── 0..1 ── fallback Asset Item
└── 1 ── Visual Theme
           ├── * Style Color
           ├── * Material Role
           ├── 1 Typography Set
           └── * Effect Style

Visual Role ── uses ──> Visual Theme material/color/effect keys
Runtime gameplay root ── owns ──> replaceable child Visual Role
```

## 8. State Transitions

```text
Missing
  ├──> InProduction ──> Integrated ──> Validated
  ├──> Candidate ──> Integrated ──> Validated
  ├──> Deferred
  └──> Rejected

Candidate ──> Rejected
InProduction ──> Deferred
Integrated ──> InProduction (revision required)
Validated ──> InProduction (replacement/version update)
```

Transitions to `Integrated` or `Validated` fail when provenance, fallback or required-role constraints are not satisfied.

## 9. Ownership Boundary

- `Game.Core`: no design asset types.
- `Game.Simulation`: no design asset types.
- `Game.Data`: catalog/theme definitions and references.
- `Game.Runtime`: resolves roles and builds/renders presentation.
- `Game.Editor`: creates/rebuilds assets and validates import settings.
- Documentation: human-readable catalog, style guide and notices mirror the authoritative project asset declarations.
