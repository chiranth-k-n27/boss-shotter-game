using NUnit.Framework;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Boss;

namespace MobileShooter.Tests.Boss
{
    [TestFixture]
    public class BossHitboxTests
    {
        private GameObject _bossGameObject;
        private BossHitbox _hitbox;

        [SetUp]
        public void Setup()
        {
            _bossGameObject = new GameObject("BossHitboxTestObject");
            _hitbox = _bossGameObject.AddComponent<BossHitbox>();
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_bossGameObject);
        }

        [Test]
        public void GetDamageMultiplier_HeadHitbox_ReturnsHeadMultiplier()
        {
            // Arrange
            _hitbox.hitboxType = HitboxType.Head;

            // Act
            float multiplier = _hitbox.GetDamageMultiplier();

            // Assert
            Assert.AreEqual(2.5f, multiplier, "Head hitbox should return 2.5f damage multiplier.");
        }

        [Test]
        public void GetDamageMultiplier_ArmorHitbox_ReturnsArmorMultiplier()
        {
            // Arrange
            _hitbox.hitboxType = HitboxType.Armor;

            // Act
            float multiplier = _hitbox.GetDamageMultiplier();

            // Assert
            Assert.AreEqual(0.5f, multiplier, "Armor hitbox should return 0.5f damage multiplier.");
        }

        [Test]
        public void GetDamageMultiplier_BodyHitbox_ReturnsNormalMultiplier()
        {
            // Arrange
            _hitbox.hitboxType = HitboxType.Body;

            // Act
            float multiplier = _hitbox.GetDamageMultiplier();

            // Assert
            Assert.AreEqual(1.0f, multiplier, "Body hitbox should return 1.0f damage multiplier.");
        }
    }
}
