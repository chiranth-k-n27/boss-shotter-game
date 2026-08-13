using UnityEngine;

namespace MobileShooter.Core
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "MobileShooter/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "Assault Rifle Prototype";

        [Header("Base Gun Stats")]
        public float baseDamage = 25f;
        public float baseFireRate = 0.15f;      // Seconds per shot
        public float baseRecoil = 0.5f;         // Camera kick back
        public float baseBulletVelocity = 100f;  // Speed
        public float baseADSSpeed = 8.0f;       // Transition lerp speed
        public float defaultZoomFOV = 40f;      // Camera FOV when aiming
        public int baseMagazineSize = 30;
        public float reloadTime = 2.0f;
    }
}
