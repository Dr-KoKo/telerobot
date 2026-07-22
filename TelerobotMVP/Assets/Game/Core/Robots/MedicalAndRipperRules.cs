using System;

namespace Telerobot.Game.Core
{
    public static class MedicalRules
    {
        public static float HealPlayer(MedicalConfig config, RuntimeModifiers modifiers, HealthState player,
            float distance, float deltaTime, bool medicalAlive)
        {
            if (!medicalAlive || config == null || player == null || distance > config.Radius) return 0f;
            return CombatRules.Heal(player, config.HealPerSecond * modifiers.MedicalHealMultiplier * Math.Max(0f, deltaTime));
        }

        public static bool ShouldApplyIncidentalDamage(float distanceToMedical, float attackRange, bool pursuingPriorityTarget)
        {
            return pursuingPriorityTarget && distanceToMedical >= 0f && distanceToMedical <= Math.Max(0f, attackRange);
        }
    }

    public static class RipperRules
    {
        public static TargetCandidate SelectTarget(ZombieConfig ripper, params TargetCandidate[] candidates)
        {
            if (ripper == null || ripper.Type != ZombieType.Ripper) throw new ArgumentException("Ripper config required.");
            return TargetingSystem.Select(ripper, candidates);
        }
    }
}
