using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MobileShooter.Core;
using MobileShooter.Input;
using MobileShooter.Player;
using MobileShooter.Boss;
using MobileShooter.UI;
using MobileShooter.Level;
using MobileShooter.Challenge;

namespace MobileShooter.Bootstrap
{
    public static class GameSceneBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapScene()
        {
            if (GameObject.Find("ArenaFloor") != null || GameObject.FindWithTag("Player") != null)
            {
                Debug.Log("[GameSceneBootstrapper] Scene already populated. Skipping auto-bootstrap.");
                return;
            }

            Debug.Log("[GameSceneBootstrapper] Executing Automated Scene Bootstrap with Multilevel & Challenge Systems...");

            // 1. Build Environment
            BuildEnvironment();

            // 2. Spawn Player & Camera & Weapon
            GameObject playerObj = BuildPlayer();

            // 3. Spawn Boss Monster & Hitboxes
            GameObject bossObj = BuildBoss(playerObj.transform);

            // 4. Attach Level & Challenge Managers
            GameObject managersObj = new GameObject("GameSystemManagers");
            LevelManager levelManager = managersObj.AddComponent<LevelManager>();
            levelManager.playerTransform = playerObj.transform;
            levelManager.RegisterActiveBoss(bossObj);

            ChallengeManager challengeManager = managersObj.AddComponent<ChallengeManager>();
            challengeManager.playerWeaponAssembler = playerObj.GetComponentInChildren<WeaponAssembler>();

            // 5. Build UI Canvas (HUD + Crafting + Challenge Panels)
            BuildUI(playerObj, bossObj);
        }

        private static void BuildEnvironment()
        {
            // Light
            GameObject lightObj = new GameObject("DirectionalLight");
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Floor Plane
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "ArenaFloor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(6f, 1f, 6f);
            SetObjectColor(floor, new Color(0.2f, 0.25f, 0.3f));

            // Cover Blocks
            Vector3[] coverPositions = new Vector3[]
            {
                new Vector3(-8f, 1.5f, 5f),
                new Vector3(8f, 1.5f, 5f),
                new Vector3(-6f, 1.5f, -8f),
                new Vector3(6f, 1.5f, -8f),
                new Vector3(0f, 1.5f, -12f)
            };

            for (int i = 0; i < coverPositions.Length; i++)
            {
                GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cover.name = $"CoverBlock_{i}";
                cover.transform.position = coverPositions[i];
                cover.transform.localScale = new Vector3(3f, 3f, 3f);
                SetObjectColor(cover, new Color(0.4f, 0.45f, 0.5f));
            }
        }

        private static GameObject BuildPlayer()
        {
            GameObject player = new GameObject("PlayerPlayer");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1.0f, -18f);

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2.0f;
            cc.radius = 0.5f;

            MobileTouchInput input = player.AddComponent<MobileTouchInput>();
            PlayerShooter shooter = player.AddComponent<PlayerShooter>();

            // Camera Pivot & Camera
            GameObject camPivot = new GameObject("CameraPivot");
            camPivot.transform.SetParent(player.transform, false);
            camPivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            GameObject camObj = new GameObject("PlayerCamera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(camPivot.transform, false);
            camObj.transform.localPosition = Vector3.zero;
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            camObj.AddComponent<AudioListener>();

            // Weapon Receiver Base
            GameObject weaponObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weaponObj.name = "WeaponBaseReceiver";
            weaponObj.transform.SetParent(camPivot.transform, false);
            weaponObj.transform.localPosition = new Vector3(0.35f, -0.3f, 0.6f);
            weaponObj.transform.localScale = new Vector3(0.12f, 0.15f, 0.5f);
            SetObjectColor(weaponObj, new Color(0.15f, 0.15f, 0.15f));
            Object.Destroy(weaponObj.GetComponent<Collider>());

            // Weapon Sockets
            Transform barrelSocket = CreateSocket(weaponObj.transform, "BarrelSocket", new Vector3(0f, 0f, 0.3f));
            Transform scopeSocket = CreateSocket(weaponObj.transform, "ScopeSocket", new Vector3(0f, 0.12f, 0f));
            Transform stockSocket = CreateSocket(weaponObj.transform, "StockSocket", new Vector3(0f, 0f, -0.3f));
            Transform magazineSocket = CreateSocket(weaponObj.transform, "MagazineSocket", new Vector3(0f, -0.15f, 0.05f));

            WeaponAssembler assembler = weaponObj.AddComponent<WeaponAssembler>();
            assembler.barrelSocket = barrelSocket;
            assembler.scopeSocket = scopeSocket;
            assembler.stockSocket = stockSocket;
            assembler.magazineSocket = magazineSocket;

            shooter.cameraPivot = camPivot.transform;
            shooter.playerCamera = cam;
            shooter.weaponAssembler = assembler;

            return player;
        }

