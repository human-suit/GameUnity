using UnityEngine;

/// <summary>
/// Круг света вокруг игрока — остальная карта в темноте (как Fear and Hunger).
/// Вешается на Main Camera. В Player перетащи объект Player.
/// </summary>
public class VisionFog : MonoBehaviour
{
    [Tooltip("Объект Player на сцене")]
    public Transform player;

    [Tooltip("Радиус яркого круга вокруг героя (при Cell Size ~3 ставь 12+)")]
    public float innerRadius = 12f;

    [Tooltip("Где уже полная темнота")]
    public float outerRadius = 20f;

    [Tooltip("Насколько резко уходит в темноту (больше = резче)")]
    public float falloffPower = 2.5f;

    [Tooltip("Цвет и сила затемнения")]
    public Color fogColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Насколько больше экрана покрывает маска")]
    public float coverageScale = 1.4f;

    private Transform _overlay;
    private Material _material;
    private Camera _camera;

    private static readonly int PlayerPosId = Shader.PropertyToID("_PlayerWorldPos");
    private static readonly int InnerRadiusId = Shader.PropertyToID("_InnerRadius");
    private static readonly int OuterRadiusId = Shader.PropertyToID("_OuterRadius");
    private static readonly int FalloffPowerId = Shader.PropertyToID("_FalloffPower");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        CreateOverlay();
    }

    private void CreateOverlay()
    {
        if (_overlay != null) return;

        var shader = Shader.Find("UnstableExperiment/VisionFog");
        if (shader == null)
        {
            Debug.LogWarning("VisionFog: шейдер не найден. Проверь Assets/Shaders/VisionFog.shader");
            return;
        }

        _material = new Material(shader);

        var go = new GameObject("VisionFogOverlay");
        go.transform.SetParent(transform);

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = BuildQuad();

        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = _material;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.sortingOrder = 1000;
        _overlay = go.transform;
    }

    private void LateUpdate()
    {
        if (_overlay == null || _material == null || _camera == null) return;

        var camPos = transform.position;
        _overlay.position = new Vector3(camPos.x, camPos.y, 0f);

        float height = _camera.orthographicSize * 2f * coverageScale;
        float width = height * _camera.aspect;
        _overlay.localScale = new Vector3(width, height, 1f);

        var p = player != null ? player.position : camPos;
        _material.SetVector(PlayerPosId, new Vector4(p.x, p.y, 0f, 0f));
        _material.SetFloat(InnerRadiusId, innerRadius);
        _material.SetFloat(OuterRadiusId, outerRadius);
        _material.SetFloat(FalloffPowerId, falloffPower);
        _material.SetColor(ColorId, fogColor);
    }

    private static Mesh BuildQuad()
    {
        var mesh = new Mesh { name = "VisionFogQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f)
        };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateNormals();
        return mesh;
    }

    private void OnDestroy()
    {
        if (_material != null) Destroy(_material);
    }
}
