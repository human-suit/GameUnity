using UnityEngine;

/// <summary>
/// Окно чтения. Вид только из префаба Resources/UI/ReadPanelView.
/// </summary>
[DefaultExecutionOrder(-100)]
public class ReadPanel : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    public static bool ClosedThisFrame { get; private set; }

    private static ReadPanel _instance;
    private GameObject _root;
    private ReadPanelView _view;

    public static void Show(string text, Sprite portrait = null)
    {
        if (_instance == null)
        {
            var go = new GameObject("ReadPanel");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ReadPanel>();
        }

        _instance.Open(text, portrait);
    }

    public static void Hide()
    {
        if (_instance != null)
            _instance.Close();
    }

    private void Update()
    {
        ClosedThisFrame = false;
        if (!IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            Close();
    }

    private void Open(string text, Sprite portrait)
    {
        if (_root == null)
            SpawnView();

        if (_view != null && _view.body != null)
            _view.body.text = text ?? "";

        if (_view != null && _view.portrait != null)
        {
            _view.portrait.sprite = portrait;
            _view.portrait.gameObject.SetActive(portrait != null);
        }

        if (_root != null)
            _root.SetActive(true);

        IsOpen = true;
        PlayerMove.Frozen = true;
    }

    private void Close()
    {
        if (_root != null)
            _root.SetActive(false);

        IsOpen = false;
        ClosedThisFrame = true;
        PlayerMove.Frozen = false;
    }

    private void SpawnView()
    {
        var prefab = Resources.Load<GameObject>("UI/ReadPanelView");
        if (prefab == null)
        {
            Debug.LogError("Нет префаба Resources/UI/ReadPanelView");
            return;
        }

        _root = Instantiate(prefab);
        DontDestroyOnLoad(_root);
        _root.name = "ReadPanelView";
        _view = _root.GetComponent<ReadPanelView>();
        _root.SetActive(false);
    }
}
