using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    public class ChaosWorkshopDemoBootstrap : MonoBehaviour
    {
        public bool createDemoOnStart = true;
        public BattleManager battleManager;
        public Font overrideFont;

        private Font defaultFont;

        private void Start()
        {
            if (createDemoOnStart)
            {
                BuildDemo();
            }
        }

        [ContextMenu("生成演示")]
        public void BuildDemo()
        {
            defaultFont = LoadBuiltinFont();
            CleanupExistingDemoObjects();

            if (battleManager == null)
            {
                GameObject battleObject = new GameObject("战斗管理器");
                battleManager = battleObject.AddComponent<BattleManager>();
            }

            battleManager.playerStartPosition = 4f;
            battleManager.enemyStartPosition = 11f;

            CombatUnit player = CreateUnit("陆夕云", Team.Player, CharacterArchetype.Swordsman, WeaponKind.LongSword, 48, 3, battleManager.playerStartPosition, new Vector3(-5f, 0f, 0f), Color.cyan);
            CombatUnit enemy = CreateUnit("失控傀儡", Team.Enemy, CharacterArchetype.Enemy, WeaponKind.BattleAxe, 28, 3, battleManager.enemyStartPosition, new Vector3(5f, 0f, 0f), Color.red);
            EnemyAI enemyAI = enemy.gameObject.AddComponent<EnemyAI>();
            enemyAI.Configure("rogue_puppet");
            enemyAI.battleManager = battleManager;
            List<CardDefinition> deck = BalancePrototypeFactory.CreateStartingDeck(CharacterArchetype.Swordsman);

            battleManager.StartBattle(player, enemy, deck);
            if (defaultFont == null)
            {
                Debug.LogWarning("混沌工坊 Demo 已生成战斗运行时，但由于没有可用字体，已跳过界面创建。");
                return;
            }

            CreateCanvas();
        }

        private Font LoadBuiltinFont()
        {
            if (overrideFont != null)
            {
                return overrideFont;
            }

            try
            {
                Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (builtinFont != null)
                {
                    return builtinFont;
                }
            }
            catch
            {
            }

            Font[] loadedFonts = Resources.FindObjectsOfTypeAll<Font>();
            for (int i = 0; i < loadedFonts.Length; i++)
            {
                if (loadedFonts[i] != null)
                {
                    return loadedFonts[i];
                }
            }

            return null;
        }

        private CombatUnit CreateUnit(string name, Team team, CharacterArchetype archetype, WeaponKind weapon, int hp, int energy, float arenaPosition, Vector3 worldPosition, Color color)
        {
            GameObject unitObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            unitObject.name = name;
            unitObject.transform.position = worldPosition;
            unitObject.transform.localScale = new Vector3(1f, 2f, 1f);

            Renderer renderer = unitObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            CombatUnit unit = unitObject.AddComponent<CombatUnit>();
            unit.displayName = name;
            unit.team = team;
            unit.archetype = archetype;
            unit.weaponKind = weapon;
            unit.maxHealth = hp;
            unit.maxEnergy = energy;
            if (archetype == CharacterArchetype.Enemy)
            {
                unit.attackRangeOverride = 2;
                unit.baseDamageOverride = 6;
                unit.speedOverride = 2f;
            }

            unit.InitializeRuntime(arenaPosition);

            UnitWorldView view = unitObject.AddComponent<UnitWorldView>();
            view.unit = unit;
            view.battleManager = battleManager;
            if (view.textureRendererBinding != null)
            {
                view.textureRendererBinding.targetRenderer = renderer;
            }

            return unit;
        }

        private void CreateCanvas()
        {
            GameObject canvasObject = new GameObject("演示画布");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            BattleHud hud = canvasObject.AddComponent<BattleHud>();
            hud.battleManager = battleManager;

            Image playerPanel = CreatePanel(canvasObject.transform, "Player Panel", new Vector2(24f, -24f), new Vector2(420f, 164f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            Image enemyPanel = CreatePanel(canvasObject.transform, "Enemy Panel", new Vector2(-24f, -24f), new Vector2(420f, 164f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            Image centerPanel = CreatePanel(canvasObject.transform, "Center Panel", new Vector2(0f, -24f), new Vector2(640f, 180f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

            hud.playerPortraitImage = CreateDecorativeImage(playerPanel.transform, "Player Portrait", new Vector2(78f, -82f), new Vector2(112f, 112f), new Color(0.2f, 0.45f, 0.55f, 0.55f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            hud.enemyPortraitImage = CreateDecorativeImage(enemyPanel.transform, "Enemy Portrait", new Vector2(-78f, -82f), new Vector2(112f, 112f), new Color(0.55f, 0.24f, 0.24f, 0.55f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            hud.playerNameText = CreateText(playerPanel.transform, "Player Name", new Vector2(144f, -26f), new Vector2(240f, 28f), TextAnchor.UpperLeft);
            hud.playerNameText.fontSize = 24;
            hud.playerStatsText = CreateText(playerPanel.transform, "Player Stats", new Vector2(144f, -62f), new Vector2(236f, 44f), TextAnchor.UpperLeft);
            hud.playerStatusText = CreateText(playerPanel.transform, "Player Status", new Vector2(144f, -110f), new Vector2(236f, 40f), TextAnchor.UpperLeft);
            hud.playerStatusText.fontSize = 14;

            hud.enemyNameText = CreateText(enemyPanel.transform, "Enemy Name", new Vector2(-144f, -26f), new Vector2(240f, 28f), TextAnchor.UpperLeft, new Vector2(1f, 1f), new Vector2(1f, 1f));
            hud.enemyNameText.fontSize = 24;
            hud.enemyStatsText = CreateText(enemyPanel.transform, "Enemy Stats", new Vector2(-144f, -62f), new Vector2(236f, 44f), TextAnchor.UpperLeft, new Vector2(1f, 1f), new Vector2(1f, 1f));
            hud.enemyStatusText = CreateText(enemyPanel.transform, "Enemy Status", new Vector2(-144f, -110f), new Vector2(236f, 40f), TextAnchor.UpperLeft, new Vector2(1f, 1f), new Vector2(1f, 1f));
            hud.enemyStatusText.fontSize = 14;

            hud.playerText = CreateText(canvasObject.transform, "Player Text", new Vector2(40f, -40f), new Vector2(340f, 120f), TextAnchor.UpperLeft);
            hud.playerText.gameObject.SetActive(false);
            hud.enemyText = CreateText(canvasObject.transform, "Enemy Text", new Vector2(-40f, -40f), new Vector2(340f, 120f), TextAnchor.UpperLeft, new Vector2(1f, 1f), new Vector2(1f, 1f));
            hud.enemyText.gameObject.SetActive(false);

            hud.playerDamagePopupText = CreateText(canvasObject.transform, "Player Damage Popup", new Vector2(214f, -188f), new Vector2(140f, 34f), TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(0f, 1f));
            hud.playerDamagePopupText.fontSize = 28;
            hud.playerDamagePopupText.color = new Color(1f, 0.2f, 0.2f, 1f);
            hud.enemyDamagePopupText = CreateText(canvasObject.transform, "Enemy Damage Popup", new Vector2(-214f, -188f), new Vector2(140f, 34f), TextAnchor.MiddleCenter, new Vector2(1f, 1f), new Vector2(1f, 1f));
            hud.enemyDamagePopupText.fontSize = 28;
            hud.enemyDamagePopupText.color = new Color(1f, 0.2f, 0.2f, 1f);

            hud.stateText = CreateText(centerPanel.transform, "State Text", new Vector2(0f, -22f), new Vector2(580f, 34f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            hud.stateText.fontSize = 22;
            hud.logText = CreateText(centerPanel.transform, "Log Text", new Vector2(0f, -58f), new Vector2(620f, 32f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            hud.logText.fontSize = 18;
            hud.cardDetailText = CreateText(centerPanel.transform, "Card Detail Text", new Vector2(0f, -104f), new Vector2(592f, 64f), TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            hud.cardDetailText.fontSize = 15;

            hud.distanceText = CreateText(canvasObject.transform, "Distance Text", new Vector2(0f, 320f), new Vector2(760f, 28f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            hud.distanceText.fontSize = 16;

            hud.chaosWaveSlider = CreateSlider(centerPanel.transform, "Chaos Wave", new Vector2(0f, -152f), new Vector2(520f, 16f));
            hud.endActionButton = CreateButton(canvasObject.transform, "结束行动", new Vector2(-24f, 24f), new Vector2(220f, 52f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            hud.moveTowardButton = CreateButton(canvasObject.transform, "前进", new Vector2(-24f, 88f), new Vector2(220f, 48f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            hud.moveAwayButton = CreateButton(canvasObject.transform, "后退", new Vector2(-256f, 88f), new Vector2(220f, 48f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            hud.arenaTrack = CreateArenaTrack(canvasObject.transform, hud);

            GameObject handRoot = new GameObject("Hand");
            handRoot.transform.SetParent(canvasObject.transform, false);
            RectTransform handRect = handRoot.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.5f, 0f);
            handRect.anchorMax = new Vector2(0.5f, 0f);
            handRect.pivot = new Vector2(0.5f, 0f);
            handRect.anchoredPosition = new Vector2(0f, 18f);
            handRect.sizeDelta = new Vector2(1220f, 228f);
            HorizontalLayoutGroup layout = handRoot.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(24, 24, 12, 12);

            CreatePlayerEnergyOrb(canvasObject.transform, hud, handRect);

            HandView handView = handRoot.AddComponent<HandView>();
            handView.battleManager = battleManager;
            handView.cardPrefab = CreateCardPrefab();
            handView.enabled = false;
            handView.enabled = true;

            hud.playerPortraitBinding.targetImage = hud.playerPortraitImage;
            hud.enemyPortraitBinding.targetImage = hud.enemyPortraitImage;
            hud.artworkSlots = new[]
            {
                CreateSlot("Player Panel", playerPanel),
                CreateSlot("Enemy Panel", enemyPanel),
                CreateSlot("Center Panel", centerPanel)
            };

            hud.enabled = false;
            hud.enabled = true;
        }

        private void CleanupExistingDemoObjects()
        {
            battleManager = null;
            DestroyIfExists("战斗管理器");
            DestroyIfExists("演示画布");
            DestroyIfExists("陆夕云");
            DestroyIfExists("失控傀儡");

            DestroyIfExists("Battle Manager");
            DestroyIfExists("Demo Canvas");
            DestroyIfExists("Lu Xiyun");
            DestroyIfExists("Rogue Puppet");
        }

        private void DestroyIfExists(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(existing);
            }
            else
            {
                DestroyImmediate(existing);
            }
        }

        private Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = defaultFont;
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin ?? new Vector2(0f, 1f);
            rect.anchorMax = anchorMax ?? new Vector2(0f, 1f);
            rect.pivot = new Vector2((rect.anchorMin.x + rect.anchorMax.x) * 0.5f, (rect.anchorMin.y + rect.anchorMax.y) * 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return text;
        }

        private Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(parent, false);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.interactable = false;
            RectTransform rect = sliderObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(sliderObject.transform, false);
            Image background = backgroundObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(sliderObject.transform, false);
            Image fill = fillObject.AddComponent<Image>();
            fill.color = new Color(0.1f, 0.75f, 0.95f, 0.95f);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            slider.fillRect = fillRect;
            slider.targetGraphic = fill;
            return slider;
        }

        private RectTransform CreateArenaTrack(Transform parent, BattleHud hud)
        {
            GameObject trackObject = new GameObject("Arena Track");
            trackObject.transform.SetParent(parent, false);
            Image trackImage = trackObject.AddComponent<Image>();
            trackImage.color = new Color(0.07f, 0.11f, 0.18f, 0.95f);
            RectTransform rect = trackObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 280f);
            rect.sizeDelta = new Vector2(920f, 28f);

            hud.playerRangeBar = CreateTrackBar(trackObject.transform, "Player Range", new Color(0.16f, 0.75f, 0.88f, 0.35f), 10f);
            hud.enemyRangeBar = CreateTrackBar(trackObject.transform, "Enemy Range", new Color(0.95f, 0.32f, 0.32f, 0.35f), -10f);
            hud.playerMarker = CreateTrackMarker(trackObject.transform, "Player Marker", new Color(0.1f, 0.88f, 0.96f, 1f), new Vector2(16f, 36f));
            hud.enemyMarker = CreateTrackMarker(trackObject.transform, "Enemy Marker", new Color(0.95f, 0.15f, 0.15f, 1f), new Vector2(16f, 36f));
            return rect;
        }

        private RectTransform CreateTrackBar(Transform parent, string name, Color color, float yOffset)
        {
            GameObject barObject = new GameObject(name);
            barObject.transform.SetParent(parent, false);
            Image image = barObject.AddComponent<Image>();
            image.color = color;
            RectTransform rect = barObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, yOffset);
            rect.sizeDelta = new Vector2(0f, 8f);
            return rect;
        }

        private RectTransform CreateTrackMarker(Transform parent, string name, Color color, Vector2 size)
        {
            GameObject markerObject = new GameObject(name);
            markerObject.transform.SetParent(parent, false);
            Image image = markerObject.AddComponent<Image>();
            image.color = color;
            RectTransform rect = markerObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            return rect;
        }

        private void CreatePlayerEnergyOrb(Transform parent, BattleHud hud, RectTransform handRect)
        {
            GameObject orbRootObject = new GameObject("Player Energy Orb");
            orbRootObject.transform.SetParent(parent, false);

            RectTransform rootRect = orbRootObject.AddComponent<RectTransform>();
            rootRect.anchorMin = handRect.anchorMin;
            rootRect.anchorMax = handRect.anchorMax;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = handRect.anchoredPosition + new Vector2(-handRect.sizeDelta.x * 0.5f - 84f, handRect.sizeDelta.y + 12f);
            rootRect.sizeDelta = new Vector2(128f, 128f);

            Sprite orbSprite = LoadEnergyOrbSprite();
            CreateOrbLayer(rootRect, "Halo", orbSprite, new Color(0.12f, 0.82f, 1f, 0.12f), Vector2.zero, new Vector2(140f, 140f));
            CreateOrbLayer(rootRect, "Frame", orbSprite, new Color(1f, 1f, 1f, 0.18f), Vector2.zero, new Vector2(128f, 128f));
            hud.playerEnergyOrbImage = CreateOrbLayer(rootRect, "Core", orbSprite, new Color(0.13f, 0.8f, 0.95f, 0.98f), Vector2.zero, new Vector2(104f, 104f));
            CreateOrbLayer(rootRect, "Highlight", orbSprite, new Color(1f, 1f, 1f, 0.14f), new Vector2(-16f, 18f), new Vector2(34f, 34f));

            Text valueText = CreateText(rootRect, "Energy Value", Vector2.zero, new Vector2(128f, 128f), TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(1f, 1f));
            valueText.fontSize = 44;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = Color.white;
            valueText.raycastTarget = false;

            RectTransform valueRect = valueText.rectTransform;
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;

            Outline outline = valueText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
            outline.effectDistance = new Vector2(1f, -1f);

            hud.playerEnergyValueText = valueText;
        }

        private Image CreateOrbLayer(RectTransform parent, string name, Sprite sprite, Color color, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject layerObject = new GameObject(name);
            layerObject.transform.SetParent(parent, false);

            Image image = layerObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return image;
        }

        private Sprite LoadEnergyOrbSprite()
        {
            Sprite builtinSprite = TryLoadBuiltinSprite("UI/Skin/Knob.psd");
            if (builtinSprite != null)
            {
                return builtinSprite;
            }

            builtinSprite = TryLoadBuiltinSprite("Knob.psd");
            if (builtinSprite != null)
            {
                return builtinSprite;
            }

            return CreateRuntimeOrbSprite();
        }

        private Sprite TryLoadBuiltinSprite(string resourceName)
        {
            try
            {
                return Resources.GetBuiltinResource<Sprite>(resourceName);
            }
            catch
            {
                return null;
            }
        }

        private Sprite CreateRuntimeOrbSprite()
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;

            float center = (size - 1) * 0.5f;
            float radius = size * 0.42f;
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color solid = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01((radius - distance) / 1.5f);
                    texture.SetPixel(x, y, Color.Lerp(clear, solid, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Image CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);
            Image image = panelObject.AddComponent<Image>();
            image.color = new Color(0.08f, 0.12f, 0.2f, 0.5f);
            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(anchorMin.x, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return image;
        }

        private Image CreateDecorativeImage(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return image;
        }

        private Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            GameObject buttonObject = new GameObject(label + " Button");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin ?? new Vector2(0.5f, 0f);
            rect.anchorMax = anchorMax ?? new Vector2(0.5f, 0f);
            rect.pivot = new Vector2((rect.anchorMin.x + rect.anchorMax.x) * 0.5f, (rect.anchorMin.y + rect.anchorMax.y) * 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = CreateText(buttonObject.transform, "Label", Vector2.zero, size, TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(1f, 1f));
            text.text = label;
            text.fontSize = 20;
            text.raycastTarget = false;

            RectTransform textRect = text.rectTransform;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
        }

        private CardButtonView CreateCardPrefab()
        {
            GameObject cardObject = new GameObject("Card Button Prefab");
            cardObject.SetActive(false);

            Image backgroundImage = cardObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.12f, 0.13f, 0.16f, 0.96f);
            cardObject.AddComponent<Button>();
            CardButtonView view = cardObject.AddComponent<CardButtonView>();
            view.backgroundImage = backgroundImage;

            RectTransform rect = cardObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(168f, 208f);
            LayoutElement layoutElement = cardObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 168f;
            layoutElement.preferredHeight = 208f;

            view.illustrationImage = CreateDecorativeImage(cardObject.transform, "Illustration", new Vector2(0f, -54f), new Vector2(144f, 78f), new Color(0.35f, 0.4f, 0.48f, 0.65f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            view.frameImage = CreateDecorativeImage(cardObject.transform, "Frame", new Vector2(0f, -92f), new Vector2(154f, 184f), new Color(1f, 1f, 1f, 0.08f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            view.costBadgeImage = CreateDecorativeImage(cardObject.transform, "Cost Badge", new Vector2(-18f, -18f), new Vector2(38f, 38f), new Color(0.15f, 0.55f, 0.7f, 0.75f), new Vector2(1f, 1f), new Vector2(1f, 1f));

            view.titleText = CreateText(cardObject.transform, "Title", new Vector2(12f, -12f), new Vector2(132f, 28f), TextAnchor.UpperLeft);
            view.titleText.fontSize = 18;
            view.descriptionText = CreateText(cardObject.transform, "Description", new Vector2(12f, -138f), new Vector2(144f, 56f), TextAnchor.UpperLeft);
            view.descriptionText.fontSize = 14;
            view.costText = CreateText(cardObject.transform, "Cost", new Vector2(-18f, -18f), new Vector2(36f, 28f), TextAnchor.MiddleCenter, new Vector2(1f, 1f), new Vector2(1f, 1f));
            view.costText.fontSize = 22;

            view.backgroundBinding.targetImage = backgroundImage;
            view.illustrationBinding.targetImage = view.illustrationImage;
            view.frameBinding.targetImage = view.frameImage;
            view.costBadgeBinding.targetImage = view.costBadgeImage;
            view.artworkSlots = new[]
            {
                CreateSlot("Card Background", backgroundImage),
                CreateSlot("Card Frame", view.frameImage),
                CreateSlot("Card Cost Badge", view.costBadgeImage)
            };

            return view;
        }

        private UIImageArtworkSlot CreateSlot(string slotName, Image target)
        {
            return new UIImageArtworkSlot
            {
                slotName = slotName,
                target = target,
                sprite = target != null ? target.sprite : null,
                preserveAspect = true
            };
        }
    }
}
