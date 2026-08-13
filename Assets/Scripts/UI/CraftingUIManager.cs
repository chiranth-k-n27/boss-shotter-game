using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.UI
{
    public class CraftingUIManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject craftingPanel;
        public WeaponAssembler targetWeaponAssembler;

        [Header("Dynamic Weapon Name Display")]
        public Text weaponTitleText;

        [Header("Stat Fill Sliders / Bars")]
        public Slider damageBar;
        public Slider fireRateBar;
        public Slider recoilBar;
        public Slider velocityBar;
        public Slider adsSpeedBar;
        public Text ammoCapacityText;

        [Header("Stat Value Texts")]
        public Text damageText;
        public Text fireRateText;
        public Text recoilText;
        public Text velocityText;
        public Text adsSpeedText;

        [Header("Attachment Selection Parent")]
        public Transform attachmentButtonContainer;

        [Header("Available Attachments List")]
        public List<AttachmentData> availableAttachments = new List<AttachmentData>();

        private void OnEnable()
        {
            GameEvents.OnToggleCraftingUI += ToggleCraftingUI;
            GameEvents.OnWeaponStatsUpdated += RefreshStatBars;
        }

        private void OnDisable()
        {
            GameEvents.OnToggleCraftingUI -= ToggleCraftingUI;
            GameEvents.OnWeaponStatsUpdated -= RefreshStatBars;
        }

        private void Start()
        {
            if (craftingPanel != null)
            {
                craftingPanel.SetActive(false);
            }

            BuildAttachmentButtons();
        }

        public void ToggleCraftingUI(bool open)
        {
            if (craftingPanel != null)
            {
                craftingPanel.SetActive(open);
                if (open)
                {
                    RefreshStatBars();
                }
            }
        }

        public void CloseCraftingUI()
        {
            ToggleCraftingUI(false);
        }

        public void EquipAttachment(AttachmentData attachment)
        {
            if (targetWeaponAssembler != null && attachment != null)
            {
                targetWeaponAssembler.EquipAttachment(attachment);
                RefreshStatBars();
            }
        }

        public void RefreshStatBars()
        {
            if (targetWeaponAssembler == null) return;

            float dmg = targetWeaponAssembler.EffectiveDamage;
            float fireRate = targetWeaponAssembler.EffectiveFireRate;
            float recoil = targetWeaponAssembler.EffectiveRecoil;
            float velocity = targetWeaponAssembler.EffectiveBulletVelocity;
            float adsSpeed = targetWeaponAssembler.EffectiveADSSpeed;
            int maxAmmo = targetWeaponAssembler.EffectiveMaxAmmo;

            if (weaponTitleText != null)
            {
                weaponTitleText.text = $"WEAPON: {targetWeaponAssembler.DynamicWeaponName.ToUpper()}";
            }

            if (damageBar != null) damageBar.value = Mathf.Clamp01(dmg / 120f);
            if (fireRateBar != null) fireRateBar.value = Mathf.Clamp01((0.5f - fireRate) / 0.45f);
            if (recoilBar != null) recoilBar.value = Mathf.Clamp01(1.0f - recoil / 1.0f);
            if (velocityBar != null) velocityBar.value = Mathf.Clamp01(velocity / 300f);
            if (adsSpeedBar != null) adsSpeedBar.value = Mathf.Clamp01(adsSpeed / 20f);

            if (damageText != null) damageText.text = $"Damage: {dmg:F0}";
            if (fireRateText != null) fireRateText.text = $"Fire Rate: {1f/fireRate:F1}/s";
            if (recoilText != null) recoilText.text = $"Recoil: {recoil:F2}";
            if (velocityText != null) velocityText.text = $"Velocity: {velocity:F0}m/s";
            if (adsSpeedText != null) adsSpeedText.text = $"ADS Speed: {adsSpeed:F1}";
            if (ammoCapacityText != null) ammoCapacityText.text = $"Ammo Cap: {maxAmmo}";
        }

        public void BuildAttachmentButtons()
        {
            if (attachmentButtonContainer == null) return;

            // Clear old buttons
            foreach (Transform child in attachmentButtonContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (AttachmentData att in availableAttachments)
            {
                if (att == null) continue;

                GameObject btnObj = new GameObject($"Btn_{att.attachmentName}");
                btnObj.transform.SetParent(attachmentButtonContainer, false);

                RectTransform rect = btnObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(240, 50);

                Image img = btnObj.AddComponent<Image>();
                img.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

                Button btn = btnObj.AddComponent<Button>();
                AttachmentData currentAtt = att;
                btn.onClick.AddListener(() => EquipAttachment(currentAtt));

                GameObject txtObj = new GameObject("Label");
                txtObj.transform.SetParent(btnObj.transform, false);
                Text txt = txtObj.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", 14);
                txt.text = $"[{att.type}] {att.attachmentName}";
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
                
                RectTransform txtRect = txtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.sizeDelta = Vector2.zero;
            }
        }
    }
}