        private static GameObject BuildBoss(Transform playerTransform)
        {
            GameObject boss = new GameObject("BossMonster");
            boss.transform.position = new Vector3(0f, 0f, 12f);

            // Boss Body Mesh
            GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bodyObj.name = "BossBody";
            bodyObj.transform.SetParent(boss.transform, false);
            bodyObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            bodyObj.transform.localScale = new Vector3(2.0f, 2.5f, 2.0f);
            SetObjectColor(bodyObj, Color.magenta);

            BossHitbox bodyHitbox = bodyObj.AddComponent<BossHitbox>();
            bodyHitbox.hitboxType = HitboxType.Body;

            // Head Weak Point Mesh (2.5x Damage)
            GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headObj.name = "BossHeadWeakPoint";
            headObj.transform.SetParent(boss.transform, false);
            headObj.transform.localPosition = new Vector3(0f, 5.5f, 0f);
            headObj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            SetObjectColor(headObj, Color.yellow);

            BossHitbox headHitbox = headObj.AddComponent<BossHitbox>();
            headHitbox.hitboxType = HitboxType.Head;

            // Armor Plate Mesh (0.5x Damage)
            GameObject armorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armorObj.name = "BossArmorChest";
            armorObj.transform.SetParent(boss.transform, false);
            armorObj.transform.localPosition = new Vector3(0f, 2.5f, -1.1f);
            armorObj.transform.localScale = new Vector3(1.6f, 1.8f, 0.4f);
            SetObjectColor(armorObj, Color.black);

            BossHitbox armorHitbox = armorObj.AddComponent<BossHitbox>();
            armorHitbox.hitboxType = HitboxType.Armor;

            // Attack Indicator Cylinder (Flat floor ring)
            GameObject indicatorObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicatorObj.name = "AttackTelegraphIndicator";
            indicatorObj.transform.SetParent(boss.transform, false);
            indicatorObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            indicatorObj.transform.localScale = Vector3.zero;
            SetObjectColor(indicatorObj, new Color(1f, 0f, 0f, 0.4f));
            Object.Destroy(indicatorObj.GetComponent<Collider>());
            indicatorObj.SetActive(false);

            BossHealth health = boss.AddComponent<BossHealth>();
            health.maxHealth = 800f;
            health.bossRenderers = boss.GetComponentsInChildren<Renderer>();

            BossAIController ai = boss.AddComponent<BossAIController>();
            ai.targetPlayer = playerTransform;
            ai.attackIndicator = indicatorObj.transform;

            return boss;
        }

