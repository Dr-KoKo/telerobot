using System;
using System.Linq;
using NUnit.Framework;
using Telerobot.Game.Data;
using Telerobot.Game.Runtime;
using UnityEditor;
using UnityEngine;

namespace Telerobot.Game.Tests
{
    public sealed class CharacterMotionEditModeTests
    {
        [Test]
        public void GeneratedTheme_HasUniqueProfilesForEverySupportedRole()
        {
            var theme = AssetDatabase.LoadAssetAtPath<VisualThemeDefinitionAsset>(
                "Assets/Game/Data/Assets/VisualTheme.asset");

            Assert.That(theme, Is.Not.Null);
            Assert.DoesNotThrow(theme.Validate);
            Assert.That(theme.characterMotionProfiles.Length, Is.EqualTo(8));
            Assert.That(theme.characterMotionProfiles.Select(item => item.role).Distinct().Count(),
                Is.EqualTo(8));
            Assert.That(theme.characterMotionProfiles.Select(item => item.profileId).Distinct().Count(),
                Is.EqualTo(8));
            foreach (var role in SupportedRoles())
                Assert.That(theme.MotionProfileFor(role), Is.Not.Null, role.ToString());
        }

        [Test]
        public void ProfileValidation_RejectsDuplicateAndUnsupportedRoles()
        {
            var theme = MinimalTheme();
            theme.characterMotionProfiles = new[]
            {
                Profile(PresentationRole.Runner, "runner"),
                Profile(PresentationRole.Runner, "runner-copy")
            };
            Assert.Throws<InvalidOperationException>(theme.Validate);

            theme.characterMotionProfiles = new[]
            {
                Profile(PresentationRole.PlayerCommander, "player")
            };
            Assert.Throws<InvalidOperationException>(theme.Validate);
            UnityEngine.Object.DestroyImmediate(theme);
        }

        [Test]
        public void SamplePose_ChangesOnlyPresentationHierarchy()
        {
            var actor = new GameObject("Motion Test Actor");
            actor.transform.position = new Vector3(4f, 2f, -3f);
            actor.transform.rotation = Quaternion.Euler(0f, 37f, 0f);
            var collider = actor.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1f, 0f);
            collider.radius = 0.42f;
            collider.height = 1.8f;
            var visual = new GameObject(LowPolyModelFactory.VisualRootName);
            visual.transform.SetParent(actor.transform, false);
            var head = new GameObject("head");
            head.transform.SetParent(visual.transform, false);
            head.transform.localPosition = Vector3.up;
            var driver = actor.AddComponent<CharacterMotionDriver>();
            driver.Bind(visual.transform, PresentationRole.Runner, Profile(
                PresentationRole.Runner, "runner"));

            var actorPosition = actor.transform.position;
            var actorRotation = actor.transform.rotation;
            var actorScale = actor.transform.localScale;
            var colliderCenter = collider.center;
            driver.SampleForTests(CharacterMotionState.Locomotion, 0.25f);

            Assert.That(actor.transform.position, Is.EqualTo(actorPosition));
            Assert.That(actor.transform.rotation, Is.EqualTo(actorRotation));
            Assert.That(actor.transform.localScale, Is.EqualTo(actorScale));
            Assert.That(collider.center, Is.EqualTo(colliderCenter));
            Assert.That(visual.transform.localPosition, Is.Not.EqualTo(Vector3.zero));
            Assert.That(head.transform.localRotation, Is.Not.EqualTo(Quaternion.identity));
            UnityEngine.Object.DestroyImmediate(actor);
        }

