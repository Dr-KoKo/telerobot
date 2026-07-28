using System.Collections.Generic;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class ZombieActor : MonoBehaviour
    {
        private MvpGameController game;
        private ZombieConfig config;
        private ZombieDefinitionAsset presentation;
        private Vector3[] waypoints;
        private int waypointIndex;
        private float nextAttack;
        private float nextIncidentalMedicalAttack;
        private float hitFlashUntil;
        private float deathElapsed;
        private bool dead;
        private Renderer visualRenderer;
        private Color originalColor;
        private Vector3 deathStartScale;
        private Vector3 deathStartPosition;
        private MaterialPropertyBlock presentationBlock;
        private CharacterMotionDriver motionDriver;

        public ZombieState State { get; private set; }
        public ZombieType Type { get { return State.Type; } }
        public float VisualHeight { get { return transform.localScale.y * 2f; } }
        public float SeparationRadius { get { return config == null ? 1f : config.SeparationRadius; } }
        public Vector3 CurrentNavigationPoint
        {
            get { return waypoints != null && waypointIndex < waypoints.Length ? waypoints[waypointIndex] : transform.position; }
        }
        public bool HitFlashActive { get { return !dead && Time.time < hitFlashUntil; } }
        public bool DeathFeedbackActive { get { return dead; } }
        public Color CurrentVisualColor { get { return visualRenderer == null ? Color.clear : visualRenderer.material.color; } }
        private CharacterMotionDriver MotionDriver
        {
            get
            {
                if (motionDriver == null) motionDriver = GetComponent<CharacterMotionDriver>();
                return motionDriver;
            }
        }

        public void CompleteNavigationForTests()
        {
            waypointIndex = waypoints == null ? 0 : waypoints.Length;
        }

        public void Initialize(MvpGameController owner, string id, ZombieConfig definition, RouteId route,
            Vector3[] routeWaypoints, ZombieDefinitionAsset presentationDefinition)
        {
            game = owner;
            config = definition;
            presentation = presentationDefinition;
            waypoints = routeWaypoints;
            State = new ZombieState(id, definition.Type, route, definition.MaxHealth);
            waypointIndex = 1;
            visualRenderer = GetComponent<Renderer>();
            originalColor = presentation.displayColor;
            motionDriver = GetComponent<CharacterMotionDriver>();
        }

        private void Update()
        {
            if (dead)
            {
                UpdateDeathFeedback();
                return;
            }
            if (visualRenderer != null && hitFlashUntil > 0f && Time.time >= hitFlashUntil)
            {
                visualRenderer.material.color = originalColor;
                ClearPresentationTint();
                hitFlashUntil = 0f;
            }
            if (game == null || game.IsFinished) return;
            var target = ResolveTarget();
            if (!target.HasTarget) return;
            TryIncidentalMedicalDamage(target);
            var targetPosition = target.Position;
            var distance = Vector3.Distance(transform.position, targetPosition);
            if (!target.IsNavigationPoint && distance <= config.AttackRange)
            {
                Attack(target);
                return;
            }
            MoveTowardsTarget(targetPosition, distance);
        }

        private void MoveTowardsTarget(Vector3 targetPosition, float targetDistance)
        {
            var direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f) return;
            var forward = direction.normalized;
            var movement = forward + game.GetZombieAvoidance(this) * config.SeparationStrength;
            movement.y = 0f;
            var progress = Vector3.Dot(movement, forward);
            if (progress < 0.35f) movement += forward * (0.35f - progress);
            if (movement.sqrMagnitude <= 0.0001f) movement = forward;
            movement.Normalize();
            var step = Mathf.Min(config.MoveSpeed * Time.deltaTime, targetDistance);
            transform.position += movement * step;
            transform.rotation = Quaternion.LookRotation(movement);
        }

        private RuntimeTarget ResolveTarget()
        {
            if (game.TryGetBarrier(State.Route, out var barrier) &&
                Vector3.Distance(transform.position, barrier.transform.position) <= config.AttackRange + 1.5f)
                return new RuntimeTarget(TargetKind.Base, barrier.transform, null, barrier);

            while (waypointIndex < waypoints.Length &&
                   Vector3.Distance(transform.position, waypoints[waypointIndex]) < 0.75f)
                waypointIndex++;
            if (waypointIndex < waypoints.Length)
                return RuntimeTarget.Navigation(waypoints[waypointIndex]);

            var candidates = new List<TargetCandidate>
            {
                new TargetCandidate("base", TargetKind.Base, Vector3.Distance(transform.position, game.BaseTransform.position), true),
                new TargetCandidate("player", TargetKind.Player, Vector3.Distance(transform.position, game.PlayerActor.transform.position), !game.PlayerState.Health.IsDead)
            };
            foreach (var robot in game.Robots)
                candidates.Add(new TargetCandidate(robot.State.Id, TargetKind.Robot, Vector3.Distance(transform.position, robot.transform.position), !robot.State.Health.IsDead));

            var selected = TargetingSystem.Select(config, candidates);
            if (selected == null) return RuntimeTarget.Base(game.GetBaseAttackPosition(this));
            if (selected.Kind == TargetKind.Player) return new RuntimeTarget(selected.Kind, game.PlayerActor.transform, null, null);
            if (selected.Kind == TargetKind.Robot)
            {
                var robot = game.Robots.Find(item => item.State.Id == selected.Id);
                return new RuntimeTarget(selected.Kind, robot.transform, robot, null);
            }

            return RuntimeTarget.Base(game.GetBaseAttackPosition(this));
        }

        private void Attack(RuntimeTarget target)
        {
            if (Time.time < nextAttack) return;
            nextAttack = Time.time + config.AttackInterval;
            MotionDriver?.TriggerAttack(Type == ZombieType.Ripper
                ? CharacterAttackMotion.Ripper
                : CharacterAttackMotion.Standard);
            if (target.Barrier != null)
            {
                target.Barrier.ReceiveDamage(config.BaseDamage);
                return;
            }
            if (target.Kind == TargetKind.Base) game.DamageBase(config.BaseDamage);
            else if (target.Kind == TargetKind.Player) game.PlayerActor.ReceiveDamage(config.PlayerDamage, transform.position);
            else if (target.Robot != null)
            {
                target.Robot.ReceiveZombieHit(config.RobotDamage, Type == ZombieType.Ripper);
                if (Type == ZombieType.Ripper) game.NotifyRipperAttack(target.Robot);
            }
        }

        private void TryIncidentalMedicalDamage(RuntimeTarget priorityTarget)
        {
            if (game.MedicalActor == null || !game.MedicalActor.IsAlive || !priorityTarget.HasTarget) return;
            var distance = Vector3.Distance(transform.position, game.MedicalActor.transform.position);
            if (!MedicalRules.ShouldApplyIncidentalDamage(distance, config.AttackRange, true) ||
                Time.time < nextIncidentalMedicalAttack) return;
            nextIncidentalMedicalAttack = Time.time + config.AttackInterval;
            game.MedicalActor.ReceiveDamage(config.RobotDamage);
        }

        public void ReceiveDamage(float amount, DamageSource source)
        {
            if (dead) return;
            var applied = CombatRules.ApplyDamage(State.Health, amount);
            game.RecordZombieContribution(State, source, applied);
            if (!State.Health.IsDead) ShowHitFlash();
            RefreshAfterCoreDamage(source);
        }

        public void ReceiveDamage(float amount, string legacySource)
        {
            ReceiveDamage(amount, LegacyDamageSource(legacySource));
        }

        private void ShowHitFlash()
        {
            MotionDriver?.TriggerHit();
            hitFlashUntil = Time.time + presentation.hitFlashSeconds;
            if (visualRenderer != null) visualRenderer.material.color = Color.white;
            TintPresentation(Color.white);
        }

        public void RefreshAfterCoreDamage(DamageSource source)
        {
            if (dead || !State.Health.IsDead) return;
            dead = true;
            deathElapsed = 0f;
            deathStartScale = transform.localScale;
            deathStartPosition = transform.position;
            MotionDriver?.TriggerDeath(presentation.deathEffectSeconds);
            if (visualRenderer != null) visualRenderer.material.color = new Color(0.75f, 0.04f, 0.03f);
            TintPresentation(new Color(0.75f, 0.04f, 0.03f));
            foreach (var zombieCollider in GetComponentsInChildren<Collider>()) zombieCollider.enabled = false;
            game.NotifyZombieKilled(this, source);
            game.SpawnPulse(transform.position + Vector3.up * VisualHeight * 0.35f, presentation.deathPulseSize,
                new Color(1f, 0.12f, 0.04f, 0.65f), presentation.deathEffectSeconds * 0.65f, "Zombie Death");
            Destroy(gameObject, presentation.deathEffectSeconds);
        }

        public void RefreshAfterCoreDamage(string legacySource)
        {
            RefreshAfterCoreDamage(LegacyDamageSource(legacySource));
        }

        private static DamageSource LegacyDamageSource(string source)
        {
            if (source == "player" || source == "player_grenade")
                return DamageSource.Player("player");
            if (source == "debug" || source == "test" || source == "grenade")
                return new DamageSource(DamageSourceKind.Debug, source);
            return new DamageSource(DamageSourceKind.Other, source);
        }

        private void UpdateDeathFeedback()
        {
            deathElapsed += Time.deltaTime;
            var duration = Mathf.Max(0.05f, presentation.deathEffectSeconds);
            var progress = Mathf.Clamp01(deathElapsed / duration);
            transform.localScale = Vector3.Lerp(deathStartScale, deathStartScale * 0.08f, progress);
            transform.position = deathStartPosition + Vector3.down * (deathStartScale.y * 0.7f * progress);
            transform.Rotate(Vector3.up, 240f * Time.deltaTime, Space.World);
            if (visualRenderer != null)
                visualRenderer.material.color = Color.Lerp(new Color(0.75f, 0.04f, 0.03f), Color.black, progress);
            TintPresentation(Color.Lerp(new Color(0.75f, 0.04f, 0.03f), Color.black, progress));
        }

        private void TintPresentation(Color color)
        {
            if (presentationBlock == null) presentationBlock = new MaterialPropertyBlock();
            presentationBlock.Clear();
            presentationBlock.SetColor("_BaseColor", color);
            presentationBlock.SetColor("_Color", color);
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                if (renderer != visualRenderer) renderer.SetPropertyBlock(presentationBlock);
        }

        private void ClearPresentationTint()
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                if (renderer != visualRenderer) renderer.SetPropertyBlock(null);
        }

        private struct RuntimeTarget
        {
            public TargetKind Kind;
            public Transform TargetTransform;
            public HaetaeRobotActor Robot;
            public BarrierRuntime Barrier;
            public bool IsNavigationPoint;
            private bool hasFixedPosition;
            private Vector3 fixedPosition;
            public bool HasTarget { get { return hasFixedPosition || TargetTransform != null; } }
            public Vector3 Position { get { return hasFixedPosition ? fixedPosition : TargetTransform.position; } }

            public RuntimeTarget(TargetKind kind, Transform targetTransform, HaetaeRobotActor robot, BarrierRuntime barrier)
            {
                Kind = kind;
                TargetTransform = targetTransform;
                Robot = robot;
                Barrier = barrier;
                IsNavigationPoint = false;
                hasFixedPosition = false;
                fixedPosition = Vector3.zero;
            }

            public static RuntimeTarget Navigation(Vector3 position)
            {
                return new RuntimeTarget
                {
                    Kind = TargetKind.Base,
                    TargetTransform = null,
                    Robot = null,
                    Barrier = null,
                    IsNavigationPoint = true,
                    hasFixedPosition = true,
                    fixedPosition = position
                };
            }

            public static RuntimeTarget Base(Vector3 position)
            {
                return new RuntimeTarget
                {
                    Kind = TargetKind.Base,
                    TargetTransform = null,
                    Robot = null,
                    Barrier = null,
                    IsNavigationPoint = false,
                    hasFixedPosition = true,
                    fixedPosition = position
                };
            }
        }
    }
}
