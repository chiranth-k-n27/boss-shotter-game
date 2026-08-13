using System.Collections;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Boss;
using MobileShooter.Events;

namespace MobileShooter.Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level State")]
        public int currentLevel = 1;
        public float baseBossHealth = 800f;
        public float baseBossSpeed = 3.5f;

        [Header("References")]
        public GameObject bossPrefabReference;
        public Transform playerTransform;

        private GameObject activeBossInstance;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            GameEvents.OnBossStateChanged += HandleBossStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnBossStateChanged -= HandleBossStateChanged;
        }

        private void Start()
        {
            GameEvents.TriggerLevelChanged(currentLevel);
        }

        public void RegisterActiveBoss(GameObject bossObj)
        {
            activeBossInstance = bossObj;
        }

        private void HandleBossStateChanged(BossState state)
        {
            if (state == BossState.Dead)
            {
                StartCoroutine(LevelClearedRoutine());
            }
        }

        private IEnumerator LevelClearedRoutine()
        {
            GameEvents.TriggerLevelCleared(currentLevel);

            yield return new WaitForSeconds(3.0f);

            currentLevel++;
            GameEvents.TriggerLevelChanged(currentLevel);

            RespawnScaledBoss();
        }

        private void RespawnScaledBoss()
        {
            if (activeBossInstance != null)
            {
                // Reset Boss Position
                activeBossInstance.transform.position = new Vector3(0f, 0f, 14f);
                activeBossInstance.transform.rotation = Quaternion.identity;
                activeBossInstance.SetActive(true);

                // Calculate Scaled Stats for Level
                float scaledHP = baseBossHealth * (1.0f + (currentLevel - 1) * 0.45f);
                float scaledSpeed = baseBossSpeed * (1.0f + (currentLevel - 1) * 0.10f);

                BossHealth health = activeBossInstance.GetComponent<BossHealth>();
                if (health != null)
                {
                    health.maxHealth = scaledHP;
                    health.currentHealth = scaledHP;
                    health.SetBossBaseColor(Color.magenta);
                }

                BossAIController ai = activeBossInstance.GetComponent<BossAIController>();
                if (ai != null)
                {
                    ai.baseMoveSpeed = scaledSpeed;
                    ai.ForceState(BossState.PatrolApproach);
                }

                GameEvents.TriggerBossHealthChanged(scaledHP, scaledHP, BossState.PatrolApproach);
            }
        }
    }
}
