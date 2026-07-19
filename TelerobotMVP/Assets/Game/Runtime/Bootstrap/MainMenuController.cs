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
                normal = { textColor = new Color(0.25f, 0.9f, 1f) }
            };
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.78f, 0.86f, 0.92f) }
            };
        }

        private void OnGUI()
        {
            if (catalog == null || SettingsOpen) return;
            EnsureStyles();
            var strings = catalog.strings;
            GUI.color = new Color(0.015f, 0.035f, 0.065f, 1f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var centerX = Screen.width * 0.5f;
            var top = Mathf.Max(55f, Screen.height * 0.16f);
            GUI.Label(new Rect(centerX - 430f, top, 860f, 82f), strings.Get("menu.title"), titleStyle);
            GUI.Label(new Rect(centerX - 430f, top + 76f, 860f, 46f), strings.Get("menu.subtitle"), subtitleStyle);

            var buttonTop = top + 180f;
            if (GUI.Button(new Rect(centerX - 150f, buttonTop, 300f, 58f), strings.Get("menu.play"))) StartGame();
            if (GUI.Button(new Rect(centerX - 150f, buttonTop + 76f, 300f, 58f), strings.Get("menu.settings"))) OpenSettings();
            if (GUI.Button(new Rect(centerX - 150f, buttonTop + 152f, 300f, 58f), strings.Get("menu.quit"))) QuitGame();

            GUI.Label(new Rect(centerX - 450f, Screen.height - 78f, 900f, 38f), strings.Get("menu.controls_hint"), subtitleStyle);
        }
    }
}
