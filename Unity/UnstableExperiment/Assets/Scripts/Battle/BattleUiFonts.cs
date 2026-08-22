using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class BattleUiFonts
{
    private static TMP_FontAsset _tmpFont;
    private static Font _legacyFont;
    private static bool _ready;

    public static void Ensure()
    {
        if (_ready && _tmpFont != null)
            return;

        try
        {
            _legacyFont = Resources.Load<Font>("Fonts/CyrillicUI");
            if (_legacyFont == null)
                _legacyFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (_legacyFont != null)
            {
                _tmpFont = TMP_FontAsset.CreateFontAsset(
                    _legacyFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic);
            }

            if (_tmpFont != null)
            {
                _tmpFont.name = "BattleCyrillic SDF";
                _tmpFont.TryAddCharacters(
                    "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
                    "абвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
                    "0123456789:/%-+.,!?() ");

                List<TMP_FontAsset> fallbacks = TMP_Settings.fallbackFontAssets;
                if (fallbacks != null && !fallbacks.Contains(_tmpFont))
                    fallbacks.Add(_tmpFont);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("BattleUiFonts: " + exception.Message);
            _tmpFont = null;
        }

        _ready = true;
    }

    public static void Apply(TMP_Text text)
    {
        if (text == null)
            return;

        Ensure();
        if (_tmpFont == null)
            return;

        text.font = _tmpFont;
        text.ForceMeshUpdate();
    }

    public static void Apply(Text text, int fontSize)
    {
        if (text == null)
            return;

        Ensure();
        if (_legacyFont != null)
            text.font = _legacyFont;

        text.fontSize = fontSize;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }

    public static void ApplyAllInScene()
    {
        Ensure();
        if (_tmpFont == null)
            return;

        TMP_Text[] tmpTexts = Object.FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < tmpTexts.Length; i++)
            Apply(tmpTexts[i]);

        Text[] legacyTexts = Object.FindObjectsByType<Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < legacyTexts.Length; i++)
        {
            if (_legacyFont != null)
                legacyTexts[i].font = _legacyFont;
        }
    }
}
