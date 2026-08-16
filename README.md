# GameUnity — Unstable Experiment

> Репозиторий: [human-suit/GameUnity](https://github.com/human-suit/GameUnity)  
> **Старт для команды:** [`НАЧАЛО-РАБОТЫ.md`](НАЧАЛО-РАБОТЫ.md)

## Unified v5 (актуально)

**Мастер-промпт (все правки):** `prompts/MASTER-unified-sheet.md`

| Персонаж | HP | Файл | v5 |
|----------|-----|------|-----|
| Subject 07 (ГГ) | 40 | `герои/subject-07/.../subject_07_unified_sheet.png` | v4 — **не трогали** |
| Subject 03 | 20 | `враги/subject-03/sprite-sheet/` | ✓ |
| Plague | 32 | `враги/plague/sprite-sheet/` | ✓ |
| Subject 12 | 44 | `враги/subject-12/sprite-sheet/` | ✓ |
| Ward Hulk | 28 | `враги/ward-hulk/` | ✓ |
| Mask Wretch | 18 | `враги/mask-wretch/sprite-sheet/` | ✓ |
| Pit Dweller | 14 | `враги/pit-dweller/sprite-sheet/` | ✓ |
| Patchwork Butcher | 85 | `враги/patchwork-butcher/sprite-sheet/` | ✓ |

## Что в v5 (все правки сессии)

1. Layout Ward Hulk merged (FRONT/BACK/PARTS/EXPR/CARDS/ITEMS)
2. F&H hand-drawn mottled skin, **не** cobblestone stipple
3. **Один стиль** на всём листе
4. WALK 4 направления, **R-L-R-L** на FRONT/BACK
5. IDLE/ACTION/STAGGER/DEATH как эталон
6. Soft blur, thick outlines, block shadows

## Demo

`demo/index.html` · `demo/characters/` · `demo/environment/`

## Концепция игры (master)

`КОНЦЕПЦИЯ-ИГРЫ.md` · граф: `игра/data/rooms_graph.json`  
Room A: `игра/rooms/sector-a/a_plaza_room.png` · Route Map: `игра/ui/route_map_sector_a.png`

## Окружение (F&H v1)

`окружение/ENVIRONMENT-v1.md` · `окружение/sector-*/`

## Другое

- Protocol chits: `ui-карты/sprites/protocol_chits_sheet.png`
- JSON: `ui-карты/data/tickets.json`
