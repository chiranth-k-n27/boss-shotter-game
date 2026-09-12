using System;
using NUnit.Framework;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Boss;

namespace MobileShooter.Tests.Boss
{
    public class BossHitboxTests
    {
        private GameObject _gameObject;
        private BossHitbox _bossHitbox;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject();
            _bossHitbox = _gameObject.AddComponent<BossHitbox>();
        }

        [Test]
        public void GetDamageMultiplier_HeadHitbox_ReturnsHeadMultiplier()
        {
            // Arrange
            _bossHitbox.hitboxType = HitboxType.Head;

            // Act
            float multiplier = _bossHitbox.GetDamageMultiplier();

            // Assert
            Assert.That(multiplier, Is.EqualTo(2.5f));
        }

        [Test]
        public void GetDamageMultiplier_ArmorHitbox_ReturnsArmorMultiplier()
        {
            // Arrange
            _bossHitbox.hitboxType = HitboxType.Armor;

            // Act
            float multiplier = _bossHitbox.GetDamageMultiplier();

            // Assert
            Assert.That(multiplier, Is.EqualTo(0.5f));
        }

        [Test]
        public void GetDamageMultiplier_BodyHitbox_ReturnsBodyMultiplier()
        {
            // Arrange
            _bossHitbox.hitboxType = HitboxType.Body;

            // Act
            float multiplier = _bossHitbox.GetDamageMultiplier();

            // Assert
            Assert.That(multiplier, Is.EqualTo(1.0f));
        }

        [Test]
        public void GetDamageMultiplier_InvalidHitbox_ReturnsDefaultMultiplier()
        {
            // Arrange
            _bossHitbox.hitboxType = (HitboxType)999; // Invalid/unknown type

            // Act
            float multiplier = _bossHitbox.GetDamageMultiplier();

            // Assert
            Assert.That(multiplier, Is.EqualTo(1.0f)); // Default behavior
        }
    }
}