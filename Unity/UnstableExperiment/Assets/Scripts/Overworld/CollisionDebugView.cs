using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Фиолетовая подсветка слоя Collision только в редакторе (в Play скрыта).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Tilemap))]
public class CollisionDebugView : MonoBehaviour
{
    private TilemapRenderer _renderer;
    private Tilemap _tilemap;

    private void OnEnable()
    {
        _renderer = GetComponent<TilemapRenderer>();
        _tilemap = GetComponent<Tilemap>();
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    private void Apply()
    {
        if (_renderer == null || _tilemap == null)
            return;

        _renderer.enabled = !Application.isPlaying;
        _renderer.sortingOrder = 100;
        _tilemap.color = new Color(0.2f, 1f, 0.3f, 0.35f);
    }
}
