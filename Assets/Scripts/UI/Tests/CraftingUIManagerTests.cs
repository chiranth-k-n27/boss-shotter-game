using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using MobileShooter.Core;
using MobileShooter.UI;

namespace MobileShooter.UI.Tests
{
    [TestFixture]
    public class CraftingUIManagerTests
    {
        private GameObject uiManagerObject;
        private CraftingUIManager craftingUIManager;

        [SetUp]
        public void SetUp()
        {
            uiManagerObject = new GameObject("CraftingUIManager");
            craftingUIManager = uiManagerObject.AddComponent<CraftingUIManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (uiManagerObject != null)
            {
                Object.DestroyImmediate(uiManagerObject);
            }
        }

        [Test]
        public void RefreshStatBars_NullAssembler_DoesNotThrow()
        {
            craftingUIManager.targetWeaponAssembler = null;

            // This should return early without throwing any exceptions
            Assert.DoesNotThrow(() => craftingUIManager.RefreshStatBars());
        }

        [Test]
        public void RefreshStatBars_MissingUIElements_DoesNotThrow()
        {
            GameObject assemblerObject = new GameObject("WeaponAssembler");
            WeaponAssembler assembler = assemblerObject.AddComponent<WeaponAssembler>();
            craftingUIManager.targetWeaponAssembler = assembler;

            // Leave all UI elements (Text, Slider) as null
            Assert.DoesNotThrow(() => craftingUIManager.RefreshStatBars());

            Object.DestroyImmediate(assemblerObject);
        }

        [Test]
        public void RefreshStatBars_UpdatesTextElements_WithCalculatedStats()
        {
            // Setup assembler
            GameObject assemblerObject = new GameObject("WeaponAssembler");
            WeaponAssembler assembler = assemblerObject.AddComponent<WeaponAssembler>();

            // Set base weapon data
            assembler.baseWeaponData = ScriptableObject.CreateInstance<WeaponData>();
            assembler.baseWeaponData.baseDamage = 50f;
            assembler.baseWeaponData.baseFireRate = 0.2f;
            assembler.baseWeaponData.baseRecoil = 0.8f;
            assembler.baseWeaponData.baseBulletVelocity = 150f;
            assembler.baseWeaponData.baseADSSpeed = 10f;
            assembler.baseWeaponData.baseMagazineSize = 25;

            assembler.RecalculateStats();
            craftingUIManager.targetWeaponAssembler = assembler;

            // Setup UI Elements
            GameObject uiCanvas = new GameObject("UICanvas");

            craftingUIManager.weaponTitleText = new GameObject("WeaponTitle").AddComponent<Text>();
            craftingUIManager.damageText = new GameObject("DamageText").AddComponent<Text>();
            craftingUIManager.fireRateText = new GameObject("FireRateText").AddComponent<Text>();
            craftingUIManager.recoilText = new GameObject("RecoilText").AddComponent<Text>();
            craftingUIManager.velocityText = new GameObject("VelocityText").AddComponent<Text>();
            craftingUIManager.adsSpeedText = new GameObject("ADSSpeedText").AddComponent<Text>();
            craftingUIManager.ammoCapacityText = new GameObject("AmmoText").AddComponent<Text>();

            // Act
            craftingUIManager.RefreshStatBars();

            // Assert Text Output
            Assert.AreEqual("WEAPON: STANDARD PROTOTYPE", craftingUIManager.weaponTitleText.text);
            Assert.AreEqual("Damage: 50", craftingUIManager.damageText.text);
            Assert.AreEqual("Fire Rate: 5.0/s", craftingUIManager.fireRateText.text);
            Assert.AreEqual("Recoil: 0.80", craftingUIManager.recoilText.text);
            Assert.AreEqual("Velocity: 150m/s", craftingUIManager.velocityText.text);
            Assert.AreEqual("ADS Speed: 10.0", craftingUIManager.adsSpeedText.text);
            Assert.AreEqual("Ammo Cap: 25", craftingUIManager.ammoCapacityText.text);

            // Cleanup
            Object.DestroyImmediate(assemblerObject);
            Object.DestroyImmediate(uiCanvas);
            Object.DestroyImmediate(craftingUIManager.weaponTitleText.gameObject);
            Object.DestroyImmediate(craftingUIManager.damageText.gameObject);
            Object.DestroyImmediate(craftingUIManager.fireRateText.gameObject);
            Object.DestroyImmediate(craftingUIManager.recoilText.gameObject);
            Object.DestroyImmediate(craftingUIManager.velocityText.gameObject);
            Object.DestroyImmediate(craftingUIManager.adsSpeedText.gameObject);
            Object.DestroyImmediate(craftingUIManager.ammoCapacityText.gameObject);
        }

