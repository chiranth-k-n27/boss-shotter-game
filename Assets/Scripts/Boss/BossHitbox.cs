using UnityEngine;
using MobileShooter.Core;

namespace MobileShooter.Boss
{
    using System.Collections.Generic;

    public class BossHitbox : MonoBehaviour
    {
        public static readonly List<BossHitbox> ActiveHitboxes = new List<BossHitbox>();

        public HitboxType hitboxType = HitboxType.Body;

        private void OnEnable()
        {
            if (!ActiveHitboxes.Contains(this))
            {
                ActiveHitboxes.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveHitboxes.Remove(this);
        }

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