        private static void BuildUI(GameObject playerObj, GameObject bossObj)
        {
            GameObject canvasObj = new GameObject("UICanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Setup Event System
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            HUDManager hud = canvasObj.AddComponent<HUDManager>();
            hud.hudCanvas = canvas;

            // Level Banner
            GameObject levelBannerObj = CreateUIText(canvasObj.transform, "LevelBanner", "LEVEL 1", 24, TextAnchor.UpperLeft);
            RectTransform levelRect = levelBannerObj.GetComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.05f, 0.92f);
            levelRect.anchorMax = new Vector2(0.3f, 0.98f);
            hud.levelBannerText = levelBannerObj.GetComponent<Text>();

            // Boss Health Slider Container
            GameObject hpBarObj = new GameObject("BossHPBar");
            hpBarObj.transform.SetParent(canvasObj.transform, false);
            RectTransform hpBarRect = hpBarObj.AddComponent<RectTransform>();
            hpBarRect.anchorMin = new Vector2(0.5f, 1f);
            hpBarRect.anchorMax = new Vector2(0.5f, 1f);
            hpBarRect.anchoredPosition = new Vector2(0f, -40f);
            hpBarRect.sizeDelta = new Vector2(450, 24);

            Slider slider = hpBarObj.AddComponent<Slider>();
            Image bg = hpBarObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(hpBarObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = Color.red;
            slider.fillRect = fillRect;

            hud.bossHealthSlider = slider;

            // Boss HP Text & Phase Text
            GameObject bossHpTextObj = CreateUIText(hpBarObj.transform, "BossHPText", "800 / 800 HP", 16, TextAnchor.MiddleCenter);
            hud.bossHealthText = bossHpTextObj.GetComponent<Text>();

            GameObject phaseTextObj = CreateUIText(hpBarObj.transform, "BossPhaseText", "PHASE 1: PATROL", 18, TextAnchor.MiddleCenter);
            phaseTextObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -30f);
            hud.bossPhaseText = phaseTextObj.GetComponent<Text>();

            // Challenge Panel
            GameObject challengeObj = new GameObject("ChallengeWidget");
            challengeObj.transform.SetParent(canvasObj.transform, false);
            RectTransform chalRect = challengeObj.AddComponent<RectTransform>();
            chalRect.anchorMin = new Vector2(0.5f, 0.82f);
            chalRect.anchorMax = new Vector2(0.5f, 0.88f);
            chalRect.sizeDelta = new Vector2(350, 36);

            Image chalBg = challengeObj.AddComponent<Image>();
            chalBg.color = new Color(0.1f, 0.2f, 0.35f, 0.85f);

            GameObject chalTitleObj = CreateUIText(challengeObj.transform, "Title", "CHALLENGE ACTIVE!", 16, TextAnchor.MiddleLeft);
            hud.challengeTitleText = chalTitleObj.GetComponent<Text>();

            GameObject chalProgObj = CreateUIText(challengeObj.transform, "Progress", "0 / 3", 16, TextAnchor.MiddleRight);
            hud.challengeProgressText = chalProgObj.GetComponent<Text>();
            hud.challengePanel = challengeObj;

            // Reward Notification Text
            GameObject rewardObj = CreateUIText(canvasObj.transform, "RewardText", "", 20, TextAnchor.LowerLeft);
            RectTransform rewardRect = rewardObj.GetComponent<RectTransform>();
            rewardRect.anchorMin = new Vector2(0.05f, 0.15f);
            rewardRect.anchorMax = new Vector2(0.4f, 0.25f);
            hud.rewardNotificationText = rewardObj.GetComponent<Text>();

            // Crosshair & Ammo Text
            GameObject crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(canvasObj.transform, false);
            Image crossImg = crosshair.AddComponent<Image>();
            crossImg.color = new Color(1f, 1f, 1f, 0.7f);
            crosshair.GetComponent<RectTransform>().sizeDelta = new Vector2(8, 8);

            GameObject ammoObj = CreateUIText(canvasObj.transform, "AmmoText", "30 / 30", 28, TextAnchor.LowerRight);
            RectTransform ammoRect = ammoObj.GetComponent<RectTransform>();
            ammoRect.anchorMin = new Vector2(1f, 0f);
            ammoRect.anchorMax = new Vector2(1f, 0f);
            ammoRect.anchoredPosition = new Vector2(-100f, 50f);
            hud.ammoText = ammoObj.GetComponent<Text>();

            // Buttons: ADS, Reload, Crafting
            MobileTouchInput input = playerObj.GetComponent<MobileTouchInput>();

            Button adsBtn = CreateUIButton(canvasObj.transform, "ADSBtn", "ADS", new Vector2(-80f, 160f), new Vector2(0.9f, 0.2f));
            adsBtn.onClick.AddListener(() => input.ToggleADSState());

            Button reloadBtn = CreateUIButton(canvasObj.transform, "ReloadBtn", "RELOAD", new Vector2(-80f, 90f), new Vector2(0.9f, 0.2f));
            reloadBtn.onClick.AddListener(() => input.TriggerReload());

            Button craftBtn = CreateUIButton(canvasObj.transform, "CraftBtn", "CRAFT", new Vector2(80f, 50f), new Vector2(0.1f, 0.1f));
            hud.craftingButton = craftBtn;

            // Crafting UI Panel
            BuildCraftingUI(canvasObj.transform, playerObj.GetComponentInChildren<WeaponAssembler>());
        }

