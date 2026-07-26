using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeProgressionHudPlayModeTests : RuntimeSceneTestBase
    {
        [UnityTest]
        public IEnumerator RobotRowsUseMatchingThreeLineLayoutAndLabeledHealthBatteryExperienceBars()
        {
            var hud = Object.FindFirstObjectByType<CombatHud>();
            var first = Game.Robots[0].State;
            var second = Game.Robots[1].State;
            first.Health.Current = first.Health.Maximum * 0.5f;
            var firstBatteryRatio = (Game.Config.Warnings.BatteryYellowFraction + 1f) * 0.5f;
            var secondBatteryRatio = Game.Config.Warnings.BatteryRedFraction * 0.5f;
            first.Battery = first.MaximumBattery * firstBatteryRatio;
            second.Battery = second.MaximumBattery * secondBatteryRatio;
            first.Progression.Level = 3;
            first.Progression.Experience = Game.Config.HaetaeProgression.ExperiencePerLevel * 2 +
                Game.Config.HaetaeProgression.ExperiencePerLevel / 2;
            first.Progression.UnspentMasteryPoints = 1;
            first.Progression.PowerRank = 2;
            first.Progression.AttackSpeedRank = 1;
            Assert.That(Game.SelectHaetaeSpecialization(first.Id, HaetaeSpecialization.Melee),
                Is.EqualTo(SpecializationSelectionResult.Selected));
            second.Progression.Level = 2;
            second.Progression.Experience = Game.Config.HaetaeProgression.ExperiencePerLevel + 15;

            Game.SelectedRobot = Game.Robots[0];
            var firstRow = hud.GetRobotProgressionText(first.Id);
            var secondRow = hud.GetRobotProgressionText(second.Id);

            Assert.That(firstRow, Does.Contain(first.Id));
            Assert.That(firstRow, Does.Contain("3"));
            Assert.That(firstRow, Does.Contain("근거리형"));
            Assert.That(firstRow, Does.Contain("P2/A0/E0/S1"));
            Assert.That(firstRow, Does.Contain("강화 포인트 1"));
            Assert.That(secondRow, Does.Contain("일반형"));
            Assert.That(secondRow, Does.Contain("전문화 가능"));
            Assert.That(firstRow.Split('\n'), Has.Length.EqualTo(3));
            Assert.That(secondRow.Split('\n'), Has.Length.EqualTo(3));
            Assert.That(firstRow, Does.Not.Contain("배터리"));
            Assert.That(secondRow, Does.Not.Contain("배터리"));

            Game.SelectedRobot = Game.Robots[1];
            Assert.That(hud.GetRobotProgressionText(first.Id), Is.EqualTo(firstRow));
            Assert.That(hud.GetRobotProgressionText(second.Id), Is.EqualTo(secondRow));

            Assert.That(hud.GetRobotHealthProgress(first.Id), Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(hud.GetRobotHealthBarText(first.Id),
                Is.EqualTo("체력 " + first.Health.Current.ToString("0") + " / " +
                    first.Health.Maximum.ToString("0")));
            Assert.That(hud.GetRobotBatteryProgress(first.Id),
                Is.EqualTo(firstBatteryRatio).Within(0.001f));
            Assert.That(hud.GetRobotBatteryProgress(second.Id),
                Is.EqualTo(secondBatteryRatio).Within(0.001f));
            Assert.That(hud.GetRobotBatteryBarText(first.Id),
                Is.EqualTo("배터리 " + first.Battery.ToString("0") + " / " +
                    first.MaximumBattery.ToString("0")));
            Assert.That(hud.GetRobotBatteryWarningSeverity(first.Id), Is.EqualTo(WarningSeverity.None));
            Assert.That(hud.GetRobotBatteryWarningSeverity(second.Id), Is.EqualTo(WarningSeverity.Red));
            first.Battery = first.MaximumBattery *
                ((Game.Config.Warnings.BatteryRedFraction +
                  Game.Config.Warnings.BatteryYellowFraction) * 0.5f);
            Assert.That(hud.GetRobotBatteryWarningSeverity(first.Id), Is.EqualTo(WarningSeverity.Yellow));
            Assert.That(hud.GetRobotExperienceProgress(first.Id),
                Is.EqualTo(37f / 75f).Within(0.001f));
            Assert.That(hud.GetRobotExperienceProgress(second.Id),
                Is.EqualTo(15f / 75f).Within(0.001f));
            Assert.That(hud.GetRobotExperienceBarText(first.Id), Is.EqualTo("경험치 37 / 75"));
            Assert.That(hud.GetRobotExperienceBarText(second.Id), Is.EqualTo("경험치 15 / 75"));
            Assert.That(hud.IsProgressionReadyHighlighted(second.Id), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ExperienceBarResetsAtEachLevelBoundary()
        {
            var hud = Object.FindFirstObjectByType<CombatHud>();
            var robot = Game.Robots[0].State;
            var interval = Game.Config.HaetaeProgression.ExperiencePerLevel;

            robot.Progression.Level = 1;
            robot.Progression.Experience = interval - 1;
            Assert.That(hud.GetRobotExperienceProgress(robot.Id),
                Is.EqualTo((interval - 1f) / interval).Within(0.001f));

            robot.Progression.Level = 2;
            robot.Progression.Experience = interval;
            Assert.That(hud.GetRobotExperienceProgress(robot.Id), Is.Zero);
            Assert.That(hud.GetRobotExperienceBarText(robot.Id),
                Is.EqualTo("경험치 0 / " + interval));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReadyEventShowsDataDrivenNotificationForMatchingRobot()
        {
            var hud = Object.FindFirstObjectByType<CombatHud>();
            var robot = Game.Robots[1].State;
            robot.Progression.Level = 2;
            robot.Progression.Experience = Game.Config.HaetaeProgression.ExperiencePerLevel;

            Game.Emit("haetae_specialization_ready", "robotId", robot.Id, "level", "2");
            yield return null;

            Assert.That(hud.ProgressionNotificationActive, Is.True);
            Assert.That(hud.ProgressionNotificationRobotId, Is.EqualTo(robot.Id));
            Assert.That(Game.Config.HaetaeProgression.ReadyAlertSeconds, Is.EqualTo(4f));
            Assert.That(Game.Catalog.strings.Get("haetae.specialization.ranged"), Is.EqualTo("원거리형"));
            Assert.That(Game.Catalog.strings.Get("haetae.specialization.balanced"), Is.EqualTo("균형형"));
        }
    }
}
