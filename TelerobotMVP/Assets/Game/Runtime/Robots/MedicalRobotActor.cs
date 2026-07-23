using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class MedicalRobotActor : MonoBehaviour
    {
        private MvpGameController game;
        private MedicalConfig config;
        private HealthState health;

        public bool IsAlive { get { return health != null && !health.IsDead; } }
        public bool IsZoneActive { get { return IsAlive; } }
        public float CurrentHealth { get { return health == null ? 0f : health.Current; } }

        public void Initialize(MvpGameController owner, MedicalConfig definition)
        {
            game = owner;
            config = definition;
            health = new HealthState(definition.MaxHealth);
        }

        private void Update()
        {
            if (!IsAlive || game == null || game.PlayerActor == null) return;
            var distance = Vector3.Distance(transform.position, game.PlayerActor.transform.position);
            var healed = MedicalRules.HealPlayer(config, game.Modifiers, game.PlayerState.Health, distance, Time.deltaTime, true);
            if (healed > 0f) game.Emit("medical_heal_applied", "amount", healed.ToString("F2"));
        }

        public void ReceiveDamage(float damage)
        {
            if (!IsAlive) return;
            CombatRules.ApplyDamage(health, damage);
            if (!health.IsDead) return;
            game.Emit("medical_robot_destroyed");
            game.Emit("medical_zone_disabled");
            Destroy(gameObject);
        }
    }
}
