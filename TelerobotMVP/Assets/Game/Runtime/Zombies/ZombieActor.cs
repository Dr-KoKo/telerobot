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
        private float hitFlashUntil;
        private float deathElapsed;
        private bool dead;
        private Renderer visualRenderer;
        private Color originalColor;
        private Vector3 deathStartScale;
        private Vector3 deathStartPosition;

        public ZombieState State { get; private set; }
        public ZombieType Type { get { return State.Type; } }
        public float VisualHeight { get { return transform.localScale.y * 2f; } }
        public bool HitFlashActive { get { return !dead && Time.time < hitFlashUntil; } }
        public bool DeathFeedbackActive { get { return dead; } }
        public Color CurrentVisualColor { get { return visualRenderer == null ? Color.clear : visualRenderer.material.color; } }

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
                hitFlashUntil = 0f;
            }
            if (game == null || game.IsFinished) return;
            var target = ResolveTarget();
            if (target.TargetTransform == null) return;
            var distance = Vector3.Distance(transform.position, target.TargetTransform.position);
            if (distance <= config.AttackRange)
            {
                Attack(target);
                return;
            }
            transform.position = Vector3.MoveTowards(transform.position, target.TargetTransform.position, config.MoveSpeed * Time.deltaTime);
            var direction = target.TargetTransform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direction);
        }

        private RuntimeTarget ResolveTarget()
        {
            if (game.TryGetBarrier(State.Route, out var barrier))
                return new RuntimeTarget(TargetKind.Base, barrier.transform, null, barrier);

            var candidates = new List<TargetCandidate>
            {
                new TargetCandidate("base", TargetKind.Base, Vector3.Distance(transform.position, game.BaseTransform.position), true),
                new TargetCandidate("player", TargetKind.Player, Vector3.Distance(transform.position, game.PlayerActor.transform.position), !game.PlayerState.Health.IsDead)
            };
            foreach (var robot in game.Robots)
                candidates.Add(new TargetCandidate(robot.State.Id, TargetKind.Robot, Vector3.Distance(transform.position, robot.transform.position), !robot.State.Health.IsDead));
            if (game.MedicalActor != null)
                candidates.Add(new TargetCandidate("medical", TargetKind.Robot, Vector3.Distance(transform.position, game.MedicalActor.transform.position), game.MedicalActor.IsAlive));

            var selected = TargetingSystem.Select(config, candidates);
            if (selected == null) return new RuntimeTarget(TargetKind.Base, game.BaseTransform, null, null);
            if (selected.Kind == TargetKind.Player) return new RuntimeTarget(selected.Kind, game.PlayerActor.transform, null, null);
            if (selected.Kind == TargetKind.Robot)
            {
                if (selected.Id == "medical")
                    return new RuntimeTarget(selected.Kind, game.MedicalActor.transform, null, null, game.MedicalActor);
                var robot = game.Robots.Find(item => item.State.Id == selected.Id);
                return new RuntimeTarget(selected.Kind, robot.transform, robot, null);
            }

            if (waypointIndex < waypoints.Length)
            {
                var point = waypoints[waypointIndex];
                if (Vector3.Distance(transform.position, point) < 0.6f) waypointIndex++;
                if (waypointIndex < waypoints.Length) return new RuntimeTarget(TargetKind.Base, game.RouteTargets[State.Route][waypointIndex], null, null);
            }
            return new RuntimeTarget(TargetKind.Base, game.BaseTransform, null, null);
        }

        private void Attack(RuntimeTarget target)
        {
            if (Time.time < nextAttack) return;
            nextAttack = Time.time + config.AttackInterval;
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
            else if (target.Medical != null)
            {
                target.Medical.ReceiveDamage(config.RobotDamage);
            }
        }

        public void ReceiveDamage(float amount, string source)
        {
            if (dead) return;
            CombatRules.ApplyDamage(State.Health, amount);
            if (!State.Health.IsDead) ShowHitFlash();
            RefreshAfterCoreDamage(source);
        }

        private void ShowHitFlash()
        {
            hitFlashUntil = Time.time + presentation.hitFlashSeconds;
            if (visualRenderer != null) visualRenderer.material.color = Color.white;
        }

        public void RefreshAfterCoreDamage(string source)
        {
            if (dead || !State.Health.IsDead) return;
            dead = true;
            deathElapsed = 0f;
            deathStartScale = transform.localScale;
            deathStartPosition = transform.position;
            if (visualRenderer != null) visualRenderer.material.color = new Color(0.75f, 0.04f, 0.03f);
            foreach (var zombieCollider in GetComponentsInChildren<Collider>()) zombieCollider.enabled = false;
            game.NotifyZombieKilled(this, source);
            game.SpawnPulse(transform.position + Vector3.up * VisualHeight * 0.35f, presentation.deathPulseSize,
                new Color(1f, 0.12f, 0.04f, 0.65f), presentation.deathEffectSeconds * 0.65f, "Zombie Death");
            Destroy(gameObject, presentation.deathEffectSeconds);
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
        }

        private struct RuntimeTarget
        {
            public TargetKind Kind;
            public Transform TargetTransform;
            public HaetaeRobotActor Robot;
            public BarrierRuntime Barrier;
            public MedicalRobotActor Medical;

            public RuntimeTarget(TargetKind kind, Transform targetTransform, HaetaeRobotActor robot, BarrierRuntime barrier, MedicalRobotActor medical = null)
            {
                Kind = kind;
                TargetTransform = targetTransform;
                Robot = robot;
                Barrier = barrier;
                Medical = medical;
            }
        }
    }
}
