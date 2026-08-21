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
    [SerializeField] private Slider enemyHealthSlider;
    [Range(0.01f, 1f)]
    [SerializeField] private float alphaHitTestThreshold = 0.1f;
    [SerializeField] private Material destroyedPartMaterial;

    [Header("Cards")]
    [SerializeField] private List<BattleCardData> startingDeck =
        new List<BattleCardData>();
    [SerializeField] private BattleCardView cardViewPrefab;
    [SerializeField] private RectTransform handRoot;
    [SerializeField] private List<RectTransform> handSlots =
        new List<RectTransform>();
    [SerializeField] private RectTransform playerDropZone;
    [SerializeField] private TMP_Text bagText;

    [Header("Battle presentation")]
    [SerializeField] private Image battleBackgroundImage;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] private Sprite playerPortrait;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private TMP_Text playerLevelText;
    [SerializeField] private TMP_Text playerMoneyText;

    public BattleState currentState;
    private string battleMessage = "Выберите действие.";
    private readonly List<BattleCardData> bag = new List<BattleCardData>();
    private readonly List<BattleCardData> hand = new List<BattleCardData>();
    private readonly List<BattleCardView> handViews =
        new List<BattleCardView>();

    private void OnValidate()
    {
        if (playerPortraitImage == null)
            return;

        if (playerPortrait != null)
            playerPortraitImage.sprite = playerPortrait;

        playerPortraitImage.preserveAspect = true;
        playerPortraitImage.enabled = playerPortraitImage.sprite != null;
    }

    private void Start()
    {
        currentState = BattleState.PlayerTurn;
        currentEnergy = maxEnergy;
        UpdateEnergyUI();
        UpdateBlockUI();
        ShowBodyPartsPanel(true);
        UpdateEnemyHealthUI();
        ConfigureBodyPartButtons();
        RefreshBodyPartButtons();
        ApplyBattlePresentation();
        InitializeCardBattle();
        UpdatePlayerStatusUI();
        SetBattleMessage("Выберите действие.");

        Debug.Log("=== BATTLE STARTED ===");
        Debug.Log("Player Turn");
    }

    public void EndPlayerTurn()
    {
        Debug.Log("End turn botton pressed");

        if (currentState != BattleState.PlayerTurn)
            return;

        ReturnHandToBag();
        ShowBodyPartsPanel(false);
        Debug.Log("end player turn");

        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        currentState = BattleState.EnemyTurn;
        ShowBodyPartsPanel(false);

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
        if (blockText != null)
            blockText.text = $"Block: {currentBlock}";
    }

    public void SetBattleMessage(string message)
    {
        battleMessage = message;

        if (battleMessageText != null)
            battleMessageText.text = battleMessage;
    }

    private void InitializeCardBattle()
    {
        GameState.InitializeCardDeck(startingDeck);

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

        DrawCardsOfType(BattleCardType.Attack, 3);
        DrawCardsOfType(BattleCardType.Defense, 2);

        while (hand.Count < 5 && bag.Count > 0)
            DrawRandomCardFromBag();

        RebuildHandViews();
        UpdateBagUI();
    }

    private void DrawCardsOfType(BattleCardType type, int count)
    {
        for (int drawn = 0; drawn < count; drawn++)
        {
            List<int> matchingIndices = new List<int>();

            for (int i = 0; i < bag.Count; i++)
            {
                if (bag[i] != null && bag[i].type == type)
                    matchingIndices.Add(i);
            }

            if (matchingIndices.Count == 0)
                return;

            int randomMatch = Random.Range(0, matchingIndices.Count);
            int bagIndex = matchingIndices[randomMatch];
            hand.Add(bag[bagIndex]);
            bag.RemoveAt(bagIndex);
        }
    }

    private void DrawRandomCardFromBag()
    {
        if (bag.Count == 0)
            return;

        int index = Random.Range(0, bag.Count);
        hand.Add(bag[index]);
        bag.RemoveAt(index);
    }

    private void RebuildHandViews()
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
            handViews.Add(view);
        }
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
        return view != null &&
            view.Data != null &&
            currentState == BattleState.PlayerTurn &&
            currentEnergy >= view.Data.energyCost &&
            hand.Contains(view.Data);
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
        if (playerDropZone == null ||
            !RectTransformUtility.RectangleContainsScreenPoint(
                playerDropZone,
                eventData.position,
                eventData.pressEventCamera))
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

    private void UpdateBagUI()
    {
        if (bagText != null)
            bagText.text = $"Мешок: {bag.Count}/{GameState.CardDeck.Count}";
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
        ConfigureBodyPartButton(headButton);
        ConfigureBodyPartButton(torsoButton);
        ConfigureBodyPartButton(leftArmButton);
        ConfigureBodyPartButton(rightArmButton);
        ConfigureBodyPartButton(leftLegButton);
        ConfigureBodyPartButton(rightLegButton);
    }

    private void ConfigureBodyPartButton(Button button)
    {
        if (button == null)
            return;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        if (button.image != null)
            button.image.alphaHitTestMinimumThreshold = alphaHitTestThreshold;
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

    private void ApplyBattlePresentation()
    {
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

        if (playerPortraitImage != null)
        {
            if (playerPortrait != null)
                playerPortraitImage.sprite = playerPortrait;

            playerPortraitImage.enabled =
                playerPortraitImage.sprite != null;
        }

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
                $"HP: {GameState.PlayerHealth}/{GameState.PlayerMaxHealth}";
        }

        PlayerData playerData = GameManager.Instance != null
            ? GameManager.Instance.playerData
            : null;

        if (playerLevelText != null)
        {
            playerLevelText.text = playerData != null
                ? $"LVL: {playerData.level}"
                : "LVL: 1";
        }

        if (playerMoneyText != null)
        {
            playerMoneyText.text = playerData != null
                ? $"Золото: {playerData.money}"
                : "Золото: 0";
        }

        RefreshStatusText();
    }

    private void RefreshStatusText()
    {
        if (energyText == null)
            return;

        PlayerData playerData = GameManager.Instance != null
            ? GameManager.Instance.playerData
            : null;
        int level = playerData != null ? playerData.level : 1;
        int money = playerData != null ? playerData.money : 0;

        energyText.text =
            $"HP: {GameState.PlayerHealth}/{GameState.PlayerMaxHealth}\n" +
            $"LVL: {level}\n" +
            $"Золото: {money}\n" +
            $"Энергия: {currentEnergy}/{maxEnergy}";
    }
}
