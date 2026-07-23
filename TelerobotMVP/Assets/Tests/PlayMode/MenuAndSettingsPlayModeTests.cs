using System.Collections;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using Telerobot.Game.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Telerobot.Game.Tests
{
    public sealed class MenuAndSettingsPlayModeTests
    {
        [SetUp]
        public void SetUp()
        {
            PlayerPreferences.ClearSavedValuesForTests();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPreferences.ClearSavedValuesForTests();
            AudioListener.volume = 1f;
            Time.timeScale = 1f;
        }

        [Test]
        public void SavedPreferencesRoundTripAndClampToDataBounds()
        {
            var defaults = ScriptableObject.CreateInstance<PlayerSettingsAsset>();
            defaults.minimumMouseSensitivity = 0.04f;
            defaults.maximumMouseSensitivity = 0.35f;
            defaults.defaultMouseSensitivity = 0.12f;
            defaults.defaultMasterVolume = 0.8f;
            defaults.defaultEffectsVolume = 0.7f;
            defaults.defaultResolutionWidth = 1280;
            defaults.defaultResolutionHeight = 720;
            defaults.defaultPerspective = CameraPerspective.ThirdPerson;

            PlayerPreferences.Initialize(defaults, false);
            PlayerPreferences.Save(defaults, 9f, -1f, 2f, 1600, 900, true,
                CameraPerspective.FirstPerson, false);
            PlayerPreferences.Initialize(defaults, false);

            Assert.That(PlayerPreferences.MouseSensitivity, Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(PlayerPreferences.MasterVolume, Is.EqualTo(0f).Within(0.001f));
            Assert.That(PlayerPreferences.EffectsVolume, Is.EqualTo(1f).Within(0.001f));
            Assert.That(PlayerPreferences.ResolutionWidth, Is.EqualTo(1600));
            Assert.That(PlayerPreferences.ResolutionHeight, Is.EqualTo(900));
            Assert.That(PlayerPreferences.Fullscreen, Is.True);
            Assert.That(PlayerPreferences.DefaultPerspective, Is.EqualTo(CameraPerspective.FirstPerson));
            Object.DestroyImmediate(defaults);
        }

        [UnityTest]
        public IEnumerator MainMenuOpensSettingsAndStartsMvpWithSavedPerspective()
        {
            var operation = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;

            var menu = Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(menu.SettingsOpen, Is.False);
            Assert.That(Cursor.visible, Is.True);

            menu.OpenSettings();
            Assert.That(menu.SettingsOpen, Is.True);
            menu.Settings.SetDraftPerspectiveForTests(CameraPerspective.FirstPerson);
            menu.Settings.ApplyAndClose();
            Assert.That(menu.SettingsOpen, Is.False);

            menu.StartGame();
            while (SceneManager.GetActiveScene().name != "MVP") yield return null;
            yield return null;

            var game = Object.FindFirstObjectByType<MvpGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.PlayerActor.Perspective, Is.EqualTo(CameraPerspective.FirstPerson));
        }
    }
}
