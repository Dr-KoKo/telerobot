using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class HaetaeRobotActor : MonoBehaviour
    {
        private MvpGameController game;
        private RobotConfig config;
        private BatterySystem batterySystem;
        private RobotAttackSystem attackSystem;
        private RobotCombatPolicy combatPolicy;
        private readonly RobotDurabilitySystem durabilitySystem = new RobotDurabilitySystem();
        private float telemetryTimer;
        private RobotMode previousMode;
        private BatteryBand previousBatteryBand;
        private WarningSeverity previousBatteryWarning;
        private Renderer visualRenderer;
        private Color activeColor;
        private Quaternion activeRotation;
        private Vector3 activeScale;
        private ZombieActor followUpTarget;
        private HaetaeSpecialization presentedSpecialization = (HaetaeSpecialization)(-1);

        public RobotState State { get; private set; }
        public float SeparationRadius { get { return config == null ? 1f : config.SeparationRadius; } }
        public RobotMovementIntent LastMovementIntent { get; private set; }
        public RobotAttackKind LastAttackKind { get; private set; }
        public GameObject LastAttackCue { get; private set; }

        public void Initialize(MvpGameController owner, string id, RobotConfig robotConfig, BatteryConfig batteryConfig, RouteId route)
        {
            game = owner;
            config = robotConfig;
            batterySystem = new BatterySystem(batteryConfig);
            attackSystem = new RobotAttackSystem(robotConfig);
            combatPolicy = new RobotCombatPolicy(owner.Config);
            State = new RobotState(id, robotConfig.MaxHealth, batteryConfig.Maximum) { AssignedRoute = route };
            previousMode = State.Mode;
            previousBatteryBand = State.BatteryBand;
            previousBatteryWarning = WarningSeverity.None;
            visualRenderer = GetComponent<Renderer>();
            activeColor = visualRenderer == null ? Color.white : visualRenderer.material.color;
            activeRotation = transform.rotation;
            activeScale = transform.localScale;
            RefreshSpecializationPresentation();
        }

        private void Update()
        {
            if (game == null || game.IsFinished || State.IsDestroyed) return;
            RefreshSpecializationPresentation();
            if (State.Mode == RobotMode.Disabled || State.Mode == RobotMode.Recovery)
            {
                batterySystem.TickDisabledRecovery(State, Time.deltaTime);
                EmitStateChanges();
                return;
            }
            if (State.Command == RobotCommand.ReturnToBase)
            {
                var rally = game.GetRobotFormationPosition(this, game.ToVector(game.Config.World.BaseRally));
                if (Vector3.Distance(transform.position, rally) > game.Config.World.ChargingArrivalRadius)
                {
                    State.Mode = RobotMode.ReturnToCharge;
                    MoveTo(rally, config.MoveSpeed * batterySystem.MoveMultiplier(State));
                    batterySystem.Drain(State, RobotActivity.Idle, Time.deltaTime, game.Modifiers.CombatDrainMultiplier);
                    EmitStateChanges();
                    return;
                }
                State.Command = RobotCommand.DefendPosition;
                State.Mode = RobotMode.Standby;
            }
            if (State.Command == RobotCommand.DefendPosition &&
                Vector3.Distance(transform.position, game.BaseTransform.position) > config.DefendLeashRadius)
            {
                followUpTarget = null;
                attackSystem.EndEngagement(State);
                State.Mode = RobotMode.Standby;
                MoveTo(game.GetRobotFormationPosition(this, game.ToVector(game.Config.World.BaseRally)),
                    config.MoveSpeed * batterySystem.MoveMultiplier(State));
                batterySystem.Drain(State, RobotActivity.Idle, Time.deltaTime, game.Modifiers.CombatDrainMultiplier);
                EmitStateChanges();
                return;
            }

            var wasCharging = State.Mode == RobotMode.Charging;
            var target = game.IsRobotFollowUpTargetValid(this, followUpTarget, config.DetectionRadius)
                ? followUpTarget
                : wasCharging ? game.FindRobotFollowUpTarget(this, config.DetectionRadius)
                : game.FindRobotTarget(this, config.DetectionRadius);
            if (target != followUpTarget) followUpTarget = null;
            if (target != null && wasCharging)
            {
                followUpTarget = target;
                State.BatteryBand = batterySystem.BandFor(State);
                State.Mode = State.BatteryBand == BatteryBand.Normal ? RobotMode.Standby : RobotMode.LowBattery;
            }
            if (target == null && IsInsideBaseChargingZone() && State.Battery < State.MaximumBattery)
            {
                followUpTarget = null;
                attackSystem.EndEngagement(State);
                UpdateCharging();
                EmitStateChanges();
                return;
            }
            if (State.Mode == RobotMode.ReturnToCharge || State.Mode == RobotMode.Charging)
            {
                UpdateCharging();
                EmitStateChanges();
                return;
            }
            var activity = State.Command == RobotCommand.PatrolRoute ? RobotActivity.Patrol : RobotActivity.Idle;
            if (target != null && State.CanAttack)
            {
                activity = RobotActivity.Combat;
                State.Mode = RobotMode.Engage;
                var distance = Vector3.Distance(transform.position, target.transform.position);
                attackSystem.Tick(State, Time.deltaTime * batterySystem.AttackMultiplier(State));
                attackSystem.BeginEngagement(State, target.State.Id);
                var decision = combatPolicy.Decide(State, distance);
                var profile = combatPolicy.ActiveProfile(State);
                LastMovementIntent = decision.Movement;
                if (decision.Movement == RobotMovementIntent.Approach)
                {
                    var stopDistance = State.Progression.Specialization == HaetaeSpecialization.Ranged
                        ? profile.PreferredMaxRange
                        : config.EngageRange * 0.8f;
                    MoveTo(target.transform.position, config.MoveSpeed * batterySystem.MoveMultiplier(State), stopDistance);
                }
                else if (decision.Movement == RobotMovementIntent.Retreat)
                {
                    MoveAwayFrom(target.transform.position, config.MoveSpeed * batterySystem.MoveMultiplier(State));
                }
                else if (decision.Movement == RobotMovementIntent.Hold)
                {
                    MoveTo(transform.position, config.MoveSpeed * batterySystem.MoveMultiplier(State));
                }
                var attack = attackSystem.Advance(State, target.State.Id, decision, profile,
                    game.Modifiers.FirstDashDamageMultiplier,
                    game.Config.HaetaeProgression.AttackCooldownMultiplier(State.Progression));
                attack.Damage *= game.Config.HaetaeProgression.DamageMultiplier(State.Progression);
                if (attack.Damage > 0f)
                {
                    LastAttackKind = attack.Kind;
                    var affected = game.GetRobotAttackTargets(this, target, attack);
                    foreach (var affectedTarget in affected)
                        affectedTarget.ReceiveDamage(attack.Damage, DamageSource.Haetae(State.Id));
                    LastAttackCue = attack.Kind == RobotAttackKind.Ranged
                        ? game.SpawnTracer(transform.position + Vector3.up * 0.4f,
                            target.transform.position + Vector3.up * target.VisualHeight * 0.3f,
                            SpecializationTracerColor(), State.Id + " Ranged")
                        : game.SpawnPulse(target.transform.position + Vector3.up * target.VisualHeight * 0.3f,
                            attack.AreaRadius > 0f ? attack.AreaRadius * 2f : 0.42f,
                            SpecializationPulseColor(), 0.12f, State.Id + " Attack");
                    if (target.State.Health.IsDead)
                    {
                        followUpTarget = game.FindRobotFollowUpTarget(this, config.DetectionRadius);
                        if (followUpTarget != null)
                            attackSystem.BeginEngagement(State, followUpTarget.State.Id);
                    }
                }
            }
            else
            {
                followUpTarget = null;
                attackSystem.EndEngagement(State);
                UpdateCommandMovement();
            }

            if (activity != RobotActivity.Idle || !IsInsideBaseChargingZone())
            {
                var roleMultiplier = activity == RobotActivity.Combat
                    ? combatPolicy.ActiveProfile(State).CombatBatteryMultiplier *
                      game.Config.HaetaeProgression.CombatBatteryMultiplier(State.Progression)
                    : 1f;
                batterySystem.Drain(State, activity, Time.deltaTime,
                    game.Modifiers.CombatDrainMultiplier * roleMultiplier);
            }
            EmitStateChanges();
        }

        private void UpdateCommandMovement()
        {
            if (State.Command == RobotCommand.PatrolRoute)
            {
                State.Mode = RobotMode.Patrol;
                var route = game.Catalog.Route(State.AssignedRoute);
                var patrolPoint = route.waypoints[Mathf.Max(0, route.waypoints.Length / 2)];
                patrolPoint = game.GetRobotFormationPosition(this, patrolPoint);
                MoveTo(patrolPoint, config.MoveSpeed * batterySystem.MoveMultiplier(State));
                return;
            }
            State.Mode = State.BatteryBand == BatteryBand.Normal ? RobotMode.Standby : RobotMode.LowBattery;
            if (State.Command == RobotCommand.DefendPosition)
                MoveTo(game.GetRobotFormationPosition(this, game.ToVector(game.Config.World.BaseRally)),
                    config.MoveSpeed * batterySystem.MoveMultiplier(State));
        }

        private void UpdateCharging()
        {
            if (!IsInsideBaseChargingZone())
            {
                State.Mode = RobotMode.ReturnToCharge;
                MoveTo(game.GetRobotFormationPosition(this, game.ToVector(game.Config.World.BaseRally)),
                    config.MoveSpeed * batterySystem.MoveMultiplier(State));
                return;
            }
            batterySystem.Charge(State, Time.deltaTime, game.Modifiers.ChargeRateMultiplier);
        }

        private bool IsInsideBaseChargingZone()
        {
            var delta = transform.position - game.BaseTransform.position;
            delta.y = 0f;
            return delta.sqrMagnitude <= game.Config.World.BaseChargingRadius * game.Config.World.BaseChargingRadius;
        }

        private void MoveTo(Vector3 target, float speed, float stopDistance = 0.1f)
        {
            var direction = target - transform.position;
            direction.y = 0f;
            var distance = direction.magnitude;
            var desired = distance > stopDistance ? direction.normalized : Vector3.zero;
            var avoidance = game.GetRobotAvoidance(this) * config.SeparationStrength;
            var movement = desired + avoidance;
            if (desired.sqrMagnitude > 0f)
            {
                var progress = Vector3.Dot(movement, desired);
                if (progress < 0.3f) movement += desired * (0.3f - progress);
            }
            if (movement.sqrMagnitude <= 0.0001f) return;
            movement.Normalize();
            var step = speed * Time.deltaTime;
            if (desired.sqrMagnitude > 0f && avoidance.sqrMagnitude < 0.01f)
                step = Mathf.Min(step, Mathf.Max(0f, distance - stopDistance));
            else if (desired.sqrMagnitude <= 0f)
                step *= Mathf.Clamp01(avoidance.magnitude);
            transform.position = game.ResolveRobotSeparation(this, transform.position + movement * step);
            transform.rotation = Quaternion.LookRotation(movement);
        }

        private void MoveAwayFrom(Vector3 threat, float speed)
        {
            var direction = transform.position - threat;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f) direction = transform.right;
            MoveTo(transform.position + direction.normalized * 2f, speed);
        }

        private void EmitStateChanges()
        {
            telemetryTimer += Time.deltaTime;
            var telemetry = game.Config.Telemetry;
            var fraction = State.MaximumBattery <= 0f ? 0f : State.Battery / State.MaximumBattery;
            var warning = fraction < game.Config.Warnings.BatteryRedFraction ? WarningSeverity.Red
                : fraction < game.Config.Warnings.BatteryYellowFraction ? WarningSeverity.Yellow : WarningSeverity.None;
            var thresholdDue = (telemetry.BatteryEmitPolicy & BatteryEmitPolicy.OnThresholdCrossing) != 0 &&
                (State.BatteryBand != previousBatteryBand || warning != previousBatteryWarning);
            var periodicDue = (telemetry.BatteryEmitPolicy & BatteryEmitPolicy.EveryNSeconds) != 0 &&
                telemetryTimer >= telemetry.BatteryEmitIntervalSeconds;
            if (thresholdDue || periodicDue)
            {
                if (periodicDue) telemetryTimer -= telemetry.BatteryEmitIntervalSeconds;
                game.Emit("robot_battery_changed", "robotId", State.Id, "value", State.Battery.ToString("F1"), "state", State.BatteryBand.ToString());
            }
            previousBatteryBand = State.BatteryBand;
            previousBatteryWarning = warning;
            if (State.Mode == previousMode) return;
            if (State.Mode == RobotMode.Charging) game.Emit("robot_auto_charge_started", "robotId", State.Id);
            if (State.Mode == RobotMode.Disabled) game.Emit("robot_disabled", "robotId", State.Id);
            previousMode = State.Mode;
        }

        public bool Issue(RobotCommand command, RouteId route)
        {
            var accepted = game.CommandSystem.IssueCommand(State, command, route);
            if (accepted)
            {
                followUpTarget = null;
                attackSystem.EndEngagement(State);
            }
            return accepted;
        }

        public void ReceiveZombieHit(float damage, bool ripper)
        {
            if (State.IsDestroyed) return;
            var before = State.Health.Current;
            var multiplier = combatPolicy == null
                ? 1f
                : combatPolicy.ActiveProfile(State).IncomingDamageMultiplier *
                  game.Config.HaetaeProgression.IncomingDamageMultiplier(State.Progression);
            var destroyed = durabilitySystem.ApplyDamage(State, damage * multiplier);
            var applied = before - State.Health.Current;
            game.Emit("robot_damaged", "robotId", State.Id, "amount", applied.ToString("F1"),
                "hp", State.Health.Current.ToString("F1"));
            if (ripper && !destroyed) batterySystem.ApplyRipperHit(State);
            if (!destroyed) return;
            ShowDestroyedRubble();
            game.Emit("robot_destroyed", "robotId", State.Id);
        }

        public void RestoreForPhaseStart()
        {
            if (!State.IsDestroyed) return;
            followUpTarget = null;
            durabilitySystem.RestoreAtPhaseStart(State, config.MaxHealth, State.MaximumBattery);
            transform.rotation = activeRotation;
            presentedSpecialization = (HaetaeSpecialization)(-1);
            RefreshSpecializationPresentation();
            foreach (var robotCollider in GetComponentsInChildren<Collider>()) robotCollider.enabled = true;
            previousMode = State.Mode;
        }

        private void ShowDestroyedRubble()
        {
            followUpTarget = null;
            transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
            transform.localScale = new Vector3(activeScale.x, activeScale.y * 0.55f, activeScale.z);
            if (visualRenderer != null) visualRenderer.material.color = new Color(0.16f, 0.17f, 0.18f);
        }

        private void RefreshSpecializationPresentation()
        {
            if (State == null || State.Progression.Specialization == presentedSpecialization) return;
            presentedSpecialization = State.Progression.Specialization;
            if (presentedSpecialization == HaetaeSpecialization.Unselected)
            {
                transform.localScale = activeScale;
                if (visualRenderer != null) visualRenderer.material.color = activeColor;
                return;
            }
            var definition = System.Array.Find(game.Catalog.haetaeSpecializations,
                item => item != null && item.id == presentedSpecialization);
            if (definition == null) return;
            transform.localScale = Vector3.Scale(activeScale, definition.scaleMultiplier);
            if (visualRenderer != null) visualRenderer.material.color = definition.bodyColor;
        }

        private Color SpecializationPulseColor()
        {
            var definition = System.Array.Find(game.Catalog.haetaeSpecializations,
                item => item != null && item.id == State.Progression.Specialization);
            return definition == null ? activeColor : definition.attackPulseColor;
        }

        private Color SpecializationTracerColor()
        {
            var definition = System.Array.Find(game.Catalog.haetaeSpecializations,
                item => item != null && item.id == State.Progression.Specialization);
            return definition == null ? activeColor : definition.tracerColor;
        }
    }
}
