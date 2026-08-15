using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using MobileShooter.Challenge;
using MobileShooter.Core;
using MobileShooter.Events;
using System;

namespace MobileShooter.Tests.Challenge
{
    public class ChallengeManagerTests
    {
        private GameObject challengeManagerObject;
        private ChallengeManager challengeManager;
        private MethodInfo handleBossTookDamageMethod;

        [SetUp]
        public void Setup()
        {
            challengeManagerObject = new GameObject("ChallengeManager");
            challengeManager = challengeManagerObject.AddComponent<ChallengeManager>();

            // Get private method HandleBossTookDamage
            handleBossTookDamageMethod = typeof(ChallengeManager).GetMethod(
                "HandleBossTookDamage",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [TearDown]
        public void Teardown()
        {
            if (challengeManagerObject != null)
            {
                UnityEngine.Object.DestroyImmediate(challengeManagerObject);
            }
        }

        private void SetPrivateField(string fieldName, object value)
        {
            FieldInfo field = typeof(ChallengeManager).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            field.SetValue(challengeManager, value);
        }

        private object GetPrivateField(string fieldName)
        {
            FieldInfo field = typeof(ChallengeManager).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            return field.GetValue(challengeManager);
        }

        private void InvokeHandleBossTookDamage(float damage, HitboxType type, Vector3 hitPos)
        {
            handleBossTookDamageMethod.Invoke(challengeManager, new object[] { damage, type, hitPos });
        }

        [Test]
        public void HandleBossTookDamage_InactiveChallenge_DoesNothing()
        {
            // Arrange
            SetPrivateField("isChallengeActive", false);
            SetPrivateField("activeChallenge", ChallengeType.HeadshotMaster);
            SetPrivateField("currentProgress", 0);

            bool eventFired = false;
            Action<string> listener = (msg) => eventFired = true;
            GameEvents.OnChallengeProgressUpdated += listener;

            try
            {
                // Act
                InvokeHandleBossTookDamage(10f, HitboxType.Head, Vector3.zero);

                // Assert
                Assert.AreEqual(0, GetPrivateField("currentProgress"), "Progress should not change when challenge is inactive");
                Assert.IsFalse(eventFired, "Event should not fire when challenge is inactive");
            }
            finally
            {
                // Cleanup
                GameEvents.OnChallengeProgressUpdated -= listener;
            }
        }

        [Test]
        public void HandleBossTookDamage_HeadshotMaster_IncrementsOnHeadshot()
        {
            // Arrange
            SetPrivateField("isChallengeActive", true);
            SetPrivateField("activeChallenge", ChallengeType.HeadshotMaster);
            SetPrivateField("currentProgress", 0);
            SetPrivateField("targetGoal", 3);

            string updatedProgressMsg = null;
            Action<string> listener = (msg) => updatedProgressMsg = msg;
            GameEvents.OnChallengeProgressUpdated += listener;

            try
            {
                // Act
                InvokeHandleBossTookDamage(10f, HitboxType.Head, Vector3.zero);

                // Assert
                Assert.AreEqual(1, GetPrivateField("currentProgress"), "Progress should increment on headshot");
                Assert.AreEqual("1 / 3", updatedProgressMsg, "Event should fire with correct progress string");
            }
            finally
            {
                // Cleanup
                GameEvents.OnChallengeProgressUpdated -= listener;
            }
        }

        [Test]
        public void HandleBossTookDamage_HeadshotMaster_IgnoresBodyShots()
        {
            // Arrange
            SetPrivateField("isChallengeActive", true);
            SetPrivateField("activeChallenge", ChallengeType.HeadshotMaster);
            SetPrivateField("currentProgress", 0);
            SetPrivateField("targetGoal", 3);

            // Act
            InvokeHandleBossTookDamage(10f, HitboxType.Body, Vector3.zero);

            // Assert
            Assert.AreEqual(0, GetPrivateField("currentProgress"), "Progress should not increment on body shot");
        }

        [Test]
        public void HandleBossTookDamage_HeadshotMaster_CompletesChallenge()
        {
            // Arrange
            SetPrivateField("isChallengeActive", true);
            SetPrivateField("activeChallenge", ChallengeType.HeadshotMaster);
            SetPrivateField("currentProgress", 2); // 1 away from target
            SetPrivateField("targetGoal", 3);

            string completedReward = null;
            Action<string> listener = (msg) => completedReward = msg;
            GameEvents.OnChallengeCompleted += listener;

            try
            {
                // Act
                InvokeHandleBossTookDamage(10f, HitboxType.Head, Vector3.zero);

                // Assert
                Assert.AreEqual(3, GetPrivateField("currentProgress"), "Progress should reach 3");
                Assert.IsFalse((bool)GetPrivateField("isChallengeActive"), "Challenge should be marked inactive upon completion");
                Assert.IsNotNull(completedReward, "Completion event should fire");
                Assert.IsTrue(completedReward.Contains("OVERDRIVE"), "Completion event should mention OVERDRIVE");
            }
            finally
            {
                // Cleanup
                GameEvents.OnChallengeCompleted -= listener;
            }
        }

        [Test]
        public void HandleBossTookDamage_DPSRush_AddsRoundedDamage()
        {
            // Arrange
            SetPrivateField("isChallengeActive", true);
            SetPrivateField("activeChallenge", ChallengeType.DPSRush);
            SetPrivateField("currentProgress", 0);
            SetPrivateField("targetGoal", 300);

            string updatedProgressMsg = null;
            Action<string> listener = (msg) => updatedProgressMsg = msg;
            GameEvents.OnChallengeProgressUpdated += listener;

            try
            {
                // Act
                InvokeHandleBossTookDamage(15.6f, HitboxType.Body, Vector3.zero);

                // Assert
                // Mathf.RoundToInt(15.6f) is 16
                Assert.AreEqual(16, GetPrivateField("currentProgress"), "Progress should increase by rounded damage");
                Assert.AreEqual("16 / 300", updatedProgressMsg, "Event should fire with correct progress string");
            }
            finally
            {
                // Cleanup
                GameEvents.OnChallengeProgressUpdated -= listener;
            }
        }

        [Test]
        public void HandleBossTookDamage_DPSRush_CompletesChallenge()
        {
            // Arrange
            SetPrivateField("isChallengeActive", true);
            SetPrivateField("activeChallenge", ChallengeType.DPSRush);
            SetPrivateField("currentProgress", 290);
            SetPrivateField("targetGoal", 300);

            string completedReward = null;
            Action<string> listener = (msg) => completedReward = msg;
            GameEvents.OnChallengeCompleted += listener;

            try
            {
                // Act
                InvokeHandleBossTookDamage(15.1f, HitboxType.Body, Vector3.zero); // Adds 15

                // Assert
                Assert.AreEqual(305, GetPrivateField("currentProgress"), "Progress should exceed target");
                Assert.IsFalse((bool)GetPrivateField("isChallengeActive"), "Challenge should be inactive");
                Assert.IsNotNull(completedReward, "Completion event should fire");
                Assert.IsTrue(completedReward.Contains("INFINITE AMMO"), "Completion event should mention INFINITE AMMO");
            }
            finally
            {
                // Cleanup
                GameEvents.OnChallengeCompleted -= listener;
            }
        }
    }
}
