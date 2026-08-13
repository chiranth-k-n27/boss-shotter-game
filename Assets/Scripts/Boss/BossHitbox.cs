using UnityEngine;
using MobileShooter.Core;

namespace MobileShooter.Boss
{
    public class BossHitbox : MonoBehaviour
    {
        public HitboxType hitboxType = HitboxType.Body;

        public float GetDamageMultiplier()
        {
            switch (hitboxType)
            {
                case HitboxType.Head:
                    return 2.5f; // Weak point
                case HitboxType.Armor:
                    return 0.5f; // Reduced damage
                case HitboxType.Body:
                default:
                    return 1.0f; // Normal damage
            }
        }
    }
}
