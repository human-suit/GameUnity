using UnityEngine;

/// <summary>
/// Зона двери. RoomManager создаёт их автоматически.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTrigger : MonoBehaviour
{
    public string doorId;
    public string labelRu;
    public string targetRoom;
    public string requiresKey;
    public string lockedHintRu;

    [Header("Подсказка E")]
    public float promptRadius = 1.8f;
    public float promptHeight = 1.1f;

    private Transform _promptRoot;
    private static Transform _player;

    public bool IsLocked =>
        !string.IsNullOrEmpty(requiresKey) && !GameState.HasKey(requiresKey);

    public string HintText
    {
        get
        {
            if (IsLocked)
                return string.IsNullOrEmpty(lockedHintRu) ? "Заперто" : lockedHintRu;
            return $"[E] {labelRu}";
        }
    }

    private void Start() => BuildPrompt();

    private void Update()
    {
        if (_promptRoot == null)
            return;

        if (_player == null)
        {
            var playerGo = GameObject.Find("Player");
            if (playerGo != null)
                _player = playerGo.transform;
        }

        if (_player == null)
            return;

        float dist = Vector2.Distance(_player.position, transform.position);
        _promptRoot.gameObject.SetActive(dist <= promptRadius);
    }

    private void BuildPrompt()
    {
        _promptRoot = new GameObject("Prompt_E").transform;
        _promptRoot.SetParent(transform, false);
        _promptRoot.localPosition = new Vector3(0f, promptHeight, 0f);
        _promptRoot.gameObject.SetActive(false);

        var bg = new GameObject("Bg");
        bg.transform.SetParent(_promptRoot, false);
        var bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = PromptSprites.Circle;
        bgSr.color = new Color(0f, 0f, 0f, 0.75f);
        bgSr.sortingOrder = 50;

        var letter = new GameObject("Letter");
        letter.transform.SetParent(_promptRoot, false);
        var tm = letter.AddComponent<TextMesh>();
        tm.text = "E";
        tm.fontSize = 64;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1f, 0.92f, 0.2f, 1f);
        tm.fontStyle = FontStyle.Bold;

        var meshRenderer = letter.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.sortingOrder = 51;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = IsLocked ? new Color(1f, 0.3f, 0.2f, 0.85f) : new Color(0.2f, 1f, 0.3f, 0.85f);
        Gizmos.DrawWireCube(transform.position, new Vector3(1.8f, 1.35f, 0f));
        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, promptRadius);
    }
#endif
}

static class PromptSprites
{
    private static Sprite _circle;

    public static Sprite Circle
    {
        get
        {
            if (_circle != null)
                return _circle;

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float r = size * 0.5f - 1f;
            var center = new Vector2(r + 1f, r + 1f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
                }
            }

            tex.Apply();
            _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _circle;
        }
    }
}
