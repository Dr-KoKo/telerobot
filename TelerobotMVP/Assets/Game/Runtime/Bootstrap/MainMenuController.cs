using Telerobot.Game.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Telerobot.Game.Runtime
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MvpContentCatalog catalog;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;

        public MvpContentCatalog Catalog { get { return catalog; } }
        public SettingsOverlay Settings { get; private set; }
        public bool SettingsOpen { get { return Settings != null && Settings.IsOpen; } }
        public bool QuitRequested { get; private set; }

        public void SetCatalog(MvpContentCatalog value)
        {
            catalog = value;
        }

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (catalog == null)
            {
                Debug.LogError("Main menu catalog is not assigned. Run Tools/Telerobot/Build MVP Project.");
                enabled = false;
                return;
            }
            MvpDataMapper.Validate(catalog);
            PlayerPreferences.Initialize(catalog.playerSettings);
            Settings = GetComponent<SettingsOverlay>();
            if (Settings == null) Settings = gameObject.AddComponent<SettingsOverlay>();
            Settings.Initialize(catalog);
        }

        private void Update()
        {
            if (SettingsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Settings.CancelAndClose();
        }

        private void Start()
        {
            if (RuntimePlayerSmoke.IsRequested) StartGame();
        }

        public void OpenSettings()
        {
            Settings?.Open();
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MVP");
        }

        public void QuitGame()
        {
            QuitRequested = true;
            Application.Quit();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = GuardianGuiTheme.ResolveColor(catalog, "ally.haetae", new Color(0.9f, 0.66f, 0.17f)) }
            };
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = GuardianGuiTheme.ResolveColor(catalog, "ui.text", new Color(0.78f, 0.86f, 0.92f)) }
            };
            GuardianGuiTheme.ApplyFont(titleStyle, catalog, true);
            GuardianGuiTheme.ApplyFont(subtitleStyle, catalog, false);
            buttonStyle = GuardianGuiTheme.CreateButton(catalog, 20);
        }

        private void OnGUI()
        {
            if (catalog == null || SettingsOpen) return;
            EnsureStyles();
            var strings = catalog.strings;
            GuardianGuiTheme.DrawBackdrop(catalog);

            var centerX = Screen.width * 0.5f;
            var top = Mathf.Max(55f, Screen.height * 0.16f);
            GuardianGuiTheme.DrawPanel(new Rect(centerX - 470f, top - 20f, 940f, 150f), catalog, 0.58f);
            GUI.Label(new Rect(centerX - 430f, top, 860f, 82f), strings.Get("menu.title"), titleStyle);
            GUI.Label(new Rect(centerX - 430f, top + 76f, 860f, 46f), strings.Get("menu.subtitle"), subtitleStyle);

            var buttonTop = top + 180f;
            if (GUI.Button(new Rect(centerX - 150f, buttonTop, 300f, 58f), strings.Get("menu.play"), buttonStyle)) StartGame();
            if (GUI.Button(new Rect(centerX - 150f, buttonTop + 76f, 300f, 58f), strings.Get("menu.settings"), buttonStyle)) OpenSettings();
            if (GUI.Button(new Rect(centerX - 150f, buttonTop + 152f, 300f, 58f), strings.Get("menu.quit"), buttonStyle)) QuitGame();

            GUI.Label(new Rect(centerX - 450f, Screen.height - 78f, 900f, 38f), strings.Get("menu.controls_hint"), subtitleStyle);
        }
    }
}
