using NUnit.Framework;
using UnityEngine;
using MobileShooter.Core;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace MobileShooter.Tests
{
    [TestFixture]
    public class WeaponAssemblerTest
    {
        private GameObject go;
        private WeaponAssembler assembler;

        [SetUp]
        public void Setup()
        {
            go = new GameObject();
            assembler = go.AddComponent<WeaponAssembler>();

            // Set base weapon data
            assembler.baseWeaponData = ScriptableObject.CreateInstance<WeaponData>();
            assembler.baseWeaponData.baseDamage = 25f;
            assembler.baseWeaponData.baseFireRate = 0.15f;
            assembler.baseWeaponData.baseRecoil = 0.5f;
            assembler.baseWeaponData.baseBulletVelocity = 100f;
            assembler.baseWeaponData.baseADSSpeed = 8.0f;
            assembler.baseWeaponData.baseMagazineSize = 30;

            // Awake usually calls RecalculateStats, we'll just call it explicitly
            assembler.RecalculateStats();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RecalculateStats_WithNoAttachments_ReturnsBaseStats()
        {
            Assert.AreEqual(25f, assembler.EffectiveDamage);
            Assert.AreEqual(0.15f, assembler.EffectiveFireRate);
            Assert.AreEqual(0.5f, assembler.EffectiveRecoil);
            Assert.AreEqual(100f, assembler.EffectiveBulletVelocity);
            Assert.AreEqual(8.0f, assembler.EffectiveADSSpeed);
            Assert.AreEqual(30, assembler.EffectiveMaxAmmo);
        }

        [Test]
        public void RecalculateStats_WithDamageBuff_AppliesCorrectly()
        {
            assembler.damageMultiplierBuff = 2.0f;
            assembler.RecalculateStats();

            Assert.AreEqual(50f, assembler.EffectiveDamage);
        }

        [Test]
        public void RecalculateStats_WithAttachments_AggregatesCorrectly()
        {
            // Barrel: +20% damage, -0.05s fire rate
            AttachmentData barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.2f;
            barrel.fireRateOffset = -0.05f;

            // Scope: 30% recoil reduction, 1.5x ADS speed
            AttachmentData scope = ScriptableObject.CreateInstance<AttachmentData>();
            scope.type = AttachmentType.Scope;
            scope.recoilReduction = 0.3f;
            scope.adsSpeedMultiplier = 1.5f;

            // Magazine: +10 capacity, 0.9x velocity
            AttachmentData mag = ScriptableObject.CreateInstance<AttachmentData>();
            mag.type = AttachmentType.Magazine;
            mag.bonusMagazineCapacity = 10;
            mag.velocityMultiplier = 0.9f;

            assembler.EquipAttachment(barrel);
            assembler.EquipAttachment(scope);
            assembler.EquipAttachment(mag);

            // Verify EffectiveStats
            Assert.AreEqual(25f * 1.2f, assembler.EffectiveDamage);
            Assert.AreEqual(0.15f - 0.05f, assembler.EffectiveFireRate);
            Assert.AreEqual(0.5f * (1.0f - 0.3f), assembler.EffectiveRecoil);
            Assert.AreEqual(100f * 0.9f, assembler.EffectiveBulletVelocity);
            Assert.AreEqual(8.0f * 1.5f, assembler.EffectiveADSSpeed);
            Assert.AreEqual(30 + 10, assembler.EffectiveMaxAmmo);
        }

        [Test]
        public void RecalculateStats_ClampValues_Correctly()
        {
            // Attachments to push below mins
            AttachmentData barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 0.01f; // Expected min is 1.0f
            barrel.fireRateOffset = -1.0f; // Expected min is 0.04f
            barrel.recoilReduction = 2.0f; // Expected max recoil reduction is 1.0f (recoil multiplier 0.0) -> Effective Recoil min 0.05f
            barrel.velocityMultiplier = 0.01f; // Expected min is 10f
            barrel.adsSpeedMultiplier = 0.01f; // Expected min is 1.0f
            barrel.bonusMagazineCapacity = -50; // Expected min is 1

            assembler.EquipAttachment(barrel);

            Assert.AreEqual(1.0f, assembler.EffectiveDamage);
            Assert.AreEqual(0.04f, assembler.EffectiveFireRate);
            Assert.AreEqual(0.05f, assembler.EffectiveRecoil);
            Assert.AreEqual(10f, assembler.EffectiveBulletVelocity);
            Assert.AreEqual(1.0f, assembler.EffectiveADSSpeed);
            Assert.AreEqual(1, assembler.EffectiveMaxAmmo);
        }
    }
}
