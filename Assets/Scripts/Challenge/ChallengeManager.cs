using System.Collections;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Boss;
using MobileShooter.Events;

namespace MobileShooter.Challenge
{
    public enum ChallengeType
    {
        HeadshotMaster,
        DodgeExpert,
        DPSRush
    }

    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance { get; private set; }

        [Header("Challenge Settings")]
        public float challengeInterval = 35.0f;
        private bool isChallengeActive;

        private ChallengeType activeChallenge;
        private int currentProgress;
        private int targetGoal;
        private float challengeTimeRemaining;
        private Coroutine challengeTimerCoroutine;

        public WeaponAssembler playerWeaponAssembler;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnBossTookDamage += HandleBossTookDamage;
            GameEvents.OnBossStateChanged += HandleBossStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnBossTookDamage -= HandleBossTookDamage;
            GameEvents.OnBossStateChanged -= HandleBossStateChanged;
        }

        private void Start()
        {
            StartCoroutine(ChallengeLoop());
        }

        private IEnumerator ChallengeLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(challengeInterval);

                if (!isChallengeActive)
                {
                    StartRandomChallenge();
                }
            }
        }

        private void StartRandomChallenge()
        {
            isChallengeActive = true;
            currentProgress = 0;
            activeChallenge = (ChallengeType)Random.Range(0, 3);

            string title = "";
            float duration = 12.0f;

            switch (activeChallenge)
            {
                case ChallengeType.HeadshotMaster:
                    targetGoal = 3;
                    title = "CHALLENGE: Land 3 Headshots!";
                    duration = 12f;
                    break;
                case ChallengeType.DodgeExpert:
                    targetGoal = 1;
                    title = "CHALLENGE: Survive Telegraphed Attack!";
                    duration = 15f;
                    break;
                case ChallengeType.DPSRush:
                    targetGoal = 300;
                    title = "CHALLENGE: Deal 300 Damage!";
                    duration = 10f;
                    break;
            }

            GameEvents.TriggerChallengeStarted(title, duration);
            GameEvents.TriggerChallengeProgressUpdated($"0 / {targetGoal}");

            if (challengeTimerCoroutine != null) StopCoroutine(challengeTimerCoroutine);
            challengeTimerCoroutine = StartCoroutine(ChallengeTimerRoutine(duration, title));
        }

        private IEnumerator ChallengeTimerRoutine(float duration, string title)
        {
            challengeTimeRemaining = duration;

            while (challengeTimeRemaining > 0f)
            {
                challengeTimeRemaining -= Time.deltaTime;
                yield return null;
            }

            if (isChallengeActive)
            {
                // Failed challenge
                isChallengeActive = false;
                GameEvents.TriggerChallengeFailed(title);
            }
        }

        private void HandleBossTookDamage(float damage, HitboxType type, Vector3 hitPos)
        {
            if (!isChallengeActive) return;

            if (activeChallenge == ChallengeType.HeadshotMaster && type == HitboxType.Head)
            {
                currentProgress++;
                GameEvents.TriggerChallengeProgressUpdated($"{currentProgress} / {targetGoal}");

                if (currentProgress >= targetGoal)
                {
                    CompleteChallenge("OVERDRIVE DAMAGE (+100% DMG for 12s)");
                }
            }
            else if (activeChallenge == ChallengeType.DPSRush)
            {
                currentProgress += Mathf.RoundToInt(damage);
                GameEvents.TriggerChallengeProgressUpdated($"{currentProgress} / {targetGoal}");

                if (currentProgress >= targetGoal)
                {
                    CompleteChallenge("INFINITE AMMO (10s)");
                }
            }
        }

        private void HandleBossStateChanged(BossState state)
        {
            if (!isChallengeActive) return;

            if (activeChallenge == ChallengeType.DodgeExpert && state == BossState.TelegraphedAttack)
            {
                // Check back after windup
                StartCoroutine(VerifyDodgeRoutine());
            }
        }

        private IEnumerator VerifyDodgeRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            if (isChallengeActive && activeChallenge == ChallengeType.DodgeExpert)
            {
                CompleteChallenge("ARMOR SHATTER (Boss Armor Stripped)");
            }
        }

        private void CompleteChallenge(string rewardName)
        {
            isChallengeActive = false;
            if (challengeTimerCoroutine != null) StopCoroutine(challengeTimerCoroutine);

            GameEvents.TriggerChallengeCompleted($"CHALLENGE COMPLETE! REWARD: {rewardName}");

            ApplyReward(rewardName);
        }

        private void ApplyReward(string rewardName)
        {
            if (rewardName.Contains("OVERDRIVE"))
            {
                StartCoroutine(OverdriveBuffRoutine(12f));
            }
            else if (rewardName.Contains("INFINITE"))
            {
                StartCoroutine(InfiniteAmmoBuffRoutine(10f));
            }
            else if (rewardName.Contains("ARMOR"))
            {
                ShatterBossArmor();
            }

            GameEvents.TriggerRewardGranted(rewardName, 12f);
        }

        private IEnumerator OverdriveBuffRoutine(float duration)
        {
            if (playerWeaponAssembler != null)
            {
                playerWeaponAssembler.damageMultiplierBuff = 2.0f; // 2x damage
                playerWeaponAssembler.RecalculateStats();

                yield return new WaitForSeconds(duration);

                playerWeaponAssembler.damageMultiplierBuff = 1.0f;
                playerWeaponAssembler.RecalculateStats();
            }
        }

        private IEnumerator InfiniteAmmoBuffRoutine(float duration)
        {
            if (playerWeaponAssembler != null)
            {
                playerWeaponAssembler.isInfiniteAmmoBuffActive = true;

                yield return new WaitForSeconds(duration);

                playerWeaponAssembler.isInfiniteAmmoBuffActive = false;
            }
        }

        private void ShatterBossArmor()
        {
            foreach (var h in BossHitbox.AllHitboxes)
            {
                if (h.hitboxType == HitboxType.Armor)
                {
                    h.hitboxType = HitboxType.Body; // Strip armor reduction
                }
            }
        }
    }
}
