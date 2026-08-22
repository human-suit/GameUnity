using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class BattleCardView : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
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
    [SerializeField] private float hoverScale = 1.35f;
    [SerializeField] private float hoverLift = 28f;
    [SerializeField] private float hoverAnimDuration = 0.22f;

    public BattleCardData Data { get; private set; }
    public bool IsDealing { get; private set; }

    private BattleManager _battle;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Transform _handParent;
    private Transform _dealSlot;
    private int _handSiblingIndex;
    private Vector2 _restAnchoredPosition;
    private bool _isDragging;
    private bool _isHovered;
    private Coroutine _hoverRoutine;
    private Vector2 _dragScreenOffset;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _restAnchoredPosition = _rectTransform.anchoredPosition;
    }

    public void Bind(BattleCardData data, BattleManager battle)
    {
        Data = data;
        _battle = battle;

        if (titleText != null)
            titleText.text = "";
        if (descriptionText != null)
            descriptionText.text = "";
        if (energyText != null)
            energyText.text = data != null ? data.energyCost.ToString() : "0";
        if (fallbackTitleText != null)
            fallbackTitleText.text = "";
        if (fallbackDescriptionText != null)
            fallbackDescriptionText.text = "";
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
            if (data.artwork != null)
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

    public void PrepareForDeal(Transform slot)
    {
        _dealSlot = slot;
        _handParent = null;
        _handSiblingIndex = transform.GetSiblingIndex();
    }

    public void AnimateDealFrom(
        Vector3 startWorldPosition,
        float delay,
        float duration)
    {
        StopAllCoroutines();
        StartCoroutine(DealRoutine(startWorldPosition, delay, duration));
    }

    private IEnumerator DealRoutine(
        Vector3 startWorldPosition,
        float delay,
        float duration)
    {
        IsDealing = true;
        if (_hoverRoutine != null)
            StopCoroutine(_hoverRoutine);
        _hoverRoutine = null;
        if (_canvasGroup != null)
            _canvasGroup.blocksRaycasts = false;

        Vector3 targetPosition = transform.position;
        Vector3 targetScale = Vector3.one;
        transform.position = startWorldPosition;
        transform.localScale = Vector3.one * 0.35f;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);
        Vector3 startScale = transform.localScale;

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / safeDuration);
            transform.position = Vector3.Lerp(
                startWorldPosition,
                targetPosition,
                t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        Transform slot = _dealSlot != null ? _dealSlot : transform.parent;
        transform.SetParent(slot, false);
        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.localRotation = Quaternion.identity;
            _rectTransform.localScale = Vector3.one;
            _restAnchoredPosition = Vector2.zero;
        }

        if (_canvasGroup != null)
            _canvasGroup.blocksRaycasts = true;
        IsDealing = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsDealing || _isDragging)
            return;

        _isHovered = true;
        if (_battle != null)
            _battle.ShowCardInfo(this, Data);
        ApplyHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        if (_battle != null && !_isDragging)
            _battle.HideCardInfo(this);
        if (_isDragging || IsDealing)
            return;

        ApplyHover(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsDealing || _battle == null || !_battle.CanDragCard(this))
            return;

        _isDragging = true;
        StopHoverForDrag();
        _handParent = transform.parent;
        _handSiblingIndex = transform.GetSiblingIndex();
        _canvasGroup.blocksRaycasts = false;

        Canvas canvas = FindHandCanvas();
        RectTransform canvasRect = canvas != null
            ? canvas.transform as RectTransform
            : null;
        Camera camera = EventCamera(canvas, eventData);
        Vector2 cardScreen = RectTransformUtility.WorldToScreenPoint(
            camera,
            _rectTransform.position);
        _dragScreenOffset = cardScreen - eventData.position;

        if (canvasRect != null)
        {
            transform.SetParent(canvasRect, false);
            transform.SetAsLastSibling();
        }

        _rectTransform.localScale = Vector3.one;
        _rectTransform.localRotation = Quaternion.identity;
        SetDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || _handParent == null)
            return;

        SetDragPosition(eventData);
        _battle.UpdateCardHover(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging || _handParent == null)
            return;

        _canvasGroup.blocksRaycasts = true;

        bool played = _battle != null &&
            _battle.TryPlayCard(this, eventData);

        if (_battle != null)
            _battle.ClearTargetHighlights();

        if (!played)
            ReturnToHand();

        _handParent = null;
        _isDragging = false;
        if (_isHovered)
            ApplyHover(true);
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
        _restAnchoredPosition = Vector2.zero;
    }

    private void StopHoverForDrag()
    {
        if (_hoverRoutine != null)
            StopCoroutine(_hoverRoutine);
        _hoverRoutine = null;
    }

    private void SetDragPosition(PointerEventData eventData)
    {
        RectTransform parent = _rectTransform.parent as RectTransform;
        if (parent == null || eventData == null)
            return;

        Canvas canvas = parent.GetComponentInParent<Canvas>();
        Camera camera = EventCamera(canvas, eventData);
        Vector2 screen = eventData.position + _dragScreenOffset;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                screen,
                camera,
                out Vector2 local))
        {
            _rectTransform.anchoredPosition = local;
        }
    }

    private Canvas FindHandCanvas()
    {
        Transform from = _handParent != null
            ? _handParent
            : _dealSlot != null
                ? _dealSlot
                : transform.parent;
        if (from == null)
            return null;

        Canvas canvas = from.GetComponentInParent<Canvas>();
        return canvas != null ? canvas.rootCanvas : null;
    }

    private static Camera EventCamera(Canvas canvas, PointerEventData eventData)
    {
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        if (canvas != null && canvas.worldCamera != null)
            return canvas.worldCamera;

        return eventData != null ? eventData.pressEventCamera : null;
    }

    private void ApplyHover(bool hovered)
    {
        if (_rectTransform == null || _isDragging)
            return;

        if (_hoverRoutine != null)
            StopCoroutine(_hoverRoutine);

        if (!isActiveAndEnabled)
        {
            ApplyHoverInstant(hovered);
            return;
        }

        _hoverRoutine = StartCoroutine(HoverRoutine(hovered));
    }

    private IEnumerator HoverRoutine(bool hovered)
    {
        Vector3 startScale = _rectTransform.localScale;
        Vector2 startPosition = _rectTransform.anchoredPosition;
        Vector3 targetScale = hovered
            ? Vector3.one * hoverScale
            : Vector3.one;
        Vector2 targetPosition = hovered
            ? _restAnchoredPosition + new Vector2(0f, hoverLift)
            : _restAnchoredPosition;
        float duration = Mathf.Max(0.05f, hoverAnimDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            _rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                t);
            yield return null;
        }

        _rectTransform.localScale = targetScale;
        _rectTransform.anchoredPosition = targetPosition;
        _hoverRoutine = null;
    }

    private void ApplyHoverInstant(bool hovered)
    {
        _rectTransform.localScale = hovered
            ? Vector3.one * hoverScale
            : Vector3.one;
        _rectTransform.anchoredPosition = hovered
            ? _restAnchoredPosition + new Vector2(0f, hoverLift)
            : _restAnchoredPosition;
    }
}
