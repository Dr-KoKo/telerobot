using System.Collections.Generic;
using Telerobot.Game.Core;
using UnityEngine;

namespace Telerobot.Game.Runtime
{
    public sealed class UpgradeSelectionView : MonoBehaviour
    {
        private MvpGameController game;
        private List<UpgradeConfig> offer;
        public bool IsOpen { get; private set; }
        public IReadOnlyList<UpgradeConfig> Offer { get { return offer == null ? System.Array.Empty<UpgradeConfig>() : offer; } }

        public void Initialize(MvpGameController owner)
        {
            game = owner;
        }

        public void Show(List<UpgradeConfig> choices)
        {
            offer = choices;
            IsOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            IsOpen = false;
            offer = null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnGUI()
        {
            if (!IsOpen || offer == null || game == null) return;
            GUI.color = new Color(0f, 0f, 0f, 0.86f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.color = Color.white;
            var width = 260f;
            var gap = 24f;
            var total = width * 3f + gap * 2f;
            var start = Screen.width * 0.5f - total * 0.5f;
            GUI.Label(new Rect(Screen.width * 0.5f - 200f, Screen.height * 0.5f - 150f, 400f, 40f), game.Catalog.strings.Get("hud.upgrade"));
            for (var index = 0; index < offer.Count; index++)
            {
                var selected = offer[index];
                if (GUI.Button(new Rect(start + index * (width + gap), Screen.height * 0.5f - 70f, width, 150f),
                        game.Catalog.strings.Get(selected.DisplayNameKey)))
                {
                    game.SelectUpgrade(selected);
                    break;
                }
            }
        }
    }
}
