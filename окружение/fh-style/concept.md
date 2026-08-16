# Окружение — Fear & Hunger style

## Стиль (референсы пользователя)

| Приём | Описание |
|-------|----------|
| Линия | Толстый чёрный контур, «уголь/тушь», неровный |
| Свет | Жёсткий спереди, **vignette**, углы в чёрный |
| Палитра | Серый, грязный бурый, бледная плоть, **единственный акцент — кровь** |
| Пол | Неровная каменная кладка, швы чёрные |
| Стены | Арки в **полную тьму**, каменные маски/скulptures |
| Не pixel | Hand-drawn battle layer для card combat overlay |

## Слои Unity

| Asset | Use |
|-------|-----|
| `sector-*/combat_bg_sector_*.png` | Фон card combat |
| `sector-*/topdown_sector_*_sheet.png` | Top-down props 32px |

## Sector mapping

| Сектор | Фон |
|--------|-----|
| A | Каменный коридор + арки |
| B | Тот же камень + зелёный tint + лужи |
| C | Двор/склад + прожектор, кучи |

## Файлы

**v1 (актуально):** `окружение/ENVIRONMENT-v1.md` + `sector-*/`

| Сектор | Combat BG | Top-down sheet |
|--------|-----------|----------------|
| A | `sector-a-village/combat_bg_sector_a.png` | `topdown_sector_a_sheet.png` |
| B | `sector-b-swamp/combat_bg_sector_b.png` | `topdown_sector_b_sheet.png` |
| C | `sector-c-yard/combat_bg_sector_c.png` | `topdown_sector_c_sheet.png` |

**Архив:** `references/` — скрины F&H пользователя
