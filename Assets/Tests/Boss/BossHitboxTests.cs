using NUnit.Framework;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Boss;

namespace MobileShooter.Tests.Boss
{
    [TestFixture]
    public class BossHitboxTests
    {
        private GameObject _hitboxObj;
        private BossHitbox _bossHitbox;

        [SetUp]
        public void Setup()
        {
            _hitboxObj = new GameObject("BossHitbox");
            _bossHitbox = _hitboxObj.AddComponent<BossHitbox>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_hitboxObj != null)
            {
                UnityEngine.Object.DestroyImmediate(_hitboxObj);
            }
        }

        [Test]
        public void GetDamageMultiplier_Head_ReturnsCorrectMultiplier()
        {
            _bossHitbox.hitboxType = HitboxType.Head;
            float multiplier = _bossHitbox.GetDamageMultiplier();
            Assert.That(multiplier, Is.EqualTo(2.5f));
        }

        [Test]
        public void GetDamageMultiplier_Armor_ReturnsCorrectMultiplier()
        {
            _bossHitbox.hitboxType = HitboxType.Armor;
            float multiplier = _bossHitbox.GetDamageMultiplier();
            Assert.That(multiplier, Is.EqualTo(0.5f));
        }

        [Test]
        public void GetDamageMultiplier_Body_ReturnsCorrectMultiplier()
        {
            _bossHitbox.hitboxType = HitboxType.Body;
            float multiplier = _bossHitbox.GetDamageMultiplier();
            Assert.That(multiplier, Is.EqualTo(1.0f));
        }

        [Test]
        public void GetDamageMultiplier_Default_ReturnsCorrectMultiplier()
        {
            // Set to an undefined enum value
            _bossHitbox.hitboxType = (HitboxType)999;
            float multiplier = _bossHitbox.GetDamageMultiplier();
            Assert.That(multiplier, Is.EqualTo(1.0f));
        }
    }
}
