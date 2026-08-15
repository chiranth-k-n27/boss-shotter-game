using System.Collections;
using UnityEngine;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.Boss
{
    public class BossAIController : MonoBehaviour, IBossAI
    {
        [Header("Target & Movement")]
        public Transform targetPlayer;
        public float baseMoveSpeed = 3.5f;
        public float attackRange = 5.0f;
        public float attackCooldown = 4.0f;

        [Header("Telegraph Visual Indicator")]
        public Transform attackIndicator;

        [Header("State Machine")]
        [SerializeField] private BossState currentState = BossState.Idle;

        private float currentMoveSpeed;
        private float currentAttackCooldown;
        private float lastAttackTime;
        private bool isAttacking;

        private BossHealth bossHealth;

        public BossState CurrentState => currentState;

        private void Awake()
        {
            bossHealth = GetComponent<BossHealth>();
        }

        private void Start()
        {
            currentMoveSpeed = baseMoveSpeed;
            currentAttackCooldown = attackCooldown;

            if (targetPlayer == null)
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) targetPlayer = p.transform;
            }

            TransitionToState(BossState.PatrolApproach);
        }

        private void Update()
        {
            if (bossHealth != null && bossHealth.IsDead) return;
            if (targetPlayer == null) return;

            switch (currentState)
            {
                case BossState.Idle:
                    // Idle logic or transition to approach
                    TransitionToState(BossState.PatrolApproach);
                    break;

                case BossState.PatrolApproach:
                case BossState.Phase2Enrage:
                    HandleApproachAndCombatLoop();
                    break;

                case BossState.TelegraphedAttack:
                    // Handled inside attack routine
                    break;
            }
        }

        private void HandleApproachAndCombatLoop()
        {
            if (isAttacking) return;

            float distance = Vector3.Distance(transform.position, targetPlayer.position);

            // Rotate towards player smoothly
            Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
            }

            // Move towards player if outside attack range
            if (distance > attackRange)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, currentMoveSpeed * Time.deltaTime);
            }
            else
            {
                // In range -> Perform telegraphed attack if cooldown ready
                if (Time.time >= lastAttackTime + currentAttackCooldown)
                {
                    StartCoroutine(TelegraphedAttackRoutine());
                }
            }
        }

        private IEnumerator TelegraphedAttackRoutine()
        {
            isAttacking = true;
            BossState previousState = currentState;
            TransitionToState(BossState.TelegraphedAttack);

            // Enable visual telegraph indicator (e.g. growing red cylinder/ring)
            if (attackIndicator != null)
            {
                attackIndicator.gameObject.SetActive(true);
                attackIndicator.transform.localScale = Vector3.zero;
            }

            // Wind-Up Phase (Telegraph tell)
            float windupTime = currentState == BossState.Phase2Enrage ? 1.0f : 1.8f;
            float elapsed = 0f;

            while (elapsed < windupTime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / windupTime;

                if (attackIndicator != null)
                {
                    float scale = Mathf.Lerp(0f, attackRange * 2f, progress);
                    attackIndicator.transform.localScale = new Vector3(scale, 0.1f, scale);
                }
                yield return null;
            }

            // Execute Attack Slam / Blast
            ExecuteBossSlam();

            if (attackIndicator != null)
            {
                attackIndicator.gameObject.SetActive(false);
            }

            lastAttackTime = Time.time;
            isAttacking = false;

            TransitionToState(previousState == BossState.Phase2Enrage ? BossState.Phase2Enrage : BossState.PatrolApproach);
        }

        private void ExecuteBossSlam()
        {
            // Calculate distance to player
            if (targetPlayer != null)
            {
                float dist = Vector3.Distance(transform.position, targetPlayer.position);
                if (dist <= attackRange * 1.2f)
                {
                    // Deal damage to player if player has damage receiver
                    IDamageable playerDamageable = targetPlayer.GetComponent<IDamageable>();
                    if (playerDamageable != null)
                    {
                        playerDamageable.TakeDamage(25f, HitboxType.Body, targetPlayer.position, Vector3.up);
                    }
                }
            }
        }

        public void ForceState(BossState newState)
        {
            TransitionToState(newState);
        }

        private void TransitionToState(BossState newState)
        {
            currentState = newState;

            if (newState == BossState.Phase2Enrage)
            {
                currentMoveSpeed = baseMoveSpeed * 1.6f;
                currentAttackCooldown = attackCooldown * 0.5f;
            }

            GameEvents.TriggerBossStateChanged(newState);
        }
    }
}
