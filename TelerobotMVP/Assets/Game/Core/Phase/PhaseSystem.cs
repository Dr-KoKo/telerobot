using System;

namespace Telerobot.Game.Core
{
    public enum PhaseTransition
    {
        None,
        Defeat,
        AwaitingUpgrade,
        NextPhase,
        Victory
    }

    public sealed class PhaseSystem
    {
        private readonly BaseConfig config;

        public PhaseSystem(BaseConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this.config = config;
        }

        public PhaseTransition Evaluate(SessionState session, PhaseState phase, BaseState baseState, PlayerState player)
        {
            if (session.Result != GameResult.InProgress) return PhaseTransition.None;
            if (phase.Cleared) return PhaseTransition.None;
            if (baseState.Health.IsDead)
            {
                session.Result = GameResult.Defeat;
                session.DefeatReason = DefeatReason.BaseDestroyed;
                return PhaseTransition.Defeat;
            }
            if (player.Health.IsDead)
            {
                session.Result = GameResult.Defeat;
                session.DefeatReason = DefeatReason.PlayerDeath;
                return PhaseTransition.Defeat;
            }
            if (!phase.AllSpawned || phase.AliveCount > 0) return PhaseTransition.None;

            phase.Cleared = true;
            CombatRules.RecoverBase(baseState, config.PhaseRecoveryFraction);
            if (phase.Number >= 3)
            {
                session.Result = GameResult.Victory;
                return PhaseTransition.Victory;
            }
            return session.SelectedUpgrades.Count < phase.Number ? PhaseTransition.AwaitingUpgrade : PhaseTransition.NextPhase;
        }

        public PhaseState StartNext(SessionState session, PhaseConfig next)
        {
            if (next == null) throw new ArgumentNullException("next");
            session.CurrentPhase = next.Number;
            return new PhaseState(next.Number, next.OpenRoutes);
        }
    }
}
