# Комнаты 2.5D — Sector A (Fear & Hunger style)

Все sheet: **8×4**, ячейка **64×64**, фон **magenta** → в Unity **Alpha Is Transparency** или Color to Alpha.

---

## Файлы

| Sheet | Слой Unity | Назначение |
|-------|------------|------------|
| `room_25d_floors_corners_sector_a.png` | **Ground** | Пол + углы (ряды 1–2 пол, 3–4 углы) |
| `room_25d_back_walls_sector_a.png` | **Wall_Back** | **Задняя** стена (верх экрана), ряды 1–4 |
| `room_25d_side_segments_sector_a.png` | **Wall_Left / Wall_Right** | Бок с толщиной (ref угол) |
| `room_25d_corners_interior_sector_a.png` | **Углы** | TL/TR/BL/BR + стыки |
| `room_25d_environment_mine_walls.png` | **Mine / Curved** | Шахта, балки, дуги (ref карт. 3) |
| `room_25d_front_cap_doors_sector_a.png` | **Wall_Front + Doors** | **Перед** чёрный cap (ряд 1–2), арки/двери (ряд 3–4) |

---

## Сборка комнаты (Hierarchy)

```
Grid
├── Ground              ← floors sheet, ряды 1–2
├── Wall_Back           ← back walls, 2–3 ряда сверху комнаты
├── Wall_Left           ← side sheet, левая колонка
├── Wall_Right          ← side sheet, правая колонка
├── Wall_Front          ← front cap, 1 ряд снизу (чёрная полоса)
├── Doors               ← арки из front sheet ряд 3
├── Props               ← колодец, табличка, забор (старый sheet)
└── Colliders           ← невидимые BoxCollider2D по краям
```

---

## Схема (вид сверху, игрок внутри)

```
╔══ Wall_Back (задняя, красивая) ══════════════╗
║ Wall_L │                              │ Wall_R ║
║   E    │         GROUND (пол)         │   W    ║
║   F    │                              │   T    ║
║   T    │                              │        ║
╠══ Wall_Front (чёрный cap — «рез») ═══════════╣
║              [ арка / дверь ]                 ║
╚══════════════════════════════════════════════╝
```

- **Север** = Back — высокий фасад  
- **Юг** = Front cap — **не** полная стена, игрок виден  
- **Запад/восток** = Side profile — узкие, с «крышкой»  
- **Туман** (Vision Fog) — прячет углы как на скрине F&H  

---

## Боковые стены (как на ref с углом)

**`room_25d_side_segments_sector_a.png`**
- **Ряд 1** — левая колонка, кистью **сверху вниз** (не поворачивать)
- **Ряд 2** — правая колонка
- **Ряд 3–4** — столбы, косяки дверей

Каждый тайл: **верхняя «плита»** + **тёмная внутренняя грань** = толщина стены.

**Углы:** `room_25d_corners_interior_sector_a.png` — ряд 1: TL, TR, BL, BR.

---

## Окружение как на 3-й картинке (шахта / пещера)

**Да — это тоже спрайты-тайлы**, не 3D:

| Что видишь | Sheet `environment_mine_walls` |
|------------|--------------------------------|
| Изогнутая каменная стена | ряд 2 — curved pieces |
| Деревянные стойки | ряд 1–2 — post / beam |
| Пол с камушками | ряд 3 |
| Рельсы | ряд 4 |

Собираешь **дугу** из curved + straight + post между швами.

---

| Слой | Order |
|------|-------|
| Ground | 0 |
| Wall_Back | 1 |
| Props (на полу) | 2 |
| Player | 3 |
| Wall_Left / Wall_Right | 4 |
| Wall_Front | 5 |

Или **Transparency Sort Mode: Custom Axis** Y для объектов на полу.

---

## Unity — import

1. PNG → **Sprite Multiple**, **PPU 64**, **Point**
2. **Slice Grid 8×4**
3. Отдельные **Tile Palette**: `25d_Ground`, `25d_Back`, `25d_Sides`, `25d_Front`
4. Рисуй на **отдельных Tilemap** (не один Walls на всё)

---

## Коллизия

- **Ground** — без коллайдера  
- **Wall_*** — Tilemap Collider 2D + Composite на каждом слое стен ИЛИ один **Colliders** parent с BoxCollider2D по периметру (проще для джема)  
- **Дверь** — trigger без коллайдера на проходе  

---

## Пример размеров комнаты

**12×10** тайлов пола + 1–2 ряда back + 1 ряд front cap + 1 колонка sides.

Старт: **a_plaza** — скопируй layout из `rooms_graph.json` sizeTiles [18,14] можно уменьшить для теста до 12×8.

---

## Старые sheet

`room_walls_sector_a_sheet.png` и `profile_sheet` — можно оставить для top-down; для **2.5D используй папку `25d/`**.
