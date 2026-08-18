# F&H 2.5D — как на референсах

## Три типа стены (главное!)

| Тип | Как выглядит на карте | Какой sheet |
|-----|----------------------|-------------|
| **Задняя (North)** | Высокий **фасад** — видно кирпич/камень | `back_wall_tall_facades` — **2–3 ряда** тайлов вверх |
| **Боковая (East/West)** | **Узкая** полоска: верхняя кромка + тень | `side_segments` — **вертикально** |
| **Горизонтальная кромка** | Тонкая линия сверху коридора (E–W) | `wall_caps_horizontal` — **горизонтально** |

**Не поворачивай** фасад на 90° — для боков другой тайл!

---

## Референс → sheet

| Картинка | Что там | Чем собирать |
|----------|---------|--------------|
| **Подземелье** (коридоры) | N-стена фасад, E-W тонкие кромки, дуги | caps + back + arches_curves |
| **Thicket / двор** | Высокий забор сзади, бока-полоски, выход снизу | back_tall ×3 + side_segments + front_cap |
| **Двор с виселицами** | Арка INNER HALL, изогнутые углы | arches_curves + corners_interior |
| **Ma'habre tomb** | Песчаник, длинный коридор, статуи | back_tall (tan row) + side + props отдельно |

---

## Сборка комнаты (как F&H)

```
     [ back_tall row 3 ]
     [ back_tall row 2 ]
     [ back_tall row 1 ]  ← задняя стена (3 ряда!)
side │    GROUND (пол)    │ side
seg  │                    │ seg
     [ front_cap / арка ]
```

**Коридор горизонтальный:**
```
wall_caps_horizontal (верх и низ коридора)
side_segments (бок если нужен)
```

**Угол / дуга:** `arches_curves_corridors` + `corners_interior`

---

## Sorting

Ground 0 → Back 1 → Props 2 → Player 3 → Side 4 → Front 5

+ Vision Fog.

---

## Все sheet в папке `25d/`

### Боковые (вертикальные profile) — по сеттингу

| Sheet | Сектор | Где использовать |
|-------|--------|------------------|
| `room_25d_side_segments_sector_a_v2.png` | A — подземелье | Серый камень + грязь, как на ref |
| `room_25d_side_segments_sector_a_outdoor_fence.png` | A — деревня | Камень + деревянный забор |
| `room_25d_side_segments_sector_b_swamp_v2.png` | B — болото | **Тот же layout что v2**, зелёный мох/слизь |
| `room_25d_side_segments_tomb_sandstone_v2.png` | Ma'habre / катакомбы | **Тот же layout**, песчаный камень |
| `room_25d_side_segments_sector_c_courtyard_v2.png` | C — двор | **Тот же layout**, outdoor cobble |

> Старые `*_swamp.png` / `*_sandstone.png` без `_v2` — **не использовать** (декоративные тайлы, не profile-бока).

**Ряды:** 1 = левая колонка, 2 = правая, 3 = двойная/столб, 4 = косяк/обломок.

### Горизонтальные кромки (E–W коридоры)

| Sheet | Назначение |
|-------|------------|
| `room_25d_wall_caps_horizontal_variants.png` | Тонкая **верхняя кромка** стены (вид сверху), ряды 1–2 = камень, 3 = болото, 4 = обломок |

### Углы

| Sheet | Назначение |
|-------|------------|
| `room_25d_corners_interior_v2.png` | TL/TR/BL/BR + T-стыки + арка в углу |

### Остальное

- `room_25d_back_wall_tall_facades.png` — задний фасад (3 ряда)
- `room_25d_arches_curves_corridors.png` — дуги, арки
- `room_25d_floors_corners_sector_a.png` — пол
- `room_25d_front_cap_doors_sector_a.png` — перед + двери
- `room_25d_environment_mine_walls.png` — шахта

Import: **Multiple, PPU 64, Slice 8×4**, Point filter.
