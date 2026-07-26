using System;
using System.Collections.Generic;

namespace Telerobot.Game.Core
{
    public enum ContributionResult
    {
        Recorded,
        AlreadyRecorded,
        NotEligible,
        UnknownRobot
    }

    public enum SpecializationSelectionResult
    {
        Selected,
        NotLevelTwo,
        AlreadySelected,
        InvalidChoice,
        UnknownRobot
    }

    public enum MasterySelectionResult
    {
        Selected,
        NoPoint,
        NotSpecialized,
        InvalidChoice,
        UnknownRobot
    }

    [Serializable]
    public sealed class ExperienceAwardResult
    {
        public string RobotId;
        public string ZombieId;
        public ZombieType ZombieType;
        public int RewardAmount;
        public int AppliedAmount;
        public int ExperienceBefore;
        public int ExperienceAfter;
        public int LevelBefore;
        public int LevelAfter;
        public bool LevelReached;
        public bool SpecializationUnlocked;
        public int MasteryPointsGained;
    }

    public sealed class HaetaeProgressionSystem
    {
        public ContributionResult RecordContribution(
            ZombieState zombie,
            DamageSource source,
            float appliedDamage,
            IReadOnlyList<RobotState> knownRobots)
        {
            if (zombie == null) throw new ArgumentNullException("zombie");
            if (knownRobots == null) throw new ArgumentNullException("knownRobots");
            if (appliedDamage <= 0f || source.Kind != DamageSourceKind.Haetae)
                return ContributionResult.NotEligible;
            if (string.IsNullOrWhiteSpace(source.SourceId) || FindRobot(knownRobots, source.SourceId) == null)
                return ContributionResult.UnknownRobot;
            return zombie.Contribution.HaetaeIds.Add(source.SourceId)
                ? ContributionResult.Recorded
                : ContributionResult.AlreadyRecorded;
        }

        public List<ExperienceAwardResult> AwardForDeath(
            ZombieState zombie,
            int reward,
            IList<RobotState> robots,
            HaetaeProgressionConfig config)
        {
            if (zombie == null) throw new ArgumentNullException("zombie");
            if (robots == null) throw new ArgumentNullException("robots");
            if (config == null) throw new ArgumentNullException("config");
            if (!zombie.Health.IsDead || reward <= 0 || zombie.Contribution.ExperienceAwarded)
                return new List<ExperienceAwardResult>();
            if (config.ExperiencePerLevel <= 0)
                throw new InvalidOperationException("Invalid Haetae progression configuration.");

            zombie.Contribution.ExperienceAwarded = true;
            var contributorIds = new List<string>(zombie.Contribution.HaetaeIds);
            contributorIds.Sort(StringComparer.Ordinal);
            var results = new List<ExperienceAwardResult>(contributorIds.Count);
            foreach (var contributorId in contributorIds)
            {
                var robot = FindRobot(robots, contributorId);
                if (robot == null) continue;
                var progression = robot.Progression;
                var experienceBefore = progression.Experience;
                var levelBefore = progression.Level;
                var totalExperience = Math.Min(int.MaxValue, (long)Math.Max(0, experienceBefore) + reward);
                var experienceAfter = (int)totalExperience;
                progression.Experience = experienceAfter;
                progression.Level = config.LevelForExperience(experienceAfter);
                var specializationUnlocked = levelBefore < 2 && progression.Level >= 2;
                var masteryPointsGained = Math.Max(0, progression.Level - 2) -
                    Math.Max(0, levelBefore - 2);
                progression.UnspentMasteryPoints += masteryPointsGained;

                results.Add(new ExperienceAwardResult
                {
                    RobotId = robot.Id,
                    ZombieId = zombie.Id,
                    ZombieType = zombie.Type,
                    RewardAmount = reward,
                    AppliedAmount = experienceAfter - experienceBefore,
                    ExperienceBefore = experienceBefore,
                    ExperienceAfter = experienceAfter,
                    LevelBefore = levelBefore,
                    LevelAfter = progression.Level,
                    LevelReached = levelBefore < progression.Level,
                    SpecializationUnlocked = specializationUnlocked,
                    MasteryPointsGained = masteryPointsGained
                });
            }
            return results;
        }

        public SpecializationSelectionResult SelectSpecialization(
            RobotState robot,
            HaetaeSpecialization requested)
        {
            if (robot == null) return SpecializationSelectionResult.UnknownRobot;
            if (!IsSelectable(requested)) return SpecializationSelectionResult.InvalidChoice;
            if (robot.Progression.Level < 2) return SpecializationSelectionResult.NotLevelTwo;
            if (robot.Progression.Specialization != HaetaeSpecialization.Unselected)
                return SpecializationSelectionResult.AlreadySelected;
            robot.Progression.Specialization = requested;
            return SpecializationSelectionResult.Selected;
        }

        public MasterySelectionResult SelectMasteryUpgrade(
            RobotState robot,
            HaetaeMasteryUpgrade requested)
        {
            if (robot == null) return MasterySelectionResult.UnknownRobot;
            if (!Enum.IsDefined(typeof(HaetaeMasteryUpgrade), requested))
                return MasterySelectionResult.InvalidChoice;
            if (robot.Progression.Specialization == HaetaeSpecialization.Unselected)
                return MasterySelectionResult.NotSpecialized;
            if (robot.Progression.UnspentMasteryPoints <= 0)
                return MasterySelectionResult.NoPoint;

            robot.Progression.UnspentMasteryPoints--;
            if (requested == HaetaeMasteryUpgrade.Power) robot.Progression.PowerRank++;
            else if (requested == HaetaeMasteryUpgrade.Armor) robot.Progression.ArmorRank++;
            else if (requested == HaetaeMasteryUpgrade.Efficiency) robot.Progression.EfficiencyRank++;
            else robot.Progression.AttackSpeedRank++;
            return MasterySelectionResult.Selected;
        }

        private static bool IsSelectable(HaetaeSpecialization requested)
        {
            return requested == HaetaeSpecialization.Melee ||
                   requested == HaetaeSpecialization.Ranged ||
                   requested == HaetaeSpecialization.Balanced;
        }

        private static RobotState FindRobot(IReadOnlyList<RobotState> robots, string id)
        {
            for (var index = 0; index < robots.Count; index++)
                if (robots[index] != null && string.Equals(robots[index].Id, id, StringComparison.Ordinal))
                    return robots[index];
            return null;
        }

        private static RobotState FindRobot(IList<RobotState> robots, string id)
        {
            for (var index = 0; index < robots.Count; index++)
                if (robots[index] != null && string.Equals(robots[index].Id, id, StringComparison.Ordinal))
                    return robots[index];
            return null;
        }
    }
}
