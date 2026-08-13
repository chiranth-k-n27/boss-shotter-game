using UnityEngine;
using UnityEngine.UI;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Canvas Reference")]
        public Canvas hudCanvas;

        [Header("Player Ammo UI")]
        public Text ammoText;

        [Header("Boss Health UI")]
        public Slider bossHealthSlider;
        public Text bossHealthText;
        public Text bossPhaseText;

        [Header("Level Banner UI")]
        public Text levelBannerText;

        [Header("Challenge & Reward UI")]
        public GameObject challengePanel;
        public Text challengeTitleText;
        public Text challengeProgressText;
        public Text rewardNotificationText;

        [Header("Mobile Control Buttons")]
        public Button fireButton;
        public Button adsButton;
        public Button reloadButton;
        public Button craftingButton;

        private void OnEnable()
        {
            GameEvents.OnAmmoChanged += UpdateAmmoDisplay;
            GameEvents.OnBossHealthChanged += UpdateBossHealthDisplay;
            GameEvents.OnBossStateChanged += UpdateBossStateDisplay;
            GameEvents.OnBossTookDamage += HandleBossTookDamage;

            GameEvents.OnLevelChanged += UpdateLevelDisplay;
            GameEvents.OnLevelCleared += HandleLevelCleared;

            GameEvents.OnChallengeStarted += HandleChallengeStarted;
            GameEvents.OnChallengeProgressUpdated += HandleChallengeProgress;
            GameEvents.OnChallengeCompleted += HandleChallengeCompleted;
            GameEvents.OnChallengeFailed += HandleChallengeFailed;
            GameEvents.OnRewardGranted += HandleRewardGranted;
        }

        private void OnDisable()
        {
            GameEvents.OnAmmoChanged -= UpdateAmmoDisplay;
            GameEvents.OnBossHealthChanged -= UpdateBossHealthDisplay;
            GameEvents.OnBossStateChanged -= UpdateBossStateDisplay;
            GameEvents.OnBossTookDamage -= HandleBossTookDamage;

            GameEvents.OnLevelChanged -= UpdateLevelDisplay;
            GameEvents.OnLevelCleared -= HandleLevelCleared;

            GameEvents.OnChallengeStarted -= HandleChallengeStarted;
            GameEvents.OnChallengeProgressUpdated -= HandleChallengeProgress;
            GameEvents.OnChallengeCompleted -= HandleChallengeCompleted;
            GameEvents.OnChallengeFailed -= HandleChallengeFailed;
            GameEvents.OnRewardGranted -= HandleRewardGranted;
        }

        private void Start()
        {
            if (craftingButton != null)
            {
                craftingButton.onClick.AddListener(() => GameEvents.TriggerToggleCraftingUI(true));
            }

            if (challengePanel != null)
            {
                challengePanel.SetActive(false);
            }
        }

        private void UpdateAmmoDisplay(int current, int max)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{current} / {max}";
            }
        }

        private void UpdateBossHealthDisplay(float current, float max, BossState state)
        {
            if (bossHealthSlider != null)
            {
                bossHealthSlider.maxValue = max;
                bossHealthSlider.value = current;
            }

            if (bossHealthText != null)
            {
                bossHealthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)} HP";
            }
        }

        private void UpdateBossStateDisplay(BossState state)
        {
            if (bossPhaseText == null) return;

            switch (state)
            {
                case BossState.Phase2Enrage:
                    bossPhaseText.text = "PHASE 2: ENRAGED!";
                    bossPhaseText.color = Color.red;
                    break;
                case BossState.TelegraphedAttack:
                    bossPhaseText.text = "WARNING: INCOMING ATTACK!";
                    bossPhaseText.color = Color.yellow;
                    break;
                case BossState.Dead:
                    bossPhaseText.text = "BOSS DEFEATED!";
                    bossPhaseText.color = Color.green;
                    break;
                default:
                    bossPhaseText.text = "PHASE 1: PATROL";
                    bossPhaseText.color = Color.white;
                    break;
            }
        }

        private void UpdateLevelDisplay(int level)
        {
            if (levelBannerText != null)
            {
                levelBannerText.text = $"LEVEL {level}";
                levelBannerText.color = Color.cyan;
            }
        }

        private void HandleLevelCleared(int level)
        {
            if (levelBannerText != null)
            {
                levelBannerText.text = $"LEVEL {level} CLEARED!";
                levelBannerText.color = Color.green;
            }
        }

        private void HandleChallengeStarted(string desc, float duration)
        {
            if (challengePanel != null) challengePanel.SetActive(true);
            if (challengeTitleText != null) challengeTitleText.text = desc;
        }

        private void HandleChallengeProgress(string progress)
        {
            if (challengeProgressText != null) challengeProgressText.text = progress;
        }

        private void HandleChallengeCompleted(string title)
        {
            if (challengeTitleText != null) challengeTitleText.text = title;
            if (challengePanel != null) StartCoroutine(HideChallengePanelRoutine(3f));
        }

        private void HandleChallengeFailed(string title)
        {
            if (challengeTitleText != null) challengeTitleText.text = "CHALLENGE FAILED!";
            if (challengePanel != null) StartCoroutine(HideChallengePanelRoutine(2f));
        }

        private System.Collections.IEnumerator HideChallengePanelRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (challengePanel != null) challengePanel.SetActive(false);
        }

        private void HandleRewardGranted(string rewardName, float duration)
        {
            if (rewardNotificationText != null)
            {
                rewardNotificationText.text = $"BUFF ACTIVE: {rewardName}";
                StartCoroutine(ClearRewardNotificationRoutine(duration));
            }
        }

        private System.Collections.IEnumerator ClearRewardNotificationRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (rewardNotificationText != null) rewardNotificationText.text = "";
        }

        private void HandleBossTookDamage(float damage, HitboxType hitboxType, Vector3 hitPos)
        {
            FloatingDamageNumber.Spawn(damage, hitboxType, hitPos, hudCanvas);
        }
    }
}
