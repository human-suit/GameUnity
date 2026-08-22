using UnityEngine;
using UnityEngine.UI;

public class StatusPlaqueHud : MonoBehaviour
{
    private Image _hpFill;
    private Image[] _energyLamps;
    private Sprite _energyOn;
    private Sprite _energyOff;
    private BattleManager _battle;

    public static StatusPlaqueHud Ensure(BattleManager battle)
    {
        StatusPlaqueHud hud = FindFirstObjectByType<StatusPlaqueHud>();
        if (hud == null)
        {
            GameObject go = new GameObject("StatusPlaqueHud");
            hud = go.AddComponent<StatusPlaqueHud>();
        }

        hud._battle = battle;
        hud.BuildIfNeeded();
        hud.Refresh();
        return hud;
    }

    private void BuildIfNeeded()
    {
        if (_hpFill != null)
            return;

        Canvas canvas = FindHudCanvas();
        if (canvas == null)
            return;

        Sprite frame = Load("ui_plaque_frame");
        Sprite hpSlot = Load("ui_hp_slot");
        Sprite hpFill = Load("ui_hp_fill");
        _energyOn = Load("ui_energy_on");
        _energyOff = Load("ui_energy_off");
        Sprite gold = Load("ui_gold_token");
        Sprite lvl = Load("ui_lvl_stamp");

        RectTransform root = GetComponent<RectTransform>();
        if (root == null)
            root = gameObject.AddComponent<RectTransform>();

        root.SetParent(canvas.transform, false);
        root.SetSiblingIndex(1);
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(-720f, 370f);
        root.sizeDelta = new Vector2(500f, 280f);

        CreateImage(root, "Frame", frame, Vector2.zero, new Vector2(500f, 280f), false);

        RectTransform slot = CreateImage(
            root,
            "HpSlot",
            hpSlot,
            new Vector2(20f, 70f),
            new Vector2(300f, 36f),
            true);

        _hpFill = CreateImage(
            slot,
            "HpFill",
            hpFill,
            Vector2.zero,
            new Vector2(250f, 18f),
            true).GetComponent<Image>();
        _hpFill.type = Image.Type.Filled;
        _hpFill.fillMethod = Image.FillMethod.Horizontal;
        _hpFill.fillOrigin = 0;

        _energyLamps = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            RectTransform lamp = CreateImage(
                root,
                "EnergyLamp" + i,
                _energyOn,
                new Vector2(-40f + i * 46f, 8f),
                new Vector2(40f, 40f),
                true);
            _energyLamps[i] = lamp.GetComponent<Image>();
        }

        CreateImage(root, "LvlIcon", lvl, new Vector2(-150f, -70f), new Vector2(70f, 28f), true);
        CreateImage(root, "GoldIcon", gold, new Vector2(-40f, -70f), new Vector2(36f, 36f), true);
    }

    public void Refresh()
    {
        if (_hpFill != null)
        {
            float max = Mathf.Max(1, GameState.PlayerMaxHealth);
            _hpFill.fillAmount = GameState.PlayerHealth / max;
        }

        if (_energyLamps == null || _battle == null)
            return;

        int energy = _battle.CurrentEnergy;
        for (int i = 0; i < _energyLamps.Length; i++)
        {
            if (_energyLamps[i] == null)
                continue;
            _energyLamps[i].sprite = i < energy ? _energyOn : _energyOff;
        }
    }

    private static Canvas FindHudCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                return canvases[i];
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    private static Sprite Load(string name)
    {
        return Resources.Load<Sprite>("UI/StatusPlaque/" + name);
    }

    private static RectTransform CreateImage(
        Transform parent,
        string objectName,
        Sprite sprite,
        Vector2 anchoredPosition,
        Vector2 size,
        bool centered)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        if (centered)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }
        else
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.enabled = sprite != null;
        return rect;
    }
}
