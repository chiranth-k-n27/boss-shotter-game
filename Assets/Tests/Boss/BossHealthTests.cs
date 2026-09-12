using NUnit.Framework;
using UnityEngine;
using MobileShooter.Boss;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.Tests.Boss
{
    public class BossHealthTests
    {
        private GameObject bossObject;
        private BossHealth bossHealth;

        [SetUp]
        public void Setup()
        {
            bossObject = new GameObject();
            bossHealth = bossObject.AddComponent<BossHealth>();
            bossHealth.maxHealth = 1000f;
            bossHealth.currentHealth = 1000f;

            // Add a dummy BossAIController so GetComponent<BossAIController>() doesn't return null if it matters,
            // though it is guarded by a null check in TriggerEnragePhase.
            // bossObject.AddComponent<BossAIController>();
        }

        [TearDown]
        public void Teardown()
        {
            GameObject.DestroyImmediate(bossObject);
        }

        [Test]
        public void TakeDamage_BodyHitbox_AppliesNormalDamage()
        {
            float initialHealth = bossHealth.CurrentHealth;
            bossHealth.TakeDamage(100f, HitboxType.Body, Vector3.zero, Vector3.up);

            Assert.AreEqual(initialHealth - 100f, bossHealth.CurrentHealth);
        }

        [Test]
        public void TakeDamage_HeadHitbox_AppliesCriticalDamage()
        {
            float initialHealth = bossHealth.CurrentHealth;
            bossHealth.TakeDamage(100f, HitboxType.Head, Vector3.zero, Vector3.up);

            Assert.AreEqual(initialHealth - 250f, bossHealth.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ArmorHitbox_AppliesReducedDamage()
        {
            float initialHealth = bossHealth.CurrentHealth;
            bossHealth.TakeDamage(100f, HitboxType.Armor, Vector3.zero, Vector3.up);

            Assert.AreEqual(initialHealth - 50f, bossHealth.CurrentHealth);
        }

        [Test]
        public void TakeDamage_HealthBelowZero_ClampsToZeroAndDies()
        {
            bossHealth.TakeDamage(2000f, HitboxType.Body, Vector3.zero, Vector3.up);

            Assert.AreEqual(0f, bossHealth.CurrentHealth);
            Assert.IsTrue(bossHealth.IsDead);
        }

        [Test]
        public void TakeDamage_WhenDead_IgnoresDamage()
        {
            // First kill the boss
            bossHealth.TakeDamage(1000f, HitboxType.Body, Vector3.zero, Vector3.up);
            Assert.IsTrue(bossHealth.IsDead);

            // Try to damage again
            bossHealth.TakeDamage(100f, HitboxType.Body, Vector3.zero, Vector3.up);

            // Health should still be 0
            Assert.AreEqual(0f, bossHealth.CurrentHealth);
        }
    }
}
