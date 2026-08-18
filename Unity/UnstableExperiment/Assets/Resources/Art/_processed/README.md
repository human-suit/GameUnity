# Обработанные спрайты

Скрипт: `Tools/sprite_outline_batch.py`

| Папка | Что внутри |
|-------|------------|
| `outlined/` | Прозрачный фон + **чёрная обводка** (2 px) — для игры |
| `transparent/` | Только прозрачный фон, **без** обводки |

**Не трогает:** окружение, комнаты, tilesets, карты маршрута.

## Перезапустить

```bash
python Tools/sprite_outline_batch.py
```

Положи новый PNG (например Subject 07 sheet) в `Characters/` или `герои/` → запусти снова.

## В Unity

1. Кликни PNG → **Sprite (2D and UI)** → **Alpha Is Transparency** → Apply
2. Для игры бери файлы из **`outlined/`**