        private static void BuildCraftingUI(Transform canvasTransform, WeaponAssembler assembler)
        {
            GameObject craftPanelObj = new GameObject("CraftingPanel");
            craftPanelObj.transform.SetParent(canvasTransform, false);
            RectTransform panelRect = craftPanelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImg = craftPanelObj.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

            CraftingUIManager craftManager = craftPanelObj.AddComponent<CraftingUIManager>();
            craftManager.craftingPanel = craftPanelObj;
            craftManager.targetWeaponAssembler = assembler;

            // Dynamic Weapon Title
            GameObject wTitleObj = CreateUIText(craftPanelObj.transform, "WeaponTitle", "WEAPON: TACTICAL CARBINE", 22, TextAnchor.UpperCenter);
            wTitleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);
            craftManager.weaponTitleText = wTitleObj.GetComponent<Text>();

            // Container for Attachment Buttons
            GameObject containerObj = new GameObject("AttachmentContainer");
            containerObj.transform.SetParent(craftPanelObj.transform, false);
            RectTransform contRect = containerObj.AddComponent<RectTransform>();
            contRect.anchorMin = new Vector2(0.05f, 0.15f);
            contRect.anchorMax = new Vector2(0.45f, 0.8f);
            contRect.offsetMin = Vector2.zero;
            contRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup grid = containerObj.AddComponent<VerticalLayoutGroup>();
            grid.spacing = 8f;
            grid.childControlHeight = true;
            grid.childControlWidth = true;

            craftManager.attachmentButtonContainer = containerObj.transform;

            // Stats Comparison Panel
            craftManager.damageText = CreateStatText(craftPanelObj.transform, "DmgText", "Damage: 25", new Vector2(0.55f, 0.65f));
            craftManager.fireRateText = CreateStatText(craftPanelObj.transform, "FRText", "Fire Rate: 6.6/s", new Vector2(0.55f, 0.55f));
            craftManager.recoilText = CreateStatText(craftPanelObj.transform, "RecText", "Recoil: 0.5", new Vector2(0.55f, 0.45f));
            craftManager.velocityText = CreateStatText(craftPanelObj.transform, "VelText", "Velocity: 100m/s", new Vector2(0.55f, 0.35f));
            craftManager.adsSpeedText = CreateStatText(craftPanelObj.transform, "ADSText", "ADS Speed: 8.0", new Vector2(0.55f, 0.25f));
            craftManager.ammoCapacityText = CreateStatText(craftPanelObj.transform, "CapText", "Ammo Cap: 30", new Vector2(0.55f, 0.15f));

            // Close Button
            Button closeBtn = CreateUIButton(craftPanelObj.transform, "CloseBtn", "BACK TO COMBAT", new Vector2(0f, 30f), new Vector2(0.5f, 0f));
            closeBtn.onClick.AddListener(() => craftManager.CloseCraftingUI());

