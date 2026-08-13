using System;
using UnityEngine;
using MobileShooter.Core;

namespace MobileShooter.Events
{
    public static class GameEvents
    {
        // Weapon Events
        public static event Action<int, int> OnAmmoChanged; // currentAmmo, maxAmmo
        public static event Action OnWeaponFired;
        public static event Action OnWeaponReloaded;
        public static event Action OnWeaponStatsUpdated;

        // Boss Events
        public static event Action<float, float, BossState> OnBossHealthChanged; // currentHP, maxHP, currentState
        public static event Action<float, HitboxType, Vector3> OnBossTookDamage; // damage, hitboxType, hitPosition
        public static event Action<BossState> OnBossStateChanged;

        // Level & Progression Events
        public static event Action<int> OnLevelChanged; // current level number
        public static event Action<int> OnLevelCleared;

        // Challenge & Reward Events
        public static event Action<string, float> OnChallengeStarted; // challengeDescription, duration
        public static event Action<string> OnChallengeProgressUpdated; // e.g. "2 / 3 Headshots"
        public static event Action<string> OnChallengeCompleted; // challengeTitle
        public static event Action<string> OnChallengeFailed;
        public static event Action<string, float> OnRewardGranted; // rewardName, duration

        // UI Events
        public static event Action<bool> OnToggleCraftingUI;

        // Event Invokers
        public static void TriggerAmmoChanged(int current, int max) => OnAmmoChanged?.Invoke(current, max);
        public static void TriggerWeaponFired() => OnWeaponFired?.Invoke();
        public static void TriggerWeaponReloaded() => OnWeaponReloaded?.Invoke();
        public static void TriggerWeaponStatsUpdated() => OnWeaponStatsUpdated?.Invoke();

        public static void TriggerBossHealthChanged(float current, float max, BossState state) => OnBossHealthChanged?.Invoke(current, max, state);
        public static void TriggerBossTookDamage(float damage, HitboxType type, Vector3 pos) => OnBossTookDamage?.Invoke(damage, type, pos);
        public static void TriggerBossStateChanged(BossState newState) => OnBossStateChanged?.Invoke(newState);

        public static void TriggerLevelChanged(int level) => OnLevelChanged?.Invoke(level);
        public static void TriggerLevelCleared(int level) => OnLevelCleared?.Invoke(level);

        public static void TriggerChallengeStarted(string desc, float duration) => OnChallengeStarted?.Invoke(desc, duration);
        public static void TriggerChallengeProgressUpdated(string progress) => OnChallengeProgressUpdated?.Invoke(progress);
        public static void TriggerChallengeCompleted(string title) => OnChallengeCompleted?.Invoke(title);
        public static void TriggerChallengeFailed(string title) => OnChallengeFailed?.Invoke(title);
        public static void TriggerRewardGranted(string rewardName, float duration) => OnRewardGranted?.Invoke(rewardName, duration);

        public static void TriggerToggleCraftingUI(bool open) => OnToggleCraftingUI?.Invoke(open);
    }
}