        [Test]
        public void RefreshStatBars_UpdatesSliderValues_ClampedCorrectly()
        {
            // Setup assembler
            GameObject assemblerObject = new GameObject("WeaponAssembler");
            WeaponAssembler assembler = assemblerObject.AddComponent<WeaponAssembler>();

            // Set base weapon data with extreme values to test clamping
            assembler.baseWeaponData = ScriptableObject.CreateInstance<WeaponData>();

            // High Values
            assembler.baseWeaponData.baseDamage = 200f; // > 120
            assembler.baseWeaponData.baseFireRate = -0.1f; // (0.5 - (-0.1)) / 0.45 > 1
            assembler.baseWeaponData.baseRecoil = -0.5f; // 1.0 - (-0.5)/1.0 = 1.5 > 1
            assembler.baseWeaponData.baseBulletVelocity = 500f; // > 300
            assembler.baseWeaponData.baseADSSpeed = 40f; // > 20

            assembler.RecalculateStats();
            craftingUIManager.targetWeaponAssembler = assembler;

            // Setup Sliders
            craftingUIManager.damageBar = new GameObject("DamageBar").AddComponent<Slider>();
            craftingUIManager.fireRateBar = new GameObject("FireRateBar").AddComponent<Slider>();
            craftingUIManager.recoilBar = new GameObject("RecoilBar").AddComponent<Slider>();
            craftingUIManager.velocityBar = new GameObject("VelocityBar").AddComponent<Slider>();
            craftingUIManager.adsSpeedBar = new GameObject("ADSSpeedBar").AddComponent<Slider>();

            // Act - High Values
            craftingUIManager.RefreshStatBars();

            // Assert High Values (Should clamp to 1.0)
            Assert.AreEqual(1.0f, craftingUIManager.damageBar.value);
            Assert.AreEqual(1.0f, craftingUIManager.fireRateBar.value);
            Assert.AreEqual(1.0f, craftingUIManager.recoilBar.value);
            Assert.AreEqual(1.0f, craftingUIManager.velocityBar.value);
            Assert.AreEqual(1.0f, craftingUIManager.adsSpeedBar.value);

            // Low Values
            assembler.baseWeaponData.baseDamage = -10f; // < 0
            assembler.baseWeaponData.baseFireRate = 1.0f; // (0.5 - 1.0) / 0.45 < 0
            assembler.baseWeaponData.baseRecoil = 2.0f; // 1.0 - 2.0/1.0 = -1.0 < 0
            assembler.baseWeaponData.baseBulletVelocity = -50f; // < 0
            assembler.baseWeaponData.baseADSSpeed = -5f; // < 0

            assembler.RecalculateStats();
            craftingUIManager.RefreshStatBars();

            // Assert Low Values (Should clamp to 0.0)
            // Note: RecalculateStats has its own Mathf.Max logic that might affect these,
            // e.g. baseFireRate has Mathf.Max(0.04f, ...), baseDamage has Mathf.Max(1.0f, ...).
            // We need to check effective stats first to know exact expected slider values,
            // or just ensure they are between 0 and 1, specifically 0 if effective goes low enough.
            // EffectiveDamage is Min 1.0 -> 1.0 / 120 = 0.00833
            // EffectiveFireRate Min 0.04 -> (0.5 - 0.04) / 0.45 = 1.02 -> 1.0! Wait, if baseFireRate=1.0 -> Effective=1.0 -> (0.5 - 1.0)/0.45 = -1.11 -> clamp 0
            // EffectiveRecoil Min 0.05 -> 1.0 - 0.05/1.0 = 0.95 -> 0.95! Wait, if baseRecoil=2.0 -> Effective=2.0 -> 1.0 - 2.0/1.0 = -1.0 -> clamp 0
            // EffectiveVelocity Min 10.0 -> 10.0 / 300 = 0.0333
            // EffectiveADSSpeed Min 1.0 -> 1.0 / 20 = 0.05

            Assert.AreEqual(Mathf.Clamp01(assembler.EffectiveDamage / 120f), craftingUIManager.damageBar.value, 0.001f);
            Assert.AreEqual(Mathf.Clamp01((0.5f - assembler.EffectiveFireRate) / 0.45f), craftingUIManager.fireRateBar.value, 0.001f);
            Assert.AreEqual(Mathf.Clamp01(1.0f - assembler.EffectiveRecoil / 1.0f), craftingUIManager.recoilBar.value, 0.001f);
            Assert.AreEqual(Mathf.Clamp01(assembler.EffectiveBulletVelocity / 300f), craftingUIManager.velocityBar.value, 0.001f);
            Assert.AreEqual(Mathf.Clamp01(assembler.EffectiveADSSpeed / 20f), craftingUIManager.adsSpeedBar.value, 0.001f);

            // Cleanup
            Object.DestroyImmediate(assemblerObject);
            Object.DestroyImmediate(craftingUIManager.damageBar.gameObject);
            Object.DestroyImmediate(craftingUIManager.fireRateBar.gameObject);
            Object.DestroyImmediate(craftingUIManager.recoilBar.gameObject);
            Object.DestroyImmediate(craftingUIManager.velocityBar.gameObject);
            Object.DestroyImmediate(craftingUIManager.adsSpeedBar.gameObject);
        }
    }
}
