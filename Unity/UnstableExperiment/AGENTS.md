# Unstable Experiment — Unity (рабочая папка игры)

> **Открывай в Unity Hub И в Cursor именно эту папку:**  
> `Unity/UnstableExperiment`

## Быстрый старт

1. Unity Hub → **Open** → `Unity/UnstableExperiment`
2. Дождись импорта → открой сцену **`Assets/Scenes/Main.unity`**
3. **Play** ▶

Cursor/AI правит код в `Assets/Scripts/`. После правок Unity перекомпилирует — снова Play.

## Управление

| Клавиша | Действие |
|---------|----------|
| WASD | Движение |
| E | Дверь / лут |
| Tab | Route Map (после карты в Доме) |
| REDEEM / End Turn | Бой (GUI) |

## Структура

```
Assets/
  Scenes/Main.unity       ← главная сцена (Play сразу)
  Scripts/                ← весь код игры
  Resources/Data/         ← rooms_graph, tickets, unstable_rules
  Art/                    ← спрайты комнат и персонажей
  Docs/                   ← копия GDD для справки в редакторе
  Editor/                 ← меню Unstable Experiment
```

## Данные

Source of truth для level design: `Assets/Resources/Data/rooms_graph.json`  
Синхрон с корнем репо: `игра/data/rooms_graph.json`

## AI / Cursor

При правках игры:
- Код: `Assets/Scripts/**/*.cs`
- Баланс: `Assets/Resources/Data/*.json`
- Арт комнаты: `Assets/Art/Rooms/{room_id}_room.png`
- Новая комната: добавить PNG + запись в `rooms_graph.json`

## Меню Unity

**Unstable Experiment → Create Demo Scene** — пересоздать Main.unity  
**Unstable Experiment → Reimport Art** — обновить спрайты из Art/

## Версия

Unity **2022.3 LTS**
