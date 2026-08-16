# Room: a_plaza — Площадь (Sector A hub)

> Граф: `игра/data/rooms_graph.json` · master: `КОНЦЕПЦИЯ-ИГРЫ.md`

## Параметры

| Поле | Значение |
|------|----------|
| id | `a_plaza` |
| type | hub |
| size | **18×14** tiles (32px) |
| start | `true` (spawn south-center при новом run) |

## Layout

```
        [N: WELL]
            |
[W:GATE]— PLAZA —[E: ALLEY]
   LOCKED      |
            [S: HOME]
```

## Двери

| id | edge | target | state |
|----|------|--------|-------|
| north | top | `a_well` | open |
| east | right | `a_alley` | open |
| south | bottom | `a_home` | open |
| west | left | `a_gate` | **locked** until `rust_key` |

## Spawns

| enemy | count | notes |
|-------|-------|-------|
| subject_03 | 1 | patrol center, tutorial aggro |

## Props

- Sign: SECTOR A BASELINE
- Broken lamp (decoration, block)
- House facades NW/NE corners (block)

## Assets

| Файл | Use |
|------|-----|
| `a_plaza_room.png` | Room tilemap reference / Unity background layer |
| `doors_sector_a_sheet.png` | Door trigger overlays (4 variants) |
| `../../окружение/sector-a-village/topdown_sector_a_sheet.png` | Individual tile slices |

## Unity

- PPU 32, layers: Ground / Collision / Props / Doors / Actors
- Player spawn: tile (9, 12) south path
- Door triggers: 2-tile wide at each edge opening

## Prompt (regenerate)

```
Top-down room 18x14 tiles 32px. F&H hand-drawn horror Sector A plaza hub.
Cobble center, grass walkable, four edge doors WELL north ALLEY east HOME south GATE west locked chains.
Sign SECTOR A BASELINE, broken lamp, house walls corners blocking.
No characters. Walkable paths clear.
```
