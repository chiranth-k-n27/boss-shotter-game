using System.Collections;
using NUnit.Framework;
using UnityEngine;
using MobileShooter.Boss;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.Tests.Boss
{
    public class BossHealthTests
    {
        private GameObject _bossObject;
        private BossHealth _bossHealth;
        private BossAIController _bossAI;
        private BossHitbox _bossHitbox;

        [SetUp]
        public void Setup()
        {
            _bossObject = new GameObject("Boss");
            _bossHealth = _bossObject.AddComponent<BossHealth>();
            _bossAI = _bossObject.AddComponent<BossAIController>();
            _bossHitbox = _bossObject.AddComponent<BossHitbox>();

            // Set up basic attributes for testing
            _bossHealth.maxHealth = 1000f;
            _bossHealth.currentHealth = 1000f;

            // Initialize event listeners state if any need manual resetting
        }

        [TearDown]
        public void Teardown()
        {
            GameObject.DestroyImmediate(_bossObject);
        }

        [Test]
        public void TakeDamage_WithHeadHitbox_MultipliesDamageBy2Point5()
        {
            float initialHealth = _bossHealth.CurrentHealth;
            float rawDamage = 100f;

            _bossHealth.TakeDamage(rawDamage, HitboxType.Head, Vector3.zero, Vector3.up);

            float expectedDamage = rawDamage * 2.5f;
            Assert.AreEqual(initialHealth - expectedDamage, _bossHealth.CurrentHealth,
                "Head hitbox should multiply damage by 2.5.");
        }

        [Test]
        public void TakeDamage_WithArmorHitbox_MultipliesDamageBy0Point5()
        {
            float initialHealth = _bossHealth.CurrentHealth;
            float rawDamage = 100f;

            _bossHealth.TakeDamage(rawDamage, HitboxType.Armor, Vector3.zero, Vector3.up);

            float expectedDamage = rawDamage * 0.5f;
            Assert.AreEqual(initialHealth - expectedDamage, _bossHealth.CurrentHealth,
                "Armor hitbox should multiply damage by 0.5.");
        }

        [Test]
        public void TakeDamage_WithBodyHitbox_DoesNotMultiplyDamage()
        {
            float initialHealth = _bossHealth.CurrentHealth;
            float rawDamage = 100f;

            _bossHealth.TakeDamage(rawDamage, HitboxType.Body, Vector3.zero, Vector3.up);

            Assert.AreEqual(initialHealth - rawDamage, _bossHealth.CurrentHealth,
                "Body hitbox should apply 1x damage multiplier.");
        }

        [Test]
        public void TakeDamage_HealthDoesNotDropBelowZero()
        {
            _bossHealth.TakeDamage(2000f, HitboxType.Body, Vector3.zero, Vector3.up);

            Assert.AreEqual(0f, _bossHealth.CurrentHealth,
                "Current health should not drop below 0.");
        }

        [Test]
        public void TakeDamage_WhenDead_IgnoresFurtherDamage()
        {
            // Kill the boss first
            _bossHealth.TakeDamage(1000f, HitboxType.Body, Vector3.zero, Vector3.up);
            Assert.IsTrue(_bossHealth.IsDead, "Boss should be dead.");

            float healthAfterDeath = _bossHealth.CurrentHealth;

            // Try damaging again
            _bossHealth.TakeDamage(100f, HitboxType.Body, Vector3.zero, Vector3.up);

            Assert.AreEqual(healthAfterDeath, _bossHealth.CurrentHealth,
                "Dead boss should not take further damage.");
        }

        [Test]
        public void TakeDamage_DropsTo50Percent_TriggersEnragePhase()
        {
            // Reset to clean slate for event tracking
            bool eventTriggered = false;
            BossState reportedState = BossState.Idle;

            System.Action<BossState> handler = (state) => {
                if (state == BossState.Phase2Enrage) {
                    eventTriggered = true;
                    reportedState = state;
                }
            };

            GameEvents.OnBossStateChanged += handler;

            try
            {
                // Deal exactly 50% damage
                _bossHealth.TakeDamage(500f, HitboxType.Body, Vector3.zero, Vector3.up);

                Assert.IsTrue(eventTriggered, "Enrage phase event should be triggered at 50% health.");
                Assert.AreEqual(BossState.Phase2Enrage, reportedState, "Reported state should be Phase2Enrage.");
            }
            finally
            {
                GameEvents.OnBossStateChanged -= handler;
            }
        }

        [Test]
        public void TakeDamage_DropsToZeroHealth_TriggersDeathAndDeactivates()
        {
            bool deathEventTriggered = false;

            System.Action<BossState> handler = (state) => {
                if (state == BossState.Dead) {
                    deathEventTriggered = true;
                }
            };

            GameEvents.OnBossStateChanged += handler;

            try
            {
                _bossHealth.TakeDamage(1000f, HitboxType.Body, Vector3.zero, Vector3.up);

                Assert.IsTrue(deathEventTriggered, "Death event should be triggered at 0 health.");
                Assert.IsTrue(_bossHealth.IsDead, "IsDead flag should be true.");
                Assert.IsFalse(_bossObject.activeSelf, "Boss GameObject should be deactivated upon death.");
            }
            finally
            {
                GameEvents.OnBossStateChanged -= handler;
            }
        }
    }
}
