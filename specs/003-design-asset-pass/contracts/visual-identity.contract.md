# Contract: Visual Identity and Readability

## Unit mappings

| Role | Silhouette | Primary cue | Secondary non-color cue |
|------|------------|-------------|--------------------------|
| Haetae general | low quadruped guardian chassis | gold/cyan energy | swept central horn, crown crest and unit-marker sockets |
| Haetae unit 1 | general chassis | cyan unit accent | single crest notch/marker |
| Haetae unit 2 | general chassis | orange unit accent | double crest notch/marker |
| Melee preview | broad front armor/ram | gold-red impact energy | enlarged horns/shoulders |
| Ranged preview | rear/turret volume | cyan beam energy | elevated barrel and slim front |
| Balanced preview | compact turret + jaw | gold/cyan mix | asymmetric mixed attachments |
| Medical | small support body | green-teal | halo/ring and no attack horn |
| Runner | narrow forward-leaning | red | long legs/forward fins |
| Bruiser | wide heavy torso | dark red | shoulder blocks and short stance |
| Ripper | tall angular hunter | magenta | blade forearms and bright core |

## World mappings

| Role | Shape cue | Color/effect cue |
|------|-----------|------------------|
| Central base | octagonal guardian core/tower | cyan core with gold trim |
| Charging station | concentric pad and twin coils | cyan rotating/pulsing ring |
| Safe supply | closed compact crate | green cross/stack marker |
| Risky supply | open beacon/crate | amber warning fins |
| Emergency barrier | segmented wall | cyan-lit vertical ribs |
| North route | chevron/tower | cool blue |
| East route | stacked alley pylons | amber |
| South route | repeated arch/tunnel | violet |

## UI hierarchy

1. Danger/critical alert
2. Reticle and immediate hit/reload feedback
3. Player health/ammo/grenade
4. Base/route pressure
5. Haetae state and command context
6. Radio/status feed
7. Decorative frame

Decorative elements must not enter the central aiming safe area. Red is reserved for imminent danger and enemy confirmation, not neutral controls.

## Effect constraints

- no repeating full-screen white flash;
- transient opacity returns to zero within its configured lifetime;
- ordinary hit effects remain smaller than a zombie torso at the target distance;
- route warnings do not occlude target silhouettes;
- pooled/concurrent counts remain bounded;
- effect disablement leaves all gameplay outcomes unchanged.

## Fallback

If a themed role cannot build, keep the previous primitive renderer and semantic display color. If menu art is missing, use the theme background gradient. If the project font is missing, use the verified Korean system-font chain.
