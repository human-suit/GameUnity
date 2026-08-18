using UnityEngine;

/// <summary>
/// Цветные фоны для комнат без PNG.
/// </summary>
public static class RoomSprites
{
    private static Sprite _hub;
    private static Sprite _key;
    private static Sprite _combat;
    private static Sprite _event;
    private static Sprite _default;

    public static Sprite Placeholder(string roomType)
    {
        return roomType switch
        {
            "hub" => _hub ??= Make(new Color(0.22f, 0.24f, 0.2f)),
            "key" => _key ??= Make(new Color(0.2f, 0.18f, 0.15f)),
            "combat" => _combat ??= Make(new Color(0.18f, 0.14f, 0.14f)),
            "event" => _event ??= Make(new Color(0.14f, 0.18f, 0.2f)),
            _ => _default ??= Make(new Color(0.15f, 0.15f, 0.16f))
        };
    }

    private static Sprite Make(Color c)
    {
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