        [Test]
        public void DualLodTargets_UseTheSamePosePhase()
        {
            var actor = new GameObject("LOD Motion Actor");
            var visual = new GameObject(LowPolyModelFactory.VisualRootName);
            visual.transform.SetParent(actor.transform, false);
            var lod0 = JointHierarchy(visual.transform, "LOD0");
            var lod1 = JointHierarchy(visual.transform, "LOD1");
            var driver = actor.AddComponent<CharacterMotionDriver>();
            driver.Bind(visual.transform, PresentationRole.Ripper, Profile(
                PresentationRole.Ripper, "ripper"));

            driver.SampleForTests(
                CharacterMotionState.Attack, 0.2f, CharacterAttackMotion.Ripper);

            Assert.That(driver.BoundTargetCount, Is.EqualTo(4));
            Assert.That(lod0[0].localRotation, Is.EqualTo(lod1[0].localRotation));
            Assert.That(lod0[1].localRotation, Is.EqualTo(lod1[1].localRotation));
            UnityEngine.Object.DestroyImmediate(actor);
        }

        [Test]
        public void MissingJointsAndRepeatedAttach_KeepOneRootFallbackDriver()
        {
            var theme = MinimalTheme();
            theme.characterMotionProfiles = new[]
            {
                Profile(PresentationRole.Runner, "runner")
            };
            var library = new PresentationMaterialLibrary(theme, null);
            var factory = new LowPolyModelFactory(library);
            var actor = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            factory.Attach(actor, PresentationRole.Runner);
            factory.Attach(actor, PresentationRole.Runner);
            var driver = actor.GetComponent<CharacterMotionDriver>();
            Assert.That(driver, Is.Not.Null);
            Assert.That(actor.GetComponents<CharacterMotionDriver>().Length, Is.EqualTo(1));
            Assert.That(driver.BindCount, Is.EqualTo(2));
            Assert.DoesNotThrow(() =>
                driver.SampleForTests(CharacterMotionState.Death, 0.8f));

            library.Dispose();
            UnityEngine.Object.DestroyImmediate(actor);
            UnityEngine.Object.DestroyImmediate(theme);
        }

        private static Transform[] JointHierarchy(Transform parent, string name)
        {
            var lod = new GameObject(name);
            lod.transform.SetParent(parent, false);
            var skinnedBody = new GameObject("Zombie_Body");
            skinnedBody.transform.SetParent(lod.transform, false);
            skinnedBody.AddComponent<SkinnedMeshRenderer>();
            var head = new GameObject("head");
            head.transform.SetParent(lod.transform, false);
            var arm = new GameObject("upper_arm_l");
            arm.transform.SetParent(lod.transform, false);
            return new[] { head.transform, arm.transform };
        }

        private static CharacterMotionProfileDefinition Profile(
            PresentationRole role, string id)
        {
            return new CharacterMotionProfileDefinition
            {
                role = role,
                profileId = id,
                cycleHz = 1.5f,
                idleBob = 0.02f,
                locomotionBob = 0.08f,
                swayDegrees = 6f,
                forwardLeanDegrees = 10f,
                strideDegrees = 32f,
                attackDegrees = 48f,
                attackRecoil = 0.18f,
                hitDegrees = 16f,
                deathDegrees = 82f,
                attackDuration = 0.3f,
                hitDuration = 0.15f
            };
        }

        private static VisualThemeDefinitionAsset MinimalTheme()
        {
            var theme = ScriptableObject.CreateInstance<VisualThemeDefinitionAsset>();
            theme.themeId = "motion-test";
            theme.haetaeOcclusionMaterialTemplate = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Game/Art/Materials/ally-haetae-occlusion.mat");
            theme.colors = VisualThemeDefinitionAsset.RequiredColorKeys
                .Select(key => new SemanticColorDefinition { key = key, value = Color.white })
                .ToArray();
            theme.materials = VisualThemeDefinitionAsset.RequiredMaterialKeys
                .Select(key => new MaterialRoleDefinition { key = key, baseColor = Color.white })
                .ToArray();
            return theme;
        }

        private static PresentationRole[] SupportedRoles()
        {
            return new[]
            {
                PresentationRole.HaetaeGeneralUnit1,
                PresentationRole.HaetaeGeneralUnit2,
                PresentationRole.HaetaeMeleePreview,
                PresentationRole.HaetaeRangedPreview,
                PresentationRole.HaetaeBalancedPreview,
                PresentationRole.Runner,
                PresentationRole.Bruiser,
                PresentationRole.Ripper
            };
        }
    }
}
