using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MobileShooter.Boss;
using MobileShooter.Core;

namespace MobileShooter.Tests
{
    [TestFixture]
    public class BossHitboxTests
    {
        private GameObject _gameObject;
        private BossHitbox _bossHitbox;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject();
            _bossHitbox = _gameObject.AddComponent<BossHitbox>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void GetDamageMultiplier_WhenHitboxTypeIsHead_ReturnsHeadMultiplier()
        {
            // Arrange
            _bossHitbox.hitboxType = HitboxType.Head;

            // Act
            float multiplier = _bossHitbox.GetDamageMultiplier();

            // Assert
            Assert.That(multiplier, Is.EqualTo(2.5f));
        }

        [Test]
        public void GetDamageMultiplier_WhenHitboxTypeIsArmor_ReturnsArmorMultiplier()
        {
            // Arrange
            _bossHitbox.hitboxType = HitboxType.Armor;

            // Act
            float multiplier = _bossHitbox.GetDamageMultiplier();

            // Assert
            Assert.That(multiplier, Is.EqualTo(0.5f));
        }

        [Test]
        public void GetDamageMultiplier_WhenHitboxTypeIsBody_ReturnsBodyMultiplier()
        {
            // Arrange
            _bossHitbox.hitboxType = HitboxType.Body;

            // Act
            float multiplier = _bossHitbox.GetDamageMultiplier();

            // Assert
            Assert.That(multiplier, Is.EqualTo(1.0f));
        }
    }
}
