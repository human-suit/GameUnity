using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class PlayerPortraitSlot : MonoBehaviour
{
    private Image portraitImage;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (portraitImage == null)
            portraitImage = GetComponent<Image>();

        if (portraitImage != null)
            portraitImage.enabled = portraitImage.sprite != null;
    }
}
