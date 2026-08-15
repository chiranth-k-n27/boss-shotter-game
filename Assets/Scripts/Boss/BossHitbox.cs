using System.Collections.Generic;
using UnityEngine;
using MobileShooter.Core;

namespace MobileShooter.Boss
{
    public class BossHitbox : MonoBehaviour
    {
        public static readonly List<BossHitbox> AllHitboxes = new List<BossHitbox>();

        public HitboxType hitboxType = HitboxType.Body;

        private void OnEnable()
        {
            AllHitboxes.Add(this);
        }

        private void OnDisable()
        {
            AllHitboxes.Remove(this);
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
