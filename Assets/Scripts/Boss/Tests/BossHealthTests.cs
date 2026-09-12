using NUnit.Framework;
using UnityEngine;
using MobileShooter.Boss;
using MobileShooter.Core;

namespace MobileShooter.Boss.Tests
{
    public class BossHealthTests
    {
        private GameObject bossObject;
        private BossHealth bossHealth;

        [SetUp]
        public void Setup()
        {
            bossObject = new GameObject("Boss");
            bossHealth = bossObject.AddComponent<BossHealth>();
        }

        [TearDown]
        public void Teardown()
        {
            if (bossObject != null)
            {
                Object.DestroyImmediate(bossObject);
            }
        }

        [Test]
        public void TakeDamage_NegativeOrZeroDamage_DoesNotIncreaseHealth()
        {
            // Arrange
            float initialHealth = bossHealth.CurrentHealth;
            float rawDamage = -50f;

            // Act
            bossHealth.TakeDamage(rawDamage, HitboxType.Body, Vector3.zero, Vector3.up);

            // Assert
            Assert.AreEqual(initialHealth, bossHealth.CurrentHealth, "Boss health should not increase when taking negative damage.");

            // Act
            bossHealth.TakeDamage(0f, HitboxType.Body, Vector3.zero, Vector3.up);

            // Assert
            Assert.AreEqual(initialHealth, bossHealth.CurrentHealth, "Boss health should not change when taking zero damage.");
        }
    }
}
