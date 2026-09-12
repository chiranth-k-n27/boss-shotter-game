using NUnit.Framework;
using MobileShooter.Core;
using UnityEngine;

namespace MobileShooter.Tests
{
    public class WeaponAssemblerTests
    {
        private WeaponAssembler _weaponAssembler;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject();
            _weaponAssembler = go.AddComponent<WeaponAssembler>();
            _weaponAssembler.baseWeaponData = ScriptableObject.CreateInstance<WeaponData>();
        }

        [Test]
        public void GenerateDynamicWeaponName_NoAttachments_ReturnsStandardName()
        {
            _weaponAssembler.RecalculateStats();
            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Carbine"));
        }

        [Test]
        public void GenerateDynamicWeaponName_BarrelHighDamage_ReturnsHeavyPlasma()
        {
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.3f; // > 1.2f

            _weaponAssembler.EquipAttachment(barrel);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Heavy Plasma Carbine"));
        }

        [Test]
        public void GenerateDynamicWeaponName_BarrelHighFireRate_ReturnsRapidCQB()
        {
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.0f;
            barrel.fireRateOffset = -0.1f; // < 0f

            _weaponAssembler.EquipAttachment(barrel);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Rapid CQB Carbine"));
        }

        [Test]
        public void GenerateDynamicWeaponName_BarrelHighVelocity_ReturnsHighVelocity()
        {
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.0f;
            barrel.fireRateOffset = 0f;
            barrel.velocityMultiplier = 1.4f; // > 1.3f

            _weaponAssembler.EquipAttachment(barrel);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("High-Velocity Carbine"));
        }

        [Test]
        public void GenerateDynamicWeaponName_BarrelDefault_ReturnsTactical()
        {
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.0f;
            barrel.fireRateOffset = 0f;
            barrel.velocityMultiplier = 1.0f;

            _weaponAssembler.EquipAttachment(barrel);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Tactical Carbine"));
        }

        [Test]
        public void GenerateDynamicWeaponName_ScopeHighADS_ReturnsSniperCannon()
        {
            var scope = ScriptableObject.CreateInstance<AttachmentData>();
            scope.type = AttachmentType.Scope;
            scope.adsSpeedMultiplier = 1.3f; // > 1.2f

            _weaponAssembler.EquipAttachment(scope);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Sniper Cannon"));
        }

        [Test]
        public void GenerateDynamicWeaponName_ScopeDefault_ReturnsBattleRifle()
        {
            var scope = ScriptableObject.CreateInstance<AttachmentData>();
            scope.type = AttachmentType.Scope;
            scope.adsSpeedMultiplier = 1.0f;

            _weaponAssembler.EquipAttachment(scope);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Battle Rifle"));
        }

        [Test]
        public void GenerateDynamicWeaponName_MagazineHighBonus_ReturnsOverloaded()
        {
            var mag = ScriptableObject.CreateInstance<AttachmentData>();
            mag.type = AttachmentType.Magazine;
            mag.bonusMagazineCapacity = 25; // >= 20

            _weaponAssembler.EquipAttachment(mag);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Carbine Overloaded"));
        }

        [Test]
        public void GenerateDynamicWeaponName_MagazineLowBonus_ReturnsExtended()
        {
            var mag = ScriptableObject.CreateInstance<AttachmentData>();
            mag.type = AttachmentType.Magazine;
            mag.bonusMagazineCapacity = 10; // > 0

            _weaponAssembler.EquipAttachment(mag);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Carbine Extended"));
        }

        [Test]
        public void GenerateDynamicWeaponName_AllAttachmentsCombination_ReturnsComplexName()
        {
            var barrel = ScriptableObject.CreateInstance<AttachmentData>();
            barrel.type = AttachmentType.Barrel;
            barrel.damageMultiplier = 1.5f; // Heavy Plasma

            var scope = ScriptableObject.CreateInstance<AttachmentData>();
            scope.type = AttachmentType.Scope;
            scope.adsSpeedMultiplier = 1.5f; // Sniper Cannon

            var mag = ScriptableObject.CreateInstance<AttachmentData>();
            mag.type = AttachmentType.Magazine;
            mag.bonusMagazineCapacity = 30; // Overloaded

            _weaponAssembler.EquipAttachment(barrel);
            _weaponAssembler.EquipAttachment(scope);
            _weaponAssembler.EquipAttachment(mag);

            Assert.That(_weaponAssembler.DynamicWeaponName, Is.EqualTo("Heavy Plasma Sniper Cannon Overloaded"));
        }
    }
}
