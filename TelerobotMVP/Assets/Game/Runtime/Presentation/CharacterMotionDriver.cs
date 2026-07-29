using System;
using System.Collections.Generic;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public enum CharacterMotionState
    {
        Idle,
        Locomotion,
        Attack,
        Hit,
        Death
    }

    public enum CharacterAttackMotion
    {
        Standard,
        Melee,
        Ranged,
        Balanced,
        Ripper
    }

    [DefaultExecutionOrder(1000)]
    public sealed class CharacterMotionDriver : MonoBehaviour
    {
        private enum TargetKind
        {
            Body,
            Head,
            LeftArm,
            RightArm,
            LeftLeg,
            RightLeg,
            Tail,
            Weapon
        }

        private sealed class TargetBinding
        {
            public Transform target;
            public TargetKind kind;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
        }

        private readonly List<TargetBinding> bindings = new List<TargetBinding>(48);
        private Transform visualRoot;
        private CharacterMotionProfileDefinition profile;
        private Vector3 visualLocalPosition;
        private Quaternion visualLocalRotation;
        private Vector3 visualLocalScale;
        private Vector3 previousWorldPosition;
        private float cycle;
        private float attackElapsed;
        private float attackRemaining;
        private float hitElapsed;
        private float hitRemaining;
        private float deathElapsed;
        private float deathDuration;
        private float phaseOffset;
        private bool deathTriggered;
        private CharacterAttackMotion attackMotion;

        public PresentationRole Role { get; private set; }
        public string ProfileId { get { return profile == null ? string.Empty : profile.profileId; } }
        public CharacterMotionState State { get; private set; }
        public float NormalizedPhase { get; private set; }
        public int BoundTargetCount { get { return bindings.Count; } }
        public int BindCount { get; private set; }
        public Transform VisualRoot { get { return visualRoot; } }

        public void Bind(
            Transform presentationRoot,
            PresentationRole role,
            CharacterMotionProfileDefinition motionProfile)
        {
            RestoreBaselines();
            bindings.Clear();
            visualRoot = presentationRoot;
            Role = role;
            profile = motionProfile;
            attackRemaining = 0f;
            hitRemaining = 0f;
            deathTriggered = false;
            State = CharacterMotionState.Idle;
            NormalizedPhase = 0f;
            previousWorldPosition = transform.position;
            cycle = 0f;
            phaseOffset = StablePhase(gameObject.name);

            if (visualRoot != null)
            {
                visualLocalPosition = visualRoot.localPosition;
                visualLocalRotation = visualRoot.localRotation;
                visualLocalScale = visualRoot.localScale;
                CacheTargets(visualRoot);
            }
            BindCount++;
            enabled = visualRoot != null && profile != null;
        }

        public void Unbind()
        {
            RestoreBaselines();
            bindings.Clear();
            visualRoot = null;
            profile = null;
            State = CharacterMotionState.Idle;
            NormalizedPhase = 0f;
            enabled = false;
        }

        public void TriggerAttack(CharacterAttackMotion kind)
        {
            if (profile == null || deathTriggered) return;
            attackMotion = kind;
            attackElapsed = 0f;
            attackRemaining = profile.attackDuration;
        }

        public void TriggerHit()
        {
            if (profile == null || deathTriggered) return;
            hitElapsed = 0f;
            hitRemaining = profile.hitDuration;
        }

        public void TriggerDeath(float duration)
        {
            if (profile == null) return;
            deathTriggered = true;
            deathElapsed = 0f;
            deathDuration = Mathf.Max(0.05f, duration);
        }

        public void SampleForTests(
            CharacterMotionState state,
            float normalizedPhase,
            CharacterAttackMotion kind = CharacterAttackMotion.Standard)
        {
            if (profile == null || visualRoot == null) return;
            State = state;
            NormalizedPhase = Mathf.Clamp01(normalizedPhase);
            attackMotion = kind;
            ApplyPose(State, NormalizedPhase);
        }

        private void LateUpdate()
        {
            if (profile == null || visualRoot == null) return;

            var deltaTime = Time.deltaTime;
            var displacement = transform.position - previousWorldPosition;
            displacement.y = 0f;
            var moving = deltaTime > 0f && displacement.sqrMagnitude > 0.000001f;
            previousWorldPosition = transform.position;
            cycle = Mathf.Repeat(cycle + deltaTime * profile.cycleHz, 1f);

            if (deathTriggered)
            {
                deathElapsed += deltaTime;
                State = CharacterMotionState.Death;
                NormalizedPhase = Mathf.Clamp01(deathElapsed / deathDuration);
            }
            else if (hitRemaining > 0f)
            {
                hitElapsed += deltaTime;
                hitRemaining = Mathf.Max(0f, hitRemaining - deltaTime);
                State = CharacterMotionState.Hit;
                NormalizedPhase = Mathf.Clamp01(hitElapsed / profile.hitDuration);
            }
            else if (attackRemaining > 0f)
            {
                attackElapsed += deltaTime;
                attackRemaining = Mathf.Max(0f, attackRemaining - deltaTime);
                State = CharacterMotionState.Attack;
                NormalizedPhase = Mathf.Clamp01(attackElapsed / profile.attackDuration);
            }
            else
            {
                State = moving ? CharacterMotionState.Locomotion : CharacterMotionState.Idle;
                NormalizedPhase = Mathf.Repeat(cycle + phaseOffset, 1f);
            }

            ApplyPose(State, NormalizedPhase);
        }

        private void CacheTargets(Transform root)
        {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < transforms.Length; index++)
            {
                var target = transforms[index];
                if (target == root || target.GetComponent<SkinnedMeshRenderer>() != null ||
                    !TryTargetKind(target.name, target.localPosition, out var kind)) continue;
                bindings.Add(new TargetBinding
                {
                    target = target,
                    kind = kind,
                    localPosition = target.localPosition,
                    localRotation = target.localRotation,
                    localScale = target.localScale
                });
            }
        }

        private void ApplyPose(CharacterMotionState state, float phase)
        {
            RestoreBaselines();
            var wave = Mathf.Sin(phase * Mathf.PI * 2f);
            var alternate = Mathf.Cos(phase * Mathf.PI * 2f);
            var rootEuler = Vector3.zero;
            var rootOffset = Vector3.zero;

            if (state == CharacterMotionState.Idle)
            {
                rootOffset.y = wave * profile.idleBob;
                rootEuler.x = profile.forwardLeanDegrees + wave * profile.swayDegrees * 0.22f;
                rootEuler.z = alternate * profile.swayDegrees * 0.35f;
            }
            else if (state == CharacterMotionState.Locomotion)
            {
                rootOffset.y = Mathf.Abs(wave) * profile.locomotionBob;
                rootEuler.x = profile.forwardLeanDegrees + Mathf.Abs(alternate) * 2f;
                rootEuler.z = wave * profile.swayDegrees;
            }
            else if (state == CharacterMotionState.Attack)
            {
                var impact = 1f - Mathf.SmoothStep(0f, 1f, phase);
                var recover = Mathf.Sin(phase * Mathf.PI);
                var ranged = attackMotion == CharacterAttackMotion.Ranged;
                rootOffset.z = (ranged ? -1f : 1f) * impact * profile.attackRecoil;
                rootEuler.x = (ranged ? -0.35f : 1f) * impact * profile.attackDegrees;
                rootEuler.z = attackMotion == CharacterAttackMotion.Ripper
                    ? recover * profile.swayDegrees * 1.8f : 0f;
            }
            else if (state == CharacterMotionState.Hit)
            {
                var kick = Mathf.Sin(phase * Mathf.PI) * (1f - phase);
                rootOffset.z = -kick * profile.attackRecoil * 0.7f;
                rootEuler.x = -kick * profile.hitDegrees;
                rootEuler.z = Mathf.Sin(phase * Mathf.PI * 3f) * profile.hitDegrees * 0.35f;
            }
            else
            {
                var collapse = Mathf.SmoothStep(0f, 1f, phase);
                rootOffset.y = -profile.locomotionBob * collapse;
                rootEuler.x = profile.forwardLeanDegrees + profile.deathDegrees * collapse;
                rootEuler.z = profile.swayDegrees * collapse;
            }

            visualRoot.localPosition = visualLocalPosition + rootOffset;
            visualRoot.localRotation = visualLocalRotation * Quaternion.Euler(rootEuler);
            ApplyTargetPoses(state, phase, wave, alternate);
        }

        private void ApplyTargetPoses(
            CharacterMotionState state, float phase, float wave, float alternate)
        {
            for (var index = 0; index < bindings.Count; index++)
            {
                var binding = bindings[index];
                if (binding.target == null) continue;
                var euler = Vector3.zero;
                if (state == CharacterMotionState.Idle)
                {
                    if (binding.kind == TargetKind.Head) euler.y = wave * profile.swayDegrees;
                    else if (binding.kind == TargetKind.Tail) euler.z = wave * profile.strideDegrees * 0.35f;
                    else if (binding.kind == TargetKind.Body) euler.x = alternate * profile.swayDegrees * 0.2f;
                }
                else if (state == CharacterMotionState.Locomotion)
                {
                    var side = IsRight(binding.kind) ? -1f : 1f;
                    if (IsArm(binding.kind)) euler.x = wave * profile.strideDegrees * side;
                    else if (IsLeg(binding.kind)) euler.x = alternate * profile.strideDegrees * side;
                    else if (binding.kind == TargetKind.Head) euler.x = -Mathf.Abs(wave) * profile.swayDegrees;
                    else if (binding.kind == TargetKind.Tail) euler.z = wave * profile.strideDegrees * 0.55f;
                }
                else if (state == CharacterMotionState.Attack)
                {
                    var impact = 1f - Mathf.SmoothStep(0f, 1f, phase);
                    var ranged = attackMotion == CharacterAttackMotion.Ranged;
                    if (binding.kind == TargetKind.Head) euler.x = (ranged ? -0.4f : 0.35f) * impact * profile.attackDegrees;
                    else if (binding.kind == TargetKind.Weapon)
                        euler.x = (ranged ? -1f : 1f) * impact * profile.attackDegrees;
                    else if (IsArm(binding.kind))
                        euler.x = impact * profile.attackDegrees *
                                  (attackMotion == CharacterAttackMotion.Ripper ? 1.25f : 0.75f);
                    else if (binding.kind == TargetKind.Body)
                        euler.y = attackMotion == CharacterAttackMotion.Balanced
                            ? impact * profile.swayDegrees : 0f;
                }
                else if (state == CharacterMotionState.Hit)
                {
                    var kick = Mathf.Sin(phase * Mathf.PI) * (1f - phase);
                    if (binding.kind == TargetKind.Head) euler.z = kick * profile.hitDegrees;
                    else if (binding.kind == TargetKind.Body) euler.x = -kick * profile.hitDegrees * 0.35f;
                }
                else
                {
                    var collapse = Mathf.SmoothStep(0f, 1f, phase);
                    if (binding.kind == TargetKind.Head) euler.x = collapse * profile.deathDegrees * 0.4f;
                    else if (IsArm(binding.kind)) euler.z = (IsRight(binding.kind) ? -1f : 1f) *
                                                            collapse * profile.deathDegrees * 0.35f;
                    else if (IsLeg(binding.kind)) euler.x = -collapse * profile.deathDegrees * 0.45f;
                    else if (binding.kind == TargetKind.Tail) euler.z = collapse * profile.deathDegrees * 0.55f;
                }
                binding.target.localRotation = binding.localRotation * Quaternion.Euler(euler);
            }
        }

        private void RestoreBaselines()
        {
            if (visualRoot != null)
            {
                visualRoot.localPosition = visualLocalPosition;
                visualRoot.localRotation = visualLocalRotation;
                visualRoot.localScale = visualLocalScale;
            }
            for (var index = 0; index < bindings.Count; index++)
            {
                var binding = bindings[index];
                if (binding.target == null) continue;
                binding.target.localPosition = binding.localPosition;
                binding.target.localRotation = binding.localRotation;
                binding.target.localScale = binding.localScale;
            }
        }

        private static bool TryTargetKind(string targetName, Vector3 localPosition, out TargetKind kind)
        {
            var name = targetName.ToLowerInvariant();
            if (name.Contains("head") || name.Contains("neck"))
            {
                kind = TargetKind.Head;
                return true;
            }
            if (name.Contains("tail"))
            {
                kind = TargetKind.Tail;
                return true;
            }
            if (name.Contains("barrel") || name.Contains("turret") || name.Contains("ram") ||
                name.Contains("fang") || name.Contains("blade") || name.Contains("scythe"))
            {
                kind = TargetKind.Weapon;
                return true;
            }
            if (name.Contains("arm") || name.Contains("hand"))
            {
                kind = IsLeftName(name, localPosition) ? TargetKind.LeftArm : TargetKind.RightArm;
                return true;
            }
            if (name.Contains("thigh") || name.Contains("shin") || name.Contains("foot") ||
                name.Contains("leg"))
            {
                kind = IsLeftName(name, localPosition) ? TargetKind.LeftLeg : TargetKind.RightLeg;
                return true;
            }
            if (name.Contains("body") || name.Contains("chest") || name.Contains("spine") ||
                name.Contains("hips") || name.Contains("torso") || name.Contains("chassis"))
            {
                kind = TargetKind.Body;
                return true;
            }
            kind = TargetKind.Body;
            return false;
        }

        private static bool IsLeftName(string name, Vector3 localPosition)
        {
            return name.Contains("_l") || name.Contains("left") ||
                   (!name.Contains("_r") && !name.Contains("right") && localPosition.x < 0f);
        }

        private static bool IsArm(TargetKind kind)
        {
            return kind == TargetKind.LeftArm || kind == TargetKind.RightArm;
        }

        private static bool IsLeg(TargetKind kind)
        {
            return kind == TargetKind.LeftLeg || kind == TargetKind.RightLeg;
        }

        private static bool IsRight(TargetKind kind)
        {
            return kind == TargetKind.RightArm || kind == TargetKind.RightLeg;
        }

        private static float StablePhase(string value)
        {
            unchecked
            {
                var hash = 17;
                for (var index = 0; index < value.Length; index++) hash = hash * 31 + value[index];
                return Mathf.Abs(hash % 997) / 997f;
            }
        }
    }
}
