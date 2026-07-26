# Contract: Asset Licensing and Provenance

## Adoption gate

An external file may enter a committed runtime folder only when all are true:

- creator and official source are identified;
- commercial game use is allowed;
- modification is allowed for the intended normalization;
- compiled game distribution is allowed;
- raw-source redistribution restrictions are understood;
- license evidence is saved or linked;
- retrieval date and exact original filename are recorded;
- required attribution/notice text is included.

If any item is unknown, the asset remains Candidate or Rejected and the fallback is used.

## Preferred licenses

- CC0-1.0 for general art/audio assets;
- OFL-1.1 for fonts;
- other licenses only after explicit review of attribution and redistribution duties.

Account-gated royalty-free services may be candidates, but acquisition must be reproducible and raw library files must not be redistributed as a standalone collection.

## Generated/project-owned assets

Every generated raster must include:

- generation date;
- final prompt/recipe;
- original generated file path;
- post-processing notes;
- final project path;
- allowed in-project uses.

Code-generated meshes, icons and effects are recorded as project-owned recipes with the source code path.

## Notice output

`TelerobotMVP/Documentation/Art/THIRD-PARTY-NOTICES.md` is the build-facing summary. It must include every adopted external source even when attribution is optional, so provenance remains auditable.
