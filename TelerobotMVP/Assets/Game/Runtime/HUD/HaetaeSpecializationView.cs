using System.Collections.Generic;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class HaetaeSpecializationView : MonoBehaviour
    {
        private MvpGameController game;
        private int targetIndex;

        public bool IsOpen { get; private set; }
        public int ChoiceCount { get { return IsChoosingMastery ? 4 : 3; } }
        public bool IsChoosingMastery
        {
            get { return IsChoosingMasteryFor(TargetRobot()); }
        }

        public string TargetRobotId
        {
            get
            {
                var robot = TargetRobot();
                return robot == null || robot.State == null ? string.Empty : robot.State.Id;
            }
        }

        public void Initialize(MvpGameController owner)
        {
            game = owner;
        }

        public bool Open()
        {
            var choices = BuildChoiceRobots();
            if (choices.Count == 0) return false;
            targetIndex = 0;
            if (game.SelectedRobot != null)
            {
                var selectedIndex = choices.FindIndex(item => item == game.SelectedRobot);
                if (selectedIndex >= 0) targetIndex = selectedIndex;
            }
            IsOpen = true;
            game.RefreshCursorState();
            return true;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (game != null) game.RefreshCursorState();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public bool CycleTarget()
        {
            var choices = BuildChoiceRobots();
            if (choices.Count < 2) return false;
            targetIndex = (targetIndex + 1) % choices.Count;
            return true;
        }

        public SpecializationSelectionResult Select(HaetaeSpecialization specialization)
        {
            var robotId = TargetRobotId;
            if (string.IsNullOrEmpty(robotId) || game == null) return SpecializationSelectionResult.UnknownRobot;
            var result = game.SelectHaetaeSpecialization(robotId, specialization);
            if (result == SpecializationSelectionResult.Selected) RefreshAfterChoice();
            return result;
        }

        public MasterySelectionResult SelectMastery(HaetaeMasteryUpgrade upgrade)
        {
            var robotId = TargetRobotId;
            if (string.IsNullOrEmpty(robotId) || game == null) return MasterySelectionResult.UnknownRobot;
            var result = game.SelectHaetaeMastery(robotId, upgrade);
            if (result == MasterySelectionResult.Selected) RefreshAfterChoice();
            return result;
        }

        private void RefreshAfterChoice()
        {
            var choices = BuildChoiceRobots();
            if (choices.Count == 0) Close();
            else targetIndex = Mathf.Clamp(targetIndex, 0, choices.Count - 1);
        }

        private HaetaeRobotActor TargetRobot()
        {
            var choices = BuildChoiceRobots();
            return choices.Count == 0 ? null : choices[Mathf.Clamp(targetIndex, 0, choices.Count - 1)];
        }

        private List<HaetaeRobotActor> BuildChoiceRobots()
        {
            if (game == null) return new List<HaetaeRobotActor>();
            return game.Robots.FindAll(item =>
                item != null && item.State != null && item.State.Progression != null &&
                (item.State.Progression.SpecializationReady ||
                 item.State.Progression.Specialization != HaetaeSpecialization.Unselected &&
                 item.State.Progression.UnspentMasteryPoints > 0));
        }

        private static bool IsChoosingMasteryFor(HaetaeRobotActor robot)
        {
            return robot != null && robot.State != null && robot.State.Progression != null &&
                   robot.State.Progression.Specialization != HaetaeSpecialization.Unselected &&
                   robot.State.Progression.UnspentMasteryPoints > 0;
        }

        private void OnGUI()
        {
            if (!IsOpen || game == null) return;
            var robot = TargetRobot();
            if (robot == null || robot.State == null || robot.State.Progression == null)
            {
                Close();
                return;
            }

            var strings = game.Catalog.strings;
            var choosingMastery = IsChoosingMasteryFor(robot);
            var panelHeight = choosingMastery ? 570f : 460f;
            var panel = new Rect(Screen.width * 0.5f - 310f, Screen.height * 0.5f - panelHeight * 0.5f,
                620f, panelHeight);
            GUI.color = new Color(0.015f, 0.04f, 0.08f, 0.96f);
            GUI.Box(panel, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 24f, panel.y + 18f, 440f, 32f),
                strings.Get(choosingMastery
                    ? "haetae.mastery.panel_title"
                    : "haetae.specialization.panel_title") + " - " + robot.State.Id);
            if (GUI.Button(new Rect(panel.x + 472f, panel.y + 16f, 124f, 34f),
                    strings.Get("haetae.specialization.close")))
            {
                Close();
                return;
            }
            if (BuildChoiceRobots().Count > 1 &&
                GUI.Button(new Rect(panel.x + 442f, panel.y + 58f, 154f, 32f),
                    strings.Get("haetae.specialization.next_robot")))
            {
                CycleTarget();
                return;
            }

            if (choosingMastery)
            {
                GUI.Label(new Rect(panel.x + 24f, panel.y + 62f, 350f, 30f),
                    strings.Get("hud.haetae_mastery_points") + " " + robot.State.Progression.UnspentMasteryPoints);
                if (DrawMasteryChoice(panel.y + 105f, robot, HaetaeMasteryUpgrade.Power)) return;
                if (DrawMasteryChoice(panel.y + 210f, robot, HaetaeMasteryUpgrade.Armor)) return;
                if (DrawMasteryChoice(panel.y + 315f, robot, HaetaeMasteryUpgrade.Efficiency)) return;
                DrawMasteryChoice(panel.y + 420f, robot, HaetaeMasteryUpgrade.AttackSpeed);
                return;
            }

            if (DrawSpecializationChoice(panel.y + 105f, HaetaeSpecialization.Melee)) return;
            if (DrawSpecializationChoice(panel.y + 210f, HaetaeSpecialization.Ranged)) return;
            DrawSpecializationChoice(panel.y + 315f, HaetaeSpecialization.Balanced);
        }

        private bool DrawSpecializationChoice(float y, HaetaeSpecialization specialization)
        {
            var definition = System.Array.Find(game.Catalog.haetaeSpecializations,
                item => item != null && item.id == specialization);
            if (definition == null) return false;
            var strings = game.Catalog.strings;
            var name = strings.Get(definition.displayNameKey);
            var description = strings.Get(definition.descriptionKey);
            if (GUI.Button(new Rect(Screen.width * 0.5f - 280f, y, 180f, 78f), name))
            {
                Select(specialization);
                return true;
            }
            GUI.Label(new Rect(Screen.width * 0.5f - 80f, y + 4f, 350f, 70f), description);
            return false;
        }

        private bool DrawMasteryChoice(float y, HaetaeRobotActor robot, HaetaeMasteryUpgrade upgrade)
        {
            if (robot == null || robot.State == null || robot.State.Progression == null) return true;
            var progression = robot.State.Progression;
            var key = MasteryKey(upgrade);
            var rank = MasteryRank(progression, upgrade);
            var strings = game.Catalog.strings;
            var name = strings.Get("haetae.mastery." + key) + "  Lv." + rank;
            var description = strings.Get("haetae.mastery." + key + ".description");
            if (GUI.Button(new Rect(Screen.width * 0.5f - 280f, y, 180f, 78f), name))
            {
                SelectMastery(upgrade);
                return true;
            }
            GUI.Label(new Rect(Screen.width * 0.5f - 80f, y + 4f, 350f, 70f), description);
            return false;
        }

        private static string MasteryKey(HaetaeMasteryUpgrade upgrade)
        {
            switch (upgrade)
            {
                case HaetaeMasteryUpgrade.Power: return "power";
                case HaetaeMasteryUpgrade.Armor: return "armor";
                case HaetaeMasteryUpgrade.Efficiency: return "efficiency";
                case HaetaeMasteryUpgrade.AttackSpeed: return "attack_speed";
                default: return string.Empty;
            }
        }

        private static int MasteryRank(HaetaeProgressionState progression, HaetaeMasteryUpgrade upgrade)
        {
            switch (upgrade)
            {
                case HaetaeMasteryUpgrade.Power: return progression.PowerRank;
                case HaetaeMasteryUpgrade.Armor: return progression.ArmorRank;
                case HaetaeMasteryUpgrade.Efficiency: return progression.EfficiencyRank;
                case HaetaeMasteryUpgrade.AttackSpeed: return progression.AttackSpeedRank;
                default: return 0;
            }
        }
    }
}
