using System.Collections;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.Boss
{
    public class BossHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        public float maxHealth = 1000f;
        public float currentHealth;

        [Header("Visual Feedback")]
        public Renderer[] bossRenderers;
        public Color defaultColor = Color.magenta;
        public Color flashColor = Color.white;
        public Color enrageColor = new Color(1f, 0.2f, 0.2f);

        private bool isDead;
        private bool isEnraged;
        private Coroutine flashCoroutine;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Start()
        {
            currentHealth = maxHealth;
            if (bossRenderers == null || bossRenderers.Length == 0)
            {
                bossRenderers = GetComponentsInChildren<Renderer>();
            }
            SetBossBaseColor(defaultColor);
            GameEvents.TriggerBossHealthChanged(currentHealth, maxHealth, BossState.Idle);
        }

        public void TakeDamage(float rawDamage, HitboxType hitboxType, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (isDead) return;

            // Calculate multiplier from hitbox
            float multiplier = 1.0f;
            switch (hitboxType)
            {
                case HitboxType.Head: multiplier = 2.5f; break;
                case HitboxType.Armor: multiplier = 0.5f; break;
                case HitboxType.Body: multiplier = 1.0f; break;
            }

            float finalDamage = rawDamage * multiplier;
            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

            // Trigger events for UI damage numbers & HUD
            GameEvents.TriggerBossTookDamage(finalDamage, hitboxType, hitPoint);
            
            BossState currentState = isEnraged ? BossState.Phase2Enrage : BossState.PatrolApproach;
            GameEvents.TriggerBossHealthChanged(currentHealth, maxHealth, currentState);

            // Hit Flash Visual Feedback
            TriggerHitFlash();

            // Check Phase 2 Enrage Transition (at 50% HP)
            if (!isEnraged && currentHealth <= maxHealth * 0.5f && currentHealth > 0)
            {
                TriggerEnragePhase();
            }

            // Check Death
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void TriggerHitFlash()
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            Color baseCol = isEnraged ? enrageColor : defaultColor;
            SetBossBaseColor(flashColor);
            yield return new WaitForSeconds(0.08f);
            SetBossBaseColor(baseCol);
        }

        private void TriggerEnragePhase()
        {
            isEnraged = true;
            SetBossBaseColor(enrageColor);
            
            BossAIController ai = GetComponent<BossAIController>();
            if (ai != null)
            {
                ai.ForceState(BossState.Phase2Enrage);
            }

            GameEvents.TriggerBossStateChanged(BossState.Phase2Enrage);
        }

        private void Die()
        {
            isDead = true;
            GameEvents.TriggerBossStateChanged(BossState.Dead);
            gameObject.SetActive(false);
        }

        public void SetBossBaseColor(Color col)
        {
            if (bossRenderers == null) return;
            foreach (Renderer ren in bossRenderers)
            {
                if (ren != null && ren.material != null)
                {
                    ren.material.color = col;
                }
            }
        }
    }
}
