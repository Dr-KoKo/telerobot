using System;
using System.Collections.Generic;
using Telerobot.Game.Data;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class RuntimeIconLibrary : IDisposable
    {
        private readonly VisualThemeDefinitionAsset theme;
        private readonly Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>(StringComparer.Ordinal);

        public RuntimeIconLibrary(VisualThemeDefinitionAsset visualTheme)
        {
            theme = visualTheme;
        }

        public int Count { get { return icons.Count; } }

        public Texture2D Get(string key)
        {
            if (icons.TryGetValue(key, out var existing)) return existing;
            var texture = DrawIcon(key);
            texture.name = "Icon " + key;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            icons.Add(key, texture);
            return texture;
        }

        private Texture2D DrawIcon(string key)
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var clear = new Color(0f, 0f, 0f, 0f);
            var pixels = new Color[size * size];
            for (var index = 0; index < pixels.Length; index++) pixels[index] = clear;
            texture.SetPixels(pixels);

            var color = theme == null ? Color.cyan : theme.ColorFor("ui.line", Color.cyan);
            var danger = theme == null ? Color.red : theme.ColorFor("state.danger", Color.red);
            if (key.Contains("warning")) color = danger;

            if (key.Contains("health") || key.Contains("medical"))
            {
                Fill(texture, 13, 5, 6, 22, color);
                Fill(texture, 5, 13, 22, 6, color);
            }
            else if (key.Contains("battery"))
            {
                Outline(texture, 5, 8, 21, 16, color, 2);
                Fill(texture, 26, 12, 2, 8, color);
                Fill(texture, 9, 12, 12, 8, color);
            }
            else if (key.Contains("ammo") || key.Contains("ranged"))
            {
                Fill(texture, 7, 13, 19, 6, color);
                Fill(texture, 21, 10, 5, 12, color);
                Line(texture, 5, 16, 27, 16, color, 2);
            }
            else if (key.Contains("grenade"))
            {
                Circle(texture, 16, 18, 8, color, false);
                Fill(texture, 13, 6, 6, 5, color);
                Line(texture, 18, 6, 24, 3, color, 2);
            }
            else if (key.Contains("route") || key.Contains("patrol"))
            {
                Line(texture, 5, 24, 16, 7, color, 3);
                Line(texture, 16, 7, 27, 24, color, 3);
                Line(texture, 9, 20, 23, 20, color, 2);
            }
            else if (key.Contains("melee") || key.Contains("defend"))
            {
                Outline(texture, 8, 5, 16, 21, color, 2);
                Line(texture, 8, 7, 24, 25, color, 2);
                Line(texture, 24, 7, 8, 25, color, 2);
            }
            else if (key.Contains("return"))
            {
                Circle(texture, 16, 16, 9, color, false);
                Line(texture, 7, 16, 13, 10, color, 3);
                Line(texture, 7, 16, 13, 22, color, 3);
            }
            else
            {
                Circle(texture, 16, 16, 10, color, false);
                Circle(texture, 16, 16, 3, color, true);
            }

            texture.Apply(false, false);
            return texture;
        }

        private static void Fill(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (var iy = Mathf.Max(0, y); iy < Mathf.Min(texture.height, y + height); iy++)
                for (var ix = Mathf.Max(0, x); ix < Mathf.Min(texture.width, x + width); ix++)
                    texture.SetPixel(ix, iy, color);
        }

        private static void Outline(Texture2D texture, int x, int y, int width, int height, Color color, int thickness)
        {
            Fill(texture, x, y, width, thickness, color);
            Fill(texture, x, y + height - thickness, width, thickness, color);
            Fill(texture, x, y, thickness, height, color);
            Fill(texture, x + width - thickness, y, thickness, height, color);
        }

        private static void Line(Texture2D texture, int x0, int y0, int x1, int y1, Color color, int thickness)
        {
            var dx = Mathf.Abs(x1 - x0);
            var dy = Mathf.Abs(y1 - y0);
            var steps = Mathf.Max(dx, dy);
            for (var step = 0; step <= steps; step++)
            {
                var t = steps == 0 ? 0f : step / (float)steps;
                var x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
                var y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
                Fill(texture, x - thickness / 2, y - thickness / 2, thickness, thickness, color);
            }
        }

        private static void Circle(Texture2D texture, int cx, int cy, int radius, Color color, bool filled)
        {
            for (var y = -radius; y <= radius; y++)
            {
                for (var x = -radius; x <= radius; x++)
                {
                    var distance = x * x + y * y;
                    var inside = filled ? distance <= radius * radius :
                        distance <= radius * radius && distance >= (radius - 2) * (radius - 2);
                    if (inside) texture.SetPixel(cx + x, cy + y, color);
                }
            }
        }

        public void Dispose()
        {
            foreach (var pair in icons)
            {
                if (pair.Value == null) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(pair.Value);
                else UnityEngine.Object.DestroyImmediate(pair.Value);
            }
            icons.Clear();
        }
    }
}
