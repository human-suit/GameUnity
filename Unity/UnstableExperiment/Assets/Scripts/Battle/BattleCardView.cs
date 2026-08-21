using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class BattleCardView : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Editable card UI")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image artworkImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Text fallbackTitleText;
    [SerializeField] private Text fallbackDescriptionText;
    [SerializeField] private Text fallbackEnergyText;
    [SerializeField] private Color attackColor = new Color(0.45f, 0.12f, 0.12f);
    [SerializeField] private Color defenseColor = new Color(0.12f, 0.25f, 0.45f);

    public BattleCardData Data { get; private set; }

    private BattleManager _battle;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Canvas _rootCanvas;
    private Transform _handParent;
    private int _handSiblingIndex;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Bind(BattleCardData data, BattleManager battle)
    {
        Data = data;
        _battle = battle;

        if (titleText != null)
            titleText.text = data != null ? data.displayName : "?";
        if (descriptionText != null)
            descriptionText.text = data != null ? data.description : "";
        if (energyText != null)
            energyText.text = data != null ? data.energyCost.ToString() : "0";
        if (fallbackTitleText != null)
            fallbackTitleText.text = data != null ? data.displayName : "?";
        if (fallbackDescriptionText != null)
            fallbackDescriptionText.text =
                data != null ? data.description : "";
        if (fallbackEnergyText != null)
            fallbackEnergyText.text =
                data != null ? data.energyCost.ToString() : "0";

        if (artworkImage != null)
        {
            artworkImage.sprite = data != null ? data.artwork : null;
            artworkImage.enabled = data != null && data.artwork != null;
        }

        if (cardBackground != null && data != null)
        {
            if (data.artwork != null && artworkImage == null)
            {
                cardBackground.sprite = data.artwork;
                cardBackground.type = Image.Type.Simple;
                cardBackground.preserveAspect = true;
                cardBackground.color = Color.white;
            }
            else
            {
                cardBackground.color = data.type == BattleCardType.Attack
                    ? attackColor
                    : defenseColor;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_battle == null || !_battle.CanDragCard(this))
            return;

        _handParent = transform.parent;
        _handSiblingIndex = transform.GetSiblingIndex();
        _canvasGroup.blocksRaycasts = false;

        if (_rootCanvas != null)
            transform.SetParent(_rootCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_handParent == null)
            return;

        float scaleFactor = _rootCanvas != null
            ? _rootCanvas.scaleFactor
            : 1f;
        _rectTransform.anchoredPosition += eventData.delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_handParent == null)
            return;

        _canvasGroup.blocksRaycasts = true;

        bool played = _battle != null &&
            _battle.TryPlayCard(this, eventData);

        if (!played)
            ReturnToHand();

        _handParent = null;
    }

    private void ReturnToHand()
    {
        if (_handParent == null)
            return;

        transform.SetParent(_handParent, false);
        transform.SetSiblingIndex(_handSiblingIndex);
        _rectTransform.anchoredPosition = Vector2.zero;
        _rectTransform.localRotation = Quaternion.identity;
        _rectTransform.localScale = Vector3.one;
    }
}
