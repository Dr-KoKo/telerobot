using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public static class GuardianGuiTheme
    {
        public static Color ResolveColor(MvpContentCatalog catalog, string key, Color fallback)
        {
            return catalog != null && catalog.visualTheme != null
                ? catalog.visualTheme.ColorFor(key, fallback)
                : fallback;
        }

        public static void ApplyFont(GUIStyle style, MvpContentCatalog catalog, bool heading)
        {
            if (style == null || catalog == null || catalog.visualTheme == null) return;
            var font = heading ? catalog.visualTheme.headingFont : catalog.visualTheme.bodyFont;
            if (font != null) style.font = font;
        }

        public static GUIStyle CreateButton(MvpContentCatalog catalog, int fontSize = 18)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(18, 18, 8, 8)
            };
            style.normal.textColor = ResolveColor(catalog, "ui.text", new Color(0.88f, 0.94f, 0.97f));
            style.hover.textColor = ResolveColor(catalog, "ally.energy", Color.cyan);
            style.active.textColor = ResolveColor(catalog, "ally.haetae", new Color(0.9f, 0.66f, 0.17f));
            ApplyFont(style, catalog, false);
            return style;
        }

        public static void DrawPanel(Rect rect, MvpContentCatalog catalog, float alpha = 0.92f, float lineWidth = 2f)
        {
            var previous = GUI.color;
            var panel = ResolveColor(catalog, "ui.panel", new Color(0.02f, 0.04f, 0.07f));
            panel.a = alpha;
            GUI.color = panel;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            var line = ResolveColor(catalog, "ui.line", Color.cyan);
            GUI.color = new Color(line.r, line.g, line.b, 0.88f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, lineWidth), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - lineWidth, rect.width, lineWidth), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, lineWidth, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - lineWidth, rect.y, lineWidth, rect.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public static void DrawBackdrop(MvpContentCatalog catalog)
        {
            var previous = GUI.color;
            var texture = catalog != null && catalog.visualTheme != null ? catalog.visualTheme.menuBackdrop : null;
            if (texture != null)
            {
                GUI.color = UnityEngine.Color.white;
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), texture, ScaleMode.ScaleAndCrop);
                GUI.color = new Color(0.005f, 0.012f, 0.02f, 0.34f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            }
            else
            {
                GUI.color = ResolveColor(catalog, "world.ground", new Color(0.015f, 0.035f, 0.065f));
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            }
            GUI.color = previous;
        }
    }
}
