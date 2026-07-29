# Quickstart: 정교한 좀비 모델

**Feature**: `005-zombie-model-refinement`
**Unity**: `6000.3.20f1`
**Authoring**: Blender `4.5 LTS`

## 1. Baseline

- Start from feature-004 commit `6e2154e37d7616ee5df0268b9d4430d8014af232`.
- Baseline validation: EditMode `100/100`, PlayMode `65/65`, Windows build success and standalone ready-marker smoke exit `0`.
- Keep Unity closed during model generation and batch validation.

Expected outputs:

| Role | Editable | LOD0 | LOD1 | Preview |
|------|----------|------|------|---------|
| Runner | `Zombie_Runner.blend` | `Zombie_Runner_LOD0.fbx` | `Zombie_Runner_LOD1.fbx` | `Zombie_Runner_Preview.png` |
| Bruiser | `Zombie_Bruiser.blend` | `Zombie_Bruiser_LOD0.fbx` | `Zombie_Bruiser_LOD1.fbx` | `Zombie_Bruiser_Preview.png` |
| Ripper | `Zombie_Ripper.blend` | `Zombie_Ripper_LOD0.fbx` | `Zombie_Ripper_LOD1.fbx` | `Zombie_Ripper_Preview.png` |

## 2. Generate models

From `TelerobotMVP/`:

```powershell
& 'C:\path\to\blender.exe' --background --factory-startup --python 'ArtSource/Zombies/create_zombie_models.py'
```

Confirm three source metrics, three FBX LOD metrics and `ZOMBIE_MODELS_BUILD_COMPLETE`.

## 3. Rebuild Unity references

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe'
$project = (Resolve-Path 'TelerobotMVP').Path
$results = Join-Path $project 'TestResults'

& $unity -batchmode -nographics -projectPath $project `
  -executeMethod Telerobot.Game.Editor.MvpProjectBuilder.BuildAll `
  -quit -logFile (Join-Path $results 'zombie-models-rebuild.log')
```

Expected: VisualTheme contains three stable role entries and all existing Haetae references remain unchanged.

## 4. Automated validation

```powershell
& $unity -batchmode -nographics -projectPath $project -runTests -testPlatform EditMode `
  -testResults (Join-Path $results 'editmode-zombie-models.xml') `
  -logFile (Join-Path $results 'editmode-zombie-models.log')

& $unity -batchmode -nographics -projectPath $project -runTests -testPlatform PlayMode `
  -testResults (Join-Path $results 'playmode-zombie-models.xml') `
  -logFile (Join-Path $results 'playmode-zombie-models.log')
```

Pass when all existing tests and authored zombie contracts are green.

## 5. Visual review

Review individual previews and `Zombie_Models_Gallery.png`.

- Runner: narrow forward motion, long lower legs, pursuit spines.
- Bruiser: widest silhouette, low head, massive shoulders/forearms, asymmetric corruption.
- Ripper: tallest silhouette, paired scythe arms, split crest, intense central threat cue.
- All three remain identifiable in grayscale at thumbnail size.
- Gore is avoided; corruption remains stylized and readable.

The formal five-person, five-second grayscale survey remains a manual completion item until recorded.

## 6. Windows build and smoke

```powershell
& $unity -batchmode -nographics -projectPath $project `
  -executeMethod Telerobot.Game.Editor.WindowsBuildPipeline.BuildWindowsPlaytestBatch `
  -quit -logFile (Join-Path $results 'windows-zombie-models-build.log')

& (Join-Path $project 'Builds\Windows\TelerobotMVP.exe') `
  -batchmode -nographics -telerobot-smoke `
  -logFile (Join-Path $results 'windows-zombie-models-smoke.log')
```

Pass when the build succeeds and the standalone log contains `TELEROBOT_STANDALONE_SMOKE_READY` with exit code `0`.

## 7. Validation record

Validated on 2026-07-28:

| Role | Source/LOD0 vertices | LOD1 vertices | LOD1 ratio | Materials | Exported bones |
|------|---------------------:|--------------:|-----------:|----------:|---------------:|
| Runner | 19,696 | 7,555 | 38.4% | 5 | 18 |
| Bruiser | 28,070 | 10,191 | 36.3% | 5 | 18 |
| Ripper | 21,320 | 8,186 | 38.4% | 5 | 18 |

- Blender FBX round-trip: PASS, including five populated materials and the
  required humanoid hierarchy for every role.
- Visual review: PASS after a second organic voxel-remesh pass removed the
  visible primitive-joint look.
- EditMode: `101/101` PASS.
- PlayMode: `68/68` PASS.
- Windows player build: PASS at `Builds/Windows/TelerobotMVP.exe`.
- Standalone smoke: exit `0`, log contains
  `TELEROBOT_STANDALONE_SMOKE_READY`.
- Five-person grayscale survey: remains a manual follow-up and is not claimed
  by automated validation.
