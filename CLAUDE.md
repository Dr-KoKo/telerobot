<!-- SPECKIT START -->
## Active Feature Plan

For technologies, project structure, shell commands, and other important context,
read the current implementation plan and its design artifacts:

- Plan: `specs/001-robot-base-defense-mvp/plan.md`
- Spec (product source of truth): `specs/001-robot-base-defense-mvp/spec.md`
- Research / technical decisions: `specs/001-robot-base-defense-mvp/research.md`
- Data model: `specs/001-robot-base-defense-mvp/data-model.md`
- Contracts: `specs/001-robot-base-defense-mvp/contracts/`
- Quickstart / validation: `specs/001-robot-base-defense-mvp/quickstart.md`

Engine: Unity 6.3 LTS (baseline, confirm patch in Hub) · Windows PC first · keyboard+mouse first ·
new Input System · data-driven ScriptableObjects · pure C# `Game.Core` (scene-free, EditMode-tested) ·
deterministic simulation (seeded RNG + fixed clock + headless waypoint movement, no NavMeshAgent) ·
player-facing Korean strings stored verbatim as data.
<!-- SPECKIT END -->
