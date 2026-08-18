using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text text;

    [SerializeField] private Color normalColor = new Color32(45, 38, 30, 255);
    [SerializeField] private Color hoverColor = new Color32(200, 200, 200, 255);

    private void Start()
    {
        text.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }
}
