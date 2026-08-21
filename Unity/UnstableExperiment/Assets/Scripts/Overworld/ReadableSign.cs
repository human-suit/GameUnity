using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Табличка: подойти, E — прочитать. Панель не создаётся заранее.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ReadableSign : MonoBehaviour
{
    [TextArea(3, 8)]
    [SerializeField] private string text = "Из стены сочится какая-то чёрная жидкость...";
    [SerializeField] private Sprite portrait;

    [SerializeField] private GameObject prompt;

    private static readonly HashSet<ReadableSign> Nearby = new HashSet<ReadableSign>();

    public static bool AnyNearby => Nearby.Count > 0;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;

        if (prompt != null)
            prompt.SetActive(false);
    }

    private void OnDisable()
    {
        Nearby.Remove(this);
    }

    private void Update()
    {
        if (ReadPanel.IsOpen || ReadPanel.ClosedThisFrame)
            return;

        if (!Nearby.Contains(this) || !Input.GetKeyDown(KeyCode.E))
            return;

        if (prompt != null)
            prompt.SetActive(false);

        ReadPanel.Show(text, portrait);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() == null && other.GetComponentInParent<PlayerMove>() == null)
            return;

        Nearby.Add(this);

        if (prompt != null && !ReadPanel.IsOpen)
            prompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMove>() == null && other.GetComponentInParent<PlayerMove>() == null)
            return;

        Nearby.Remove(this);

        if (prompt != null)
            prompt.SetActive(false);
    }
}
