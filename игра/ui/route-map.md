# Route Map UI — Sector A

> Механика: `КОНЦЕПЦИЯ-ИГРЫ.md` §6 · данные: `data/rooms_graph.json` → `mapNodes`, `mapEdges`

## Экран

| Элемент | Описание |
|---------|----------|
| Фон | Грязная бумага, штамп **FACILITY ROUTE SECTOR A** |
| Узлы | plaza (current) · well · alley · home · gate |
| Рёбра | Пунктир как STS |
| Gate | Серый + замок до `rust_key` |
| Tooltip | «Западная дверь на площади → Ворота (ключ)» |

## Input

- **Tab / M** — toggle (только если `sector_map_a` в инвентаре)
- Клик узла — показать `doorHintRu`, **не телепорт**

## Asset

- `route_map_sector_a.png` — mockup 1920×1080
- Unity: Canvas overlay, nodes = UI buttons read-only

## Node states (код)

| state | visual |
|-------|--------|
| hidden | `???` до first visit |
| current | красная точка YOU ARE HERE |
| visited | sepia fill |
| cleared | галочка (бой выигран / loot taken) |
| locked | серый + padlock |

## Prompt (regenerate)

```
UI Route Map Sector A grimy protocol paper node graph STS-like hand-drawn horror.
Nodes PLAZA center red WELL north ALLEY east skull HOME south key GATE west locked.
FACILITY ROUTE SECTOR A stamp. 1920x1080 mockup.
```
