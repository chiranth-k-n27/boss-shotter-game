using UnityEngine;

namespace MobileShooter.Core
{
    public enum HitboxType
    {
        Body,
        Head,
        Armor
    }

    public interface IDamageable
    {
        void TakeDamage(float amount, HitboxType hitboxType, Vector3 hitPoint, Vector3 hitNormal);
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
    }

    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsFiring { get; }
        bool IsADS { get; }
        bool IsReloadingRequested { get; }
    }

    public interface IWeapon
    {
        void Fire();
        void StartADS();
        void StopADS();
        void Reload();
        int CurrentAmmo { get; }
        int MaxAmmo { get; }
        bool IsReloading { get; }
        float CurrentDamage { get; }
        float CurrentFireRate { get; }
    }

    public interface IAttachmentHolder
    {
        void EquipAttachment(AttachmentData attachment);
        void RemoveAttachment(AttachmentType type);
        AttachmentData GetEquippedAttachment(AttachmentType type);
    }

    public interface IBossAI
    {
        BossState CurrentState { get; }
        void ForceState(BossState newState);
    }

    public enum BossState
    {
        Idle,
        PatrolApproach,
        TelegraphedAttack,
        Phase2Enrage,
        Dead
    }
}
