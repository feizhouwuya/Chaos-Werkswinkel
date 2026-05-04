using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    public class ChaosWorkshopDemoBootstrap : MonoBehaviour
    {
        public bool createDemoOnStart = true;
        public BattleManager battleManager;

        private Font defaultFont;

        private void Start()
        {
            if (createDemoOnStart)
            {
                BuildDemo();
            }
        }

        [ContextMenu("Build Demo")]
        public void BuildDemo()
        {
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (battleManager == null)
            {
                GameObject battleObject = new GameObject("Battle Manager");
                battleManager = battleObject.AddComponent<BattleManager>();
            }

            CombatUnit player = CreateUnit("Lu Xiyun", Team.Player, CharacterArchetype.Swordsman, WeaponKind.LongSword, 36, 3, battleManager.playerStartPosition, new Vector3(-5f, 0f, 0f), Color.cyan);
            CombatUnit enemy = CreateUnit("Training Duelist", Team.Enemy, CharacterArchetype.Enemy, WeaponKind.BattleAxe, 28, 2, battleManager.enemyStartPosition, new Vector3(5f, 0f, 0f), Color.red);
            List<CardDefinition> deck = CreateDemoDeck();

            battleManager.StartBattle(player, enemy, deck);
            CreateCanvas();
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
            unit.InitializeRuntime(arenaPosition);

            UnitWorldView view = unitObject.AddComponent<UnitWorldView>();
            view.unit = unit;
            view.battleManager = battleManager;
            return unit;
        }

        private List<CardDefinition> CreateDemoDeck()
        {
            List<CardDefinition> deck = new List<CardDefinition>();
            deck.Add(CreateCard("weak_point", "Weak Point", "Deal weapon damage and add weakness.", 1, 0, MoveMode.None, 0f, CardTargetRule.SingleEnemy, new CardEffect { type = CardEffectType.ApplyWeakness, amount = 2 }));
            deck.Add(CreateCard("step_cut", "Step Cut", "Move 2 toward target, then attack.", 1, 1, MoveMode.TowardTarget, 2f, CardTargetRule.SingleEnemy, null));
            deck.Add(CreateCard("withdraw", "Withdraw", "Move 3 away and gain 1 energy.", 0, 0, MoveMode.AwayFromTarget, 3f, CardTargetRule.SingleEnemy, new CardEffect { type = CardEffectType.GainEnergy, amount = 1 }));
            deck.Add(CreateCard("time_push", "Time Push", "Push target back on the timeline.", 1, 0, MoveMode.None, 0f, CardTargetRule.SingleEnemy, new CardEffect { type = CardEffectType.ChangeTimeline, amount = -3 }));
            deck.Add(CreateCard("quick_breath", "Quick Breath", "Gain speed and draw a card.", 1, 0, MoveMode.None, 0f, CardTargetRule.Self, new CardEffect { type = CardEffectType.ChangeSpeed, amount = 1 }));
            deck.Add(CreateCard("direct_cut", "Direct Cut", "A plain weapon attack.", 1, 0, MoveMode.None, 0f, CardTargetRule.SingleEnemy, null));
            deck.Add(CreateCard("direct_cut_copy", "Direct Cut", "A plain weapon attack.", 1, 0, MoveMode.None, 0f, CardTargetRule.SingleEnemy, null));
            return deck;
        }

        private CardDefinition CreateCard(string id, string title, string description, int cost, int bonusDamage, MoveMode moveMode, float moveDistance, CardTargetRule targetRule, CardEffect effect)
        {
            CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>();
            card.cardId = id;
            card.displayName = title;
            card.description = description;
            card.cost = cost;
            card.damage = bonusDamage;
            card.moveMode = moveMode;
            card.moveDistance = moveDistance;
            card.targetRule = targetRule;
            card.useWeaponDamage = targetRule != CardTargetRule.Self;
            card.useWeaponRange = true;

            if (effect != null)
            {
                card.effects.Add(effect);
            }

            if (id == "quick_breath")
            {
                card.effects.Add(new CardEffect { type = CardEffectType.DrawCards, amount = 1 });
            }

            return card;
        }

        private void CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Demo Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            BattleHud hud = canvasObject.AddComponent<BattleHud>();
            hud.battleManager = battleManager;

            hud.playerText = CreateText(canvasObject.transform, "Player Text", new Vector2(20f, -20f), new Vector2(330f, 120f), TextAnchor.UpperLeft);
            hud.enemyText = CreateText(canvasObject.transform, "Enemy Text", new Vector2(-350f, -20f), new Vector2(330f, 120f), TextAnchor.UpperLeft, new Vector2(1f, 1f), new Vector2(1f, 1f));
            hud.stateText = CreateText(canvasObject.transform, "State Text", new Vector2(0f, -20f), new Vector2(420f, 40f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            hud.logText = CreateText(canvasObject.transform, "Log Text", new Vector2(0f, -70f), new Vector2(520f, 40f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            hud.chaosWaveSlider = CreateSlider(canvasObject.transform, "Chaos Wave", new Vector2(0f, -120f), new Vector2(420f, 20f));
            hud.endActionButton = CreateButton(canvasObject.transform, "End Action", new Vector2(0f, 90f), new Vector2(160f, 42f));

            GameObject handRoot = new GameObject("Hand");
            handRoot.transform.SetParent(canvasObject.transform, false);
            RectTransform handRect = handRoot.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.5f, 0f);
            handRect.anchorMax = new Vector2(0.5f, 0f);
            handRect.pivot = new Vector2(0.5f, 0f);
            handRect.anchoredPosition = new Vector2(0f, 10f);
            handRect.sizeDelta = new Vector2(850f, 170f);
            HorizontalLayoutGroup layout = handRoot.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            HandView handView = handRoot.AddComponent<HandView>();
            handView.battleManager = battleManager;
            handView.cardPrefab = CreateCardPrefab();
            handView.enabled = false;
            handView.enabled = true;

            hud.enabled = false;
            hud.enabled = true;
        }

        private Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = defaultFont;
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = alignment;

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin ?? new Vector2(0f, 1f);
            rect.anchorMax = anchorMax ?? new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
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

        private Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject buttonObject = new GameObject(label + " Button");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = CreateText(buttonObject.transform, "Label", Vector2.zero, size, TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(1f, 1f));
            text.text = label;
            return button;
        }

        private CardButtonView CreateCardPrefab()
        {
            GameObject cardObject = new GameObject("Card Button Prefab");
            cardObject.SetActive(false);
            Image image = cardObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.13f, 0.16f, 0.96f);
            cardObject.AddComponent<Button>();
            CardButtonView view = cardObject.AddComponent<CardButtonView>();

            RectTransform rect = cardObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 160f);
            LayoutElement layoutElement = cardObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 120f;
            layoutElement.preferredHeight = 160f;

            view.titleText = CreateText(cardObject.transform, "Title", new Vector2(8f, -8f), new Vector2(104f, 30f), TextAnchor.UpperLeft);
            view.titleText.fontSize = 15;
            view.descriptionText = CreateText(cardObject.transform, "Description", new Vector2(8f, -44f), new Vector2(104f, 82f), TextAnchor.UpperLeft);
            view.descriptionText.fontSize = 12;
            view.costText = CreateText(cardObject.transform, "Cost", new Vector2(-28f, -8f), new Vector2(24f, 24f), TextAnchor.MiddleCenter, new Vector2(1f, 1f), new Vector2(1f, 1f));
            view.costText.fontSize = 18;

            return view;
        }
    }
}
