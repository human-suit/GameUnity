using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnBellButton : MonoBehaviour
{
    [SerializeField] private Image bellImage;
    [SerializeField] private Sprite bellSprite;
    [SerializeField] private float swingAngle = 18f;
    [SerializeField] private float swingDuration = 0.5f;

    private RectTransform _rect;
    private Quaternion _restRotation;
    private Coroutine _swingRoutine;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _restRotation = _rect != null
            ? _rect.localRotation
            : Quaternion.identity;

        if (bellImage == null)
            bellImage = GetComponent<Image>();

        ApplyBellSprite();
    }

    private void OnValidate()
    {
        if (bellImage == null)
            bellImage = GetComponent<Image>();

        ApplyBellSprite();
    }

    public void ApplyBellSprite(Sprite sprite = null)
    {
        if (sprite != null)
            bellSprite = sprite;

        if (bellImage == null)
            return;

        if (bellSprite != null)
            bellImage.sprite = bellSprite;

        if (bellImage.sprite != null)
        {
            bellImage.type = Image.Type.Simple;
            bellImage.preserveAspect = true;
            bellImage.color = Color.white;
        }
    }

    public void PlaySwing()
    {
        if (_rect == null)
            return;

        if (_swingRoutine != null)
            StopCoroutine(_swingRoutine);

        _swingRoutine = StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, swingDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float decay = 1f - t;
            float angle = Mathf.Sin(t * Mathf.PI * 6f) * swingAngle * decay;
            _rect.localRotation = _restRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        _rect.localRotation = _restRotation;
        _swingRoutine = null;
    }
}
