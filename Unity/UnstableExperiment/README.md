# Unstable Experiment — Unity Demo

> **Это и есть игра.** Открывай эту папку в Unity Hub и в Cursor.

## Play за 30 секунд

1. **Unity Hub** → Open → выбери **`Unity/UnstableExperiment`**
2. Unity 2022.3 LTS импортирует проект (первый раз ~1–2 мин)
3. Открой **`Assets/Scenes/Main.unity`** (создаётся автоматически при первом открытии)
4. **Play ▶**

## Cursor / AI

Открывай в Cursor **эту же папку** (`Unity/UnstableExperiment`):

| Что править | Где |
|-------------|-----|
| Геймплей | `Assets/Scripts/` |
| Комнаты, двери | `Assets/Resources/Data/rooms_graph.json` |
| Бой | `Assets/Resources/Data/tickets.json` |
| Арт комнаты | `Assets/Resources/Art/Rooms/{room_id}_room.png` |

После правок → вернись в Unity → Play (скрипты перекомпилируются сами).

См. также **`AGENTS.md`** в этой папке.

## Управление

| | |
|--|--|
| WASD | ходьба |
| E | дверь / лут |
| Tab | Route Map (после карты в Доме) |
| REDEEM / End Turn | бой |

## Demo-путь Sector A

Площадь → **Дом** (юг) → ключ + карта → **Tab** → **Ворота** (запад) → Sector B

## Меню Unity

- **Unstable Experiment → Create Demo Scene** — пересоздать Main
- **Unstable Experiment → Reimport Art** — обновить спрайты

## Структура

```
Assets/
  Scenes/Main.unity
  Scripts/          ← код
  Resources/
    Data/           ← JSON
    Art/            ← PNG
  Docs/             ← GDD копия
  Editor/           ← auto-setup
```

## Дизайн-доки (полная версия)

В корне git-репо: `НАЧАЛО-РАБОТЫ.md`, `КОНЦЕПЦИЯ-ИГРЫ.md`
