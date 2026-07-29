using Telerobot.Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Telerobot.Game.Runtime
{
    public sealed class RobotCommandMenu : MonoBehaviour
    {
        private MvpGameController game;
        private int routeIndex;
        private GUIStyle buttonStyle;
        private GUIStyle headerStyle;
        public bool IsOpen { get; private set; }

        public void Initialize(MvpGameController owner)
        {
            game = owner;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (game == null || keyboard == null || game.SpecializationOpen) return;
            if (keyboard.digit1Key.wasPressedThisFrame && game.Robots.Count > 0) game.SelectedRobot = game.Robots[0];
            if (keyboard.digit2Key.wasPressedThisFrame && game.Robots.Count > 1) game.SelectedRobot = game.Robots[1];
            if (keyboard.digit3Key.wasPressedThisFrame) game.ToggleSelectAllRobots(!game.AreAllRobotsSelected);
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                IsOpen = !IsOpen;
                game.RefreshCursorState();
            }
            if (IsOpen && keyboard.qKey.wasPressedThisFrame && game.OpenRoutes.Count > 0)
                routeIndex = (routeIndex + 1) % game.OpenRoutes.Count;
        }

        private void OnGUI()
        {
            if (!IsOpen || game == null || game.SelectedRobot == null) return;
            var strings = game.Catalog.strings;
            var route = game.OpenRoutes.Count == 0 ? RouteId.NorthRoad : game.OpenRoutes[Mathf.Clamp(routeIndex, 0, game.OpenRoutes.Count - 1)];
            var panel = new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 140f, 360f, 280f);
            if (buttonStyle == null)
            {
                buttonStyle = GuardianGuiTheme.CreateButton(game.Catalog, 17);
                headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
                headerStyle.normal.textColor = GuardianGuiTheme.ResolveColor(game.Catalog, "ally.haetae", Color.yellow);
                GuardianGuiTheme.ApplyFont(headerStyle, game.Catalog, true);
            }
            GuardianGuiTheme.DrawPanel(panel, game.Catalog, 0.96f, 3f);
            var selectionLabel = game.AreAllRobotsSelected ? strings.Get("hud.all_robots") : game.SelectedRobot.State.Id;
            GUI.Label(new Rect(panel.x + 22f, panel.y + 18f, 320f, 28f), strings.Get("hud.command") + " — " + selectionLabel, headerStyle);
            GUI.Label(new Rect(panel.x + 22f, panel.y + 48f, 320f, 26f), strings.Get("hud.target") + ": " + strings.Get(game.Catalog.Route(route).displayNameKey) + "  [Q]");

            DrawCommand(new Rect(panel.x + 30f, panel.y + 82f, 300f, 44f), "cmd.defend", RobotCommand.DefendPosition, route);
            DrawCommand(new Rect(panel.x + 30f, panel.y + 132f, 300f, 44f), "cmd.patrol", RobotCommand.PatrolRoute, route);
            DrawCommand(new Rect(panel.x + 30f, panel.y + 182f, 300f, 44f), "cmd.return", RobotCommand.ReturnToBase, route);
        }

        private void DrawCommand(Rect rect, string key, RobotCommand command, RouteId route)
        {
            if (!GUI.Button(rect, game.Catalog.strings.Get(key), buttonStyle)) return;
            game.IssueCommandToSelected(command, route);
            Close();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            if (game != null) game.RefreshCursorState();
        }
    }
}
