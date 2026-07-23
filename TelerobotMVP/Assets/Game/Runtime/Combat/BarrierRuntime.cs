using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class BarrierRuntime : MonoBehaviour
    {
        private MvpGameController game;
        private BarrierState state;
        public bool IsAlive { get { return state != null && !state.Health.IsDead; } }

        public void Initialize(MvpGameController owner, RouteId route, float health)
        {
            game = owner;
            state = new BarrierState(route, health);
        }

        public void ReceiveDamage(float amount)
        {
            if (!IsAlive) return;
            CombatRules.ApplyDamage(state.Health, amount);
            game.Emit("barrier_damaged", "routeId", state.Route.ToString(), "hp", state.Health.Current.ToString("F1"));
            if (!state.Health.IsDead) return;
            game.Emit("barrier_destroyed", "routeId", state.Route.ToString());
            Destroy(gameObject);
        }
    }
}
