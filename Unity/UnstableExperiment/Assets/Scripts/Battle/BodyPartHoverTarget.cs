using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BodyPartHoverTarget : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ICanvasRaycastFilter
{
    private BattleManager _battle;
    private EnemyBodyPartType _type;

    public void Initialize(BattleManager battle, EnemyBodyPartType type)
    {
        _battle = battle;
        _type = type;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_battle != null)
            _battle.ShowBodyPartInfo(_type, transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_battle != null)
            _battle.HideBodyPartInfo();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return IsOnVisiblePixels(GetComponent<Image>(), screenPoint, eventCamera);
    }

    private static bool IsOnVisiblePixels(
        Image image,
        Vector2 screenPoint,
        Camera eventCamera)
    {
        if (image == null || image.sprite == null)
            return false;

        RectTransform rectTransform = image.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            screenPoint,
            eventCamera,
            out Vector2 local))
        {
            return false;
        }

        Rect rect = rectTransform.rect;
        float drawWidth = rect.width;
        float drawHeight = rect.height;
        Sprite sprite = image.sprite;
        Rect spriteRect = sprite.textureRect;

        if (image.preserveAspect && spriteRect.height > 0f && rect.height > 0f)
        {
            float spriteAspect = spriteRect.width / spriteRect.height;
            float rectAspect = rect.width / rect.height;
            if (spriteAspect > rectAspect)
                drawHeight = rect.width / spriteAspect;
            else
                drawWidth = rect.height * spriteAspect;
        }

        Vector2 center = rect.center;
        float minX = center.x - drawWidth * 0.5f;
        float minY = center.y - drawHeight * 0.5f;
        if (local.x < minX || local.x > minX + drawWidth ||
            local.y < minY || local.y > minY + drawHeight)
        {
            return false;
        }

        Texture2D texture = sprite.texture;
        if (texture == null)
            return true;

        try
        {
            float u = (local.x - minX) / drawWidth;
            float v = (local.y - minY) / drawHeight;
            int x = Mathf.FloorToInt(spriteRect.x + u * spriteRect.width);
            int y = Mathf.FloorToInt(spriteRect.y + v * spriteRect.height);
            return texture.GetPixel(x, y).a >= image.alphaHitTestMinimumThreshold;
        }
        catch
        {
            return true;
        }
    }
}
