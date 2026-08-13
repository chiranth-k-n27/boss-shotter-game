using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MobileShooter.Core;
using MobileShooter.Events;

namespace MobileShooter.UI
{
    public class FloatingDamageNumber : MonoBehaviour
    {
        private Text textComponent;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private Vector3 worldPosition;
        private float lifetime = 1.0f;
        private float elapsedTime;
        private Camera mainCam;

        private static List<FloatingDamageNumber> pool = new List<FloatingDamageNumber>();
        private static Transform poolParent;

        public static void Spawn(float damage, HitboxType hitboxType, Vector3 position, Canvas parentCanvas)
        {
            if (parentCanvas == null) return;

            FloatingDamageNumber item = GetFromPool(parentCanvas.transform);
            item.Setup(damage, hitboxType, position);
        }

        private static FloatingDamageNumber GetFromPool(Transform parent)
        {
            foreach (var num in pool)
            {
                if (!num.gameObject.activeSelf)
                {
                    num.gameObject.SetActive(true);
                    return num;
                }
            }

            // Create new instance if pool exhausted
            GameObject obj = new GameObject("DamageNumber");
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            CanvasGroup group = obj.AddComponent<CanvasGroup>();
            Text txt = obj.AddComponent<Text>();

            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", 28);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 28;
            txt.raycastTarget = false;

            FloatingDamageNumber comp = obj.AddComponent<FloatingDamageNumber>();
            comp.textComponent = txt;
            comp.rectTransform = rect;
            comp.canvasGroup = group;

            pool.Add(comp);
            return comp;
        }

        private void Setup(float damage, HitboxType hitboxType, Vector3 pos)
        {
            worldPosition = pos + Vector3.up * 0.5f;
            elapsedTime = 0f;
            mainCam = Camera.main;

            int intDmg = Mathf.RoundToInt(damage);
            textComponent.text = intDmg.ToString();

            // Set color based on hit critical multiplier
            switch (hitboxType)
            {
                case HitboxType.Head:
                    textComponent.color = Color.yellow;
                    textComponent.fontSize = 36;
                    textComponent.text = $"CRIT! {intDmg}";
                    break;
                case HitboxType.Armor:
                    textComponent.color = Color.gray;
                    textComponent.fontSize = 22;
                    break;
                case HitboxType.Body:
                default:
                    textComponent.color = Color.white;
                    textComponent.fontSize = 28;
                    break;
            }

            canvasGroup.alpha = 1.0f;
            UpdateScreenPosition();
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;
            worldPosition += Vector3.up * (Time.deltaTime * 1.2f); // Float upward

            UpdateScreenPosition();

            // Fade out near end of lifetime
            float progress = elapsedTime / lifetime;
            canvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, progress);

            if (elapsedTime >= lifetime)
            {
                gameObject.SetActive(false);
            }
        }

        private void UpdateScreenPosition()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            Vector3 screenPos = mainCam.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPos;
        }
    }
}
