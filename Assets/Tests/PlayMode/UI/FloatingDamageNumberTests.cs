using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using MobileShooter.UI;
using MobileShooter.Core;

namespace MobileShooter.Tests.UI
{
    public class FloatingDamageNumberTests
    {
        [Test]
        public void Spawn_WithNullCanvas_ReturnsImmediately()
        {
            // Arrange
            Canvas parentCanvas = null;
            Vector3 position = Vector3.zero;

            // Act & Assert
            // This tests the edge case in FloatingDamageNumber.Spawn: if (parentCanvas == null) return;
            Assert.DoesNotThrow(() =>
            {
                FloatingDamageNumber.Spawn(10f, HitboxType.Body, position, parentCanvas);
            });
        }
    }
}
