# Unified Asset Spec (v3)

## Два референса пользователя

| Левый (Subject 03 sheet) | Правый (F&H encounter) |
|--------------------------|-------------------------|
| **Структура листа** | **Стиль рисования** |
| FRONT / BACK | Толстый чёрный контур |
| BODY PARTS | Cross-hatch тени |
| EXPRESSIONS | Бледная плоть, desaturated |
| IDLE WALK ACTION STAGGER DEATH | Vignette, атмосфера |
| COMBAT CARDS / ITEMS | **Лёгкое размытие/soft edges** — не crisp pixel |
| Palette + HEALTH | Камень пола, horror mood |

## Правило для ВСЕХ персонажей

Один PNG = полный лист. Hand-drawn Fear & Hunger, **слегка размытый** (soft focus, painterly).

```
TOP LEFT:    HEALTH + 7 swatches
TOP CENTER:  FRONT + BACK (3/4 или front/back)
TOP RIGHT:   BODY PARTS
MID RIGHT:   EXPRESSIONS (4–6)
BOTTOM LEFT: IDLE | WALK | ACTION | STAGGER | DEATH (side view grid)
BOTTOM RIGHT: COMBAT INTENTS/CARDS + ITEMS
```

## Демо (проверка понимания)

| Файл | Что показывает |
|------|----------------|
| `demo/ward_hulk_unified_sheet_v5.png` | Эталонный лист v5 |
| `demo/combat_mockup_demo_v2.png` | Бой: фон + враг + chits + HUD |

## Миграция (после одобрения демо)

1. Subject 07, 03, Plague, 12, Butcher → unified sheets
2. Mask Wretch, Pit Dweller → unified sheets
3. Top-down map → `окружение/sector-*/topdown_*_sheet.png` (32px F&H props)
