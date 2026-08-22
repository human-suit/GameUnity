using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private EnemyStats enemy;
    public enum BattleState
    {
        PlayerTurn,
        EnemyTurn,
        BattleWon,
        BattleLost
    }

    [SerializeField] private TMP_Text energyText;
    [SerializeField] private int maxEnergy = 3;
    private int currentEnergy;
    public int CurrentEnergy => currentEnergy;
    public int MaxEnergy => maxEnergy;

    [SerializeField] private TMP_Text blockText;
    private int currentBlock = 0;
    [SerializeField] private int blockAmount = 5;

    [Header("Body Parts UI")]
    [SerializeField] private GameObject bodyPartsPanel;
    [SerializeField] private Button headButton;
    [SerializeField] private Button torsoButton;
    [SerializeField] private Button leftArmButton;
    [SerializeField] private Button rightArmButton;
    [SerializeField] private Button leftLegButton;
    [SerializeField] private Button rightLegButton;
    [SerializeField] private TMP_Text battleMessageText;
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text bodyPartInfoText;
    [SerializeField] private RectTransform bodyPartInfoRoot;
    [SerializeField] private Slider enemyHealthSlider;
    [Range(0.01f, 1f)]
    [SerializeField] private float alphaHitTestThreshold = 0.1f;
    [SerializeField] private Material destroyedPartMaterial;

    [Header("Cards")]
    [SerializeField] private List<BattleCardData> startingDeck =
        new List<BattleCardData>();
    [SerializeField] private int handSize = 5;
    [SerializeField] private BattleCardView cardViewPrefab;
    [SerializeField] private RectTransform handRoot;
    [SerializeField] private List<RectTransform> handSlots =
        new List<RectTransform>();
    [SerializeField] private RectTransform playerDropZone;
    [SerializeField] private RectTransform bagRoot;
    [SerializeField] private Image bagImage;
    [SerializeField] private Sprite bagSprite;
    [SerializeField] private TMP_Text bagText;
    [SerializeField] private float dealDuration = 0.35f;
    [SerializeField] private float dealStagger = 0.08f;

    [Header("Battle presentation")]
    [SerializeField] private Image battleBackgroundImage;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] private Sprite playerPortrait;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Image playerHealthFill;
    [SerializeField] private Color playerHealthColor =
        new Color(0.78f, 0.12f, 0.12f, 1f);
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private TMP_Text playerLevelText;
    [SerializeField] private TMP_Text playerMoneyText;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Sprite endTurnBellSprite;

    public BattleState currentState;
    private string battleMessage = "Выберите действие.";
    private readonly List<BattleCardData> bag = new List<BattleCardData>();
    private readonly List<BattleCardData> hand = new List<BattleCardData>();
    private readonly List<BattleCardView> handViews =
        new List<BattleCardView>();
    private Image highlightedImage;
    private Color highlightedOriginal = Color.white;
    private bool isDealing;

    private string hoverInfoText = "";
    private BattleCardView hoveredCard;

    private void OnValidate()
    {
        ApplyPlayerHealthBarStyle();
        ApplyEndTurnBell(false);
        BattleUiFonts.ApplyAllInScene();
    }

    private void Awake()
    {
        BattleUiFonts.ApplyAllInScene();
    }

    private void Start()
    {
        BattleUiFonts.ApplyAllInScene();
        currentState = BattleState.PlayerTurn;
        currentEnergy = maxEnergy;
        FitBattleToScreen();
        UpdateEnergyUI();
        UpdateBlockUI();
        ShowBodyPartsPanel(true);
        UpdateEnemyHealthUI();
        ConfigureBodyPartButtons();
        RefreshBodyPartButtons();
        ApplyBattlePresentation();
        ApplyBagSprite();
        EnsureBattleHud();
        ApplyPlayerHealthBarStyle();
        ApplyEndTurnBell();
        InitializeCardBattle();
        StatusPlaqueHud.Ensure(this);
        UpdatePlayerStatusUI();
        SetBattleMessage("Выберите действие.");
        RefreshLeftPanel();

        Debug.Log("=== BATTLE STARTED ===");
        Debug.Log("Player Turn");
    }

    public void EndPlayerTurn()
    {
        Debug.Log("End turn botton pressed");

        if (currentState != BattleState.PlayerTurn || isDealing)
            return;

        EndTurnBellButton bell = endTurnButton != null
            ? endTurnButton.GetComponent<EndTurnBellButton>()
            : FindFirstObjectByType<EndTurnBellButton>();
        if (bell != null)
            bell.PlaySwing();

        ReturnHandToBag();
        ClearTargetHighlights();
        HideBodyPartInfo();
        Debug.Log("end player turn");

        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        currentState = BattleState.EnemyTurn;

        Debug.Log("Enemy Turn");

        if (enemy == null)
        {
            Debug.LogError("BattleManager: EnemyStats is not assigned.", this);
            return;
        }

        enemy.Attack();

        if (currentState != BattleState.EnemyTurn)
            return;

        // Проверяем, умер ли игрок
        if (GameState.PlayerHealth <= 0)
        {
            BattleLost();
            return;
        }

        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        currentState = BattleState.PlayerTurn;

        currentEnergy = maxEnergy;
        
        currentBlock = 0;
        
        UpdateEnergyUI();
        UpdateBlockUI();
        ShowBodyPartsPanel(true);
        RefreshBodyPartButtons();
        DrawNewHand();
        UpdatePlayerStatusUI();

        Debug.Log("Player Turn");
        Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);
    }

    // ПОБЕДА
    public void BattleWon()
    {
        if (currentState == BattleState.BattleWon ||
            currentState == BattleState.BattleLost)
            return;

        currentState = BattleState.BattleWon;

        Debug.Log("=== BATTLE WON ===");

        // Запоминаем, что враг побеждён
        GameState.MarkEnemyDefeated(BattleEncounterData.EncounterId);

        string sourceScene = BattleEncounterData.SourceScene;
        if (!string.IsNullOrEmpty(sourceScene) &&
            Application.CanStreamedLevelBeLoaded(sourceScene))
        {
            BattleEncounterData.QueueReturn();
            SceneManager.LoadScene(sourceScene);
            return;
        }

        // Запасной вариант для запуска Battle напрямую из Editor.
        BattleEncounterData.Clear();
        SceneManager.LoadScene("Main");
    }

    // =========================
    // ПОРАЖЕНИЕ
    // =========================

    public void BattleLost()
    {
        if (currentState == BattleState.BattleWon ||
            currentState == BattleState.BattleLost)
            return;

        currentState = BattleState.BattleLost;

        Debug.Log("=== BATTLE LOST ===");

        // Сбрасываем текущий забег
        GameState.ResetRun();

        // Возвращаемся в главное меню
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayerAttack()
    {
        if (currentState != BattleState.PlayerTurn)
            return;

        if (currentEnergy <= 0)
        {
            Debug.Log("Not enough energy!");
            return;
        }

        if (enemy == null)
        {
            Debug.LogError("BattleManager: EnemyStats is not assigned.", this);
            return;
        }

        ShowBodyPartsPanel(true);
        RefreshBodyPartButtons();
        SetBattleMessage("Выберите часть тела для атаки.");
    }

    private void UpdateEnergyUI()
    {
        RefreshStatusText();
    }

    public void PlayerBlock()
    {
        if (currentState != BattleState.PlayerTurn)
            return;

        if (currentEnergy <= 0)
        {
            Debug.Log("Not enough energy!");
            return;
        }

        ShowBodyPartsPanel(false);
        currentEnergy--;

        currentBlock += blockAmount;

        SetBattleMessage($"Вы получаете {blockAmount} блока.");
        Debug.Log("Player gained " + blockAmount + " Block!");
        Debug.Log("Current Block: " + currentBlock);
        Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);

        UpdateBlockUI();
        UpdateEnergyUI();
    }

    public void TakePlayerDamage(int damage)
    {
        if (currentBlock > 0)
        {
            int damageAfterBlock = damage - currentBlock;

            if (damageAfterBlock < 0)
                damageAfterBlock = 0;

            Debug.Log("Block absorbed " + Mathf.Min(damage, currentBlock) + " damage!");

            currentBlock -= damage;

            if (currentBlock < 0)
                currentBlock = 0;

            if (damageAfterBlock > 0)
                GameState.DamagePlayer(damageAfterBlock);
        }
        else
        {
            GameState.DamagePlayer(damage);
        }

        Debug.Log
        (
        "Player HP: " +
        GameState.PlayerHealth +
        " / " +
        GameState.PlayerMaxHealth
        );

        Debug.Log("Player Block: " + currentBlock);

        if (GameState.PlayerHealth <= 0)
        {
            BattleLost();
        }

        UpdateBlockUI();
        UpdatePlayerStatusUI();
    }

    private void UpdateBlockUI()
    {
        if (blockText == null)
            return;

        bool hasBlock = currentBlock > 0;
        blockText.gameObject.SetActive(hasBlock);
        blockText.text = hasBlock ? $"Защита: {currentBlock}" : string.Empty;
    }

    public void SetBattleMessage(string message)
    {
        battleMessage = message;

        RefreshLeftPanel();
    }

    private void InitializeCardBattle()
    {
        BattleCardData[] resourceCards =
            Resources.LoadAll<BattleCardData>("BattleCards");
        GameState.InitializeCardDeck(
            resourceCards != null && resourceCards.Length > 0
                ? resourceCards
                : startingDeck);

        bag.Clear();
        bag.AddRange(GameState.CardDeck);
        DrawNewHand();
    }

    private void DrawNewHand()
    {
        if (currentState != BattleState.PlayerTurn)
            return;

        if (hand.Count > 0)
            ReturnHandToBag();

        int cardsToDraw = Mathf.Min(handSize, bag.Count);
        for (int drawn = 0; drawn < cardsToDraw; drawn++)
            DrawRandomCardFromBag();

        RebuildHandViews(true);
        UpdateBagUI();
    }

    private void DrawRandomCardFromBag()
    {
        if (bag.Count == 0)
            return;

        int index = Random.Range(0, bag.Count);
        hand.Add(bag[index]);
        bag.RemoveAt(index);
    }

    private void RebuildHandViews(bool animateFromBag)
    {
        foreach (BattleCardView view in handViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        handViews.Clear();

        if (cardViewPrefab == null || handRoot == null)
        {
            Debug.LogWarning(
                "BattleManager: assign Card View Prefab and Hand Root.",
                this);
            return;
        }

        Vector3 bagPosition = bagRoot != null
            ? bagRoot.position
            : handRoot.position;

        isDealing = animateFromBag && bagRoot != null;
        if (isDealing)
            PulseBag();

        for (int i = 0; i < hand.Count; i++)
        {
            Transform parent = i < handSlots.Count &&
                handSlots[i] != null
                ? handSlots[i]
                : handRoot;

            BattleCardView view = Instantiate(cardViewPrefab, parent);
            RectTransform viewTransform =
                view.GetComponent<RectTransform>();
            viewTransform.anchoredPosition = Vector2.zero;
            view.Bind(hand[i], this);
            view.PrepareForDeal(parent);
            handViews.Add(view);

            if (isDealing)
            {
                view.AnimateDealFrom(
                    bagPosition,
                    i * dealStagger,
                    dealDuration);
            }
        }

        if (isDealing)
            StartCoroutine(FinishDealing(hand.Count));
    }

    private IEnumerator FinishDealing(int cardCount)
    {
        float wait = dealDuration + Mathf.Max(0, cardCount - 1) * dealStagger;
        yield return new WaitForSeconds(wait);
        isDealing = false;
    }

    private void PulseBag()
    {
        if (bagRoot == null)
            return;

        StopCoroutine(nameof(PulseBagRoutine));
        StartCoroutine(PulseBagRoutine());
    }

    private IEnumerator PulseBagRoutine()
    {
        Vector3 original = bagRoot.localScale;
        bagRoot.localScale = original * 1.15f;
        yield return new WaitForSeconds(0.08f);
        bagRoot.localScale = original;
    }

    private void ReturnHandToBag()
    {
        bag.AddRange(hand);
        hand.Clear();

        foreach (BattleCardView view in handViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        handViews.Clear();
        UpdateBagUI();
    }

    public bool CanDragCard(BattleCardView view)
    {
        return !isDealing &&
            view != null &&
            !view.IsDealing &&
            view.Data != null &&
            currentState == BattleState.PlayerTurn &&
            currentEnergy >= view.Data.energyCost &&
            hand.Contains(view.Data);
    }

    public void UpdateCardHover(
        BattleCardView view,
        PointerEventData eventData)
    {
        ClearTargetHighlights();

        if (!CanDragCard(view) || eventData == null)
            return;

        if (view.Data.type == BattleCardType.Attack)
        {
            if (TryGetBodyPartAtPointer(eventData, out EnemyBodyPartType type))
                HighlightImage(GetBodyPartImage(type));
            return;
        }

        if (IsOverPlayer(eventData))
            HighlightImage(
                playerPortraitImage != null
                    ? playerPortraitImage
                    : playerDropZone != null
                        ? playerDropZone.GetComponent<Image>()
                        : null);
    }

    public void ClearTargetHighlights()
    {
        if (highlightedImage != null)
            highlightedImage.color = highlightedOriginal;

        highlightedImage = null;
    }

    public bool TryPlayCard(
        BattleCardView view,
        PointerEventData eventData)
    {
        if (!CanDragCard(view) || eventData == null)
            return false;

        BattleCardData card = view.Data;
        bool played;

        if (card.type == BattleCardType.Attack)
            played = TryPlayAttackCard(card, eventData);
        else
            played = TryPlayDefenseCard(card, eventData);

        if (!played)
            return false;

        ClearTargetHighlights();
        currentEnergy -= card.energyCost;
        hand.Remove(card);
        handViews.Remove(view);
        bag.Add(card);
        Destroy(view.gameObject);

        UpdateEnergyUI();
        UpdateBlockUI();
        UpdateEnemyHealthUI();
        UpdatePlayerStatusUI();
        RefreshBodyPartButtons();
        UpdateBagUI();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (enemy != null && enemy.IsDefeated)
            BattleWon();

        return true;
    }

    private bool TryPlayAttackCard(
        BattleCardData card,
        PointerEventData eventData)
    {
        if (enemy == null ||
            !TryGetBodyPartAtPointer(eventData, out EnemyBodyPartType type))
        {
            SetBattleMessage("Перетащите карту атаки на часть тела врага.");
            return false;
        }

        if (!enemy.TryAttackBodyPart(type, card.damage, out string result))
        {
            SetBattleMessage(result);
            return false;
        }

        SetBattleMessage($"{card.displayName}: {result}");
        return true;
    }

    private bool TryPlayDefenseCard(
        BattleCardData card,
        PointerEventData eventData)
    {
        if (!IsOverPlayer(eventData))
        {
            SetBattleMessage("Перетащите карту защиты на персонажа.");
            return false;
        }

        currentBlock += card.block;
        SetBattleMessage(
            $"{card.displayName}: получено {card.block} защиты.");
        return true;
    }

    private bool TryGetBodyPartAtPointer(
        PointerEventData eventData,
        out EnemyBodyPartType type)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (MatchesButton(result.gameObject, headButton))
            {
                type = EnemyBodyPartType.Head;
                return true;
            }
            if (MatchesButton(result.gameObject, torsoButton))
            {
                type = EnemyBodyPartType.Torso;
                return true;
            }
            if (MatchesButton(result.gameObject, leftArmButton))
            {
                type = EnemyBodyPartType.LeftArm;
                return true;
            }
            if (MatchesButton(result.gameObject, rightArmButton))
            {
                type = EnemyBodyPartType.RightArm;
                return true;
            }
            if (MatchesButton(result.gameObject, leftLegButton))
            {
                type = EnemyBodyPartType.LeftLeg;
                return true;
            }
            if (MatchesButton(result.gameObject, rightLegButton))
            {
                type = EnemyBodyPartType.RightLeg;
                return true;
            }
        }

        type = default;
        return false;
    }

    private static bool MatchesButton(GameObject target, Button button)
    {
        return target != null &&
            button != null &&
            (target == button.gameObject ||
             target.transform.IsChildOf(button.transform));
    }

    private bool IsOverPlayer(PointerEventData eventData)
    {
        if (eventData == null)
            return false;

        Camera camera = eventData.pressEventCamera;

        if (playerPortraitImage != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                playerPortraitImage.rectTransform,
                eventData.position,
                camera))
        {
            return true;
        }

        return playerDropZone != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                playerDropZone,
                eventData.position,
                camera);
    }

    private Image GetBodyPartImage(EnemyBodyPartType type)
    {
        Button button = type switch
        {
            EnemyBodyPartType.Head => headButton,
            EnemyBodyPartType.Torso => torsoButton,
            EnemyBodyPartType.LeftArm => leftArmButton,
            EnemyBodyPartType.RightArm => rightArmButton,
            EnemyBodyPartType.LeftLeg => leftLegButton,
            EnemyBodyPartType.RightLeg => rightLegButton,
            _ => null
        };

        return button != null ? button.image : null;
    }

    private void HighlightImage(Image image)
    {
        if (image == null)
            return;

        highlightedImage = image;
        highlightedOriginal = image.color;
        image.color = new Color(1f, 0.86f, 0.28f, 1f);
    }

    private void UpdateBagUI()
    {
        RefreshLeftPanel();
    }

    private void AttackBodyPart(EnemyBodyPartType type)
    {
        if (currentState != BattleState.PlayerTurn ||
            currentEnergy <= 0 ||
            enemy == null)
        {
            return;
        }

        const int damage = 10;
        if (!enemy.TryAttackBodyPart(type, damage, out string result))
        {
            SetBattleMessage(result);
            RefreshBodyPartButtons();
            return;
        }

        currentEnergy--;
        SetBattleMessage(result);
        UpdateEnergyUI();
        UpdateEnemyHealthUI();
        RefreshBodyPartButtons();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Debug.Log("Energy: " + currentEnergy + "/" + maxEnergy);

        if (enemy.IsDefeated)
            BattleWon();
    }

    public void AttackHead()
    {
        AttackBodyPart(EnemyBodyPartType.Head);
    }

    public void AttackTorso()
    {
        AttackBodyPart(EnemyBodyPartType.Torso);
    }

    public void AttackLeftArm()
    {
        AttackBodyPart(EnemyBodyPartType.LeftArm);
    }

    public void AttackRightArm()
    {
        AttackBodyPart(EnemyBodyPartType.RightArm);
    }

    public void AttackLeftLeg()
    {
        AttackBodyPart(EnemyBodyPartType.LeftLeg);
    }

    public void AttackRightLeg()
    {
        AttackBodyPart(EnemyBodyPartType.RightLeg);
    }

    public void CancelBodyPartSelection()
    {
        ShowBodyPartsPanel(false);
        SetBattleMessage("Атака отменена.");
    }

    private void ShowBodyPartsPanel(bool visible)
    {
        if (bodyPartsPanel != null)
            bodyPartsPanel.SetActive(visible);
    }

    private void RefreshBodyPartButtons()
    {
        UpdateBodyPartButton(headButton, EnemyBodyPartType.Head);
        UpdateBodyPartButton(torsoButton, EnemyBodyPartType.Torso);
        UpdateBodyPartButton(leftArmButton, EnemyBodyPartType.LeftArm);
        UpdateBodyPartButton(rightArmButton, EnemyBodyPartType.RightArm);
        UpdateBodyPartButton(leftLegButton, EnemyBodyPartType.LeftLeg);
        UpdateBodyPartButton(rightLegButton, EnemyBodyPartType.RightLeg);
    }

    private void ConfigureBodyPartButtons()
    {
        ConfigureBodyPartButton(headButton, EnemyBodyPartType.Head);
        ConfigureBodyPartButton(torsoButton, EnemyBodyPartType.Torso);
        ConfigureBodyPartButton(leftArmButton, EnemyBodyPartType.LeftArm);
        ConfigureBodyPartButton(rightArmButton, EnemyBodyPartType.RightArm);
        ConfigureBodyPartButton(leftLegButton, EnemyBodyPartType.LeftLeg);
        ConfigureBodyPartButton(rightLegButton, EnemyBodyPartType.RightLeg);
    }

    private void ConfigureBodyPartButton(
        Button button,
        EnemyBodyPartType type)
    {
        if (button == null)
            return;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        if (button.image != null)
            button.image.alphaHitTestMinimumThreshold = alphaHitTestThreshold;

        BodyPartHoverTarget hover =
            button.GetComponent<BodyPartHoverTarget>();
        if (hover == null)
            hover = button.gameObject.AddComponent<BodyPartHoverTarget>();

        hover.Initialize(this, type);
    }

    public void ShowBodyPartInfo(
        EnemyBodyPartType type,
        RectTransform partRect)
    {
        if (enemy == null)
            return;

        hoverInfoText = enemy.GetBodyPartInfo(type);
        RefreshLeftPanel();
    }

    public void HideBodyPartInfo()
    {
        hoverInfoText = "";
        RefreshLeftPanel();
    }

    public void ShowCardInfo(BattleCardData card)
    {
        ShowCardInfo(null, card);
    }

    public void ShowCardInfo(BattleCardView view, BattleCardData card)
    {
        if (card == null)
        {
            HideCardInfo(view);
            return;
        }

        hoveredCard = view;
        hoverInfoText =
            $"{card.displayName}\n" +
            $"Энергия: {card.energyCost}\n" +
            card.description;
        RefreshLeftPanel();
    }

    public void HideCardInfo(BattleCardView view)
    {
        if (view != null && hoveredCard != null && hoveredCard != view)
            return;

        hoveredCard = null;
        HideBodyPartInfo();
    }

    private void PositionBodyPartInfo(RectTransform partRect)
    {
        if (bodyPartInfoRoot == null || partRect == null)
            return;

        RectTransform parent = bodyPartInfoRoot.parent as RectTransform;
        Camera camera = null;
        Canvas canvas = bodyPartInfoRoot.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            camera = canvas.worldCamera;

        Vector3[] corners = new Vector3[4];
        partRect.GetWorldCorners(corners);
        Vector3 left = (corners[0] + corners[1]) * 0.5f;
        Vector3 right = (corners[2] + corners[3]) * 0.5f;
        Vector3 center = (left + right) * 0.5f;
        bool placeRight = center.x < Screen.width * 0.5f;
        Vector3 world = placeRight
            ? right + new Vector3(24f, 0f, 0f)
            : left - new Vector3(24f, 0f, 0f);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                world,
                camera,
                out Vector2 local))
        {
            bodyPartInfoRoot.anchorMin = new Vector2(0.5f, 0.5f);
            bodyPartInfoRoot.anchorMax = new Vector2(0.5f, 0.5f);
            bodyPartInfoRoot.pivot = placeRight
                ? new Vector2(0f, 0.5f)
                : new Vector2(1f, 0.5f);
            bodyPartInfoRoot.anchoredPosition = local;
        }
    }

    private void UpdateBodyPartButton(
        Button button,
        EnemyBodyPartType type)
    {
        if (button == null || enemy == null)
            return;

        EnemyBodyPart part = enemy.GetBodyPart(type);
        if (part == null)
            return;

        button.interactable = !part.IsDestroyed;

        if (button.image != null)
        {
            button.image.enabled = true;
            button.image.material = part.IsDestroyed
                ? destroyedPartMaterial
                : null;
        }
    }

    private void UpdateEnemyHealthUI()
    {
        if (enemyHealthSlider == null || enemy == null)
            return;

        enemyHealthSlider.minValue = 0;
        enemyHealthSlider.maxValue = enemy.MaxHealth;
        enemyHealthSlider.value = enemy.CurrentHealth;
    }

    private void FitBattleToScreen()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            CanvasScaler scaler = canvases[i].GetComponent<CanvasScaler>();
            if (scaler == null)
                continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (battleBackgroundImage != null)
        {
            RectTransform rect = battleBackgroundImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling();
            battleBackgroundImage.preserveAspect = false;
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.03f, 0.03f, 1f);
        }
    }

    private void ApplyBattlePresentation()
    {
        FitBattleToScreen();
        BattleEnemyDefinition definition =
            BattleEncounterData.EnemyDefinition;

        Sprite background = BattleEncounterData.BackgroundOverride;
        if (background == null && definition != null)
            background = definition.defaultBackground;

        if (battleBackgroundImage != null && background != null)
        {
            battleBackgroundImage.sprite = background;
            battleBackgroundImage.color = Color.white;
        }

        ApplyBagSprite();
        ApplyEnemyName(definition);

        if (definition == null)
            return;

        ApplyPartSprite(headButton, definition.head);
        ApplyPartSprite(torsoButton, definition.torso);
        ApplyPartSprite(leftArmButton, definition.leftArm);
        ApplyPartSprite(rightArmButton, definition.rightArm);
        ApplyPartSprite(leftLegButton, definition.leftLeg);
        ApplyPartSprite(rightLegButton, definition.rightLeg);
    }

    private static void ApplyPartSprite(Button button, Sprite sprite)
    {
        if (button == null || button.image == null || sprite == null)
            return;

        button.image.sprite = sprite;
        button.image.preserveAspect = true;
    }

    private void ApplyEnemyName(BattleEnemyDefinition definition)
    {
        EnsureBattleHud();
        if (enemyNameText == null)
            return;

        string name = definition != null &&
            !string.IsNullOrWhiteSpace(definition.displayName)
            ? definition.displayName
            : enemy != null
                ? enemy.DisplayName
                : "Враг";

        enemyNameText.text = name;
        enemyNameText.gameObject.SetActive(true);
    }

    private void ApplyPlayerHealthBarStyle()
    {
        if (playerHealthSlider == null)
            return;

        if (playerHealthFill == null && playerHealthSlider.fillRect != null)
            playerHealthFill = playerHealthSlider.fillRect.GetComponent<Image>();

        playerHealthSlider.interactable = false;

        if (playerHealthFill != null)
            playerHealthFill.color = playerHealthColor;

        Image background = playerHealthSlider.transform.Find("Background")
            ?.GetComponent<Image>();
        if (background != null)
            background.color = new Color(0.18f, 0.05f, 0.05f, 0.9f);

        if (playerHealthSlider.handleRect != null)
            playerHealthSlider.handleRect.gameObject.SetActive(false);

        if (enemyHealthSlider == null)
            return;

        enemyHealthSlider.interactable = false;

        if (enemyHealthSlider.fillRect != null)
        {
            Image enemyFill = enemyHealthSlider.fillRect.GetComponent<Image>();
            if (enemyFill != null)
                enemyFill.color = playerHealthColor;
        }

        Image enemyBackground = enemyHealthSlider.transform.Find("Background")
            ?.GetComponent<Image>();
        if (enemyBackground != null)
            enemyBackground.color = new Color(0.18f, 0.05f, 0.05f, 0.9f);

        if (enemyHealthSlider.handleRect != null)
            enemyHealthSlider.handleRect.gameObject.SetActive(false);
    }

    private void ApplyEndTurnBell(bool createIfMissing = true)
    {
        if (endTurnButton == null)
        {
            GameObject found = GameObject.Find("EndTurnButton");
            if (found != null)
                endTurnButton = found.GetComponent<Button>();
        }

        if (endTurnButton == null)
            return;

        EndTurnBellButton bell =
            endTurnButton.GetComponent<EndTurnBellButton>();
        if (bell == null && createIfMissing)
            bell = endTurnButton.gameObject.AddComponent<EndTurnBellButton>();

        if (bell != null)
            bell.ApplyBellSprite(endTurnBellSprite);

        Transform label = endTurnButton.transform.Find("Text (TMP)");
        if (label != null)
        {
            TMP_Text text = label.GetComponent<TMP_Text>();
            if (text != null &&
                (endTurnBellSprite != null ||
                 (endTurnButton.image != null &&
                  endTurnButton.image.sprite != null &&
                  endTurnButton.image.sprite.name != "UISprite" &&
                  endTurnButton.image.sprite.name != "Background")))
            {
                text.text = "";
            }
        }
    }

    private void EnsureBattleHud()
    {
        Canvas overlay = GetHudCanvas();
        Transform infoParent = bodyPartsPanel != null
            ? bodyPartsPanel.transform.parent
            : overlay != null
                ? overlay.transform
                : null;
        if (infoParent == null)
            return;

        if (bodyPartInfoRoot == null || battleMessageText == null)
        {
            GameObject panel = new GameObject(
                "BattleInfoPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            panel.transform.SetParent(infoParent, false);
            bodyPartInfoRoot = panel.GetComponent<RectTransform>();
            bodyPartInfoRoot.anchorMin = new Vector2(0.5f, 0.5f);
            bodyPartInfoRoot.anchorMax = new Vector2(0.5f, 0.5f);
            bodyPartInfoRoot.pivot = new Vector2(0f, 0.5f);
            bodyPartInfoRoot.anchoredPosition = new Vector2(260f, 40f);
            bodyPartInfoRoot.sizeDelta = new Vector2(360f, 240f);

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.06f, 0.04f, 0.86f);
            panelImage.raycastTarget = false;

            battleMessageText = CreateHudText(
                bodyPartInfoRoot,
                "Text",
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(-28f, -28f),
                26f,
                TextAlignmentOptions.TopLeft);
            battleMessageText.color = new Color(1f, 0.93f, 0.72f, 1f);
            bodyPartInfoText = battleMessageText;
            panel.SetActive(false);
        }

        PlaceHoverPanelRightOfEnemy();

        if (bodyPartInfoRoot != null)
            bodyPartInfoRoot.gameObject.SetActive(
                !string.IsNullOrEmpty(hoverInfoText));
    }

    private void PlaceHoverPanelRightOfEnemy()
    {
        if (bodyPartInfoRoot == null)
            return;

        bodyPartInfoRoot.anchorMin = new Vector2(0.5f, 0.5f);
        bodyPartInfoRoot.anchorMax = new Vector2(0.5f, 0.5f);
        bodyPartInfoRoot.pivot = new Vector2(0f, 0.5f);
        bodyPartInfoRoot.anchoredPosition = new Vector2(260f, 40f);
        bodyPartInfoRoot.sizeDelta = new Vector2(360f, 240f);
    }

    private Canvas GetHudCanvas()
    {
        if (handRoot != null)
        {
            Canvas canvas = handRoot.GetComponentInParent<Canvas>();
            if (canvas != null)
                return canvas;
        }

        return FindFirstObjectByType<Canvas>();
    }

    private static TMP_Text CreateHudText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        BattleUiFonts.Apply(text);
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        return text;
    }

    private void ApplyBagSprite()
    {
        if (bagImage == null)
            return;

        if (bagSprite != null)
            bagImage.sprite = bagSprite;
        else if (bagImage.sprite == null)
            bagImage.sprite = Resources.Load<Sprite>("UI/Veshi/sack_011");

        bagImage.preserveAspect = true;
        bagImage.enabled = bagImage.sprite != null;
        bagImage.raycastTarget = false;
    }

    private void RefreshLeftPanel()
    {
        EnsureBattleHud();
        RefreshStatusText();
        UpdateBagCountText();

        bool hasHover = !string.IsNullOrEmpty(hoverInfoText);
        if (bodyPartInfoRoot != null)
            bodyPartInfoRoot.gameObject.SetActive(hasHover);

        if (battleMessageText != null)
            battleMessageText.text = hasHover ? hoverInfoText : "";
    }

    private void UpdateBagCountText()
    {
        if (bagText == null)
            return;

        int deckCount = GameState.CardDeck.Count;
        bagText.text = $"{bag.Count}/{deckCount}";
    }

    private void UpdatePlayerStatusUI()
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.minValue = 0;
            playerHealthSlider.maxValue = GameState.PlayerMaxHealth;
            playerHealthSlider.value = GameState.PlayerHealth;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text =
                $"{GameState.PlayerHealth}/{GameState.PlayerMaxHealth}";
        }

        if (playerLevelText != null)
            playerLevelText.text = GameState.PlayerLevel.ToString();

        if (playerMoneyText != null)
            playerMoneyText.text = GameState.PlayerMoney.ToString();

        RefreshLeftPanel();
        StatusPlaqueHud plaque = FindFirstObjectByType<StatusPlaqueHud>();
        if (plaque != null)
            plaque.Refresh();
    }

    private void RefreshStatusText()
    {
        if (energyText == null)
            return;

        energyText.text = $"{currentEnergy}/{maxEnergy}";
    }
}
