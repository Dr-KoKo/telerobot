using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Telerobot.Game.Tests
{
    public sealed class HaetaeDataConfigurationTests
    {
        private const string DataRoot = "Assets/Game/Data/Assets/";

        [Test]
        public void PhaseTwoSchema_DeclaresRequiredProgressionAndSpecializationTypes()
        {
            Assert.That(FindType("Telerobot.Game.Core.HaetaeProgressionConfig"), Is.Not.Null);
            Assert.That(FindType("Telerobot.Game.Core.HaetaeSpecializationConfig"), Is.Not.Null);
            Assert.That(FindType("Telerobot.Game.Core.RobotCombatProfileConfig"), Is.Not.Null);
            Assert.That(FindType("Telerobot.Game.Core.SimulationRunOptions"), Is.Not.Null);
            Assert.That(FindType("Telerobot.Game.Data.HaetaeProgressionDefinitionAsset"), Is.Not.Null);
            Assert.That(FindType("Telerobot.Game.Data.HaetaeSpecializationDefinitionAsset"), Is.Not.Null);
        }

        [Test]
        public void PhaseTwoSchema_AddsCatalogZombieAndSimulationFields()
        {
            AssertField("Telerobot.Game.Data.MvpContentCatalog", "haetaeProgression");
            AssertField("Telerobot.Game.Data.MvpContentCatalog", "haetaeSpecializations");
            AssertField("Telerobot.Game.Data.ZombieDefinitionAsset", "haetaeExperienceReward");
            AssertField("Telerobot.Game.Data.SimPlayerProfileAsset", "defaultSpecializationLoadout");
            AssertField("Telerobot.Game.Data.PhaseDefinitionAsset", "opensNewRoute");
            AssertField("Telerobot.Game.Core.PhaseConfig", "OpensNewRoute");
        }

        [Test]
        public void GeneratedCatalogContainsEightContiguousPhasesWithTenMinuteTarget()
        {
            var catalog = AssetDatabase.LoadMainAssetAtPath(DataRoot + "MvpBalanceCatalog.asset");
            var phases = Read<Array>(catalog, "phases").Cast<object>().ToArray();

            Assert.That(phases.Length, Is.EqualTo(8));
            Assert.That(phases.Select(item => Read<int>(item, "number")),
                Is.EqualTo(Enumerable.Range(1, 8)));
            Assert.That(phases.Sum(item => Read<float>(item, "targetDurationSeconds")),
                Is.EqualTo(615f));
            Assert.That(phases.Take(3).All(item => Read<bool>(item, "opensNewRoute")), Is.True);
            Assert.That(phases.Skip(3).All(item => !Read<bool>(item, "opensNewRoute")), Is.True);
        }

        [Test]
        public void PhaseTwoCatalogUsesMvpTwoWithoutActiveUpgradeDependencies()
        {
            var catalog = AssetDatabase.LoadMainAssetAtPath(DataRoot + "MvpBalanceCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(Read<string>(catalog, "dataVersion"), Is.EqualTo("mvp-2.0.0"));
            Assert.That(catalog.GetType().GetField("upgrades", BindingFlags.Instance | BindingFlags.Public), Is.Null);
            Assert.That(FindType("Telerobot.Game.Core.GameplayConfig")
                .GetField("Upgrades", BindingFlags.Instance | BindingFlags.Public), Is.Null);
            Assert.That(FindType("Telerobot.Game.Data.SimPlayerProfileAsset")
                .GetField("upgradeSelectionPolicy", BindingFlags.Instance | BindingFlags.Public), Is.Null);

            var telemetry = Read<object>(catalog, "telemetry");
            var enabledEvents = Read<string[]>(telemetry, "enabledEvents");
            Assert.That(enabledEvents, Does.Not.Contain("upgrade_selected"));
            Assert.That(enabledEvents, Does.Contain("haetae_specialization_selected"));
            Assert.That(enabledEvents, Does.Contain("haetae_mastery_point_gained"));
            Assert.That(enabledEvents, Does.Contain("haetae_mastery_selected"));
        }

        [Test]
        public void GeneratedAssets_UseRequiredInitialProgressionValues()
        {
            var progression = AssetDatabase.LoadMainAssetAtPath(DataRoot + "HaetaeProgression.asset");
            Assert.That(progression, Is.Not.Null);
            Assert.That(progression.GetType().GetField("maximumLevel", BindingFlags.Instance | BindingFlags.Public), Is.Null);
            Assert.That(Read<int>(progression, "experiencePerLevel"), Is.EqualTo(75));
            Assert.That(Read<float>(progression, "readyAlertSeconds"), Is.GreaterThan(0f));
            Assert.That(Read<float>(progression, "powerDamageBonusPerRank"), Is.EqualTo(0.10f));
            Assert.That(Read<float>(progression, "armorDamageReductionPerRank"), Is.EqualTo(0.08f));
            Assert.That(Read<float>(progression, "efficiencyBatteryReductionPerRank"), Is.EqualTo(0.08f));
            Assert.That(Read<float>(progression, "attackSpeedBonusPerRank"), Is.EqualTo(0.10f));
            Assert.That(Read<float>(progression, "minimumReductionMultiplier"), Is.EqualTo(0.50f));

            AssertZombieReward("Runner", 5);
            AssertZombieReward("Bruiser", 25);
            AssertZombieReward("Ripper", 20);
        }

        [Test]
        public void GeneratedAssets_ContainExactlyThreeUniqueSelectableRoles()
        {
            var catalog = AssetDatabase.LoadMainAssetAtPath(DataRoot + "MvpBalanceCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            var roles = Read<Array>(catalog, "haetaeSpecializations");
            Assert.That(roles, Is.Not.Null);
            Assert.That(roles.Length, Is.EqualTo(3));

            var ids = roles.Cast<object>().Select(item => Read<object>(item, "id").ToString()).ToArray();
            Assert.That(ids, Is.EquivalentTo(new[] { "Melee", "Ranged", "Balanced" }));
            Assert.That(ids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(3));
        }

        [Test]
        public void GeneratedRobotUsesExplicitLegacyEquivalentPhysicalFootprint()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Telerobot.Game.Data.MvpContentCatalog>(
                DataRoot + "MvpBalanceCatalog.asset");
            var robot = AssetDatabase.LoadAssetAtPath<Telerobot.Game.Data.RobotDefinitionAsset>(
                DataRoot + "HaetaeRobot.asset");
            var mapped = Telerobot.Game.Data.MvpDataMapper.Map(catalog).Robot;

            Assert.That(robot.bodyColliderRadius, Is.EqualTo(0.75f));
            Assert.That(robot.bodyColliderHeight, Is.EqualTo(1.5f));
            Assert.That(robot.bodyColliderCenterY, Is.Zero);
            Assert.That(mapped.BodyColliderRadius, Is.EqualTo(robot.bodyColliderRadius));
            Assert.That(mapped.BodyColliderHeight, Is.EqualTo(robot.bodyColliderHeight));
            Assert.That(mapped.BodyColliderCenterY, Is.EqualTo(robot.bodyColliderCenterY));
            Assert.That(typeof(Telerobot.Game.Data.HaetaeSpecializationDefinitionAsset)
                .GetField("scaleMultiplier", BindingFlags.Instance | BindingFlags.Public), Is.Null);
        }

        [Test]
        public void MapperRejectsInvalidRobotPhysicalFootprint()
        {
            var source = AssetDatabase.LoadAssetAtPath<Telerobot.Game.Data.MvpContentCatalog>(
                DataRoot + "MvpBalanceCatalog.asset");
            var catalog = UnityEngine.Object.Instantiate(source);
            catalog.robot = UnityEngine.Object.Instantiate(source.robot);
            catalog.robot.bodyColliderRadius = 0.8f;
            catalog.robot.bodyColliderHeight = 1.5f;

            Assert.Throws<InvalidOperationException>(() =>
                Telerobot.Game.Data.MvpDataMapper.Map(catalog));

            catalog.robot.bodyColliderHeight = 1.6f;
            catalog.robot.bodyColliderCenterY = float.NaN;
            Assert.Throws<InvalidOperationException>(() =>
                Telerobot.Game.Data.MvpDataMapper.Map(catalog));

            UnityEngine.Object.DestroyImmediate(catalog.robot);
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [TestCase("haetae.specialization.melee", "근거리형")]
        [TestCase("haetae.specialization.ranged", "원거리형")]
        [TestCase("haetae.specialization.balanced", "균형형")]
        public void StringTable_ResolvesRequiredRoleName(string key, string expected)
        {
            var table = AssetDatabase.LoadMainAssetAtPath(DataRoot + "StringTable.asset");
            Assert.That(table, Is.Not.Null);
            var entries = Read<IEnumerable>(table, "entries").Cast<object>();
            var match = entries.SingleOrDefault(item => Read<string>(item, "key") == key);
            Assert.That(match, Is.Not.Null, "Missing string key " + key);
            Assert.That(Read<string>(match, "value"), Is.EqualTo(expected));
        }

        [Test]
        public void StringTableContainsDistinctRadioKeysForAllEightPhases()
        {
            var table = AssetDatabase.LoadMainAssetAtPath(DataRoot + "StringTable.asset");
            var entries = Read<IEnumerable>(table, "entries").Cast<object>().ToArray();

            for (var phase = 1; phase <= 8; phase++)
                Assert.That(entries.Count(item => Read<string>(item, "key") == "radio.phase" + phase),
                    Is.EqualTo(1), "Missing or duplicate radio key for phase " + phase);
        }

        private static void AssertZombieReward(string assetName, int expected)
        {
            var zombie = AssetDatabase.LoadMainAssetAtPath(DataRoot + assetName + ".asset");
            Assert.That(zombie, Is.Not.Null);
            Assert.That(Read<int>(zombie, "haetaeExperienceReward"), Is.EqualTo(expected));
        }

        private static void AssertField(string typeName, string fieldName)
        {
            var type = FindType(typeName);
            Assert.That(type, Is.Not.Null, "Missing type " + typeName);
            Assert.That(type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public), Is.Not.Null,
                "Missing field " + typeName + "." + fieldName);
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName, false))
                .FirstOrDefault(type => type != null);
        }

        private static T Read<T>(object source, string fieldName)
        {
            Assert.That(source, Is.Not.Null);
            var field = source.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(field, Is.Not.Null, "Missing field " + source.GetType().FullName + "." + fieldName);
            return (T)field.GetValue(source);
        }
    }
}
