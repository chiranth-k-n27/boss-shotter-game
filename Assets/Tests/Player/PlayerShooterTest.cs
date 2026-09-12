using NUnit.Framework;
using UnityEngine;
using MobileShooter.Player;
using MobileShooter.Weapon;

namespace MobileShooter.Tests.Player
{
    [TestFixture]
    public class PlayerShooterTest
    {
        private GameObject playerGo;
        private PlayerShooter playerShooter;

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");

            // RequireComponent attributes don't automatically trigger in standard NUnit tests
            // without Unity's TestRunner overhead, but we can add what's necessary manually
            // if needed, or rely on GameObject.AddComponent directly if Unity's environment is present.
            playerGo.AddComponent<CharacterController>();
            playerShooter = playerGo.AddComponent<PlayerShooter>();
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null)
            {
                Object.DestroyImmediate(playerGo);
            }
        }

        [Test]
        public void DefaultMaxAmmo_Is30_WhenNoWeaponAssembler()
        {
            // Verify default behavior
            Assert.That(playerShooter.MaxAmmo, Is.EqualTo(30));
        }

        [Test]
        public void CurrentAmmo_DefaultsTo0_BeforeStart()
        {
            // Verify state before any lifecycle methods (like Start) are called to initialize ammo
            Assert.That(playerShooter.CurrentAmmo, Is.EqualTo(0));
        }
    }
}
