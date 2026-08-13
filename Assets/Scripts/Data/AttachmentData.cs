using UnityEngine;

namespace MobileShooter.Core
{
    public enum AttachmentType
    {
        Barrel,
        Scope,
        Stock,
        Magazine
    }

    public enum PrimitiveShape
    {
        Cube,
        Cylinder,
        Sphere
    }

    [CreateAssetMenu(fileName = "NewAttachment", menuName = "MobileShooter/Attachment Data")]
    public class AttachmentData : ScriptableObject
    {
        [Header("Identity")]
        public string attachmentName = "Standard Attachment";
        public AttachmentType type;
        public string description = "Modifies weapon performance.";

        [Header("Stat Modifiers")]
        public float damageMultiplier = 1.0f;     // e.g. 1.25 for +25% damage
        public float fireRateOffset = 0.0f;       // e.g. -0.05s between shots
        public float recoilReduction = 0.0f;      // e.g. 0.3 for 30% lower recoil shake
        public float velocityMultiplier = 1.0f;   // Bullet speed
        public float adsSpeedMultiplier = 1.0f;   // Aim Speed
        public int bonusMagazineCapacity = 0;    // Added bullets

        [Header("Procedural Visuals")]
        public PrimitiveShape shape = PrimitiveShape.Cylinder;
        public Vector3 localScale = Vector3.one;
        public Vector3 localOffset = Vector3.zero;
        public Color meshColor = Color.gray;
    }
}
