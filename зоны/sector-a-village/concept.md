# Sector A — Деревня

> **Модель мира:** комнаты + двери + Route Map → `КОНЦЕПЦИЯ-ИГРЫ.md` §8 · `игра/data/rooms_graph.json`

## Назначение

Tutorial-сектор ~4 мин. Стабильные правила. Учит: ходьба по **комнатам**, aggro, lottery-бой, **карта маршрута**, ключ → exit-door.

## Комнаты (не одна большая карта)

| room_id | Роль |
|---------|------|
| `a_plaza` | Hub — старт, 4 двери |
| `a_home` | Ключ + **Карта сектора A** |
| `a_well` | Event |
| `a_alley` | 2× Subject 03 |
| `a_gate` | Exit → Sector B |

## Контент

| Элемент | Детали |
|---------|--------|
| Patrol | 2–3 × Subject 03 |
| Optional | 1 дом с consumable (снять 3 poison — на будущее) |
| Fetch | ключ в доме → открыть ворота |
| Unstable | **нет** |
| Event | нет |

## Атмосфера

Серое небо, фонари не горят, табличка на входе: *«Sector A — baseline»*.

## Assets (F&H v1)

| Файл | Назначение |
|------|------------|
| `окружение/sector-a-village/combat_bg_sector_a.png` | Фон card combat |
| `окружение/sector-a-village/topdown_sector_a_sheet.png` | Top-down props 32px |

## Переход

Fade + голос: *«Sector B. Toxin parameters changed.»*
