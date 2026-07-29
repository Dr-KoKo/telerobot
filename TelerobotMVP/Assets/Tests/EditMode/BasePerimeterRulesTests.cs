using System;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Core;
using Telerobot.Game.Data;
using UnityEditor;
using UnityEngine;

namespace Telerobot.Game.Tests
{
    public sealed class BasePerimeterRulesTests
    {
        private const string DataRoot = "Assets/Game/Data/Assets/";

        [Test]
        public void GeneratedWorldLayoutMapsValidatedTerraceProfile()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MvpContentCatalog>(
                DataRoot + "MvpBalanceCatalog.asset");
            var config = MvpDataMapper.Map(catalog).World;

            Assert.That(config.BaseOuterRadius, Is.EqualTo(4f));
            Assert.That(config.BaseTerraceCount, Is.EqualTo(3));
            Assert.That(config.BaseTerraceRise, Is.EqualTo(0.25f));
            Assert.That(config.BaseTerraceCount * config.BaseTerraceRise,
                Is.LessThanOrEqualTo(0.75f));
            Assert.That(config.BaseBeaconDiameter, Is.LessThanOrEqualTo(1f));
            Assert.That(config.BaseOuterRadius -
                (config.BaseTerraceCount - 1) * config.BaseTerraceDepth -
                config.BaseTerraceSlopeRun,
                Is.GreaterThan(config.BaseBeaconDiameter * 0.5f));
        }

        [Test]
        public void MapperRejectsSightBlockingTerraceProfile()
        {
            var source = AssetDatabase.LoadAssetAtPath<MvpContentCatalog>(
                DataRoot + "MvpBalanceCatalog.asset");
            var catalog = UnityEngine.Object.Instantiate(source);
            catalog.world = UnityEngine.Object.Instantiate(source.world);
            catalog.world.baseTerraceRise = 0.3f;

            Assert.Throws<InvalidOperationException>(() => MvpDataMapper.Map(catalog));

            UnityEngine.Object.DestroyImmediate(catalog.world);
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void AttackSlotsAreDeterministicDistinctAndOutsideFootprint()
        {
            var slots = Enumerable.Range(0, 21)
                .Select(index => BasePerimeterRules.AttackSlot(
                    new Float3(2f, 1f, -3f),
                    new Float3(0f, 0f, 9f),
                    4f, index, 0.15f, 0.75f, 0.95f))
                .ToArray();
            var repeated = BasePerimeterRules.AttackSlot(
                new Float3(2f, 1f, -3f),
                new Float3(0f, 0f, 9f),
                4f, 8, 0.15f, 0.75f, 0.95f);

            Assert.That(repeated.X, Is.EqualTo(slots[8].X));
            Assert.That(repeated.Y, Is.EqualTo(slots[8].Y));
            Assert.That(repeated.Z, Is.EqualTo(slots[8].Z));
            Assert.That(slots.Take(6).Select(item =>
                item.X.ToString("F2") + ":" + item.Z.ToString("F2")).Distinct().Count(),
                Is.EqualTo(6));
            Assert.That(slots.All(item =>
            {
                var x = item.X - 2f;
                var z = item.Z + 3f;
                return Math.Sqrt(x * x + z * z) >= 4.15f - 0.001f;
            }), Is.True);
        }

        [Test]
        public void DiagonalAndZeroApproachesUseStableRadialDirections()
        {
            var diagonal = BasePerimeterRules.AttackSlot(
                new Float3(0f, 2f, 0f),
                new Float3(1f, 8f, 1f),
                4f, 3, 0.15f, 0.75f, 0.95f);
            var fallback = BasePerimeterRules.AttackSlot(
                new Float3(0f, 2f, 0f),
                new Float3(0f, 0f, 0f),
                4f, 3, 0.15f, 0.75f, 0.95f);

            Assert.That(diagonal.X, Is.EqualTo(diagonal.Z).Within(0.001f));
            Assert.That(Math.Sqrt(diagonal.X * diagonal.X + diagonal.Z * diagonal.Z),
                Is.EqualTo(4.15f).Within(0.001f));
            Assert.That(fallback.X, Is.EqualTo(0f).Within(0.001f));
            Assert.That(fallback.Z, Is.EqualTo(4.15f).Within(0.001f));
            Assert.That(fallback.Y, Is.EqualTo(2f));
        }

        [Test]
        public void InvalidPerimeterDimensionsAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BasePerimeterRules.AttackSlot(default, default, 0f, 0, 0f, 1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BasePerimeterRules.AttackSlot(default, default, 4f, 0, -1f, 1f, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BasePerimeterRules.AttackSlot(default, default, 4f, 0, 0f, float.NaN, 1f));
        }
    }
}
