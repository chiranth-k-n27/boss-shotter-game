using System;
using NUnit.Framework;
using UnityEngine;
using MobileShooter.Boss;
using MobileShooter.Core;

namespace MobileShooter.Tests.Boss
{
    [TestFixture]
    public class BossHealthTests
    {
        private GameObject _bossGameObject;
        private BossHealth _bossHealth;

        [SetUp]
        public void Setup()
        {
            _bossGameObject = new GameObject("Boss");
            _bossHealth = _bossGameObject.AddComponent<BossHealth>();
            _bossHealth.maxHealth = 100f;
            // Initialize health using reflection since Start() isn't called automatically outside Unity play mode
            _bossHealth.currentHealth = 100f;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_bossGameObject);
        }

        [Test]
        public void TakeDamage_NormalDamage_ReducesHealth()
        {
            // Act
            _bossHealth.TakeDamage(10f, HitboxType.Body, Vector3.zero, Vector3.up);

            // Assert
            Assert.That(_bossHealth.CurrentHealth, Is.EqualTo(90f));
        }

        [Test]
        public void TakeDamage_FatalDamage_ClampsHealthToZeroAndSetsIsDead()
        {
            // Act
            _bossHealth.TakeDamage(150f, HitboxType.Body, Vector3.zero, Vector3.up);

            // Assert
            Assert.That(_bossHealth.CurrentHealth, Is.EqualTo(0f));
            Assert.That(_bossHealth.IsDead, Is.True);
        }

        [Test]
        public void TakeDamage_NegativeDamage_ClampsHealthToMax()
        {
            // Arrange
            _bossHealth.currentHealth = 90f;

            // Act
            // Negative damage will result in negative final damage and act as a heal
            _bossHealth.TakeDamage(-20f, HitboxType.Body, Vector3.zero, Vector3.up);

            // Assert
            Assert.That(_bossHealth.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void TakeDamage_WithHeadHitbox_MultipliesDamageBy2_5()
        {
            // Act
            _bossHealth.TakeDamage(10f, HitboxType.Head, Vector3.zero, Vector3.up);

            // Assert
            // 10 * 2.5 = 25 damage. 100 - 25 = 75.
            Assert.That(_bossHealth.CurrentHealth, Is.EqualTo(75f));
        }

        [Test]
        public void TakeDamage_WithArmorHitbox_MultipliesDamageBy0_5()
        {
            // Act
            _bossHealth.TakeDamage(10f, HitboxType.Armor, Vector3.zero, Vector3.up);

            // Assert
            // 10 * 0.5 = 5 damage. 100 - 5 = 95.
            Assert.That(_bossHealth.CurrentHealth, Is.EqualTo(95f));
        }
    }
}
