using System;
using System.Collections.Generic;
using UnityEngine;
using MobileShooter.Events;

namespace MobileShooter.Core
{
    public class WeaponAssembler : MonoBehaviour, IAttachmentHolder
    {
        [Header("Base Data")]
        public WeaponData baseWeaponData;

        [Header("Transform Sockets")]
        public Transform barrelSocket;
        public Transform scopeSocket;
        public Transform stockSocket;
        public Transform magazineSocket;

        // Current Equipped Attachments
        private readonly Dictionary<AttachmentType, AttachmentData> equippedAttachments = new Dictionary<AttachmentType, AttachmentData>();
        private readonly Dictionary<AttachmentType, GameObject> spawnedMeshes = new Dictionary<AttachmentType, GameObject>();

        // Effective Calculated Stats
        public float EffectiveDamage { get; private set; }
        public float EffectiveFireRate { get; private set; }
        public float EffectiveRecoil { get; private set; }
        public float EffectiveBulletVelocity { get; private set; }
        public float EffectiveADSSpeed { get; private set; }
        public int EffectiveMaxAmmo { get; private set; }

        // Dynamic Custom Weapon Title
        public string DynamicWeaponName { get; private set; } = "Standard Prototype";

        // Active Challenge Buffs
        public float damageMultiplierBuff = 1.0f;
        public bool isInfiniteAmmoBuffActive = false;

        private void Awake()
        {
            if (baseWeaponData == null)
            {
                baseWeaponData = ScriptableObject.CreateInstance<WeaponData>();
            }
            RecalculateStats();
        }

        public void EquipAttachment(AttachmentData attachment)
        {
            if (attachment == null) return;

            AttachmentType type = attachment.type;
            
            // Remove existing mesh if present
            RemoveAttachment(type);

            // Store new attachment
            equippedAttachments[type] = attachment;

            // Instantiate visual mesh on socket
            Transform socket = GetSocketForType(type);
            if (socket != null)
            {
                GameObject attachmentMesh = CreatePrimitiveMesh(attachment.shape, attachment.meshColor);
                attachmentMesh.name = $"Attachment_{attachment.type}_{attachment.attachmentName}";
                attachmentMesh.transform.SetParent(socket, false);
                attachmentMesh.transform.localPosition = attachment.localOffset;
                attachmentMesh.transform.localRotation = Quaternion.identity;
                attachmentMesh.transform.localScale = attachment.localScale;

                // Strip colliders to avoid physics interference with player
                Collider col = attachmentMesh.GetComponent<Collider>();
                if (col != null) Destroy(col);

                spawnedMeshes[type] = attachmentMesh;
            }

            RecalculateStats();
        }

        public void RemoveAttachment(AttachmentType type)
        {
            if (spawnedMeshes.TryGetValue(type, out GameObject existingMesh) && existingMesh != null)
            {
                Destroy(existingMesh);
                spawnedMeshes.Remove(type);
            }
            equippedAttachments.Remove(type);
            RecalculateStats();
        }

        public AttachmentData GetEquippedAttachment(AttachmentType type)
        {
            equippedAttachments.TryGetValue(type, out AttachmentData data);
            return data;
        }

        public void RecalculateStats()
        {
            if (baseWeaponData == null) return;

            float damageMult = 1.0f;
            float fireRateOff = 0.0f;
            float recoilRed = 0.0f;
            float velocityMult = 1.0f;
            float adsSpeedMult = 1.0f;
            int bonusCapacity = 0;

            foreach (var kvp in equippedAttachments)
            {
                AttachmentData att = kvp.Value;
                if (att == null) continue;

                damageMult *= att.damageMultiplier;
                fireRateOff += att.fireRateOffset;
                recoilRed += att.recoilReduction;
                velocityMult *= att.velocityMultiplier;
                adsSpeedMult *= att.adsSpeedMultiplier;
                bonusCapacity += att.bonusMagazineCapacity;
            }

            EffectiveDamage = Mathf.Max(1.0f, baseWeaponData.baseDamage * damageMult * damageMultiplierBuff);
            EffectiveFireRate = Mathf.Max(0.04f, baseWeaponData.baseFireRate + fireRateOff);
            EffectiveRecoil = Mathf.Max(0.05f, baseWeaponData.baseRecoil * (1.0f - Mathf.Clamp01(recoilRed)));
            EffectiveBulletVelocity = Mathf.Max(10f, baseWeaponData.baseBulletVelocity * velocityMult);
            EffectiveADSSpeed = Mathf.Max(1.0f, baseWeaponData.baseADSSpeed * adsSpeedMult);
            EffectiveMaxAmmo = Mathf.Max(1, baseWeaponData.baseMagazineSize + bonusCapacity);

            DynamicWeaponName = GenerateDynamicWeaponName();

            GameEvents.TriggerWeaponStatsUpdated();
        }

        private string GenerateDynamicWeaponName()
        {
            string prefix = "";
            string core = "Carbine";
            string suffix = "";

            if (equippedAttachments.TryGetValue(AttachmentType.Barrel, out var barrel))
            {
                if (barrel.damageMultiplier > 1.2f) prefix = "Heavy Plasma";
                else if (barrel.fireRateOffset < 0f) prefix = "Rapid CQB";
                else if (barrel.velocityMultiplier > 1.3f) prefix = "High-Velocity";
                else prefix = "Tactical";
            }

            if (equippedAttachments.TryGetValue(AttachmentType.Scope, out var scope))
            {
                if (scope.adsSpeedMultiplier > 1.2f) core = "Sniper Cannon";
                else core = "Battle Rifle";
            }

            if (equippedAttachments.TryGetValue(AttachmentType.Magazine, out var mag))
            {
                if (mag.bonusMagazineCapacity >= 20) suffix = "Overloaded";
                else if (mag.bonusMagazineCapacity > 0) suffix = "Extended";
            }

            return $"{prefix} {core} {suffix}".Trim();
        }

        private Transform GetSocketForType(AttachmentType type)
        {
            switch (type)
            {
                case AttachmentType.Barrel: return barrelSocket;
                case AttachmentType.Scope: return scopeSocket;
                case AttachmentType.Stock: return stockSocket;
                case AttachmentType.Magazine: return magazineSocket;
                default: return transform;
            }
        }

        private GameObject CreatePrimitiveMesh(PrimitiveShape shape, Color color)
        {
            PrimitiveType pType = PrimitiveType.Cylinder;
            if (shape == PrimitiveShape.Cube) pType = PrimitiveType.Cube;
            else if (shape == PrimitiveShape.Sphere) pType = PrimitiveType.Sphere;

            GameObject obj = GameObject.CreatePrimitive(pType);
            Renderer ren = obj.GetComponent<Renderer>();
            if (ren != null)
            {
                Material mat = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
                mat.color = color;
                ren.material = mat;
            }
            return obj;
        }
    }
}
