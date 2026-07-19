using System.Collections.Generic;
using NUnit.Framework;
using Telerobot.Game.Core;

namespace Telerobot.Game.Tests
{
    public sealed class CombatAndHealthTests
    {
        private GameplayConfig config;

        [SetUp]
        public void SetUp() { config = TestConfigFactory.Create(); }

        [Test]
        public void RifleBodyAndHeadDamageMatchSpec()
        {
            Assert.That(CombatRules.CalculateBulletDamage(config.Weapon, HitRegion.Body), Is.EqualTo(30f));
            Assert.That(CombatRules.CalculateBulletDamage(config.Weapon, HitRegion.Head), Is.EqualTo(75f));
            var runner = new HealthState(90f);
            CombatRules.ApplyDamage(runner, 30f);
            CombatRules.ApplyDamage(runner, 30f);
            Assert.That(runner.IsDead, Is.False);
            CombatRules.ApplyDamage(runner, 30f);
            Assert.That(runner.IsDead, Is.True);
        }

        [Test]
        public void HaetaeKillTimingFitsRunnerAndBruiserBands()
        {
            var runnerHits = (int)System.Math.Ceiling(config.GetZombie(ZombieType.Runner).MaxHealth / config.Robot.AttackDamage);
            var bruiserHits = (int)System.Math.Ceiling(config.GetZombie(ZombieType.Bruiser).MaxHealth / config.Robot.AttackDamage);
            var runnerSeconds = (runnerHits - 1) * config.Robot.AttackInterval;
            var bruiserSeconds = (bruiserHits - 1) * config.Robot.AttackInterval;
            Assert.That(runnerSeconds, Is.InRange(1f, 2f));
            Assert.That(bruiserSeconds, Is.InRange(6f, 10f));
        }

        [Test]
        public void BaseRecoveryAddsFifteenPercentAndCaps()
        {
            var baseState = new BaseState(1000f);
            baseState.Health.Current = 400f;
            Assert.That(CombatRules.RecoverBase(baseState, 0.15f), Is.EqualTo(150f));
            Assert.That(baseState.Health.Current, Is.EqualTo(550f));
            baseState.Health.Current = 950f;
            Assert.That(CombatRules.RecoverBase(baseState, 0.15f), Is.EqualTo(50f));
        }

        [Test]
        public void AmmoReloadAndResupplyFollowContract()
        {
            var ammo = new AmmoState(30, 60);
            Assert.That(CombatRules.TryFire(ammo), Is.True);
            Assert.That(ammo.Loaded, Is.EqualTo(29));
            Assert.That(CombatRules.BeginReload(ammo, 2f), Is.True);
            Assert.That(CombatRules.TickReload(ammo, 1.99f), Is.False);
            Assert.That(CombatRules.TickReload(ammo, 0.01f), Is.True);
            Assert.That(ammo.Loaded, Is.EqualTo(30));
            ammo.Reserve = 0;
            CombatRules.Resupply(ammo, 180);
            Assert.That(ammo.Reserve, Is.EqualTo(180));
        }

        [Test]
        public void GrenadeUsesLinearFalloffAndTenTargetCap()
        {
            Assert.That(CombatRules.GrenadeDamage(config.Grenade, 2f), Is.EqualTo(150f));
            Assert.That(CombatRules.GrenadeDamage(config.Grenade, 5f), Is.EqualTo(60f));
            Assert.That(CombatRules.GrenadeDamage(config.Grenade, 6f), Is.Zero);
            var targets = new List<GrenadeTarget>();
            for (var index = 0; index < 12; index++) targets.Add(new GrenadeTarget(index.ToString(), 1f, new HealthState(200f)));
            Assert.That(CombatRules.ApplyGrenade(config.Grenade, targets).Count, Is.EqualTo(10));
        }
    }
}