            // Generate Preset Attachments
            craftManager.availableAttachments = GeneratePresetAttachments();
            craftManager.BuildAttachmentButtons();
        }

        private static List<AttachmentData> GeneratePresetAttachments()
        {
            List<AttachmentData> list = new List<AttachmentData>();

            // Barrels
            AttachmentData b1 = ScriptableObject.CreateInstance<AttachmentData>();
            b1.attachmentName = "Heavy Plasma Barrel";
            b1.type = AttachmentType.Barrel;
            b1.damageMultiplier = 1.4f;
            b1.velocityMultiplier = 1.6f;
            b1.fireRateOffset = 0.04f;
            b1.shape = PrimitiveShape.Cylinder;
            b1.localScale = new Vector3(0.08f, 0.45f, 0.08f);
            b1.localOffset = new Vector3(0f, 0f, 0.4f);
            b1.meshColor = Color.cyan;
            list.Add(b1);

            AttachmentData b2 = ScriptableObject.CreateInstance<AttachmentData>();
            b2.attachmentName = "Rapid CQB Barrel";
            b2.type = AttachmentType.Barrel;
            b2.fireRateOffset = -0.05f;
            b2.recoilReduction = 0.2f;
            b2.shape = PrimitiveShape.Cylinder;
            b2.localScale = new Vector3(0.12f, 0.2f, 0.12f);
            b2.localOffset = new Vector3(0f, 0f, 0.2f);
            b2.meshColor = Color.yellow;
            list.Add(b2);

            // Scopes
            AttachmentData s1 = ScriptableObject.CreateInstance<AttachmentData>();
            s1.attachmentName = "Thermal Sniper Scope";
            s1.type = AttachmentType.Scope;
            s1.adsSpeedMultiplier = 1.4f;
            s1.damageMultiplier = 1.15f;
            s1.shape = PrimitiveShape.Cube;
            s1.localScale = new Vector3(0.1f, 0.12f, 0.25f);
            s1.localOffset = new Vector3(0f, 0.08f, 0f);
            s1.meshColor = Color.red;
            list.Add(s1);

            // Stocks
            AttachmentData st1 = ScriptableObject.CreateInstance<AttachmentData>();
            st1.attachmentName = "Heavy Armored Stock";
            st1.type = AttachmentType.Stock;
            st1.recoilReduction = 0.5f;
            st1.adsSpeedMultiplier = 0.85f;
            st1.shape = PrimitiveShape.Cube;
            st1.localScale = new Vector3(0.1f, 0.15f, 0.35f);
            st1.localOffset = new Vector3(0f, -0.05f, -0.25f);
            st1.meshColor = Color.blue;
            list.Add(st1);

            // Magazines
            AttachmentData m1 = ScriptableObject.CreateInstance<AttachmentData>();
            m1.attachmentName = "Hyper Drum Mag 50R";
            m1.type = AttachmentType.Magazine;
            m1.bonusMagazineCapacity = 20;
            m1.shape = PrimitiveShape.Cylinder;
            m1.localScale = new Vector3(0.22f, 0.15f, 0.22f);
            m1.localOffset = new Vector3(0f, -0.1f, 0.05f);
            m1.meshColor = Color.green;
            list.Add(m1);

            return list;
        }

        private static Transform CreateSocket(Transform parent, string name, Vector3 localPos)
        {
            GameObject sock = new GameObject(name);
            sock.transform.SetParent(parent, false);
            sock.transform.localPosition = localPos;
            return sock.transform;
        }

        private static GameObject CreateUIText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 40);

            Text txt = obj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = Color.white;
            return obj;
        }

        private static Text CreateStatText(Transform parent, string name, string content, Vector2 anchorPosition)
        {
            GameObject obj = CreateUIText(parent, name, content, 18, TextAnchor.MiddleLeft);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.anchoredPosition = Vector2.zero;
            return obj.GetComponent<Text>();
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 position, Vector2 anchor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(130, 45);

            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.4f, 0.8f, 0.85f);

            Button btn = btnObj.AddComponent<Button>();

            GameObject txtObj = CreateUIText(btnObj.transform, "Text", label, 16, TextAnchor.MiddleCenter);
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private static void SetObjectColor(GameObject obj, Color col)
        {
            Renderer r = obj.GetComponent<Renderer>();
            if (r != null)
            {
                Material m = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
                m.color = col;
                r.material = m;
            }
        }
    }
}
