using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class HaetaeRobotActor : MonoBehaviour
    {
        private MvpGameController game;
        private RobotConfig config;
        private BatterySystem batterySystem;
        private float nextAttack;
        private float telemetryTimer;
        private RobotMode previousMode;

        public RobotState State { get; private set; }

        public void Initialize(MvpGameController owner, string id, RobotConfig robotConfig, BatteryConfig batteryConfig, RouteId route)
        {
            game = owner;
            config = robotConfig;
            batterySystem = new BatterySystem(batteryConfig);
            State = new RobotState(id, robotConfig.MaxHealth, batteryConfig.Maximum) { AssignedRoute = route };
            previousMode = State.Mode;
        }

        private void Update()
        {
            if (game == null || game.IsFinished || State.Health.IsDead) return;
            if (State.Mode == RobotMode.Disabled || State.Mode == RobotMode.Recovery)
            {
                batterySystem.TickDisabledRecovery(State, Time.deltaTime);
                EmitStateChanges();
                return;
            }
            if (State.Command == RobotCommand.ReturnToBase)
            {
                State.Mode = RobotMode.ReturnToCharge;
                MoveTo(game.ToVector(game.Config.World.BaseRally), config.MoveSpeed * batterySystem.MoveMultiplier(State));
                batterySystem.Drain(State, RobotActivity.Idle, Time.deltaTime, game.Modifiers.CombatDrainMultiplier);
                EmitStateChanges();
                return;
            }
            if (State.Command == RobotCommand.Charge || State.Mode == RobotMode.ReturnToCharge || State.Mode == RobotMode.Charging)
            {
                UpdateCharging();
                EmitStateChanges();
                return;
            }

            var target = game.FindRobotTarget(this, config.DetectionRadius);
            var activity = State.Command == RobotCommand.PatrolRoute ? RobotActivity.Patrol : RobotActivity.Idle;
            if (target != null && State.CanAttack)
            {
                activity = RobotActivity.Combat;
                State.Mode = RobotMode.Engage;
                var distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance > config.AttackRange)
                    MoveTo(target.transform.position, config.MoveSpeed * batterySystem.MoveMultiplier(State));
                else if (Time.time >= nextAttack)
                {
                    var interval = config.AttackInterval / Mathf.Max(0.01f, batterySystem.AttackMultiplier(State));
                    nextAttack = Time.time + interval;
                    var damage = config.AttackDamage * (!State.FirstDashUsed ? game.Modifiers.FirstDashDamageMultiplier : 1f);
                    State.FirstDashUsed = true;
                    target.ReceiveDamage(damage, State.Id);
                }
            }
            else
            {
                State.FirstDashUsed = false;
                UpdateCommandMovement();
            }

            batterySystem.Drain(State, activity, Time.deltaTime, game.Modifiers.CombatDrainMultiplier);
            EmitStateChanges();
        }

        private void UpdateCommandMovement()
        {
            if (State.Command == RobotCommand.PatrolRoute)
            {
                State.Mode = RobotMode.Patrol;
                var route = game.Catalog.Route(State.AssignedRoute);
                var patrolPoint = route.waypoints[Mathf.Max(0, route.waypoints.Length / 2)];
                MoveTo(patrolPoint, config.MoveSpeed * batterySystem.MoveMultiplier(State));
                return;
            }
            State.Mode = State.BatteryBand == BatteryBand.Normal ? RobotMode.Standby : RobotMode.LowBattery;
        }

        private void UpdateCharging()
        {
            var distance = Vector3.Distance(transform.position, game.ChargingPosition);
            if (distance > game.Config.World.ChargingArrivalRadius)
            {
                State.Mode = RobotMode.ReturnToCharge;
                MoveTo(game.ChargingPosition, config.MoveSpeed * batterySystem.MoveMultiplier(State));
                return;
            }
            batterySystem.Charge(State, Time.deltaTime, game.Modifiers.ChargeRateMultiplier);
        }

        private void MoveTo(Vector3 target, float speed)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            var direction = target - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direction);
        }

        private void EmitStateChanges()
        {
            telemetryTimer += Time.deltaTime;
            if (telemetryTimer >= 1f)
            {
                telemetryTimer = 0f;
                game.Emit("robot_battery_changed", "robotId", State.Id, "value", State.Battery.ToString("F1"), "state", State.BatteryBand.ToString());
            }
            if (State.Mode == previousMode) return;
            if (State.Mode == RobotMode.Disabled) game.Emit("robot_disabled", "robotId", State.Id);
            previousMode = State.Mode;
        }

        public bool Issue(RobotCommand command, RouteId route)
        {
            var accepted = game.CommandSystem.IssueCommand(State, command, route);
            if (accepted && command == RobotCommand.Charge) game.Emit("robot_charge_commanded", "robotId", State.Id);
            return accepted;
        }

        public void ReceiveZombieHit(float damage, bool ripper)
        {
            CombatRules.ApplyDamage(State.Health, damage);
            if (ripper) batterySystem.ApplyRipperHit(State);
            if (State.Health.IsDead) game.Emit("robot_destroyed", "robotId", State.Id);
        }
    }
}
