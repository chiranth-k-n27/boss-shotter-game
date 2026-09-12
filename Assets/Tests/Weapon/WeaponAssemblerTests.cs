using NUnit.Framework;
using UnityEngine;
using MobileShooter.Core;

namespace MobileShooter.Tests
{
    [TestFixture]
    public class WeaponAssemblerTests
    {
        private WeaponData baseWeaponData;
        private WeaponAssembler weaponAssembler;

        [SetUp]
        public void Setup()
        {
            baseWeaponData = ScriptableObject.CreateInstance<WeaponData>();
            baseWeaponData.baseDamage = 25f;
            baseWeaponData.baseFireRate = 0.15f;
            baseWeaponData.baseRecoil = 0.5f;
            baseWeaponData.baseBulletVelocity = 100f;
            baseWeaponData.baseADSSpeed = 8.0f;
            baseWeaponData.baseMagazineSize = 30;

            var go = new GameObject("WeaponAssembler");
            weaponAssembler = go.AddComponent<WeaponAssembler>();
            weaponAssembler.baseWeaponData = baseWeaponData;
        }

        [Test]
        public void RecalculateStats_NoAttachments_ReturnsBaseStats()
        {
            // Act
            weaponAssembler.RecalculateStats();

            // Assert
            Assert.AreEqual(25f, weaponAssembler.EffectiveDamage, 0.001f);
            Assert.AreEqual(0.15f, weaponAssembler.EffectiveFireRate, 0.001f);
            Assert.AreEqual(0.5f, weaponAssembler.EffectiveRecoil, 0.001f);
            Assert.AreEqual(100f, weaponAssembler.EffectiveBulletVelocity, 0.001f);
            Assert.AreEqual(8.0f, weaponAssembler.EffectiveADSSpeed, 0.001f);
            Assert.AreEqual(30, weaponAssembler.EffectiveMaxAmmo);
        }

        [Test]
        public void RecalculateStats_WithDamageBuff_AppliesBuff()
        {
            // Arrange
            weaponAssembler.damageMultiplierBuff = 1.5f;

            // Act
            weaponAssembler.RecalculateStats();

            // Assert
            Assert.AreEqual(25f * 1.5f, weaponAssembler.EffectiveDamage, 0.001f);
        }

        [Test]
        public void RecalculateStats_WithSingleAttachment_CalculatesCorrectly()
        {
            // Arrange
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.25f; // +25% damage
            barrel.fireRateOffset = -0.05f;  // Faster fire rate

            weaponAssembler.EquipAttachment(barrel);

            // Act
            weaponAssembler.RecalculateStats();

            // Assert
            Assert.AreEqual(25f * 1.25f, weaponAssembler.EffectiveDamage, 0.001f);
            Assert.AreEqual(0.10f, weaponAssembler.EffectiveFireRate, 0.001f);
        }

        [Test]
        public void RecalculateStats_WithMultipleAttachments_CalculatesCorrectly()
        {
            // Arrange
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.2f;

            var magazine = ScriptableObject.CreateInstance<AttachmentData>();
            magazine.type = AttachmentType.Magazine;
            magazine.bonusMagazineCapacity = 10;
            magazine.adsSpeedMultiplier = 0.9f;

            var stock = ScriptableObject.CreateInstance<AttachmentData>();
            stock.type = AttachmentType.Stock;
            stock.recoilReduction = 0.4f;

            weaponAssembler.EquipAttachment(barrel);
            weaponAssembler.EquipAttachment(magazine);
            weaponAssembler.EquipAttachment(stock);

            // Act
            weaponAssembler.RecalculateStats();

            // Assert
            Assert.AreEqual(25f * 1.2f, weaponAssembler.EffectiveDamage, 0.001f);
            Assert.AreEqual(40, weaponAssembler.EffectiveMaxAmmo);
            Assert.AreEqual(8.0f * 0.9f, weaponAssembler.EffectiveADSSpeed, 0.001f);
            Assert.AreEqual(0.5f * (1.0f - 0.4f), weaponAssembler.EffectiveRecoil, 0.001f);
        }

        [Test]
        public void RecalculateStats_ValueClamping_PreventsInvalidStats()
        {
            // Arrange
            var extremeBarrel = ScriptableObject.CreateInstance<AttachmentData>();
            extremeBarrel.type = AttachmentType.Barrel;
            extremeBarrel.fireRateOffset = -0.2f; // Would make fire rate negative (0.15 - 0.2 = -0.05)
            extremeBarrel.recoilReduction = 1.5f; // Would make recoil multiplier negative (1.0 - 1.5 = -0.5)
            extremeBarrel.velocityMultiplier = 0.01f; // Very slow velocity

            weaponAssembler.EquipAttachment(extremeBarrel);

            // Act
            weaponAssembler.RecalculateStats();

            // Assert
            // FireRate clamped to 0.04f min
            Assert.AreEqual(0.04f, weaponAssembler.EffectiveFireRate, 0.001f);
            // Recoil reduction clamped to 1.0f (0.5 * (1.0 - 1.0) = 0), BUT EffectiveRecoil clamped to 0.05f min
            Assert.AreEqual(0.05f, weaponAssembler.EffectiveRecoil, 0.001f);
            // BulletVelocity clamped to 10f min
            Assert.AreEqual(10f, weaponAssembler.EffectiveBulletVelocity, 0.001f);
        }

        [Test]
        public void DynamicWeaponName_HeavyPlasma_GeneratedCorrectly()
        {
            // Arrange
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.3f; // > 1.2f

            weaponAssembler.EquipAttachment(barrel);

            // Act
            weaponAssembler.RecalculateStats();

            // Assert
            Assert.AreEqual("Heavy Plasma Carbine", weaponAssembler.DynamicWeaponName);
        }

        [Test]
        public void DynamicWeaponName_SniperCannonOverloaded_GeneratedCorrectly()
        {
            // Arrange
            var scope = ScriptableObject.CreateInstance<AttachmentData>();
            scope.type = AttachmentType.Scope;
            scope.adsSpeedMultiplier = 1.3f; // > 1.2f

            var mag = ScriptableObject.CreateInstance<AttachmentData>();
            mag.type = AttachmentType.Magazine;
            mag.bonusMagazineCapacity = 25; // >= 20

            weaponAssembler.EquipAttachment(scope);
            weaponAssembler.EquipAttachment(mag);

            // Act
            weaponAssembler.RecalculateStats();

            // Assert
            Assert.AreEqual("Sniper Cannon Overloaded", weaponAssembler.DynamicWeaponName);
        }
    }
}