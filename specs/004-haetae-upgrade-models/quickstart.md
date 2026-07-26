# Quickstart: 해태 업그레이드 모델

**Feature**: `004-haetae-upgrade-models`
**Unity**: `6000.3.20f1`
**Authoring**: Blender `4.5 LTS`

## 1. Prerequisites

- Start from feature-003 commit `8f09b427e070a7912b773295d40ebfa7b1360953`.
- Baseline validation: Unity `6000.3.20f1`, EditMode `99/99`, PlayMode `63/63`, Windows build success and standalone ready-marker smoke exit `0`.
- Keep Unity closed during batch generation and testing.
- Use Blender only to regenerate checked-in authoring outputs; Unity and player builds consume FBX files.
- Keep `.specify/feature.json` pointed to `specs/004-haetae-upgrade-models`.

Expected delivery matrix:

| Role | Editable | LOD0 | LOD1 | Preview |
|------|----------|------|------|---------|
| Melee | `Haetae_Melee.blend` | `Haetae_Melee_LOD0.fbx` | `Haetae_Melee_LOD1.fbx` | `Haetae_Melee_Preview.png` |
| Ranged | `Haetae_Ranged.blend` | `Haetae_Ranged_LOD0.fbx` | `Haetae_Ranged_LOD1.fbx` | `Haetae_Ranged_Preview.png` |
| Balanced | `Haetae_Balanced.blend` | `Haetae_Balanced_LOD0.fbx` | `Haetae_Balanced_LOD1.fbx` | `Haetae_Balanced_Preview.png` |

## 2. Generate models

From `TelerobotMVP/`:

```powershell
& 'C:\path\to\blender.exe' --background --factory-startup --python 'ArtSource/Haetae/create_haetae_upgrades.py'
```

Expected outputs:

- `ArtSource/Haetae/Haetae_Melee.blend`
- `ArtSource/Haetae/Haetae_Ranged.blend`
- `ArtSource/Haetae/Haetae_Balanced.blend`
- six `Assets/Game/Art/Models/Haetae/Haetae_*_LOD*.fbx` files
- three role previews and `Haetae_Upgrades_Gallery.png`

Confirm the generation log reports each role's source vertices and completion marker.

## 3. Rebuild Unity references

Run:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe' `
  -batchmode -nographics `
  -projectPath (Resolve-Path 'TelerobotMVP').Path `
  -executeMethod Telerobot.Game.Editor.MvpProjectBuilder.BuildAll `
  -quit -logFile 'TelerobotMVP/TestResults/haetae-upgrades-rebuild.log'
```

Expected:

- `VisualTheme.asset` contains Melee, Ranged and Balanced authored LOD pairs.
- General references remain unchanged.
- role entries survive a second project rebuild with stable GUIDs.

## 4. Automated validation

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'
$project = (Resolve-Path 'TelerobotMVP').Path
$results = Join-Path $project 'TestResults'

& $unity -batchmode -nographics -projectPath $project -runTests -testPlatform EditMode `
  -testResults (Join-Path $results 'editmode-haetae-upgrades.xml') `
  -logFile (Join-Path $results 'editmode-haetae-upgrades.log')

& $unity -batchmode -nographics -projectPath $project -runTests -testPlatform PlayMode `
  -testResults (Join-Path $results 'playmode-haetae-upgrades.xml') `
  -logFile (Join-Path $results 'playmode-haetae-upgrades.log')
```

Pass when:

- all existing tests remain green;
- every role satisfies vertex, LOD, material, hierarchy and marker contracts;
- all three roles instantiate their matching authored asset;
- marker counts 1/2 work for every role;
- removing one role's reference selects only that role's procedural fallback;
- repeated role replacement leaves no old presentation root or LOD group.

## 5. Visual review

Review individual previews and `Haetae_Upgrades_Gallery.png`.

- Melee: ram/horns/shoulders/front bracers dominate.
- Ranged: turret/barrel/sensors/rear stabilizers dominate.
- Balanced: compact turret and asymmetric reinforced jaw/foreleg both read.
- General lineage remains visible in head, mane, central horn, paws and tail.
- All roles remain distinct in grayscale at thumbnail size.

The formal SC-001 five-person, five-second grayscale survey remains a manual completion item until recorded.

## 6. Windows build and smoke

```powershell
& $unity -batchmode -nographics -projectPath $project `
  -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch `
  -quit -logFile (Join-Path $results 'windows-haetae-upgrades-build.log')

& (Join-Path $project 'Builds\Windows\TelerobotMVP.exe') `
  -batchmode -nographics -telerobot-smoke `
  -logFile (Join-Path $results 'windows-haetae-upgrades-smoke.log')
```

Pass when the build succeeds and the standalone log contains `TELEROBOT_STANDALONE_SMOKE_READY` with exit code `0`.

## 7. Validation record

Validated on 2026-07-27 with Blender `4.5.11 LTS` and Unity `6000.3.20f1`.

| Role | Source vertices | FBX LOD0 | FBX LOD1 | LOD1 ratio |
|------|----------------:|---------:|---------:|-----------:|
| Melee | 31,700 | 31,700 | 16,684 | 52.63% |
| Ranged | 29,808 | 29,808 | 15,675 | 52.59% |
| Balanced | 29,752 | 29,752 | 15,656 | 52.62% |

- Every role reports visible polygons in all five semantic material slots.
- Individual previews and the combined gallery were visually reviewed with complete silhouettes in frame.
- Unity asset rebuild completed successfully.
- EditMode: `100/100` passed.
- PlayMode: `65/65` passed, including authored role selection, two-marker visibility, collider preservation, independent missing-reference fallback and repeated replacement cleanup.
- Windows playtest build: success.
- Standalone smoke: `TELEROBOT_STANDALONE_SMOKE_READY`, exit code `0`.
